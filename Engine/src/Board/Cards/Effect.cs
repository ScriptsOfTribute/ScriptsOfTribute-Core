namespace ScriptsOfTribute.Board.Cards;

public interface ComplexEffect
{
    public UniqueComplexEffect MakeUniqueCopy(UniqueCard parentCard);
}

public class Effect : ComplexEffect
{
    public readonly EffectType Type;
    public readonly int Amount;
    public readonly int Combo;

    public Effect(EffectType type, int amount, int combo)
    {
        Type = type;
        Amount = amount;
        Combo = combo;
    }

    public override string ToString()
    {
        return Type switch
        {
            EffectType.GAIN_COIN => $"Coin +{Amount}",
            EffectType.GAIN_PRESTIGE => $"Prestige +{Amount}",
            EffectType.GAIN_POWER => $"Power +{Amount}",
            EffectType.OPP_LOSE_PRESTIGE => $"Enemy prestige -{Amount}",
            EffectType.REPLACE_TAVERN => $"Replace {Amount} cards in tavern",
            EffectType.ACQUIRE_TAVERN => $"Get card from tavern with cost up to {Amount}",
            EffectType.DESTROY_CARD => $"Destroy {Amount} cards in play",
            EffectType.DRAW => $"Draw {Amount} cards",
            EffectType.OPP_DISCARD => $"Enemy discards {Amount} cards from hand at start of their turn",
            EffectType.RETURN_TOP => $"Put {Amount} cards from cooldown pile on top of draw pile",
            EffectType.RETURN_AGENT_TOP => $"Put {Amount} agents from cooldown pile on top of draw pile",
            EffectType.TOSS => $"Choose up to {Amount} of draw pile cards to move to your cooldown pile.",
            EffectType.KNOCKOUT => $"Knockout {Amount} enemy agents",
            EffectType.KNOCKOUT_ALL => $"Knockout all agents",
            EffectType.PATRON_CALL => $"Get {Amount} patron calls",
            EffectType.CREATE_SUMMERSET_SACKING => $"Create {Amount} Summerset Sacking cards and place it in CD pile",
            EffectType.HEAL => $"Heal this agent by {Amount}",
            EffectType.DONATE => $"Discard up to {Amount} cards from hand, draw {Amount} cards",
            _ => ""
        };
    }

    public UniqueComplexEffect MakeUniqueCopy(UniqueCard parentCard)
    {
        return new UniqueEffect(Type, Amount, Combo, parentCard);
    }

    public static EffectType MapEffectType(string effect)
    {
        return effect switch
        {
            "Coin" => EffectType.GAIN_COIN,
            "Power" => EffectType.GAIN_POWER,
            "Prestige" => EffectType.GAIN_PRESTIGE,
            "OppLosePrestige" => EffectType.OPP_LOSE_PRESTIGE,
            "Remove" => EffectType.REPLACE_TAVERN,
            "Acquire" => EffectType.ACQUIRE_TAVERN,
            "Destroy" => EffectType.DESTROY_CARD,
            "Draw" => EffectType.DRAW,
            "Discard" => EffectType.OPP_DISCARD,
            "Return" => EffectType.RETURN_TOP,
            "ReturnAgent" => EffectType.RETURN_AGENT_TOP,
            "Toss" => EffectType.TOSS,
            "KnockOut" => EffectType.KNOCKOUT,
            "KnockOutAll" => EffectType.KNOCKOUT_ALL,
            "Patron" => EffectType.PATRON_CALL,
            "CreateSummersetSacking" => EffectType.CREATE_SUMMERSET_SACKING,
            "Heal" => EffectType.HEAL,
            "Donate" => EffectType.DONATE,
            _ => throw new EngineException($"Invalid effect type: {effect}")
        };
    }

    public string ToSimpleString()
    {
        return $"{Type} {Amount}";
    }
}

public class EffectOr : ComplexEffect
{
    private readonly Effect _left;
    private readonly Effect _right;
    public readonly int Combo;

    public EffectOr(Effect left, Effect right, int combo)
    {
        _left = left;
        _right = right;
        Combo = combo;
    }

    public UniqueComplexEffect MakeUniqueCopy(UniqueCard parentCard)
    {
        return new UniqueEffectOr(
            _left.MakeUniqueCopy(parentCard) as UniqueEffect ?? throw new InvalidOperationException(),
            _right.MakeUniqueCopy(parentCard) as UniqueEffect ?? throw new InvalidOperationException(),
            Combo,
            parentCard
        );
    }

    public override string ToString()
    {
        return $"{this._left} OR {this._right}";
    }

    public string ToSimpleString()
    {
        return $"{this._left.ToSimpleString()} OR {this._right.ToSimpleString()}";
    }
}

public class EffectComposite : ComplexEffect
{
    private readonly Effect _left;
    private readonly Effect _right;

    public EffectComposite(Effect left, Effect right)
    {
        _left = left;
        _right = right;
    }

    public UniqueComplexEffect MakeUniqueCopy(UniqueCard parentCard)
    {
        return new UniqueEffectComposite(
            _left.MakeUniqueCopy(parentCard) as UniqueEffect ?? throw new InvalidOperationException(),
            _right.MakeUniqueCopy(parentCard) as UniqueEffect ?? throw new InvalidOperationException(),
            parentCard
        );
    }

    public override string ToString()
    {
        return $"{this._left} AND {this._right}";
    }

    public string ToSimpleString()
    {
        return $"{this._left.ToSimpleString()} AND {this._right.ToSimpleString()}";
    }
}