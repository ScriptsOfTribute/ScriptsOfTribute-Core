namespace TalesOfTribute
{
    public static class Command
    {
        public static string PLAY_CARD = "PlayCard";
        public static string ATTACK = "Attack";
        public static string BUY_CARD = "BuyCard";
        public static string TREASURY = "Treasury";
        public static string DUKE_OF_CROWS = "Crows";
        public static string RED_EAGLE = "RedEagle";
        public static string ANSEI = "Ansei";
        public static string PELIN = "Pelin";
        public static string RAJHIN = "Rajhin";
        public static string ORGNUM = "Orgnum";
        public static string PSIJIC = "Psijic";
        public static string HLAALU = "Hlaalu";
        public static string END_TURN = "EndTurn";

        public static bool ValidateStringCommand(string move){
            switch (move){
                case "PlayCard":
                    return true;
                case "Attack":
                    return true;
                case "BuyCard":
                    return true;
                case "Tresury":
                    return true;
                case "Crows":
                    return true;
                case "RedEagle":
                    return true;
                case "Ansei":
                    return true;
                case "Pelin":
                    return true;
                case "Rajhin":
                    return true;
                case "Orgnum":
                    return true;
                case "Psijic":
                    return true;
                case "Hlaalu":
                    return true;
                case "EndTurn":
                    return true;
                default:
                    return false;
            }
        }
    }


    public struct Move{
        public Move(string command, int value=-1){
            Command = command;
            Value = value;
        }

        public double Command { get; }
        public double Value { get; }

        public override string ToString() => $"({Command} {Value})";
    }
}