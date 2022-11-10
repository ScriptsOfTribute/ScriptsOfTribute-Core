namespace TalesOfTribute
{
    public enum EffectType
    {
        GAIN_COIN,
        GAIN_POWER,
        GAIN_PRESTIGE,
        OPP_LOSE_PRESTIGE,
        REMOVE_TAVERN,
        ACQUIRE_TAVERN,
        DESTROY_CARD,
        DRAW,
        OPP_DISCARD,
        RETURN_TOP,
        TOSS,
        KNOCKOUT,
        PATRON_CALL,
        CREATE_BOARDINGPARTY,
        DUMMY // null effect
    }
    public class Effect
    {
        public EffectType Type;
        public int Amount;

        // if card has 2 possible effects
        public string? op; // OR or AND
        public EffectType? secondType;
        public int? secondAmount;

        public Effect()
        {
            this.Type = EffectType.DUMMY;
            this.Amount = -1;
        }

        public Effect(EffectType type, int amount, string? _op = null, EffectType? secondtype = null, int? secondamount = null)
        {
            Type = type;
            Amount = amount;
            op = _op;
            secondType = secondtype;
            secondAmount = secondamount;
        }

        public Result Enact(Player player, Player enemy, Tavern tavern)
        {
            if (op == "OR")
            {
                return new Choice<EffectType>(new List<EffectType> { Type, (EffectType)secondType },
                    choice =>
                    {
                        // This won't be implemented here, I propose separate classes, but this is just an experiment.
                        if (choice == EffectType.GAIN_POWER)
                        {
                            player.PowerAmount += Amount;
                        }
                        else
                        {
                            return new Failure("Not implemented yet!");
                        }

                        return new Success();
                    });
            }

            return new Failure("Not implemented yet!");
        }

        public override string ToString()
        {
            if (this.op != null)
            {
                return String.Format($"Effect: {this.Type} {this.Amount} {this.op} {this.secondType} {this.secondAmount}");
            }
            return String.Format($"Effect: {this.Type} {this.Amount}");
        }

        public static EffectType MapEffectType(string effect)
        {
            switch (effect)
            {
                case "Coin":
                    return EffectType.GAIN_COIN;
                case "Power":
                    return EffectType.GAIN_POWER;
                case "Prestige":
                    return EffectType.GAIN_PRESTIGE;
                case "OppPrestige":
                    return EffectType.OPP_LOSE_PRESTIGE;
                case "Remove":
                    return EffectType.REMOVE_TAVERN;
                case "Acquire":
                    return EffectType.ACQUIRE_TAVERN;
                case "Destroy":
                    return EffectType.DESTROY_CARD;
                case "Draw":
                    return EffectType.DRAW;
                case "Discard":
                    return EffectType.OPP_DISCARD;
                case "Return":
                    return EffectType.RETURN_TOP;
                case "Toss":
                    return EffectType.TOSS;
                case "KnockOut":
                    return EffectType.KNOCKOUT;
                case "Patron":
                    return EffectType.PATRON_CALL;
                case "Create":
                    return EffectType.CREATE_BOARDINGPARTY;
                default:
                    return EffectType.DUMMY;
            }
        }
    }
}
