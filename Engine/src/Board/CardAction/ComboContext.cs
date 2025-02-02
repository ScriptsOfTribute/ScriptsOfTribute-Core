using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute;

public class Combo
{
    public static int MAX_COMBO = 4;
    public List<UniqueBaseEffect>[] EffectQueue { get; } = new List<UniqueBaseEffect>[MAX_COMBO];
    public int ComboCounter { get; private set; } = 0;

    public Combo()
    {
        for (var i = 0; i < MAX_COMBO; i++)
        {
            EffectQueue[i] = new List<UniqueBaseEffect>();
        }
    }

    private void IncrementCombo()
    {
        ComboCounter += 1;
        ComboCounter = ComboCounter > MAX_COMBO ? MAX_COMBO : ComboCounter;
    }

    public void AddEffects(UniqueComplexEffect?[] effects)
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

    public IEnumerable<UniqueBaseEffect> GetCurrentComboEffects()
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

    private Combo(List<UniqueBaseEffect>[] effects, int comboCounter)
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

    public (List<UniqueBaseEffect> immediateEffects, List<UniqueBaseEffect> startOfNextTurnEffects) PlayCard(UniqueCard card)
    {
        var combo = GetCombo(card);

        combo.AddEffects(card.Effects);

        var allEffects = combo.GetCurrentComboEffects().ToList();
        var immediateEffects = allEffects.Where(e => (e as UniqueEffect)?.Type != EffectType.OPP_DISCARD).ToList();
        var startOfNextTurnEffects = allEffects.Where(e => (e as UniqueEffect)?.Type == EffectType.OPP_DISCARD).ToList();

        return (immediateEffects, startOfNextTurnEffects);
    }

    public void Reset()
    {
        _combos.Clear();
    }

    private Combo GetCombo(UniqueCard card)
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
