using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public enum PatronId
    {
        ANSEI = 0,
        DUKE_OF_CROWS = 1,
        RAJHIN = 2,
        PSIJIC = 3,
        ORGNUM = 4,
        HLAALU = 5,
        PELIN = 6,
        RED_EAGLE = 7,
        TREASURY = 8,
        SAINT_ALESSIA = 9,
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
        public PlayerEnum FavoredPlayer { get; set; } = PlayerEnum.NO_PLAYER_SELECTED;

        public abstract (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy);
        public abstract void PatronPower(Player activator, Player enemy);

        public abstract PatronId PatronID { get; }
        public abstract List<CardId> GetStarterCards();

        public abstract bool CanPatronBeActivated(Player activator, Player enemy);

        public static Patron FromId(PatronId patron)
        {
            return patron switch
            {
                PatronId.ANSEI => new Ansei(),
                PatronId.DUKE_OF_CROWS => new DukeOfCrows(),
                PatronId.RAJHIN => new Rajhin(),
                PatronId.ORGNUM => new Orgnum(),
                PatronId.HLAALU => new Hlaalu(),
                PatronId.PSIJIC => new Psijic(),
                PatronId.PELIN => new Pelin(),
                PatronId.RED_EAGLE => new RedEagle(),
                PatronId.TREASURY => new Treasury(),
                PatronId.SAINT_ALESSIA => new SaintAlessia(),
                _ => throw new InvalidOperationException()
            };
        }

        public static PatronId IdFromString(string patron)
        {
            return patron switch
            {
                "Hlaalu" => PatronId.HLAALU,
                "Red Eagle" => PatronId.RED_EAGLE,
                "Crows" => PatronId.DUKE_OF_CROWS,
                "Ansei" => PatronId.ANSEI,
                "Psijic" => PatronId.PSIJIC,
                "Pelin" => PatronId.PELIN,
                "Rajhin" => PatronId.RAJHIN,
                "Orgnum" => PatronId.ORGNUM,
                "Treasury" => PatronId.TREASURY,
                "Saint Alessia" => PatronId.SAINT_ALESSIA,
                _ => throw new InvalidOperationException()
            };
        }

        public static List<Patron> FromSerializedBoard(FullGameState board)
        {
            var result = new List<Patron>();
            foreach (var (patronId, favor) in board.PatronStates.All)
            {
                var p = Patron.FromId(patronId);
                p.FavoredPlayer = favor;
                result.Add(p);
            }
            return result;
        }
    }
}
