using System.Text;
using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Serializers;

namespace TalesOfTribute
{
    public class SerializedBoard
    {
        /*
         * Class that is supposed to be representation of board
         * for the user/bot. Contains all info that player needs to calculate 
         * and make next move
         */

        public readonly SerializedPlayer CurrentPlayer;
        public readonly SerializedPlayer EnemyPlayer;
        public readonly PatronStates PatronStates;
        public List<PatronId> Patrons => PatronStates.All.Select(p => p.Key).ToList();
        public readonly List<Card> TavernAvailableCards;
        public readonly List<Card> TavernCards;
        public readonly BoardState BoardState;
        public readonly SerializedChoice? PendingChoice;
        public readonly ComboStates ComboStates;
        public readonly List<BaseEffect> UpcomingEffects;
        public readonly List<BaseEffect> StartOfNextTurnEffects;
        public readonly List<CompletedAction> CompletedActions;

        public SerializedBoard(
            IPlayer currentPlayer, IPlayer enemyPlayer, ITavern tavern, IEnumerable<Patron> patrons,
            BoardState state, Choice? maybeChoice, ComboContext comboContext, IEnumerable<BaseEffect> upcomingEffects, IEnumerable<BaseEffect> startOfNextTurnEffects, List<CompletedAction> completedActions
        )
        {
            CurrentPlayer = new SerializedPlayer(currentPlayer);
            EnemyPlayer = new SerializedPlayer(enemyPlayer);
            TavernAvailableCards = tavern.AvailableCards.ToList();
            TavernCards = tavern.Cards;
            PatronStates = new PatronStates(patrons.ToList());
            BoardState = state;
            PendingChoice = maybeChoice?.Serialize();
            ComboStates = comboContext.ToComboStates();
            UpcomingEffects = upcomingEffects.ToList();
            StartOfNextTurnEffects = startOfNextTurnEffects.ToList();
            CompletedActions = completedActions;
        }

        // TODO: Add EndGameState and exception handling, because now incorrect moves crash (also, what happens if player tries to make move on already ended game? Handle this edge case).
        public (SerializedBoard, List<Move>) ApplyState(Move move)
        {
            var api = TalesOfTributeApi.FromSerializedBoard(this);
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
                        case MakeChoiceMove<Card> cardMove:
                            api.MakeChoice(cardMove.Choices);
                            break;
                        case MakeChoiceMove<Effect> effectMove:
                            api.MakeChoice(effectMove.Choices.First());
                            break;
                        default:
                            throw new Exception("Invalid choice type.");
                    }
                    break;
                case CommandEnum.END_TURN:
                    api.EndTurn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (api.GetSerializer(), api.GetListOfPossibleMoves());
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
