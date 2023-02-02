using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute;

public class Agent
{
    public int CurrentHp { get; private set; }
    public UniqueCard RepresentingCard { get; }
    public bool Activated { get; private set; } = false;

    public Agent(UniqueCard representingCard)
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

    public static Agent FromCard(UniqueCard card)
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
