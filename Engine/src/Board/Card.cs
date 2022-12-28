namespace TalesOfTribute
{
    public enum CardType
    {
        ACTION,
        AGENT,
        CONTRACT_ACTION,
        CONTRACT_AGENT,
        STARTER,
        CURSE
    }

    public class Card : IChoosable
    {
        public readonly string Name;
        public readonly PatronId Deck;
        public readonly CardId CommonId;
        public readonly int Cost;
        public readonly CardType Type;
        public readonly int HP; // >=0 for Agent, -1 for other types
        public readonly ComplexEffect?[] Effects; // 0th - On activation, 1st - combo 2, 2nd - combo 3, 3rd - combo 4
        public readonly int Hash;
        public readonly CardId? Family;
        public readonly bool Taunt;
        public UniqueId UniqueId { get; } = UniqueId.Empty;

        public Card(string name, PatronId deck, CardId commonId, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, bool taunt)
        {
            Name = name;
            Deck = deck;
            CommonId = commonId;
            Cost = cost;
            Type = type;
            HP = hp;
            Taunt = taunt;
            Effects = effects;
            Hash = hash;
            Family = family;
            Taunt = taunt;
        }

        public Card(string name, PatronId deck, CardId commonId, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, bool taunt, UniqueId uniqueId)
            : this(name, deck, commonId, cost, type, hp, effects, hash, family, taunt)
        {
            UniqueId = uniqueId;
        }

        public Card CreateUniqueCopy()
        {
            var uniqueId = UniqueId.Create();
            return new Card(Name, Deck, CommonId, Cost, Type, HP,
                Effects.Select(effect => effect?.MakeUniqueCopy(uniqueId)).ToArray(),
                Hash, Family, Taunt, uniqueId);
        }

        public override string ToString()
        {
            return string.Format($"Card: {this.Name}, " +
                $"Deck: {this.Deck}, Cost: {this.Cost}, Type: {this.Type}, UniqueId: {UniqueId.Value}");
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                Card card = (Card)obj;
                return this.UniqueId == card.UniqueId;
            }
        }

        public static bool operator ==(Card? card1, Card? card2)
        {
            if (card1 is null)
            {
                return card2 is null;
            }

            return card1.Equals(card2);
        }

        public static bool operator !=(Card? card1, Card? card2)
        {
            return !(card1 == card2);
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }
    }
}
