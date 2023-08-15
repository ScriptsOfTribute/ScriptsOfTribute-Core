using ScriptsOfTribute;

namespace Bots;

// public static class GPCardTierList
// {
//     const int S = 50;
//     const int A = 30;
//     const int B = 15;
//     const int WOC = 10;
//     const int C = 3;
//     const int D = 1;
//     const int UNKNOWN = 0;
//     const int CONTRACT_ACTION = 0;
//     const int CURSE = -3;

//     static readonly Dictionary<string, int[]> cardTierDict = new Dictionary<string, int[]> {
//         { "Currency Exchange", new int[] { S, S, A } },
//         { "Luxury Exports",    new int[] { S, S, C } },
//         { "Oathman",           new int[] { A, A, B } },
//         { "Ebony Mine",        new int[] { C, B, C } },
//         { "Hlaalu Councilor",  new int[] { A, A, C } },
//         { "Hlaalu Kinsman",    new int[] { A, A, C } },
//         { "House Embassy",     new int[] { A, A, C } },
//         { "House Marketplace", new int[] { B, A, C } },
//         { "Hireling",          new int[] { C, C, D } },
//         { "Hostile Takeover",  new int[] { B, C, D } },
//         { "Kwama Egg Mine",    new int[] { C, B, C } },
//         { "Customs Seizure",   new int[] { D, D, D } },
//         { "Goods Shipment",    new int[] { D, D, D } },

//         { "Midnight Raid",     new int[] { S, S, S } },
//         { "Blood Sacrifice",   new int[] { S, S, S } },
//         { "Bloody Offering",   new int[] { S, S, S } },
//         { "Bonfire",           new int[] { C, B, C } },
//         { "Briarheart Ritual", new int[] { C, B, C } },
//         { "Clan-Witch",        new int[] { C, B, D } },
//         { "Elder Witch",       new int[] { C, B, D } },
//         { "Hagraven",          new int[] { D, B, D } },
//         { "Hagraven Matron",   new int[] { D, A, C } },
//         { "Imperial Plunder",  new int[] { B, A, B } },
//         { "Imperial Spoils",   new int[] { B, A, B } },
//         { "Karth Man-Hunter",  new int[] { C, B, C } },
//         { "War Song",          new int[] { D, D, D } },

//         { "Blackfeather Knave",     new int[] { S, S, A } },
//         { "Plunder",                new int[] { S, S, S } },
//         { "Toll of Flesh",          new int[] { S, S, A } },
//         { "Toll of Silver",         new int[] { S, S, A } },
//         { "Murder of Crows",        new int[] { S, S, A } },
//         { "Pilfer",                 new int[] { A, S, A } },
//         { "Squawking Oratory",      new int[] { A, S, A } },
//         { "Law of Sovereign Roost", new int[] { B, A, B } },
//         { "Pool of Shadow",         new int[] { B, A, B } },
//         { "Scratch",                new int[] { A, A, B } },
//         { "Blackfeather Brigand",   new int[] { C, C, C } },
//         { "Blackfeather Knight",    new int[] { B, B, C } },
//         { "Peck",                   new int[] { C, C, C } },

//         { "Conquest",          new int[] { A, A, B } },
//         { "Grand Oratory",     new int[] { B, S, S } },
//         { "Hira's End",        new int[] { S, S, S } },
//         { "Hel Shira Herald",  new int[] { B, A, B } },
//         { "March on Hattu",    new int[] { A, A, A } },
//         { "Shehai Summoning",  new int[] { B, B, B } },
//         { "Warrior Wave",      new int[] { S, A, B } },
//         { "Ansei Assault",     new int[] { B, A, B } },
//         { "Ansei's Victory",   new int[] { B, A, B } },
//         { "Battle Meditation", new int[] { C, B, B } },
//         { "No Shira Poet",     new int[] { C, C, C } },
//         { "Way of the Sword",  new int[] { D, D, D } },

//         { "Rally",                  new int[] { A, S, A } },
//         { "Siege Weapon Volley",    new int[] { A, S, B } },
//         { "The Armory",             new int[] { A, S, A } },
//         { "Banneret",               new int[] { A, S, A } },
//         { "Knight Commander",       new int[] { S, S, A } },
//         { "Reinforcements",         new int[] { S, A, B } },
//         { "Archers' Volley",        new int[] { B, A, B } },
//         { "Legion's Arrival",       new int[] { A, A, B } },
//         { "Shield Bearer",          new int[] { C, B, B } },
//         { "Bangkorai Sentries",     new int[] { C, B, C } },
//         { "Knights of Saint Pelin", new int[] { C, A, C } },
//         { "The Portcullis",         new int[] { C, C, D } },
//         { "Fortify",                new int[] { D, D, D } },

