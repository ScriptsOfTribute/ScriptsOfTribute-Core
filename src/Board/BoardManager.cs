namespace TalesOfTribute
{
    public enum BoardState
    {
        NORMAL,
        CHOICE_PENDING,
        START_OF_TURN_CHOICE_PENDING,
    }

    public class BoardManager
    {
        private PlayerEnum CurrentPlayerId;
        public Patron[] Patrons;
        public Tavern Tavern;
        private Player[] _players;
        private Random _rnd;

        public Player CurrentPlayer => _players[(int)CurrentPlayerId];
        public Player EnemyPlayer => _players[1 - (int)CurrentPlayerId];

        public BoardState State { get; set; } = BoardState.NORMAL;
        private int PrestigeTreshold = 40;

        public BoardManager(PatronId[] patrons)
        {
            this.Patrons = GetPatrons(patrons);
            // TODO: This is actually not correct, as some cards should have multiple copies.
            Tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            _players = new Player[] { new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2) };
            _rnd = new Random();
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public PlayResult PatronCall(PatronId patron)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            return Array.Find(Patrons, p => p.PatronID == patron).PatronActivation(CurrentPlayer, EnemyPlayer);
        }

        public ExecutionChain PlayCard(Card card)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            var result = CurrentPlayer.PlayCard(card, EnemyPlayer, Tavern);

            State = BoardState.CHOICE_PENDING;

            result.AddCompleteCallback(() => State = BoardState.NORMAL);

            return result;
        }

        public ExecutionChain BuyCard(Card card)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            Card boughtCard = this.Tavern.Acquire(card);

            if (boughtCard.Cost > CurrentPlayer.CoinsAmount)
                throw new Exception($"You dont have enough coin to buy {card}");

            CurrentPlayer.CoinsAmount -= boughtCard.Cost;

            return CurrentPlayer.AcquireCard(boughtCard, EnemyPlayer, Tavern);
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
                var card = player.DrawPile[0];
                player.Hand.Add(card);
                player.DrawPile.Remove(card);
            }
        }

        public ExecutionChain? HandleStartOfTurnChoices()
        {
            // TODO: This state should also block all other actions, just as CHOICE_PENDING state.
            if (State != BoardState.START_OF_TURN_CHOICE_PENDING)
            {
                return null;
            }

            return CurrentPlayer.StartOfTurnEffectsChain;
        }

        public void EndTurn()
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            //TODO: Attack all agents

            CurrentPlayer.PrestigeAmount += CurrentPlayer.PowerAmount;
            CurrentPlayer.CoinsAmount = 0;
            CurrentPlayer.PowerAmount = 0;
            CurrentPlayer.EndTurn();

            CurrentPlayerId = (PlayerEnum)(1 - (int)CurrentPlayerId);

            if (CurrentPlayer.StartOfTurnEffectsChain != null)
            {
                State = BoardState.START_OF_TURN_CHOICE_PENDING;
                CurrentPlayer.StartOfTurnEffectsChain.AddCompleteCallback(() => State = BoardState.NORMAL);
            }
        }

        public void SetUpGame()
        {
            CurrentPlayerId = PlayerEnum.PLAYER1;
            EnemyPlayer.CoinsAmount = 1; // Second player starts with one gold
            Tavern.DrawCards();

            List<Card> starterDecks = new List<Card>();

            foreach (var patron in this.Patrons)
            {
                starterDecks.AddRange(
                    patron.GetStarterCards().Select(cardID => GlobalCardDatabase.Instance.GetCard(cardID)).ToList()
                );
            }

            CurrentPlayer.DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
            EnemyPlayer.DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
        }

        public BoardSerializer SerializeBoard()
        {
            return new BoardSerializer(_players[0], _players[1], Tavern, Patrons, CurrentPlayerId);
        }

        public PlayerEnum GetPatronFavorism(PatronId patron)
        {
            return Array.Find(Patrons, p => p.PatronID == patron).FavoredPlayer;
        }

        public List<Card> GetAvailableTavernCards()
        {
            return this.Tavern.AvailableCards;
        }

        public List<Card> GetAffordableCards(int coinAmount)
        {
            return this.Tavern.GetAffordableCards(coinAmount);
        }

        public Player CheckAndGetWinner()
        {
            if (CurrentPlayer.PrestigeAmount >= 80)
            {
                return CurrentPlayer;
            }

            bool win = true;

            foreach (var patron in this.Patrons)
            {
                if (patron.PatronID == PatronId.TREASURY)
                {
                    continue;
                }
                if (patron.FavoredPlayer != CurrentPlayer.ID)
                {
                    win = false;
                    break;
                }
            }

            if (win)
            {
                return CurrentPlayer;
            }

            if (CurrentPlayer.PrestigeAmount < EnemyPlayer.PrestigeAmount && EnemyPlayer.PrestigeAmount >= PrestigeTreshold)
            {
                return EnemyPlayer;
            }
            else
            {
                return null;
            }
        }
    }
}
