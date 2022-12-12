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
            TavernCards = tavern.AvailableCards.ToList();
            PatronStates = new PatronStates(patrons.ToList());
            BoardState = state;
            PendingChoice = maybeChoice switch
            {
                Choice<Card> cardChoice => SerializedChoice<Card>.FromChoice(cardChoice),
                Choice<EffectType> effectChoice => SerializedChoice<EffectType>.FromChoice(effectChoice),
                _ => null
            };
        }
    }
}
