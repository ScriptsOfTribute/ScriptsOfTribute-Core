using System.Numerics;

namespace TalesOfTribute
{
    public class BoardSerializer
    {
        /*
         * Class that is supposed to be representation of board
         * for the user/bot. Contains all info that player needs to calculate 
         * and make next move
         */

        public Vector3 FirstPlayer;
        public Vector3 SecondPlayer;
        public List<Card> FirstPlayerHand;
        public List<Card> SecondPlayerHand;
        public List<Card> FirstPlayerPlayed;
        public List<Card> SecondPlayerPlayed;
        public List<Card> TavernCards;
        public List<Tuple<PatronId, PlayerEnum>> PatronStates;
        public PlayerEnum CurrentPlayer;

        public BoardSerializer(
            Player firstPlayer, Player secondPlayer, Tavern tavern, Patron[] patrons, PlayerEnum currentPlayer
        )
        {
            this.FirstPlayer = new Vector3(
                firstPlayer.CoinsAmount, firstPlayer.PrestigeAmount, firstPlayer.PowerAmount
            );
            this.SecondPlayer = new Vector3(
                secondPlayer.CoinsAmount, secondPlayer.PrestigeAmount, secondPlayer.PowerAmount
            );
            FirstPlayerHand = firstPlayer.Hand;
            SecondPlayerHand = secondPlayer.Hand;
            FirstPlayerPlayed = firstPlayer.Played;
            SecondPlayerPlayed = secondPlayer.Played;
            TavernCards = tavern.AvailableCards;
            PatronStates = new List<Tuple<PatronId, PlayerEnum>>();
            foreach (Patron patron in patrons)
            {
                PatronStates.Add(new Tuple<PatronId, PlayerEnum>(patron.PatronID, patron.FavoredPlayer));
            }
            CurrentPlayer = currentPlayer;
        }

    }
}