//         { "Bag of Tricks",     new int[] { C, B, B } },
//         { "Bewilderment",      new int[] { CURSE, CURSE, CURSE } },
//         { "Grand Larceny",     new int[] { A, A, B } },
//         { "Jarring Lullaby",   new int[] { B, A, B } },
//         { "Jeering Shadow",    new int[] { C, C, C } },
//         { "Moonlit Illusion",  new int[] { B, A, B } },
//         { "Pounce and Profit", new int[] { S, S, B } },
//         { "Prowling Shadow",   new int[] { B, C, C } },
//         { "Ring's Guile",      new int[] { C, C, C } },
//         { "Shadow's Slumber",  new int[] { A, A, B } },
//         { "Slight of Hand",    new int[] { A, B, D } },
//         { "Stubborn Shadow",   new int[] { D, C, D } },
//         { "Twilight Revelry",  new int[] { B, A, B } },
//         { "Swipe",             new int[] { D, D, D } },

//         { "Ambush",          new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Barterer",        new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Black Sacrament", new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Blackmail",       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Gold",            new [] { UNKNOWN, CURSE, CURSE } },
//         { "Harvest Season",  new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Imprisonment",    new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Ragpicker",       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Tithe",           new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { "Writ of Coin",    new [] { WOC, B, UNKNOWN } },
//         { "Unknown",         new [] { UNKNOWN, UNKNOWN, UNKNOWN } },
//     };

//     public static int GetCardTier(string cardName, GamePhase gamePhase) => cardTierDict[cardName][(int)gamePhase];
// }

public static class GPCardTierList
{
    const int S = 50;
    const int A = 30;
    const int B = 15;
    const int WOC = 10;
    const int C = 3;
    const int D = 1;
    const int UNKNOWN = 0;
    const int CONTRACT_ACTION = 0;
    const int CURSE = -3;

