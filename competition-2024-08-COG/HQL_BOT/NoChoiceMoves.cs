using ScriptsOfTribute;

namespace Bots;

public static class NoChoiceMoves
{
    static readonly HashSet<CardId> no_choice_moves = new HashSet<CardId> {
        // Hlaalu
        // "Currency Exchange"
        CardId.LUXURY_EXPORTS,
        // "Oathman"
        CardId.EBONY_MINE,
        // "Hlaalu Councilor"
        // "Hlaalu Kinsman"
        // "House Embassy"
        // "House Marketplace"
        CardId.KWAMA_EGG_MINE,
        // "Hireling"
        // "Hostile Takeover"
        // "Customs Seizure"
        CardId.GOODS_SHIPMENT,

        // Red Eagle
        CardId.MIDNIGHT_RAID,
        // "Blood Sacrifice"
        // "Bloody Offering"
        // "Bonfire"
        // "Briarheart Ritual"
        // "Clan-Witch"
        // "Elder Witch"
        // "Hagraven"
        // "Hagraven Matron"
        // "Imperial Plunder"
        // "Imperial Spoils"
        // "Karth Man-Hunter"
        CardId.WAR_SONG,

        // Crows
        CardId.BLACKFEATHER_KNAVE,
        CardId.PLUNDER,
        CardId.TOLL_OF_FLESH,
        CardId.TOLL_OF_SILVER,
        CardId.MURDER_OF_CROWS,
        CardId.PILFER,
        CardId.SQUAWKING_ORATORY,
        CardId.LAW_OF_SOVEREIGN_ROOST,
        CardId.POOL_OF_SHADOW,
        CardId.SCRATCH,
        CardId.BLACKFEATHER_BRIGAND,
        CardId.BLACKFEATHER_KNIGHT,
        CardId.PECK,

        // Ansei
        // "Conquest"
        // "Grand Oratory"
        // "Hira's End"
        // "Hel Shira Herald"
        // "March on Hattu"
        // "Shehai Summoning"
        // "Warrior Wave"
        // "Ansei Assault"
        // "Ansei's Victory"
        // "Battle Meditation"
        // "No Shira Poet"
        // "Way of the Sword"

        // Psijic - not implemented

        // Pelin
        CardId.RALLY,
        CardId.SIEGE_WEAPON_VOLLEY,
        CardId.THE_ARMORY,
        CardId.BANNERET,
        // "Knight Commander" heal?
        CardId.KNIGHT_COMMANDER,
        CardId.REINFORCEMENTS,
        CardId.ARCHERS_VOLLEY,
        CardId.LEGIONS_ARRIVAL,
        CardId.SHIELD_BEARER,
        CardId.BANGKORAI_SENTRIES,
        CardId.KNIGHTS_OF_SAINT_PELIN,
        CardId.THE_PORTCULLIS,
        CardId.FORTIFY,

        // Rajhin
        CardId.BAG_OF_TRICKS,
        CardId.BEWILDERMENT,
        // "Grand Larceny"
        // "Jarring Lullaby"
        CardId.JEERING_SHADOW,
        // "Moonlit Illusion"
        // "Pounce and Profit"
        CardId.PROWLING_SHADOW,
        CardId.RINGS_GUILE,
        // "Shadow's Slumber"
        // "Slight of Hand"
        CardId.STUBBORN_SHADOW,
        CardId.SWIPE,
        // "Twilight Revelry"

        // Orgnum
        CardId.GHOSTSCALE_SEA_SERPENT,
        CardId.KING_ORGNUMS_COMMAND,
        CardId.MAORMER_BOARDING_PARTY,
        CardId.MAORMER_CUTTER,
        CardId.PYANDONEAN_WAR_FLEET,
        CardId.SEA_ELF_RAID,
        CardId.SEA_RAIDERS_GLORY,
        CardId.SEA_SERPENT_COLOSSUS,
        // "Serpentguard Rider"
        CardId.SERPENTPROW_SCHOONER,
        CardId.SNAKESKIN_FREEBOOTER,
        // "Storm Shark Wavecaller"
        CardId.SUMMERSET_SACKING,

        // Treasury
        // "Ambush"
        // "Barterer"
        // "Black Sacrament"
        // "Blackmail"
        CardId.GOLD,
        // "Harvest Season"
        // "Imprisonment"
        // "Ragpicker"
        // "Tithe"
        CardId.WRIT_OF_COIN,

        // "Unknown"
    };

    public static bool ShouldPlay(CardId id)
    {
        return no_choice_moves.Contains(id);
    }
}