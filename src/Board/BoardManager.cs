namespace TalesOfTribute
{
    public enum BoardState
    {
        NORMAL,
        CHOICE_PENDING,
    }
    
    public class BoardManager
    {
        int _currentPlayer;
        Patron[] _patrons;
        Tavern _tavern;
        Player[] _players;
        public BoardState State { get; set; }

        public BoardManager(PatronId[] patrons)
        {
            this._patrons = GetPatrons(patrons);
            _tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            _players = new Player[] { new Player(0), new Player(1) };
            _players[1].CoinsAmount = 1; // Second player starts with one gold
            _currentPlayer = 0;
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public void PatronCall(int patronID, Player activator, Player enemy)
        {
            _patrons[patronID].PatronActivation(activator, enemy);
        }

        public ExecutionChain PlayCard(CardId cardID)
        {
            if (State == BoardState.CHOICE_PENDING)
            {
                throw new Exception("Complete pending choice first!");
            }
            
            var result = _players[_currentPlayer].PlayCard(cardID, _players[1-_currentPlayer], _tavern);

            State = BoardState.CHOICE_PENDING;

            result.AddCompleteCallback(() => State = BoardState.NORMAL);

            return result;
        }

        public void BuyCard(int cardID)
        {
            throw new NotImplementedException();
        }

        public void EndTurn()
        {
            _players[_currentPlayer].EndTurn();
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