    static readonly int[][] cardTierDict = new int[][] {

        new int[] { S, S, A }, // 0 Card: "Currency Exchange",
        new int[] { S, S, C }, // 1 Card: "Luxury Exports",
        new int[] { A, A, B }, // 2 Card: "Oathman",
        new int[] { C, B, C }, // 3 Card: "Ebony Mine",
        new int[] { A, A, C }, // 4 Card: "Hlaalu Councilor",
        new int[] { A, A, C }, // 5 Card: "Hlaalu Kinsman",
        new int[] { A, A, C }, // 6 Card: "House Embassy",
        new int[] { B, A, C }, // 7 Card: "House Marketplace",
        new int[] { C, B, C }, // 8 Card: "Kwama Egg Mine",
        new int[] { C, C, D }, // 9 Card: "Hireling",
        new int[] { B, C, D }, // 10 Card: "Hostile Takeover",
        new int[] { D, D, D }, // 11 Card: "Customs Seizure",
        new int[] { D, D, D }, // 12 Card: "Goods Shipment",

        new int[] { S, S, S }, // 13 Card: "Midnight Raid",
        new int[] { S, S, S }, // 14 Card: "Blood Sacrifice",
        new int[] { S, S, S }, // 15 Card: "Bloody Offering",
        new int[] { C, B, C }, // 16 Card: "Bonfire",
        new int[] { C, B, C }, // 17 Card: "Briarheart Ritual",
        new int[] { C, B, D }, // 18 Card: "Clan-Witch",
        new int[] { C, B, D }, // 19 Card: "Elder Witch",
        new int[] { D, B, D }, // 20 Card: "Hagraven",
        new int[] { D, A, C }, // 21 Card: "Hagraven Matron",
        new int[] { B, A, B }, // 22 Card: "Imperial Plunder",
        new int[] { B, A, B }, // 23 Card: "Imperial Spoils",
        new int[] { C, B, C }, // 24 Card: "Karth Man-Hunter",
        new int[] { D, D, D }, // 25 Card: "War Song",

        new int[] { S, S, A }, // 26 Card: "Blackfeather Knave",
        new int[] { S, S, S }, // 27 Card: "Plunder",
        new int[] { S, S, A }, // 28 Card: "Toll of Flesh",
        new int[] { S, S, A }, // 29 Card: "Toll of Silver",
        new int[] { S, S, A }, // 30 Card: "Murder of Crows",
        new int[] { A, S, A }, // 31 Card: "Pilfer",
        new int[] { A, S, A }, // 32 Card: "Squawking Oratory",
        new int[] { B, A, B }, // 33 Card: "Law of Sovereign Roost",
        new int[] { B, A, B }, // 34 Card: "Pool of Shadow",
        new int[] { A, A, B }, // 35 Card: "Scratch",
        new int[] { C, C, C }, // 36 Card: "Blackfeather Brigand",
        new int[] { A, B, C }, // 37 Card: "Blackfeather Knight",
        new int[] { C, C, C }, // 38 Card: "Peck",

        new int[] { A, A, B }, // 39 Card: "Conquest",
        new int[] { B, S, S }, // 40 Card: "Grand Oratory",
        new int[] { S, S, S }, // 41 Card: "Hira's End",
        new int[] { B, A, B }, // 42 Card: "Hel Shira Herald",
        new int[] { A, A, A }, // 43 Card: "March on Hattu",
        new int[] { B, B, B }, // 44 Card: "Shehai Summoning",
        new int[] { S, A, B }, // 45 Card: "Warrior Wave",
        new int[] { B, A, B }, // 46 Card: "Ansei Assault",
        new int[] { B, A, B }, // 47 Card: "Ansei's Victory",
        new int[] { C, B, B }, // 48 Card: "Battle Meditation",
        new int[] { C, C, C }, // 49 Card: "No Shira Poet",
        new int[] { D, D, D }, // 50 Card: "Way of the Sword",

        new int[] { 0, 0, 0 }, // 51 Card: "Prophesy",
        new int[] { 0, 0, 0 }, // 52 Card: "Scrying Globe",
        new int[] { 0, 0, 0 }, // 53 Card: "The Dreaming Cave",
        new int[] { 0, 0, 0 }, // 54 Card: "Augur's Counsel",
        new int[] { 0, 0, 0 }, // 55 Card: "Psijic Relicmaster",
        new int[] { 0, 0, 0 }, // 56 Card: "Sage Counsel",
        new int[] { 0, 0, 0 }, // 57 Card: "Prescience",
        new int[] { 0, 0, 0 }, // 58 Card: "Psijic Apprentice",
        new int[] { 0, 0, 0 }, // 59 Card: "Ceporah's Insight",
        new int[] { 0, 0, 0 }, // 60 Card: "Psijic's Insight",
        new int[] { 0, 0, 0 }, // 61 Card: "Time Mastery",
        new int[] { 0, 0, 0 }, // 62 Card: "Mainland Inquiries",

        new int[] { A, S, A }, // 63 Card: "Rally",
        new int[] { A, S, B }, // 64 Card: "Siege Weapon Volley",
        new int[] { A, S, A }, // 65 Card: "The Armory",
        new int[] { A, S, A }, // 66 Card: "Banneret",
        new int[] { S, S, A }, // 67 Card: "Knight Commander",
        new int[] { S, A, B }, // 68 Card: "Reinforcements",
        new int[] { B, A, B }, // 69 Card: "Archers' Volley",
        new int[] { A, A, B }, // 70 Card: "Legion's Arrival",
        new int[] { C, B, B }, // 71 Card: "Shield Bearer",
        new int[] { C, B, C }, // 72 Card: "Bangkorai Sentries",
        new int[] { C, A, C }, // 73 Card: "Knights of Saint Pelin",
        new int[] { C, C, D }, // 74 Card: "The Portcullis",
        new int[] { D, D, D }, // 75 Card: "Fortify",

        new int[] { C, B, B }, // 76 Card: "Bag of Tricks",
        new int[] { CURSE, CURSE, CURSE }, // 77 Card: "Bewilderment",
        new int[] { A, A, B }, // 78 Card: "Grand Larceny",
        new int[] { B, A, B }, // 79 Card: "Jarring Lullaby",
        new int[] { C, C, C }, // 80 Card: "Jeering Shadow",
        new int[] { B, A, B }, // 81 Card: "Moonlit Illusion",
        new int[] { S, S, B }, // 82 Card: "Pounce and Profit",
        new int[] { B, C, C }, // 83 Card: "Prowling Shadow",
        new int[] { C, C, C }, // 84 Card: "Ring's Guile",
        new int[] { A, A, B }, // 85 Card: "Shadow's Slumber",
        new int[] { A, B, D }, // 86 Card: "Slight of Hand",
        new int[] { D, C, D }, // 87 Card: "Stubborn Shadow",
        new int[] { D, D, D }, // 88 Card: "Swipe",
        new int[] { B, A, B }, // 89 Card: "Twilight Revelry",

        new int[] { 0, 0, 0 }, // 90 Card: "Ghostscale Sea Serpent",
        new int[] { 0, 0, 0 }, // 91 Card: "King Orgnum's Command",
        new int[] { 0, 0, 0 }, // 92 Card: "Maormer Boarding Party",
        new int[] { 0, 0, 0 }, // 93 Card: "Maormer Cutter",
        new int[] { 0, 0, 0 }, // 94 Card: "Pyandonean War Fleet",
        new int[] { 0, 0, 0 }, // 95 Card: "Sea Elf Raid",
        new int[] { 0, 0, 0 }, // 96 Card: "Sea Raider's Glory",
        new int[] { 0, 0, 0 }, // 97 Card: "Sea Serpent Colossus",
        new int[] { 0, 0, 0 }, // 98 Card: "Serpentguard Rider",
        new int[] { 0, 0, 0 }, // 99 Card: "Serpentprow Schooner",
        new int[] { 0, 0, 0 }, // 100 Card: "Snakeskin Freebooter",
        new int[] { 0, 0, 0 }, // 101 Card: "Storm Shark Wavecaller",
        new int[] { 0, 0, 0 }, // 102 Card: "Summerset Sacking",

        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 103 Card: "Ambush",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 104 Card: "Barterer",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 105 Card: "Black Sacrament",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 106 Card: "Blackmail",
        new int[] { UNKNOWN, CURSE, CURSE }, // 107 Card: "Gold",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 108 Card: "Harvest Season",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 109 Card: "Imprisonment",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 110 Card: "Ragpicker",
        new int[] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION }, // 111 Card: "Tithe",
        new int[] { WOC, B, UNKNOWN }, // 112 Card: "Writ of Coin",
    };
    public static int GetCardTier(int cardId, GamePhase gamePhase) => cardTierDict[cardId][(int)gamePhase];
}


