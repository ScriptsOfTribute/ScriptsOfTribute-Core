using ScriptsOfTribute;

namespace Bots;

public static class AgentTier
{
    const int S = 5;
    const int A = 4;
    const int B = 3;
    const int C = 2;
    const int D = 1;
    const int E = 0;
    const int CURSE = -1;

    static readonly Dictionary<CardId, int> agentTierDict = new Dictionary<CardId, int> {
        { CardId.OATHMAN,                C },
        { CardId.HLAALU_COUNCILOR,       S },
        { CardId.HLAALU_KINSMAN,         S },
        { CardId.HIRELING,               C },
        { CardId.CLANWITCH,              A },
        { CardId.ELDER_WITCH,            A },
        { CardId.HAGRAVEN,               A },
        { CardId.HAGRAVEN_MATRON,        A },
        { CardId.KARTH_MANHUNTER,        B },
        { CardId.BLACKFEATHER_KNAVE,     B },
        { CardId.BLACKFEATHER_BRIGAND,   B },
        { CardId.BLACKFEATHER_KNIGHT,    B },
        { CardId.HEL_SHIRA_HERALD,       B },
        { CardId.NO_SHIRA_POET,          C },
        { CardId.BANNERET,               S },
        { CardId.KNIGHT_COMMANDER,       S },
        { CardId.SHIELD_BEARER,          B },
        { CardId.BANGKORAI_SENTRIES,     B },
        { CardId.KNIGHTS_OF_SAINT_PELIN, A },
        { CardId.JEERING_SHADOW,         C },
        { CardId.PROWLING_SHADOW,        B },
        { CardId.STUBBORN_SHADOW,        C },

        { CardId.SERPENTGUARD_RIDER,     D },
        { CardId.SNAKESKIN_FREEBOOTER,   D },
        { CardId.STORM_SHARK_WAVECALLER, D },
    };

    public static int GetCardTier(CardId cardId) => agentTierDict[cardId];
}