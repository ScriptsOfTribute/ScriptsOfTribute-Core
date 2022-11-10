namespace TalesOfTribute
{
    public class BoardManager
    {
        public PlayerEnum CurrentPlayer;
        public Patron[] patrons;
        public Tavern tavern;
        public Player[] players;
        private Random _rnd;

        public BoardManager(PatronId[] patrons)
        {
            this.patrons = GetPatrons(patrons);
            tavern = new Tavern(GlobalCardDatabase.Instance.GetCardsByPatron(patrons));
            players = new Player[] { new Player(PlayerEnum.PLAYER1), new Player(PlayerEnum.PLAYER2) };
            _rnd = new Random();
        }

        private Patron[] GetPatrons(IEnumerable<PatronId> patrons)
        {
            return patrons.Select(Patron.FromId).ToArray();
        }

        public void PatronCall(int patronID, Player activator, Player enemy)
        {
            patrons[patronID].PatronActivation(activator, enemy);
        }

        public void PlayCard(Card card)
        {
            throw new NotImplementedException();

        }

        public void BuyCard(Card card)
        {
            throw new NotImplementedException();
        }

        public void EndTurn()
        {
            players[(int)CurrentPlayer].PrestigeAmount += players[(int)CurrentPlayer].PowerAmount;
            players[(int)CurrentPlayer].CoinsAmount = 0;

            CurrentPlayer = (PlayerEnum)(1 - (int)CurrentPlayer);
        }

        public void SetUpGame()
        {
            players[(int)PlayerEnum.PLAYER2].CoinsAmount = 1; // Second player starts with one gold
            CurrentPlayer = PlayerEnum.PLAYER1;
            tavern.DrawCards();

            List<Card> starterDecks = new List<Card>()
            {
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            };

            foreach (var patron in this.patrons)
            {
                starterDecks.Add(GlobalCardDatabase.Instance.GetCard(patron.GetStarterCard()));
            }

            players[(int)PlayerEnum.PLAYER1].DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
            players[(int)PlayerEnum.PLAYER2].DrawPile = starterDecks.OrderBy(x => this._rnd.Next(0, starterDecks.Count)).ToList();
        }

    }
}
