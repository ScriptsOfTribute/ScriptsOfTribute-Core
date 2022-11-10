namespace TalesOfTribute
{
    public class Combo
    {
        public List<Effect>[] EffectsToEnact { get; } = new List<Effect>[5];
        public int Counter { get; set; } = 0;

        public Combo()
        {
            for (var i = 0; i < 5; i++)
            {
                EffectsToEnact[i] = new List<Effect>();
            }
        }
    }
    
    public class Player
    {
        public int ID;
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

        public Dictionary<PatronId, Combo> Combos { get; private set; } = new();

        public Player(int iD)
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
            int iD, int coinsAmount, int prestigeAmount, int powerAmount,
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
            int iD, int coinsAmount, int prestigeAmount,
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

        public ExecutionChain PlayCard(CardId cardId, Player other, Tavern tavern)
        {
            // var card = Hand.First(card => card.Id == cardId);
            var card = GlobalCardDatabase.Instance.GetCard(cardId);
            var patron = card.Deck;

            if (!Combos.TryGetValue(patron, out var combo))
            {
                combo = new Combo();
                Combos.Add(patron, combo);
            }
            
            for (var i = 0; i < card.Effects.Length; i++)
            {
                var effect = card.Effects[i];
                if (effect.Type != EffectType.DUMMY)
                {
                    combo.EffectsToEnact[i].Add(effect);
                }
            }

            combo.Counter += 1;
            if (combo.Counter > 5)
            {
                combo.Counter = 5;
            }

            List<Func<PlayResult>> chain = new();

            // TODO: This order is probably not correct, should be discussed.
            for (var i = 0; i < combo.Counter; i++)
            {
                combo.EffectsToEnact[i].ForEach(effect =>
                {
                    chain.Add(() => effect.Enact(this, other, tavern));
                });
            }

            return new ExecutionChain(chain);
        }

        public void EndTurn()
        {
            Combos = new();
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
