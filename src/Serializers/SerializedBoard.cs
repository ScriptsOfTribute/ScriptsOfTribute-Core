using System.Numerics;
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

        public SerializedBoard(
            IPlayer currentPlayer, IPlayer enemyPlayer, ITavern tavern, IEnumerable<Patron> patrons
        )
        {
            CurrentPlayer = new SerializedPlayer(currentPlayer);
            EnemyPlayer = new SerializedPlayer(enemyPlayer);
            TavernCards = tavern.AvailableCards.ToList();
            PatronStates = new PatronStates(patrons.ToList());
        }
    }
}
