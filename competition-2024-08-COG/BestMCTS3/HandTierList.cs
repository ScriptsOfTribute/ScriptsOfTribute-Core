using ScriptsOfTribute;

namespace Bots;

public static class HandTierList
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

        { CardId.GHOSTSCALE_SEA_SERPENT, E},
        { CardId.KING_ORGNUMS_COMMAND,   E},
        { CardId.MAORMER_BOARDING_PARTY, E},
        { CardId.MAORMER_CUTTER,         E},
        { CardId.PYANDONEAN_WAR_FLEET,   E},
        { CardId.SEA_ELF_RAID,           E},
        { CardId.SEA_RAIDERS_GLORY,      E},
        { CardId.SEA_SERPENT_COLOSSUS,   E},
        { CardId.SERPENTGUARD_RIDER,     E},
        { CardId.SERPENTPROW_SCHOONER,   E},
        { CardId.SNAKESKIN_FREEBOOTER,   E},
        { CardId.STORM_SHARK_WAVECALLER, E},
        { CardId.SUMMERSET_SACKING,      E},

        { CardId.GOLD,         CURSE },
        { CardId.WRIT_OF_COIN, CURSE },
    };

    public static int GetCardTier(CardId cardId) => handTierDict[cardId];
}