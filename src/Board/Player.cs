namespace TalesOfTribute
{
    public enum PlayerEnum
    {
        PLAYER1 = 0,
        PLAYER2 = 1,
        NO_PLAYER_SELECTED = 2,
    }
    public class Player
    {
        public PlayerEnum ID;
        public int CoinsAmount;
        public int PrestigeAmount;
        public int PowerAmount;
        public List<Card> Hand;
        public List<Card> DrawPile;
        public List<Card> Played;
        public List<Card> Agents;
        public List<Card> CooldownPile;
        public uint ForcedDiscard;
        public uint PatronCalls;
        public long ShuffleSeed;

        public Player(PlayerEnum iD)
        {
            CoinsAmount = 0;
            PrestigeAmount = 0;
            PowerAmount = 0;
            Hand = new List<Card>();
            DrawPile = new List<Card>();
            Played = new List<Card>();
            Agents = new List<Card>();
            CooldownPile = new List<Card>();
            ID = iD;
        }

        public Player(
            PlayerEnum iD, int coinsAmount, int prestigeAmount, int powerAmount,
            List<Card> hand, List<Card> drawPile, List<Card> played, List<Card> agents, List<Card> cooldownPile
        )
        {
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            DrawPile = drawPile;
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
            ID = iD;
        }

        public Player(
            PlayerEnum iD, int coinsAmount, int prestigeAmount,
            int powerAmount, List<Card> hand, List<Card> drawPile, List<Card> played,
            List<Card> agents, List<Card> cooldownPile, uint forcedDiscard,
            uint patronCalls, long shuffleSeed
        )
        {
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            DrawPile = drawPile;
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
            ForcedDiscard = forcedDiscard;
            PatronCalls = patronCalls;
            ShuffleSeed = shuffleSeed;
            ID = iD;
        }

        public override string ToString()
        {
            return String.Format("Player: ({0}, {1}, {2})", this.CoinsAmount, this.PrestigeAmount, this.PowerAmount);
        }

        public List<Card> GetAllPlayersCards()
        {
            List<Card> cards = this.Hand.Concat(this.DrawPile)
                .Concat(this.Played)
                .Concat(this.Agents)
                .Concat(this.CooldownPile).ToList();
            return cards;
        }
    }
}
