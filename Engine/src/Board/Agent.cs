using TalesOfTribute.Serializers;

namespace TalesOfTribute;

public class Agent : IChoiceSource
{
    public int CurrentHp { get; private set; }
    public Card RepresentingCard { get; }
    public bool Activated { get; private set; } = false;

    public Agent(Card representingCard)
    {
        RepresentingCard = representingCard;
        CurrentHp = RepresentingCard.HP;
    }

    public void Heal(int amount)
    {
        CurrentHp = Math.Min(RepresentingCard.HP, CurrentHp + amount);
    }

    public void MarkActivated()
    {
        Activated = true;
    }

    public void Damage(int amount)
    {
        CurrentHp -= amount;
        if (CurrentHp < 0)
        {
            CurrentHp = 0;
        }
    }

    public static Agent FromCard(Card card)
    {
        return new Agent(card);
    }

    public void Refresh()
    {
        Activated = false;
    }

    public static Agent FromSerializedAgent(SerializedAgent agent)
    {
        return new Agent(agent.RepresentingCard)
        {
            CurrentHp = agent.CurrentHp,
            Activated = agent.Activated,
        };
    }
}
