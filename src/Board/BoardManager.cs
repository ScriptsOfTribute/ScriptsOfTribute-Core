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
        private Patron[] _patrons;
        private Tavern _tavern;
        private Player[] _players;
        private Random _rnd;

        public Player CurrentPlayer => _players[(int)CurrentPlayerId];
        public Player EnemyPlayer => _players[1 - (int)CurrentPlayerId];

        public BoardState State { get; set; } = BoardState.NORMAL;

        public BoardManager(PatronId[] patrons)
        {
            this._patrons = GetPatrons(patrons);
            // TODO: This is actually not correct, as some cards should have multiple copies.
            _tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            _players = new Player[] { new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2) };
            _rnd = new Random();
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public PlayResult PatronCall(int patronID, Player activator, Player enemy)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            return _patrons[patronID].PatronActivation(activator, enemy);
        }

        public ExecutionChain PlayCard(Card card)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            var result = CurrentPlayer.PlayCard(card, EnemyPlayer, _tavern);

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

            Card boughtCard = this._tavern.Acquire(card);

            if (boughtCard.Cost > CurrentPlayer.CoinsAmount)
                throw new Exception($"You dont have enough coin to buy {card}");

            CurrentPlayer.CoinsAmount -= boughtCard.Cost;

            return CurrentPlayer.AcquireCard(boughtCard, EnemyPlayer, _tavern);
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
            _tavern.DrawCards();

            List<Card> starterDecks = new List<Card>();

            foreach (var patron in this._patrons)
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
            return new BoardSerializer(_players[0], _players[1], _tavern, _patrons, CurrentPlayerId);
        }

        public PlayerEnum GetPatronFavorism(int idx)
        {
            return _patrons[idx].FavoredPlayer;
        }

        public List<Card> GetAvailableTavernCards()
        {
            return this._tavern.AvailableCards;
        }

        public List<Card> GetAffordableCards(int coinAmount)
        {
            return this._tavern.GetAffordableCards(coinAmount);
        }
    }
}