// public static class GPCardTierListX
// {
//     const int S = 50;
//     const int A = 30;
//     const int B = 15;
//     const int WOC = 10;
//     const int C = 3;
//     const int D = 1;
//     const int UNKNOWN = 0;
//     const int CONTRACT_ACTION = 0;
//     const int CURSE = -3;

//     static readonly Dictionary<CardId, int[]> cardTierDict = new Dictionary<CardId, int[]> {
//         { CardId.CURRENCY_EXCHANGE, new int[] { S, S, A } },
//         { CardId.LUXURY_EXPORTS,    new int[] { S, S, C } },
//         { CardId.OATHMAN,           new int[] { A, A, B } },
//         { CardId.EBONY_MINE,        new int[] { C, B, C } },
//         { CardId.HLAALU_COUNCILOR,  new int[] { A, A, C } },
//         { CardId.HLAALU_KINSMAN,    new int[] { A, A, C } },
//         { CardId.HOUSE_EMBASSY,     new int[] { A, A, C } },
//         { CardId.HOUSE_MARKETPLACE, new int[] { B, A, C } },
//         { CardId.HIRELING,          new int[] { C, C, D } },
//         { CardId.HOSTILE_TAKEOVER,  new int[] { B, C, D } },
//         { CardId.KWAMA_EGG_MINE,    new int[] { C, B, C } },
//         { CardId.CUSTOMS_SEIZURE,   new int[] { D, D, D } },
//         { CardId.GOODS_SHIPMENT,    new int[] { D, D, D } },

//         { CardId.MIDNIGHT_RAID,     new int[] { S, S, S } },
//         { CardId.BLOOD_SACRIFICE,   new int[] { S, S, S } },
//         { CardId.BLOODY_OFFERING,   new int[] { S, S, S } },
//         { CardId.BONFIRE,           new int[] { C, B, C } },
//         { CardId.BRIARHEART_RITUAL, new int[] { C, B, C } },
//         { CardId.CLANWITCH,         new int[] { C, B, D } },
//         { CardId.ELDER_WITCH,       new int[] { C, B, D } },
//         { CardId.HAGRAVEN,          new int[] { D, B, D } },
//         { CardId.HAGRAVEN_MATRON,   new int[] { D, A, C } },
//         { CardId.IMPERIAL_PLUNDER,  new int[] { B, A, B } },
//         { CardId.IMPERIAL_SPOILS,   new int[] { B, A, B } },
//         { CardId.KARTH_MANHUNTER,   new int[] { C, B, C } },
//         { CardId.WAR_SONG,          new int[] { D, D, D } },

