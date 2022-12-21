using System.Text;
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
        public readonly List<Card> TavernAvailableCards;
        public readonly List<Card> TavernCards;
        public readonly BoardState BoardState;
        public readonly BaseSerializedChoice? PendingChoice;

        public SerializedBoard(
            IPlayer currentPlayer, IPlayer enemyPlayer, ITavern tavern, IEnumerable<Patron> patrons,
            BoardState state, BaseChoice? maybeChoice
        )
        {
            CurrentPlayer = new SerializedPlayer(currentPlayer);
            EnemyPlayer = new SerializedPlayer(enemyPlayer);
            TavernAvailableCards = tavern.AvailableCards.ToList();
            TavernCards = tavern.Cards;
            PatronStates = new PatronStates(patrons.ToList());
            BoardState = state;
            PendingChoice = maybeChoice switch
            {
                Choice<Card> cardChoice => SerializedCardChoice.FromChoice(cardChoice),
                Choice<EffectType> effectChoice => SerializedEffectChoice.FromChoice(effectChoice),
                _ => null
            };
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
            sb.AppendLine($"Enemy player Hand:\n{string.Join('\n', EnemyPlayer.Hand.Select(c => $"\t{c.ToString()}"))}");

            return sb.ToString();
        }
    }
}
