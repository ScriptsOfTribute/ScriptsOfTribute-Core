namespace TalesOfTribute
{
    public enum BoardState
    {
        NORMAL,
        CHOICE_PENDING,
    }

    public class BoardManager
    {
        public PlayerEnum CurrentPlayer;
        public Patron[] Patrons;
        public Tavern Tavern;
        public Player[] Players;
        private Random _rnd;
        public BoardState State { get; set; } = BoardState.NORMAL;

        public BoardManager(PatronId[] patrons)
        {
            this.Patrons = GetPatrons(patrons);
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

            var result = Players[(int)CurrentPlayer].PlayCard(cardID, Players[1 - (int)CurrentPlayer], Tavern);

            State = BoardState.CHOICE_PENDING;

            result.AddCompleteCallback(() => State = BoardState.NORMAL);

            return result;
        }

        public void BuyCard(CardId card)
        {
            Card boughtCard = this.Tavern.BuyCard(card);

            if (boughtCard.Type == CardType.CONTRACT_AGENT)
                Players[(int)CurrentPlayer].Agents.Add(boughtCard);
            if (boughtCard.Type == CardType.CONTRACT_ACTION)
                Players[(int)CurrentPlayer].PlayCard(boughtCard.Id, Players[(1 - (int)CurrentPlayer)], this.Tavern);
            else
                Players[(int)CurrentPlayer].CooldownPile.Add(boughtCard);
        }

        public void DrawCards()
        {
            var player = this.Players[(int)CurrentPlayer];
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
            Players[(int)CurrentPlayer].PrestigeAmount += Players[(int)CurrentPlayer].PowerAmount;
            Players[(int)CurrentPlayer].CoinsAmount = 0;
            Players[(int)CurrentPlayer].EndTurn();

            CurrentPlayer = (PlayerEnum)(1 - (int)CurrentPlayer);
        }

        public void SetUpGame()
        {
            Players[(int)PlayerEnum.PLAYER2].CoinsAmount = 1; // Second player starts with one gold
            CurrentPlayer = PlayerEnum.PLAYER1;
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

            Players[(int)PlayerEnum.PLAYER1].DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
            Players[(int)PlayerEnum.PLAYER2].DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
        }
    }
}
