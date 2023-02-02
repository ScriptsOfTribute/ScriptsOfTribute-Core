namespace ScriptsOfTribute.Board.Cards;

public class UniqueCard : Card
{
    public new readonly UniqueComplexEffect?[] Effects;
    public UniqueId UniqueId { get; }

    public UniqueCard(string name, PatronId deck, CardId commonId, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, bool taunt, UniqueId uniqueId, int copies)
        : base(name, deck, commonId, cost, type, hp, effects, hash, family, taunt, copies)
    {
        UniqueId = uniqueId;
        Effects = effects.Select(e => e?.MakeUniqueCopy(this)).ToArray();
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
            UniqueCard card = (UniqueCard)obj;
            return this.UniqueId == card.UniqueId;
        }
    }

    public static bool operator ==(UniqueCard? card1, UniqueCard? card2)
    {
        if (card1 is null)
        {
            return card2 is null;
        }

        return card1.Equals(card2);
    }

    public static bool operator !=(UniqueCard? card1, UniqueCard? card2)
    {
        return !(card1 == card2);
    }

    public override int GetHashCode()
    {
        return UniqueId.GetHashCode();
    }
}
