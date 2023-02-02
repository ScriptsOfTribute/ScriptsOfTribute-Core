namespace ScriptsOfTribute.Board.Cards;

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
    public readonly CardId CommonId;
    public readonly int Cost;
    public readonly CardType Type;
    public readonly int HP; // >=0 for Agent, -1 for other types
    public readonly ComplexEffect?[] Effects; // 0th - On activation, 1st - combo 2, 2nd - combo 3, 3rd - combo 4
    public readonly int Hash;
    public readonly CardId? Family;
    public readonly bool Taunt;
    public readonly int Copies;

    public Card(string name, PatronId deck, CardId commonId, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, bool taunt, int copies)
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
        Copies = copies;
    }

    public UniqueCard CreateUniqueCopy()
    {
        var uniqueId = UniqueId.Create();
        return new UniqueCard(Name, Deck, CommonId, Cost, Type, HP,
            Effects,
            Hash, Family, Taunt, uniqueId, Copies);
    }
}
