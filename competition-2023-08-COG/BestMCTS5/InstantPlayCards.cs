using ScriptsOfTribute;

namespace Bots;

public static class InstantPlayCards
{
    static readonly HashSet<CardId> instantPlayCards = new HashSet<CardId> {
        // HLAALU
        // "Currency Exchange",
        // CardId.LUXURY_EXPORTS,
        // "Oathman",
        // CardId.EBONY_MINE,
        // "Hlaalu Councilor",
        // "Hlaalu Kinsman",
        // "House Embassy",
        // "House Marketplace",
        // "Hireling",
        // "Hostile Takeover",
        // CardId.KWAMA_EGG_MINE,
        // "Customs Seizure",
        // CardId.GOODS_SHIPMENT,

        // RED EAGLE
        CardId.MIDNIGHT_RAID,
        // "Blood Sacrifice",
        // "Bloody Offering",
        // "Bonfire",
        // "Briarheart Ritual",
        // "Clan-Witch",
        // "Elder Witch",
        // "Hagraven",
        // "Hagraven Matron",
        // "Imperial Plunder",
        // "Imperial Spoils",
        // "Karth Man-Hunter",
        CardId.WAR_SONG,

        // DUKE OF CROWS
        // "Blackfeather Knave",
        // "Plunder",
        // "Toll of Flesh",
        // "Toll of Silver",
        // "Murder of Crows",
        // "Pilfer",
        // "Squawking Oratory",
        // "Law of Sovereign Roost",
        // "Pool of Shadow",
        // "Scratch",
        // "Blackfeather Brigand",
        // "Blackfeather Knight",
        // "Peck",

        // ANSEI
        // "Conquest",
        // "Grand Oratory",
        // "Hira's End",
        // "Hel Shira Herald",
        // "March on Hattu",
        // "Shehai Summoning",
        // "Warrior Wave",
        // "Ansei Assault",
        // "Ansei's Victory",
        // "Battle Meditation",
        // "No Shira Poet",
        // "Way of the Sword",

        // PELIN
        // "Rally",
        CardId.SIEGE_WEAPON_VOLLEY,
        CardId.THE_ARMORY,
        // CardId.BANNERET,
        // CardId.KNIGHT_COMMANDER,
        CardId.REINFORCEMENTS,
        CardId.ARCHERS_VOLLEY,
        CardId.LEGIONS_ARRIVAL,
        // CardId.SHIELD_BEARER,
        // CardId.BANGKORAI_SENTRIES,
        // CardId.KNIGHTS_OF_SAINT_PELIN,
        CardId.THE_PORTCULLIS,
        CardId.FORTIFY,

        // RAJHIN
        // "Bag of Tricks", <- contract action
        CardId.BEWILDERMENT,
        CardId.GRAND_LARCENY,
        CardId.JARRING_LULLABY,
        // CardId.JEERING_SHADOW,
        // "Moonlit Illusion", <- contract action
        CardId.POUNCE_AND_PROFIT,
        // CardId.PROWLING_SHADOW,
        // "Ring's Guile", <- contract action
        CardId.SHADOWS_SLUMBER,
        // "Slight of Hand",
        // CardId.STUBBORN_SHADOW,
        // "Twilight Revelry",
        CardId.SWIPE,

        // TREASURY
        // "Ambush", <- contract action
        // "Barterer", <- contract action
        // "Black Sacrament", <- contract action
        // "Blackmail", <- contract action
        CardId.GOLD,
        // "Harvest Season", <- contract action
        // "Imprisonment", <- contract action
        // "Ragpicker", <- contract action
        // "Tithe", <- contract action
        CardId.WRIT_OF_COIN,
        // "Unknown", <- ?
    };

    static readonly HashSet<CardId> instantPlayAgents = new HashSet<CardId>
    {
        // HLAALU
        // "Currency Exchange",
        // CardId.LUXURY_EXPORTS,
        // "Oathman",
        // CardId.EBONY_MINE,
        // "Hlaalu Councilor",
        // "Hlaalu Kinsman",
        // "House Embassy",
        // "House Marketplace",
        // "Hireling",
        // "Hostile Takeover",
        // CardId.KWAMA_EGG_MINE,
        // "Customs Seizure",
        // CardId.GOODS_SHIPMENT,

        // RED EAGLE
        // CardId.MIDNIGHT_RAID,
        // "Blood Sacrifice",
        // "Bloody Offering",
        // "Bonfire",
        // "Briarheart Ritual",
        // "Clan-Witch",
        // "Elder Witch",
        // "Hagraven",
        // "Hagraven Matron",
        // "Imperial Plunder",
        // "Imperial Spoils",
        // "Karth Man-Hunter",
        // CardId.WAR_SONG,

        // DUKE OF CROWS
        // "Blackfeather Knave",
        // "Plunder",
        // "Toll of Flesh",
        // "Toll of Silver",
        // "Murder of Crows",
        // "Pilfer",
        // "Squawking Oratory",
        // "Law of Sovereign Roost",
        // "Pool of Shadow",
        // "Scratch",
        // "Blackfeather Brigand",
        // "Blackfeather Knight",
        // "Peck",

        // ANSEI
        // "Conquest",
        // "Grand Oratory",
        // "Hira's End",
        // "Hel Shira Herald",
        // "March on Hattu",
        // "Shehai Summoning",
        // "Warrior Wave",
        // "Ansei Assault",
        // "Ansei's Victory",
        // "Battle Meditation",
        // "No Shira Poet",
        // "Way of the Sword",

        // PELIN
        // "Rally",
        // CardId.SIEGE_WEAPON_VOLLEY,
        // CardId.THE_ARMORY,
        // CardId.BANNERET,
        // CardId.KNIGHT_COMMANDER,
        // CardId.REINFORCEMENTS,
        // CardId.ARCHERS_VOLLEY,
        // CardId.LEGIONS_ARRIVAL,
        // CardId.SHIELD_BEARER,
        // CardId.BANGKORAI_SENTRIES,
        // CardId.KNIGHTS_OF_SAINT_PELIN,
        // CardId.THE_PORTCULLIS,
        // CardId.FORTIFY,

        // RAJHIN
        // "Bag of Tricks", <- contract action
        // CardId.BEWILDERMENT,
        // CardId.GRAND_LARCENY,
        // CardId.JARRING_LULLABY,
        // CardId.JEERING_SHADOW,
        // "Moonlit Illusion", <- contract action
        // CardId.POUNCE_AND_PROFIT,
        // CardId.PROWLING_SHADOW,
        // "Ring's Guile", <- contract action
        // CardId.SHADOWS_SLUMBER,
        // "Slight of Hand",
        // CardId.STUBBORN_SHADOW,
        // "Twilight Revelry",
        // CardId.SWIPE,

        // TREASURY
        // "Ambush", <- contract action
        // "Barterer", <- contract action
        // "Black Sacrament", <- contract action
        // "Blackmail", <- contract action
        // CardId.GOLD,
        // "Harvest Season", <- contract action
        // "Imprisonment", <- contract action
        // "Ragpicker", <- contract action
        // "Tithe", <- contract action
        // CardId.WRIT_OF_COIN,
        // "Unknown", <- ?
    };

    public static bool IsInstantPlay(CardId id) => instantPlayCards.Contains(id);
    public static bool IsInstantAgent(CardId id) => instantPlayAgents.Contains(id);
}