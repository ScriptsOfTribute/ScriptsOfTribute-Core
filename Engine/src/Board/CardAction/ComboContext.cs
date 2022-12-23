using TalesOfTribute.Serializers;

namespace TalesOfTribute;

public class Combo
{
    public static int MAX_COMBO = 4;
    public List<BaseEffect>[] EffectQueue { get; } = new List<BaseEffect>[MAX_COMBO];
    public int ComboCounter { get; private set; } = 0;

    public Combo()
    {
        for (var i = 0; i < MAX_COMBO; i++)
        {
            EffectQueue[i] = new List<BaseEffect>();
        }
    }

    private void IncrementCombo()
    {
        ComboCounter += 1;
        ComboCounter = ComboCounter > MAX_COMBO ? MAX_COMBO : ComboCounter;
    }

    public void AddEffects(ComplexEffect?[] effects)
    {
        for (var i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (effect != null)
            {
                EffectQueue[i].AddRange(effect.Decompose());
            }
        }

        IncrementCombo();
    }

    public IEnumerable<BaseEffect> GetCurrentComboEffects()
    {
        for (var i = 0; i < ComboCounter; i++)
        {
            foreach (var effect in EffectQueue[i])
            {
                yield return effect;
            }
            EffectQueue[i].Clear();
        }
    }

    public ComboState ToComboState()
    {
        return new ComboState(EffectQueue.ToArray(), ComboCounter);
    }

    private Combo(List<BaseEffect>[] effects, int comboCounter)
    {
        for (var i = 0; i < MAX_COMBO; i++)
        {
            EffectQueue[i] = effects[i].ToList();
        }

        ComboCounter = comboCounter;
    }
    
    public static Combo FromComboState(ComboState state)
    {
        return new Combo(state.All, state.CurrentCombo);
    }
}

public class ComboContext
{
    private readonly Dictionary<PatronId, Combo> _combos = new();

    public ComboContext()
    {
    }

    public (List<BaseEffect> immediateEffects, List<BaseEffect> startOfNextTurnEffects) PlayCard(Card card)
    {
        var combo = GetCombo(card);

        combo.AddEffects(card.Effects);

        var allEffects = combo.GetCurrentComboEffects().ToList();
        var immediateEffects = allEffects.Where(e => (e as Effect)?.Type != EffectType.OPP_DISCARD).ToList();
        var startOfNextTurnEffects = allEffects.Where(e => (e as Effect)?.Type == EffectType.OPP_DISCARD).ToList();

        return (immediateEffects, startOfNextTurnEffects);
    }

    public void Reset()
    {
        _combos.Clear();
    }

    private Combo GetCombo(Card card)
    {
        var patron = card.Deck;

        if (_combos.TryGetValue(patron, out var combo)) return combo;

        combo = new Combo();
        _combos.Add(patron, combo);

        return combo;
    }

    public ComboStates ToComboStates()
    {
        var res = new Dictionary<PatronId, ComboState>();
        foreach (var (patronId, combo) in _combos)
        {
            res.Add(patronId, combo.ToComboState());
        }

        return new ComboStates(res);
    }

    private ComboContext(Dictionary<PatronId, Combo> combos)
    {
        _combos = combos;
    }

    public static ComboContext FromComboStates(ComboStates states)
    {
        Dictionary<PatronId, Combo> combos = new();
        foreach (var (patron, state) in states.All)
        {
            combos.Add(patron, Combo.FromComboState(state));
        }

        return new ComboContext(combos);
    }
}
