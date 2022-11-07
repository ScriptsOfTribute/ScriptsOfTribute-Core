namespace TalesOfTribute
{
    public class BoardManager
    {
        int currentPlayer;
        Patron[] patrons;
        Tavern tavern;
        Player[] players;

        public BoardManager(string[] Patrons)
        {
            patrons = GetPatrons(Patrons);
            Parser parser = new Parser(cards_config.CARDS_JSON);
            tavern = new Tavern(parser.GetCardsByDeck(Patrons));
            players = new Player[] { new Player(0), new Player(1) };
            players[1].CoinsAmount = 1; // Second player starts with one gold
            currentPlayer = 0;
        }

        private Patron[] GetPatrons(string[] patrons)
        {
            return patrons.Select(
                patron => Patron.FromEnum(
                        Patron.FromString(patron)
                    )
                ).ToArray();
        }

        public void PatronCall(int patronID, Player activator, Player enemy)
        {
            patrons[patronID].PatronActivation(activator, enemy);
        }

        public void PlayCard(int cardID)
        {
            throw new NotImplementedException();
        }

        public void BuyCard(int cardID)
        {
            throw new NotImplementedException();
        }

        public void EndTurn()
        {
            throw new NotImplementedException();
        }

        public void SetUpGame()
        {
            throw new NotImplementedException();
        }

        public void Move(string line)
        {
            /*
             * Parser for commands from external bots
             * Allowed moves:
             * - PLAY <card_id> - Use card from your hand with certain id
             * - CALL_PATRON <patron_id> - Use patron with certain id, <args> for certain patron
             * - BUY <card_id> - Buy card from tavern
             * - END - end turn
             * - CALL_AGENT <agent_id> - use your agent
             */
        }
    }
}
