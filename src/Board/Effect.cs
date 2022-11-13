using System.Reflection.Metadata.Ecma335;

namespace TalesOfTribute
{
    public enum EffectType
    {
        GAIN_COIN,
        GAIN_POWER,
        GAIN_PRESTIGE,
        OPP_LOSE_PRESTIGE,
        REPLACE_TAVERN,
        ACQUIRE_TAVERN,
        DESTROY_CARD,
        DRAW,
        OPP_DISCARD,
        RETURN_TOP,
        TOSS,
        KNOCKOUT,
        PATRON_CALL,
        CREATE_BOARDINGPARTY,
        HEAL,
    }

    public interface BaseEffect
    {
        public PlayResult Enact(Player player, Player enemy, Tavern tavern);
    }

    public interface ComplexEffect
    {
        public List<BaseEffect> Decompose();
    }

    public class Effect : BaseEffect, ComplexEffect
    {
        public readonly EffectType Type;
        public readonly int Amount;

        public Effect(EffectType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public PlayResult Enact(Player player, Player enemy, Tavern tavern)
        {
            switch (Type)
            {
                case EffectType.GAIN_POWER:
                    player.PowerAmount += Amount;
                    break;
                case EffectType.ACQUIRE_TAVERN:
                    // TODO: Implement this, for now I make a test-only implementation for choice returning choice.
                    return new Choice<CardId>(new List<CardId> { CardId.GOLD, CardId.PECK },
                        _ => new Success());
                default:
                    // TODO: This is for testing only, throw an exception here when all effects are implemented.
                    // throw new Exception("Not implemented yet!");
                    return new Success();
            }

            return new Success();
        }

        public override string ToString()
        {
            return $"Effect: {this.Type} {this.Amount}";
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public static EffectType MapEffectType(string effect)
        {
            return effect switch
            {
                "Coin" => EffectType.GAIN_COIN,
                "Power" => EffectType.GAIN_POWER,
                "Prestige" => EffectType.GAIN_PRESTIGE,
                "OppPrestige" => EffectType.OPP_LOSE_PRESTIGE,
                "Remove" => EffectType.REPLACE_TAVERN,
                "Acquire" => EffectType.ACQUIRE_TAVERN,
                "Destroy" => EffectType.DESTROY_CARD,
                "Draw" => EffectType.DRAW,
                "Discard" => EffectType.OPP_DISCARD,
                "Return" => EffectType.RETURN_TOP,
                "Toss" => EffectType.TOSS,
                "KnockOut" => EffectType.KNOCKOUT,
                "Patron" => EffectType.PATRON_CALL,
                "Create" => EffectType.CREATE_BOARDINGPARTY,
                "Heal" => EffectType.HEAL,
                _ => throw new Exception("Invalid effect type.")
            };
        }
    }

    public class EffectChoice : ComplexEffect, BaseEffect
    {
        private readonly Effect _left;
        private readonly Effect _right;

        public EffectChoice(Effect left, Effect right)
        {
            _left = left;
            _right = right;
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public PlayResult Enact(Player player, Player enemy, Tavern tavern)
        {
            return new Choice<EffectType>(new List<EffectType> { _left.Type, _right.Type },
                choice =>
                {
                    if (choice.First() == _left.Type)
                    {
                        return _left.Enact(player, enemy, tavern);
                    }

                    return _right.Enact(player, enemy, tavern);
                });
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

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { _left, _right };
        }
    }
}
