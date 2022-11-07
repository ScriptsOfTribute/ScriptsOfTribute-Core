namespace TalesOfTribute
{
    public enum PatronEnum : int
    {
        Ansei = 0,
        DukeOfCrows = 1,
        Rajhin = 2,
        Psijic = 3,
        Orgnum = 4,
        Hlaalu = 5,
        Pelin = 6,
        RedEagle = 7,
        Treasury = 8,
    }
    public abstract class Patron
    {
        /*
         * PatronActivation - used on activation, check if player can acitvate that and do action related to one-time activation
         * PatronPower - for powers that are valid every turn
         * 
         * Of course activating Patron moves favors as follows: unfavored -> neutral -> favored, with exception of Ansei which always
         * gives Favored effect
         */
        public int FavoredPlayer { get; set; } = -1; // -1 for neutral, otherwise ID of player favored

        public abstract bool PatronActivation(Player activator, Player enemy, Card? card = null);
        public abstract bool PatronPower(Player activator, Player enemy);

        public static Patron FromEnum(PatronEnum patron)
        {
            return patron switch
            {
                PatronEnum.Ansei => new Ansei(),
                PatronEnum.DukeOfCrows => new DukeOfCrows(),
                PatronEnum.Rajhin => new Rajhin(),
                PatronEnum.Orgnum => new Orgnum(),
                PatronEnum.Hlaalu => new Hlaalu(),
                PatronEnum.Psijic => new Psijic(),
                _ => throw new InvalidOperationException()
            };
        }

        public static PatronEnum FromString(string patron)
        {
            return patron switch
            {
                "Hlaalu" => PatronEnum.Hlaalu,
                "Red Eagle" => PatronEnum.RedEagle,
                "Crows" => PatronEnum.DukeOfCrows,
                "Ansei" => PatronEnum.Ansei,
                "Psijic" => PatronEnum.Psijic,
                "Pelin" => PatronEnum.Pelin,
                "Rajhin" => PatronEnum.Rajhin,
                "Orgnum" => PatronEnum.Orgnum,
                "Treasury" => PatronEnum.Treasury,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
