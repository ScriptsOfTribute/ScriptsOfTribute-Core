using System.Numerics;
using TalesOfTribute.Board;

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
        private PlayerEnum _currentPlayerId;
        public Patron[] Patrons;
        public Tavern Tavern;
        private Player[] _players;

        public Player CurrentPlayer => _players[(int)_currentPlayerId];
        public Player EnemyPlayer => _players[1 - (int)_currentPlayerId];
        public BaseChoice? PendingChoice => CurrentPlayer.GetPendingChoice(State);

        public BoardState State { get; set; } = BoardState.NORMAL;
        private int PrestigeTreshold = 40;

        public BoardManager(PatronId[] patrons)
        {
            this.Patrons = GetPatrons(patrons);
            // TODO: This is actually not correct, as some cards should have multiple copies.
            Tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            _players = new Player[] { new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2) };
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
            if (CurrentPlayer.PatronCalls <= 0)
            {
                return new Failure("You cant use Patron calls anymore");
            }

            var result = Array.Find(Patrons, p => p.PatronID == patron).PatronActivation(CurrentPlayer, EnemyPlayer);
            if (result is not Failure)
                CurrentPlayer.PatronCalls--;
            return result;
        }

        public ExecutionChain PlayCard(Card card)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            var result = CurrentPlayer.PlayCard(card, EnemyPlayer, Tavern);

            return WrapWithStateRefresh(result);
        }

        public ExecutionChain BuyCard(Card card)
        {
            if (State != BoardState.NORMAL)
            {
                throw new Exception("Complete pending choice first!");
            }

            if (card.Cost > CurrentPlayer.CoinsAmount)
                throw new Exception($"You dont have enough coin to buy {card}");

            Card boughtCard = this.Tavern.Acquire(card);

            CurrentPlayer.CoinsAmount -= boughtCard.Cost;

            return WrapWithStateRefresh(CurrentPlayer.AcquireCard(boughtCard, EnemyPlayer, Tavern));
        }

        public ExecutionChain WrapWithStateRefresh(ExecutionChain chain)
        {
            State = BoardState.CHOICE_PENDING;
            chain.AddCompleteCallback(() => State = BoardState.NORMAL);

            return chain;
        }

        public void DrawCards()
        {
            for (var i = 0; i < 5; i++)
            {
                CurrentPlayer.Draw();
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

            var agentsWithTaunt = EnemyPlayer.Agents.FindAll(agent => agent.RepresentingCard.Taunt);
            foreach(var agent in agentsWithTaunt)
            {
                CurrentPlayer.AttackAgent(agent.RepresentingCard, EnemyPlayer, Tavern);
            }

            CurrentPlayer.PrestigeAmount += CurrentPlayer.PowerAmount;
            CurrentPlayer.CoinsAmount = 0;
            CurrentPlayer.PowerAmount = 0;
            CurrentPlayer.EndTurn();

            _currentPlayerId = (PlayerEnum)(1 - (int)_currentPlayerId);

            if (CurrentPlayer.StartOfTurnEffectsChain != null)
            {
                State = BoardState.START_OF_TURN_CHOICE_PENDING;
                CurrentPlayer.StartOfTurnEffectsChain.AddCompleteCallback(() => State = BoardState.NORMAL);
            }

            foreach (var patron in Patrons)
            {
                patron.PatronPower(CurrentPlayer, EnemyPlayer);
            }

            DrawCards();
        }

        public void SetUpGame()
        {
            _currentPlayerId = PlayerEnum.PLAYER1;
            EnemyPlayer.CoinsAmount = 1; // Second player starts with one gold
            Tavern.DrawCards();

            List<Card> starterDecks = new List<Card>();

            foreach (var patron in this.Patrons)
            {
                starterDecks.AddRange(
                    patron.GetStarterCards().Select(cardId => GlobalCardDatabase.Instance.GetCard(cardId)).ToList()
                );
            }

            CurrentPlayer.InitDrawPile(starterDecks);
            EnemyPlayer.InitDrawPile(starterDecks);

            DrawCards();
        }
        
        public SerializedBoard SerializeBoard()
        {
            return new SerializedBoard(CurrentPlayer, EnemyPlayer, Tavern, Patrons, State, PendingChoice);
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

        public EndGameState? CheckAndGetWinner()
        {
            /*
             * ALWAYS USE THIS AFTER EndTurn()
             * Since we have this assumption we have to check if:
             * - Enemy (recent) player reached 80 or more prestige
             * - Enemy (recent) has 4 patron favors
             * - Current player has more than 40 prestige and Enemy (recent) player didn't match it
             */
            if (EnemyPlayer.PrestigeAmount >= 80) 
            {
                return new EndGameState(EnemyPlayer.ID, GameEndReason.PRESTIGE_OVER_80);
            }

            bool win = true;

            foreach (var patron in this.Patrons)
            {
                if (patron.PatronID == PatronId.TREASURY)
                {
                    continue;
                }
                if (patron.FavoredPlayer != EnemyPlayer.ID)
                {
                    win = false;
                    break;
                }
            }

            if (win)
            {
                return new EndGameState(EnemyPlayer.ID, GameEndReason.PATRON_FAVOR);
            }

            if (CurrentPlayer.PrestigeAmount >= PrestigeTreshold && EnemyPlayer.PrestigeAmount < PrestigeTreshold)
            {
                return new EndGameState(CurrentPlayer.ID, GameEndReason.PRESTIGE_OVER_40_NOT_MATCHED);
            }

            return null;
        }

        public ExecutionChain ActivateAgent(Card card)
        {
            return WrapWithStateRefresh(CurrentPlayer.ActivateAgent(card, EnemyPlayer, Tavern));
        }

        public ISimpleResult AttackAgent(Card agent)
        {
            return CurrentPlayer.AttackAgent(agent, EnemyPlayer, Tavern);
        }
    }
}
