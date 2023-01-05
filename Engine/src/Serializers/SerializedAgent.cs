using TalesOfTribute.Board.Cards;

namespace TalesOfTribute.Serializers;

public class SerializedAgent
{
    public readonly int CurrentHp;
    public readonly UniqueCard RepresentingCard;
    public readonly bool Activated;

    public SerializedAgent(Agent agent)
    {
        CurrentHp = agent.CurrentHp;
        RepresentingCard = agent.RepresentingCard;
        Activated = agent.Activated;
    }

    public override string ToString()
    {
        return $"{RepresentingCard.ToString()} CurrentHP: {CurrentHp} Activated: {Activated}";
    }
}
