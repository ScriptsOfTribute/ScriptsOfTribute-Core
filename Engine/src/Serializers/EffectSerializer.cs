using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

public static class EffectSerializer
{
    public static string ParseEffectToString(object effect)
    {
        if (effect == null)
        {
            return "";
        }

        if (effect is UniqueEffect baseEffect)
        {
            return baseEffect.ToSimpleString();
        }
        else if (effect is UniqueEffectOr complexEffectOr)
        {
            return complexEffectOr.ToSimpleString();
        }
        else if (effect is UniqueEffectComposite complexEffectComposite)
        {
            return complexEffectComposite.ToSimpleString();
        }
        else
        {
            throw new ArgumentException("Unsupported effect type");
        }
    }
}
