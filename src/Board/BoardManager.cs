namespace TalesOfTribute
{
    public enum BoardState
    {
        NORMAL,
        CHOICE_PENDING,
    }

    public class BoardManager
    {
        public PlayerEnum CurrentPlayerId;
        public Patron[] Patrons;
        public Tavern Tavern;
        public Player[] Players;
        private Random _rnd;

        public Player CurrentPlayer => Players[(int)CurrentPlayerId];
        public Player EnemyPlayer => Players[1-(int)CurrentPlayerId];

        public BoardState State { get; set; } = BoardState.NORMAL;

        public BoardManager(PatronId[] patrons)
        {
            this.Patrons = GetPatrons(patrons);
            // TODO: This is actually not correct, as some cards should have multiple copies.
            Tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            Players = new Player[] { new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2) };
            _rnd = new Random();
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public void PatronCall(int patronID, Player activator, Player enemy)
        {
            Patrons[patronID].PatronActivation(activator, enemy);
        }

        public ExecutionChain PlayCard(CardId cardID)
        {
            if (State == BoardState.CHOICE_PENDING)
            {
                throw new Exception("Complete pending choice first!");
            }

            var result = CurrentPlayer.PlayCard(cardID, EnemyPlayer, Tavern);

            State = BoardState.CHOICE_PENDING;

            result.AddCompleteCallback(() => State = BoardState.NORMAL);

            return result;
        }

        public ExecutionChain BuyCard(CardId card)
        {

            Card boughtCard = this.Tavern.Acquire(card);

            if (boughtCard.Cost > CurrentPlayer.CoinsAmount)
                throw new Exception($"You dont have enough coin to buy {card}");

            CurrentPlayer.CoinsAmount -= boughtCard.Cost;

            if (boughtCard.Type == CardType.CONTRACT_AGENT)
                CurrentPlayer.Agents.Add(boughtCard);
            else if (boughtCard.Type == CardType.CONTRACT_ACTION)
                return CurrentPlayer.PlayCard(
                    boughtCard.Id, EnemyPlayer, this.Tavern
                );
            else
                Players[(int)CurrentPlayerId].CooldownPile.Add(boughtCard);

            // Return empty ExecutionChain
            return new ExecutionChain(
                CurrentPlayer,
                EnemyPlayer,
                this.Tavern
            );
        }

        public void DrawCards()
        {
            var player = CurrentPlayer;
            for (var i = 0; i < 5; i++)
            {
                if (player.DrawPile.Count == 0)
                {
                    player.CooldownPile.OrderBy(x => this._rnd.Next(0, player.CooldownPile.Count)).ToList();
                    player.DrawPile.AddRange(player.CooldownPile);
                    player.CooldownPile = new List<Card>();
                }
                var card = player.DrawPile.First();
                player.Hand.Add(card);
                player.DrawPile.Remove(card);
            }
        }

        public void EndTurn()
        {
            CurrentPlayer.PrestigeAmount += CurrentPlayer.PowerAmount;
            CurrentPlayer.CoinsAmount = 0;
            CurrentPlayer.EndTurn();

            CurrentPlayerId = (PlayerEnum)(1 - (int)CurrentPlayerId);
        }

        public void SetUpGame()
        {
            EnemyPlayer.CoinsAmount = 1; // Second player starts with one gold
            CurrentPlayerId = PlayerEnum.PLAYER1;
            Tavern.DrawCards();

            List<Card> starterDecks = new List<Card>()
            {
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            };

            foreach (var patron in this.Patrons)
            {
                starterDecks.Add(GlobalCardDatabase.Instance.GetCard(patron.GetStarterCard()));
            }

            CurrentPlayer.DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
            EnemyPlayer.DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
        }
    }
}
