namespace TalesOfTribute.Serializers;

public class SerializedAgent
{
    public readonly int CurrentHp;
    public readonly Card RepresentingCard;
    public readonly bool Activated;

    public SerializedAgent(Agent agent)
    {
        CurrentHp = agent.CurrentHp;
        RepresentingCard = agent.RepresentingCard;
        Activated = agent.Activated;
    }
}
