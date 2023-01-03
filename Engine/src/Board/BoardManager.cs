using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;

namespace TalesOfTribute
{
    public class BoardManager
    {
        public Patron[] Patrons;
        public Tavern Tavern;
        private PlayerContext _playerContext;

        public Player CurrentPlayer => _playerContext.CurrentPlayer;
        public Player EnemyPlayer => _playerContext.EnemyPlayer;
        public readonly CardActionManager CardActionManager;

        private int PrestigeTreshold = 40;

        public BoardManager(PatronId[] patrons)
        {
            this.Patrons = GetPatrons(patrons);
            // TODO: This is actually not correct, as some cards should have multiple copies.
            Tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            _playerContext = new PlayerContext(new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2));
            CardActionManager = new CardActionManager(_playerContext, Tavern);
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public void PatronCall(PatronId patronId)
        {
            if (CurrentPlayer.PatronCalls <= 0)
            {
                throw new Exception("You cant use Patron calls anymore");
            }

            var patron = Array.Find(Patrons, p => p.PatronID == patronId);
            CurrentPlayer.PatronCalls--;
            CardActionManager.ActivatePatron(patron);
        }

        public void PlayCard(Card card)
        {
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.PLAY_CARD, card));
            CurrentPlayer.PlayCard(card);
            CardActionManager.PlayCard(card);
        }

        public void BuyCard(Card card)
        {
            if (card.Cost > CurrentPlayer.CoinsAmount)
                throw new Exception($"You dont have enough coin to buy {card}");
            
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.BUY_CARD, card));

            var boughtCard = this.Tavern.Acquire(card);

            CurrentPlayer.CoinsAmount -= boughtCard.Cost;
            
            switch (boughtCard.Type)
            {
                case CardType.CONTRACT_AGENT:
                {
                    var agent = Agent.FromCard(boughtCard);
                    agent.MarkActivated();
                    CurrentPlayer.Agents.Add(agent);
                    CardActionManager.PlayCard(boughtCard);
                    break;
                }
                case CardType.CONTRACT_ACTION:
                {
                    Tavern.Cards.Add(boughtCard);
                    CardActionManager.PlayCard(boughtCard);
                    break;
                }
                default:
                    CurrentPlayer.CooldownPile.Add(boughtCard);
                    break;
            }
        }

        public void DrawCards()
        {
            for (var i = 0; i < 5; i++)
            {
                CurrentPlayer.Draw();
            }
        }

        public void EndTurn()
        {
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.END_TURN));
            var agentsWithTaunt = EnemyPlayer.Agents.FindAll(agent => agent.RepresentingCard.Taunt);
            foreach(var agent in agentsWithTaunt)
            {
                CurrentPlayer.AttackAgent(agent.RepresentingCard, EnemyPlayer, Tavern);
            }

            CurrentPlayer.PrestigeAmount += CurrentPlayer.PowerAmount;
            CurrentPlayer.CoinsAmount = 0;
            CurrentPlayer.PowerAmount = 0;
            CurrentPlayer.EndTurn();

            _playerContext.Swap();

            foreach (var patron in Patrons)
            {
                patron.PatronPower(CurrentPlayer, EnemyPlayer);
            }

            DrawCards();
            
            CardActionManager.Reset(_playerContext);
        }

        public void SetUpGame()
        {
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
        
        public SerializedBoard SerializeBoard(EndGameState? endGameState)
        {
            return new SerializedBoard(endGameState, CurrentPlayer, EnemyPlayer, Tavern, Patrons, CardActionManager.State, CardActionManager.PendingChoice,
                CardActionManager.ComboContext, CardActionManager.PendingEffects, CardActionManager.StartOfNextTurnEffects, CardActionManager.CompletedActions);
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

            var favorWin = Patrons.Where(patron => patron.PatronID != PatronId.TREASURY)
                .All(patron => patron.FavoredPlayer == EnemyPlayer.ID);

            if (favorWin)
            {
                return new EndGameState(EnemyPlayer.ID, GameEndReason.PATRON_FAVOR);
            }

            // If CurrentPlayer is over prestige threshold, that means EnemyPlayer (the one that played last round)
            // needed to match it.
            if (CurrentPlayer.PrestigeAmount >= PrestigeTreshold)
            {
                // That means EnemyPlayer didn't match the threshold, so CurrentPlayer wins.
                if (EnemyPlayer.PrestigeAmount < CurrentPlayer.PrestigeAmount)
                {
                    return new EndGameState(CurrentPlayer.ID, GameEndReason.PRESTIGE_OVER_40_NOT_MATCHED);
                }
            }

            return null;
        }

        public void ActivateAgent(Card card)
        {
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.ACTIVATE_AGENT, card));
            CurrentPlayer.ActivateAgent(card);
            CardActionManager.PlayCard(card);
        }

        public void AttackAgent(Card agent)
        {
            var attackAmount = CurrentPlayer.AttackAgent(agent, EnemyPlayer, Tavern);
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.ATTACK_AGENT, null, attackAmount, agent));
            if (!CurrentPlayer.AgentCards.Contains(agent))
            {
                CardActionManager.AddToCompletedActionsList(new CompletedAction(CompletedActionType.AGENT_DEATH, agent));
            }
        }

        private BoardManager(Patron[] patrons, Tavern tavern, PlayerContext playerContext, CardActionManager cardActionManager)
        {
            Patrons = patrons;
            Tavern = tavern;
            _playerContext = playerContext;
            CardActionManager = cardActionManager;
        }

        public static BoardManager FromSerializedBoard(SerializedBoard serializedBoard)
        {
            var patrons = Patron.FromSerializedBoard(serializedBoard);
            var tavern = TalesOfTribute.Tavern.FromSerializedBoard(serializedBoard);
            var playerContext = PlayerContext.FromSerializedBoard(serializedBoard);
            var cardActionManager = Board.CardAction.CardActionManager.FromSerializedBoard(serializedBoard, playerContext, tavern);
            return new BoardManager(patrons.ToArray(), tavern, playerContext, cardActionManager);
        }
    }
}
