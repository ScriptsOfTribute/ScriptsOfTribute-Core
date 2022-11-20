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

    public class Card
    {
        public readonly string Name;
        public readonly PatronId Deck;
        public readonly CardId Id;
        public readonly int Cost;
        public readonly CardType Type;
        public readonly int HP; // >=0 for Agent, -1 for other types
        public readonly ComplexEffect?[] Effects; // 0th - On activation, 1st - combo 2, 2nd - combo 3, 3rd - combo 4
        public readonly int Hash;
        public readonly CardId? Family;
        public Guid Guid { get; } = Guid.Empty;

        public Card(string name, PatronId deck, CardId id, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family)
        {
            Name = name;
            Deck = deck;
            Id = id;
            Cost = cost;
            Type = type;
            HP = hp;
            Effects = effects;
            Hash = hash;
            Family = family;
        }

        public Card(string name, PatronId deck, CardId id, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, Guid guid)
            : this(name, deck, id, cost, type, hp, effects, hash, family)
        {
            Guid = guid;
        }

        public Card CreateUniqueCopy()
        {
            var guid = Guid.NewGuid();
            return new Card(Name, Deck, Id, Cost, Type, HP,
                Effects.Select(effect => effect?.MakeUniqueCopy(guid)).ToArray(),
                Hash, Family, guid);
        }

        public override string ToString()
        {
            return String.Format($"Card: {this.Name}, " +
                $"Deck: {this.Deck}, Cost: {this.Cost}, Type: {this.Type}");
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Card card = (Card)obj;
                return this.Guid == card.Guid;
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
            return Guid.GetHashCode();
        }
    }
}