//         { CardId.BLACKFEATHER_KNAVE,     new int[] { S, S, A } },
//         { CardId.PLUNDER,                new int[] { S, S, S } },
//         { CardId.TOLL_OF_FLESH,          new int[] { S, S, A } },
//         { CardId.TOLL_OF_SILVER,         new int[] { S, S, A } },
//         { CardId.MURDER_OF_CROWS,        new int[] { S, S, A } },
//         { CardId.PILFER,                 new int[] { A, S, A } },
//         { CardId.SQUAWKING_ORATORY,      new int[] { A, S, A } },
//         { CardId.LAW_OF_SOVEREIGN_ROOST, new int[] { B, A, B } },
//         { CardId.POOL_OF_SHADOW,         new int[] { B, A, B } },
//         { CardId.SCRATCH,                new int[] { A, A, B } },
//         { CardId.BLACKFEATHER_BRIGAND,   new int[] { C, C, C } },
//         { CardId.BLACKFEATHER_KNIGHT,    new int[] { B, B, C } },
//         { CardId.PECK,                   new int[] { C, C, C } },

//         { CardId.CONQUEST,          new int[] { A, A, B } },
//         { CardId.GRAND_ORATORY,     new int[] { B, S, S } },
//         { CardId.HIRAS_END,         new int[] { S, S, S } },
//         { CardId.HEL_SHIRA_HERALD,  new int[] { B, A, B } },
//         { CardId.MARCH_ON_HATTU,    new int[] { A, A, A } },
//         { CardId.SHEHAI_SUMMONING,  new int[] { B, B, B } },
//         { CardId.WARRIOR_WAVE,      new int[] { S, A, B } },
//         { CardId.ANSEI_ASSAULT,     new int[] { B, A, B } },
//         { CardId.ANSEIS_VICTORY,    new int[] { B, A, B } },
//         { CardId.BATTLE_MEDITATION, new int[] { C, B, B } },
//         { CardId.NO_SHIRA_POET,     new int[] { C, C, C } },
//         { CardId.WAY_OF_THE_SWORD,  new int[] { D, D, D } },

//         { CardId.RALLY,                  new int[] { A, S, A } },
//         { CardId.SIEGE_WEAPON_VOLLEY,    new int[] { A, S, B } },
//         { CardId.THE_ARMORY,             new int[] { A, S, A } },
//         { CardId.BANNERET,               new int[] { A, S, A } },
//         { CardId.KNIGHT_COMMANDER,       new int[] { S, S, A } },
//         { CardId.REINFORCEMENTS,         new int[] { S, A, B } },
//         { CardId.ARCHERS_VOLLEY,         new int[] { B, A, B } },
//         { CardId.LEGIONS_ARRIVAL,        new int[] { A, A, B } },
//         { CardId.SHIELD_BEARER,          new int[] { C, B, B } },
//         { CardId.BANGKORAI_SENTRIES,     new int[] { C, B, C } },
//         { CardId.KNIGHTS_OF_SAINT_PELIN, new int[] { C, A, C } },
//         { CardId.THE_PORTCULLIS,         new int[] { C, C, D } },
//         { CardId.FORTIFY,                new int[] { D, D, D } },

//         { CardId.BAG_OF_TRICKS,     new int[] { C, B, B } },
//         { CardId.BEWILDERMENT,      new int[] { CURSE, CURSE, CURSE } },
//         { CardId.GRAND_LARCENY,     new int[] { A, A, B } },
//         { CardId.JARRING_LULLABY,   new int[] { B, A, B } },
//         { CardId.JEERING_SHADOW,    new int[] { C, C, C } },
//         { CardId.MOONLIT_ILLUSION,  new int[] { B, A, B } },
//         { CardId.POUNCE_AND_PROFIT, new int[] { S, S, B } },
//         { CardId.PROWLING_SHADOW,   new int[] { B, C, C } },
//         { CardId.RINGS_GUILE,       new int[] { C, C, C } },
//         { CardId.SHADOWS_SLUMBER,   new int[] { A, A, B } },
//         { CardId.SLIGHT_OF_HAND,    new int[] { A, B, D } },
//         { CardId.STUBBORN_SHADOW,   new int[] { D, C, D } },
//         { CardId.TWILIGHT_REVELRY,  new int[] { B, A, B } },
//         { CardId.SWIPE,             new int[] { D, D, D } },

//         { CardId.AMBUSH,          new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.BARTERER,        new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.BLACK_SACRAMENT, new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.BLACKMAIL,       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.GOLD,            new [] { UNKNOWN, CURSE, CURSE } },
//         { CardId.HARVEST_SEASON,  new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.IMPRISONMENT,    new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.RAGPICKER,       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.TITHE,           new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
//         { CardId.WRIT_OF_COIN,    new [] { WOC, B, UNKNOWN } },
//     };

//     public static int GetCardTier(CardId id, GamePhase gamePhase) => cardTierDict[id][(int)gamePhase];
// }