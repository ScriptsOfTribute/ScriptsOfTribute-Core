using System.Text;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute
{
    public class FullGameState
    {
        /*
         * Class that is supposed to be representation of board
         * for the user/bot. Contains all info that player needs to calculate 
         * and make next move
         */
        public readonly string StateId;
        public readonly SerializedPlayer CurrentPlayer;
        public readonly SerializedPlayer EnemyPlayer;
        public readonly PatronStates PatronStates;
        public List<PatronId> Patrons => PatronStates.All.Select(p => p.Key).ToList();
        public readonly List<UniqueCard> TavernAvailableCards;
        public readonly List<UniqueCard> TavernCards;
        public readonly BoardState BoardState;
        public readonly SerializedChoice? PendingChoice;
        public readonly ComboStates ComboStates;
        public readonly List<UniqueBaseEffect> UpcomingEffects;
        public readonly List<UniqueBaseEffect> StartOfNextTurnEffects;
        public readonly List<CompletedAction> CompletedActions;
        public readonly EndGameState? GameEndState;
        public readonly ulong InitialSeed;
        public readonly ulong CurrentSeed;
        public readonly bool Cheats = false;

        public FullGameState(SerializedPlayer currentPlayer, SerializedPlayer enemyPlayer, PatronStates patronStates, List<UniqueCard> tavernAvailableCards, List<UniqueCard> tavernCards, BoardState boardState, SerializedChoice? pendingChoice, ComboStates comboStates, List<UniqueBaseEffect> upcomingEffects, List<UniqueBaseEffect> startOfNextTurnEffects, List<CompletedAction> completedActions, EndGameState? gameEndState, ulong initialSeed, ulong currentSeed, bool cheats)
        {
            StateId = Guid.NewGuid().ToString();
            CurrentPlayer = currentPlayer;
            EnemyPlayer = enemyPlayer;
            PatronStates = patronStates;
            TavernAvailableCards = tavernAvailableCards;
            TavernCards = tavernCards;
            BoardState = boardState;
            PendingChoice = pendingChoice;
            ComboStates = comboStates;
            UpcomingEffects = upcomingEffects;
            StartOfNextTurnEffects = startOfNextTurnEffects;
            CompletedActions = completedActions;
            GameEndState = gameEndState;
            InitialSeed = initialSeed;
            CurrentSeed = currentSeed;
            Cheats = cheats;
        }

        public FullGameState(SerializedPlayer currentPlayer, SerializedPlayer enemyPlayer, PatronStates patronStates, List<UniqueCard> tavernAvailableCards, List<UniqueCard> tavernCards, ulong currentSeed, bool cheats = false)
        {
            StateId = Guid.NewGuid().ToString();
            CurrentPlayer = currentPlayer;
            EnemyPlayer = enemyPlayer;
            PatronStates = patronStates;
            TavernAvailableCards = tavernAvailableCards.ToList();
            TavernCards = tavernCards.ToList();
            CurrentSeed = currentSeed;
            ComboStates = new ComboStates(new Dictionary<PatronId, ComboState>());
            UpcomingEffects = new List<UniqueBaseEffect>();
            StartOfNextTurnEffects = new List<UniqueBaseEffect>();
            CompletedActions = new List<CompletedAction>();
            InitialSeed = currentSeed;
            CurrentSeed = currentSeed;
            Cheats = cheats;
        }

        public FullGameState(
            SeededRandom rng, EndGameState? endGameState, IPlayer currentPlayer, IPlayer enemyPlayer, ITavern tavern, IEnumerable<Patron> patrons,
            BoardState state, Choice? maybeChoice, ComboContext comboContext, IEnumerable<UniqueBaseEffect> upcomingEffects, List<UniqueBaseEffect> startOfNextTurnEffects, List<CompletedAction> completedActions, bool cheats)
        {
            StateId = Guid.NewGuid().ToString();
            CurrentPlayer = new SerializedPlayer(currentPlayer);
            EnemyPlayer = new SerializedPlayer(enemyPlayer);
            TavernAvailableCards = tavern.AvailableCards;
            TavernCards = tavern.Cards;
            PatronStates = new PatronStates(patrons.ToList());
            BoardState = state;
            PendingChoice = maybeChoice?.Serialize();
            ComboStates = comboContext.ToComboStates();
            UpcomingEffects = upcomingEffects.ToList();
            StartOfNextTurnEffects = startOfNextTurnEffects;
            CompletedActions = completedActions;
            GameEndState = endGameState;
            CurrentSeed = rng.CurrentSeed;
            InitialSeed = rng.InitialSeed;
            Cheats = cheats;
        }

        public SerializedPlayer GetPlayer(PlayerEnum id)
        {
            return CurrentPlayer.PlayerID == id ? CurrentPlayer : EnemyPlayer;
        }

        // TODO: Add EndGameState and exception handling, because now incorrect moves crash (also, what happens if player tries to make move on already ended game? Handle this edge case).
        public (FullGameState, List<Move>) ApplyMove(Move move)
        {
            var api = ScriptsOfTributeApi.FromSerializedBoard(this);
            var s = move as SimpleCardMove;
            switch (move.Command)
            {
                case CommandEnum.PLAY_CARD:
                    api.PlayCard(s!.Card);
                    break;
                case CommandEnum.ACTIVATE_AGENT:
                    api.ActivateAgent(s!.Card);
                    break;
                case CommandEnum.ATTACK:
                    api.AttackAgent(s!.Card);
                    break;
                case CommandEnum.BUY_CARD:
                    api.BuyCard(s!.Card);
                    break;
                case CommandEnum.CALL_PATRON:
                    api.PatronActivation(((SimplePatronMove)move).PatronId);
                    break;
                case CommandEnum.MAKE_CHOICE:
                    switch (move)
                    {
                        case MakeChoiceMove<UniqueCard> cardMove:
                            api.MakeChoice(cardMove.Choices);
                            break;
                        case MakeChoiceMove<UniqueEffect> effectMove:
                            api.MakeChoice(effectMove.Choices.First());
                            break;
                        default:
                            throw new EngineException("Invalid choice type.");
                    }
                    break;
                case CommandEnum.END_TURN:
                    api.EndTurn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (api.GetFullGameState(), api.GetListOfPossibleMoves());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                $"Current player: (Prestige, Power, Coins) ({CurrentPlayer.Prestige}, {CurrentPlayer.Power}, {CurrentPlayer.Coins})");
            sb.AppendLine($"Enemy player: (Prestige, Power, Coins) ({EnemyPlayer.Prestige}, {EnemyPlayer.Power}, {EnemyPlayer.Coins})");
            sb.AppendLine($"$Tavern Available Cards:\n{string.Join('\n', TavernAvailableCards.Select(c => $"\t{c.ToString()}"))}");
            sb.AppendLine($"$Tavern Cards:\n{string.Join('\n', TavernCards.Select(c => $"\t{c.ToString()}"))}");
            sb.AppendLine($"Current player Hand:\n{string.Join('\n', CurrentPlayer.Hand.Select(c => $"\t{c.ToString()}"))}");
            sb.AppendLine($"Current player Agents:\n{string.Join('\n', CurrentPlayer.Agents.Select(c => $"\t{c.ToString()}"))}");
            sb.AppendLine($"Enemy player Hand:\n{string.Join('\n', EnemyPlayer.Hand.Select(c => $"\t{c.ToString()}"))}");
            sb.AppendLine($"Enemy player Agents:\n{string.Join('\n', EnemyPlayer.Agents.Select(c => $"\t{c.ToString()}"))}");

            return sb.ToString();
        }
    }
}
