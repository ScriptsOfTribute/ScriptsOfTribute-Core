using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute
{
    public class BoardManager
    {
        public Patron[] Patrons;
        public Tavern Tavern;
        private PlayerContext _playerContext;

        public Player CurrentPlayer => _playerContext.CurrentPlayer;
        public Player EnemyPlayer => _playerContext.EnemyPlayer;
        public readonly CardActionManager CardActionManager;
        private readonly SeededRandom _rng;
        private readonly bool _cheats = false;

        private int PrestigeTreshold = 40;

        public BoardManager(PatronId[] patrons, ulong seed)
        {
            _rng = new SeededRandom(seed);
            Patrons = GetPatrons(patrons);
            var patronStarterCardsIds = Patrons.SelectMany(patron => patron.GetStarterCards()).ToArray();
            Tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons, patronStarterCardsIds, CardId.WRIT_OF_COIN), _rng);
            _playerContext = new PlayerContext(new Player(PlayerEnum.PLAYER1, _rng), new Player(PlayerEnum.PLAYER2, _rng));
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
                throw new EngineException("You cant use Patron calls anymore");
            }

            var patron = Array.Find(Patrons, p => p.PatronID == patronId);
            CurrentPlayer.PatronCalls--;
            CardActionManager.ActivatePatron(CurrentPlayer.ID, patron);
        }

        public void PlayCard(UniqueCard card)
        {
            CurrentPlayer.PlayCard(card);
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CurrentPlayer.ID, CompletedActionType.PLAY_CARD, card));
            CardActionManager.PlayCard(card);
        }

        public void BuyCard(UniqueCard card)
        {
            if (card.Cost > CurrentPlayer.CoinsAmount)
                throw new EngineException($"You dont have enough coin to buy {card}");
            
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CurrentPlayer.ID, CompletedActionType.BUY_CARD, card));

            var idx = Tavern.RemoveCard(card);
            switch (card.Type)
            {
                case CardType.CONTRACT_AGENT:
                {
                    var agent = Agent.FromCard(card);
                    agent.MarkActivated();
                    CurrentPlayer.Agents.Add(agent);
                    CardActionManager.PlayCard(card);
                    break;
                }
                case CardType.CONTRACT_ACTION:
                {
                    Tavern.Cards.Add(card);
                    CardActionManager.PlayCard(card);
                    break;
                }
                default:
                    CurrentPlayer.CooldownPile.Add(card);
                    break;
            }
            Tavern.DrawAt(idx);

            CurrentPlayer.CoinsAmount -= card.Cost;
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

            CardActionManager.Reset(_playerContext);
        }

        public void SetUpGame()
        {
            EnemyPlayer.CoinsAmount = 1; // Second player starts with one gold
            Tavern.SetUp(_rng);

            List<UniqueCard> starterDecksPlayer1 = new List<UniqueCard>();
            List<UniqueCard> starterDecksPlayer2 = new List<UniqueCard>();

            foreach (var patron in Patrons)
            {
                var starterIds = patron.GetStarterCards();
                starterDecksPlayer1.AddRange(
                    starterIds.Select(cardId => GlobalCardDatabase.Instance.GetCard(cardId))
                );
                starterDecksPlayer2.AddRange(
                    starterIds.Select(cardId => GlobalCardDatabase.Instance.GetCard(cardId))
                );
            }

            CurrentPlayer.InitDrawPile(starterDecksPlayer1);
            EnemyPlayer.InitDrawPile(starterDecksPlayer2);

            CurrentPlayer.Draw(5);
            EnemyPlayer.Draw(5);
        }
        
        public FullGameState SerializeBoard(EndGameState? endGameState)
        {
            return new FullGameState(_rng, endGameState, CurrentPlayer, EnemyPlayer, Tavern, Patrons, CardActionManager.State, CardActionManager.PendingChoice,
                CardActionManager.ComboContext, CardActionManager.PendingEffects, CardActionManager.StartOfNextTurnEffects, CardActionManager.CompletedActions, _cheats);
        }

        public PlayerEnum GetPatronFavorism(PatronId patron)
        {
            return Array.Find(Patrons, p => p.PatronID == patron).FavoredPlayer;
        }

        public List<UniqueCard> GetAvailableTavernCards()
        {
            return this.Tavern.AvailableCards;
        }

        public List<UniqueCard> GetAffordableCards(int coinAmount)
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

        public void ActivateAgent(UniqueCard card)
        {
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CurrentPlayer.ID, CompletedActionType.ACTIVATE_AGENT, card));
            CurrentPlayer.ActivateAgent(card);
            CardActionManager.PlayCard(card);
        }

        public void AttackAgent(UniqueCard agent)
        {
            var attackAmount = CurrentPlayer.AttackAgent(agent, EnemyPlayer, Tavern);
            CardActionManager.AddToCompletedActionsList(new CompletedAction(CurrentPlayer.ID, CompletedActionType.ATTACK_AGENT, null, attackAmount, agent));
            if (!EnemyPlayer.AgentCards.Contains(agent))
            {
                CardActionManager.AddToCompletedActionsList(new CompletedAction(CurrentPlayer.ID, CompletedActionType.AGENT_DEATH, agent));
            }
        }

        private BoardManager(Patron[] patrons, Tavern tavern, PlayerContext playerContext, CardActionManager cardActionManager, SeededRandom rng, bool cheats)
        {
            Patrons = patrons;
            Tavern = tavern;
            _playerContext = playerContext;
            CardActionManager = cardActionManager;
            _rng = rng;
            _cheats = cheats;
        }

        public static BoardManager FromSerializedBoard(FullGameState fullGameState)
        {
            var patrons = Patron.FromSerializedBoard(fullGameState);
            var tavern = Tavern.FromSerializedBoard(fullGameState);
            var rng = new SeededRandom(fullGameState.InitialSeed, fullGameState.CurrentSeed);
            var playerContext = PlayerContext.FromSerializedBoard(fullGameState, rng);
            var cardActionManager = CardActionManager.FromSerializedBoard(fullGameState, playerContext, tavern);
            return new BoardManager(patrons.ToArray(), tavern, playerContext, cardActionManager, rng, fullGameState.Cheats);
        }
    }
}
