using System;
using System.Collections.Generic;
using System.Text;

namespace TalesOfTribute.src.AI
{
    public class Game
    {
        private AI Player1;
        private AI Player2;
        private BoardManager board;
        public Game(AI player1, AI player2)
        {
            Player1 = player1;
            Player2 = player2;
            Player1.PlayerID = PlayerEnum.PLAYER1;
            Player2.PlayerID = PlayerEnum.PLAYER2;

            board = new BoardManager(PatronSelection());
        }

        private PatronId[] PatronSelection()
        {
            List<PatronId> patrons = Enum.GetValues(typeof(PatronId)).Cast<PatronId>().ToList();
            List<PatronId> patronsSelected = new List<PatronId>();
            PatronId patron = Player1.SelectPatron(patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patron = Player2.SelectPatron(patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patronsSelected.Add(PatronId.TREASURY);

            patron = Player2.SelectPatron(patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patron = Player1.SelectPatron(patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            return patronsSelected.ToArray();
        }

        public void Play()
        {

        }
    }
}
