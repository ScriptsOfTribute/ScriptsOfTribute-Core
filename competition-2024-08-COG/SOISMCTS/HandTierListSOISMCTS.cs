using ScriptsOfTribute;

namespace Bots;

public static class HandTierListSOISMCTS
{
    const int S = 5;
    const int A = 4;
    const int B = 3;
    const int C = 2;
    const int D = 1;
    const int E = 0;
    const int CURSE = -1;

    static readonly Dictionary<CardId, int> handTierDict = new Dictionary<CardId, int> {
        { CardId.CURRENCY_EXCHANGE, B },
        { CardId.LUXURY_EXPORTS,    D },
        { CardId.OATHMAN,           D },
        { CardId.HLAALU_COUNCILOR,  S },
        { CardId.HLAALU_KINSMAN,    S },
        { CardId.HOUSE_EMBASSY,     S },
        { CardId.HOUSE_MARKETPLACE, A },
        { CardId.HIRELING,          D },
        { CardId.HOSTILE_TAKEOVER,  D },
        { CardId.CUSTOMS_SEIZURE,   D },
        { CardId.GOODS_SHIPMENT,    CURSE },

        { CardId.MIDNIGHT_RAID,   B },
        { CardId.HAGRAVEN,        B },
        { CardId.HAGRAVEN_MATRON, A },
        { CardId.WAR_SONG,        CURSE },

        { CardId.BLACKFEATHER_KNAVE,   B },
        { CardId.PLUNDER,              S },
        { CardId.TOLL_OF_FLESH,        C },
        { CardId.TOLL_OF_SILVER,       C },
        { CardId.MURDER_OF_CROWS,      A },
        { CardId.PILFER,               B },
        { CardId.SQUAWKING_ORATORY,    A },
        { CardId.POOL_OF_SHADOW,       C },
        { CardId.SCRATCH,              D },
        { CardId.BLACKFEATHER_BRIGAND, D },
        { CardId.BLACKFEATHER_KNIGHT,  B },
        { CardId.PECK,                 CURSE },

        { CardId.CONQUEST,         C },
        { CardId.HIRAS_END,        S },
        { CardId.HEL_SHIRA_HERALD, S },
        { CardId.MARCH_ON_HATTU,   S },
        { CardId.SHEHAI_SUMMONING, S },
        { CardId.WARRIOR_WAVE,     B },
        { CardId.ANSEI_ASSAULT,    A },
        { CardId.ANSEIS_VICTORY,   S },
        { CardId.NO_SHIRA_POET,    C },
        { CardId.WAY_OF_THE_SWORD, CURSE },

        { CardId.RALLY,                  A },
        { CardId.SIEGE_WEAPON_VOLLEY,    C },
        { CardId.THE_ARMORY,             B },
        { CardId.BANNERET,               S },
        { CardId.KNIGHT_COMMANDER,       S },
        { CardId.REINFORCEMENTS,         D },
        { CardId.ARCHERS_VOLLEY,         D },
        { CardId.LEGIONS_ARRIVAL,        D },
        { CardId.BANGKORAI_SENTRIES,     A },
        { CardId.KNIGHTS_OF_SAINT_PELIN, A },
        { CardId.THE_PORTCULLIS,         D },
        { CardId.FORTIFY,                CURSE },

        { CardId.BEWILDERMENT,      CURSE },
        { CardId.GRAND_LARCENY,     C },
        { CardId.JARRING_LULLABY,   C },
        { CardId.JEERING_SHADOW,    E },
        { CardId.POUNCE_AND_PROFIT, C },
        { CardId.PROWLING_SHADOW,   B },
        { CardId.SHADOWS_SLUMBER,   B },
        { CardId.SLIGHT_OF_HAND,    CURSE },
        { CardId.STUBBORN_SHADOW,   D },
        { CardId.TWILIGHT_REVELRY,  C },
        { CardId.SWIPE,             CURSE },
        
        { CardId.GOLD,         CURSE },
        { CardId.WRIT_OF_COIN, CURSE },
        { CardId.SEA_ELF_RAID,  E}, // (updated by AO) Following cards are for Sorcerer King Ognum introduced since last year, tier choices are somewhat random...
        { CardId.PYANDONEAN_WAR_FLEET, A},
        { CardId.SEA_SERPENT_COLOSSUS, B},
        { CardId.GHOSTSCALE_SEA_SERPENT, S}, 
        { CardId.KING_ORGNUMS_COMMAND, B},
        { CardId.MAORMER_BOARDING_PARTY, S},
        { CardId.MAORMER_CUTTER, B},
        { CardId.SEA_RAIDERS_GLORY, B},
        { CardId.SERPENTPROW_SCHOONER, S},
        { CardId.SUMMERSET_SACKING, A},
        { CardId.STORM_SHARK_WAVECALLER,  B}, //TODO:: check, game engine seems to classify the following sorcerer orgnum agents as actions? Or am I misunderstanding what a handTier list is?
        { CardId.SNAKESKIN_FREEBOOTER,  A},
        { CardId.SERPENTGUARD_RIDER, A},
    };

    //updated by AO to include a try/catch block
    public static int GetCardTier(CardId cardId)
    {
        try
        {
            return handTierDict[cardId];
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"HandTier key exception: {ex.Message}");
            return B;
        }
    }
}