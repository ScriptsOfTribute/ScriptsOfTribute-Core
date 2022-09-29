using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //OR ?????
        RETURN_TOP,
        REFRESH_AGENT,
        TOSS,
        KNOCKOUT,
        PATRON_CALL,
        CREATE_BOARDINGPARTY
    }
    public class Effect
    {
        public EffectType Type;
        public int Amount;

        public Effect(EffectType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public override string ToString()
        {
            return String.Format("Effect: {0} {1}", this.Type, this.Amount);
        }
    }
}
