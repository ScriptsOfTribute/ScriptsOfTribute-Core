using ScriptsOfTribute;

namespace Bots;


public class FilePaths
{
    private static string dir = Environment.CurrentDirectory;
    private static string botsPath = dir + @"/HQL_BOT/";
    public static string qTablePath = botsPath + "QTABLE.txt";
    public static string tmpFile = botsPath + "tmp_file.txt";
    public static string errorFile = botsPath + "error_file.txt";
}

public static class Consts
{
    public static int number_of_all_cards = 113;
    public static int max_stage = 3;
    public static int max_combo = 3;
    public static int max_deck_cards = 19;

    public static int[] prestige_weight = new int[]{15, 16, 20, 20};
    public static int[] power_weight = new int[]{14, 15, 19, 19};
    public static int[] coins_weight = new int[]{2, 1, 1, 1};
    public static int[] patron_weight = new int[]{18, 18, 18, 18};
    public static int[] neutral_patron_weight = new int[]{2, 2, 2, 2};
    public static int[] card_weight = new int[]{5, 5, 4, 4};
    public static int[] combo_weight = new int[]{2, 2, 2, 2};
    public static int[] active_agent_weight = new int[]{12, 12, 12, 12};
    public static int[] hp_weight = new int[]{4, 4, 4, 4};
    public static int[] ansei_weight = new int[]{3, 2, 1, 1};
    public static int[] crow_weight = new int[]{-1000, -1000, 0, 20};
    public static int[] orgnum_weight = new int[]{1, 1, 3, 5};
    public static int[] gold_card_weight = new int[]{0, -2, -4, -6};
}

public enum Stage
{
    Start = 0,
    Early = 1,
    Middle = 2,
    Late = 3,
}

