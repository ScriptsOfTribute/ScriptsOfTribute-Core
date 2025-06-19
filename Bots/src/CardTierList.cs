using ScriptsOfTribute;

namespace Bots;

public enum TierEnum
{
    /*
    S = 1000,
    A = 400,
    B = 200,
    C = 90,
    D = 40,
    */
    S = 50,
    A = 25,
    B = 10,
    C = 5,
    D = 1,
    UNKNOWN = 0,
}

public class CardTier
{
    public string Name;
    public PatronId Deck;
    public TierEnum Tier;

    public CardTier(string name, PatronId deck, TierEnum tier)
    {
        Name = name;
        Deck = deck;
        Tier = tier;
    }
}
public class CardTierList
{
    private static CardTier[] CardTierArray = {
        new CardTier("Currency Exchange", PatronId.HLAALU, TierEnum.S),
        new CardTier("Luxury Exports", PatronId.HLAALU, TierEnum.S),
        new CardTier("Oathman", PatronId.HLAALU, TierEnum.A),
        new CardTier("Ebony Mine", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Councilor", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Kinsman", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Embassy", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Marketplace", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hireling", PatronId.HLAALU, TierEnum.C),
        new CardTier("Hostile Takeover", PatronId.HLAALU, TierEnum.C),
        new CardTier("Kwama Egg Mine", PatronId.HLAALU, TierEnum.C),
        new CardTier("Customs Seizure", PatronId.HLAALU, TierEnum.D),
        new CardTier("Goods Shipment", PatronId.HLAALU, TierEnum.D),
        new CardTier("Midnight Raid", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Blood Sacrifice", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bloody Offering", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bonfire", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Briarheart Ritual", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Clan-Witch", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Elder Witch", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven Matron", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Plunder", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Spoils", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Karth Man-Hunter", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("War Song", PatronId.RED_EAGLE, TierEnum.D),
        new CardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Plunder", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Pilfer", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Scratch", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Peck", PatronId.DUKE_OF_CROWS, TierEnum.D),
        new CardTier("Conquest", PatronId.ANSEI, TierEnum.S),
        new CardTier("Grand Oratory", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hira's End", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hel Shira Herald", PatronId.ANSEI, TierEnum.A),
        new CardTier("March on Hattu", PatronId.ANSEI, TierEnum.A),
        new CardTier("Shehai Summoning", PatronId.ANSEI, TierEnum.A),
        new CardTier("Warrior Wave", PatronId.ANSEI, TierEnum.A),
        new CardTier("Ansei Assault", PatronId.ANSEI, TierEnum.B),
        new CardTier("Ansei's Victory", PatronId.ANSEI, TierEnum.B),
        new CardTier("Battle Meditation", PatronId.ANSEI, TierEnum.B),
        new CardTier("No Shira Poet", PatronId.ANSEI, TierEnum.C),
        new CardTier("Way of the Sword", PatronId.ANSEI, TierEnum.D),
        new CardTier("Prophesy", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Scrying Globe", PatronId.PSIJIC, TierEnum.S),
        new CardTier("The Dreaming Cave", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Augur's Counsel", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Relicmaster", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Sage Counsel", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Prescience", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Apprentice", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Ceporah's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Psijic's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Time Mastery", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Mainland Inquiries", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Rally", PatronId.PELIN, TierEnum.S),
        new CardTier("Siege Weapon Volley", PatronId.PELIN, TierEnum.S),
        new CardTier("The Armory", PatronId.PELIN, TierEnum.S),
        new CardTier("Banneret", PatronId.PELIN, TierEnum.A),
        new CardTier("Knight Commander", PatronId.PELIN, TierEnum.A),
        new CardTier("Reinforcements", PatronId.PELIN, TierEnum.A),
        new CardTier("Archers' Volley", PatronId.PELIN, TierEnum.B),
        new CardTier("Legion's Arrival", PatronId.PELIN, TierEnum.B),
        new CardTier("Shield Bearer", PatronId.PELIN, TierEnum.B),
        new CardTier("Bangkorai Sentries", PatronId.PELIN, TierEnum.C),
        new CardTier("Knights of Saint Pelin", PatronId.PELIN, TierEnum.C),
        new CardTier("The Portcullis", PatronId.PELIN, TierEnum.D),
        new CardTier("Fortify", PatronId.PELIN, TierEnum.D),
        new CardTier("Bag of Tricks", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Bewilderment", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Grand Larceny", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Jarring Lullaby", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Jeering Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Moonlit Illusion", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Pounce and Profit", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Prowling Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Ring's Guile", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Shadow's Slumber", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Slight of Hand", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Stubborn Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Swipe", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Twilight Revelry", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Ghostscale Sea Serpent", PatronId.ORGNUM, TierEnum.B),
        new CardTier("King Orgnum's Command", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Maormer Boarding Party", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Maormer Cutter", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Pyandonean War Fleet", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Sea Elf Raid", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Raider's Glory", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Serpent Colossus", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Serpentguard Rider", PatronId.ORGNUM, TierEnum.A),
        new CardTier("Serpentprow Schooner", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Snakeskin Freebooter", PatronId.ORGNUM, TierEnum.S),
        new CardTier("Storm Shark Wavecaller", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Summerset Sacking", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Ambush", PatronId.TREASURY, TierEnum.B),
        new CardTier("Barterer", PatronId.TREASURY, TierEnum.C),
        new CardTier("Black Sacrament", PatronId.TREASURY, TierEnum.B),
        new CardTier("Blackmail", PatronId.TREASURY, TierEnum.B),
        new CardTier("Gold", PatronId.TREASURY, TierEnum.UNKNOWN),
        new CardTier("Harvest Season", PatronId.TREASURY, TierEnum.C),
        new CardTier("Imprisonment", PatronId.TREASURY, TierEnum.C),
        new CardTier("Ragpicker", PatronId.TREASURY, TierEnum.C),
        new CardTier("Tithe", PatronId.TREASURY, TierEnum.C),
        new CardTier("Writ of Coin", PatronId.TREASURY, TierEnum.D),
        new CardTier("Unknown", PatronId.TREASURY, TierEnum.UNKNOWN),
        // Added Saint Alessia
        new CardTier("Alessian Rebel", PatronId.SAINT_ALESSIA, TierEnum.D),
        new CardTier("Ayleid Defector", PatronId.SAINT_ALESSIA, TierEnum.A),
        new CardTier("Ayleid Quartermaster", PatronId.SAINT_ALESSIA, TierEnum.A),
        new CardTier("Chainbreaker Captain", PatronId.SAINT_ALESSIA, TierEnum.A),
        new CardTier("Chainbreaker Sergeant", PatronId.SAINT_ALESSIA, TierEnum.B),
        new CardTier("Morihaus, Sacred Bull", PatronId.SAINT_ALESSIA, TierEnum.S),
        new CardTier("Morihaus, the Archer", PatronId.SAINT_ALESSIA, TierEnum.A),
        new CardTier("Pelinal Whitestrake", PatronId.SAINT_ALESSIA, TierEnum.S),
        new CardTier("Priestess of the Eight", PatronId.SAINT_ALESSIA, TierEnum.B),
        new CardTier("Saint's Wrath", PatronId.SAINT_ALESSIA, TierEnum.B),
        new CardTier("Soldier of the Empire", PatronId.SAINT_ALESSIA, TierEnum.C),
        new CardTier("Whitestrake Ascendant", PatronId.SAINT_ALESSIA, TierEnum.S),
    };

    public static TierEnum GetCardTier(string cardName)
    {
        try
        {
            return Array.Find(CardTierArray, x => x.Name == cardName).Tier;
        }
        catch
        {
            return TierEnum.UNKNOWN;
        }
        
    }
}