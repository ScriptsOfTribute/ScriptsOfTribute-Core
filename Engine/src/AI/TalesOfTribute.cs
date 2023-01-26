using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.utils;

namespace TalesOfTribute.AI;

public class TalesOfTribute
{
    private AI[] _players = new AI[2];

    private TalesOfTributeGame? _game;
    private ulong _seed;

    public ulong Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _players[0].Seed = FreeFunctions.ScrambleSeed(value);
            _players[1].Seed = FreeFunctions.ScrambleSeed(value);
        }
    }
    public TextWriter P1LogTarget { get; set; } = Console.Out;
    public TextWriter P2LogTarget { get; set; } = Console.Out;
    public bool P1LoggerEnabled { get; set; } = false;
    public bool P2LoggerEnabled { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    public TalesOfTribute(AI player1, AI player2)
    {
        _players[0] = player1;
        _players[1] = player2;
        player1.Id = PlayerEnum.PLAYER1;
        player2.Id = PlayerEnum.PLAYER2;
        Seed = (ulong)Environment.TickCount;
    }

    private Task<PatronId> SelectPatronTask(AI currentPlayer, List<PatronId> availablePatrons, int round)
    {
        return Task.Run(() => currentPlayer.SelectPatron(availablePatrons, round));
    }

    private (EndGameState?, PatronId?) SelectPatronWithTimeout(PlayerEnum playerToWin, AI currentPlayer, List<PatronId> availablePatrons, int round)
    {
        var task = SelectPatronTask(currentPlayer, availablePatrons, round);

        if (task.Wait(Timeout))
        {
            var endGameState = VerifyPatronSelection(playerToWin, task.Result, availablePatrons);
            if (endGameState is not null)
            {
                return (endGameState, null);
            }

            return (null, task.Result);
        }

        return (new EndGameState(playerToWin, GameEndReason.PATRON_SELECTION_TIMEOUT), null);
    }

    private (EndGameState? ,PatronId[]?) PatronSelection()
    {
        List<PatronId> patrons = Enum.GetValues(typeof(PatronId)).Cast<PatronId>()
            .Where(patronId => patronId != PatronId.TREASURY).ToList();

        List<PatronId> patronsSelected = new List<PatronId>();
        var (endGameState, patron) = SelectPatronWithTimeout(PlayerEnum.PLAYER2, _players[0], patrons, 1);
        if (endGameState is not null)
        {
            return (endGameState, null);
        }
        patronsSelected.Add((PatronId)patron!);
        patrons.Remove((PatronId)patron);

        (endGameState, patron) = SelectPatronWithTimeout(PlayerEnum.PLAYER1, _players[1], patrons, 1);
        if (endGameState is not null)
        {
            return (endGameState, null);
        }
        patronsSelected.Add((PatronId)patron!);
        patrons.Remove((PatronId)patron);

        patronsSelected.Add(PatronId.TREASURY);

        (endGameState, patron) = SelectPatronWithTimeout(PlayerEnum.PLAYER1, _players[1], patrons, 2);
        if (endGameState is not null)
        {
            return (endGameState, null);
        }
        patronsSelected.Add((PatronId)patron!);
        patrons.Remove((PatronId)patron);

        (endGameState, patron) = SelectPatronWithTimeout(PlayerEnum.PLAYER2, _players[0], patrons, 2);
        if (endGameState is not null)
        {
            return (endGameState, null);
        }
        patronsSelected.Add((PatronId)patron!);
        patrons.Remove((PatronId)patron);

        return (null, patronsSelected.ToArray());
    }

    private EndGameState? VerifyPatronSelection(PlayerEnum playerToWin, PatronId patron, List<PatronId> patrons)
    {
        if (!patrons.Contains(patron) || !Enum.IsDefined(typeof(PatronId), patron))
        {
            return new EndGameState(playerToWin, GameEndReason.PATRON_SELECTION_FAILURE);
        }

        return null;
    }

    public (EndGameState, FullGameState?) Play()
    {
        var (endGameState, patrons) = PatronSelection();

        if (endGameState is not null)
        {
            _players[0].GameEnd(endGameState, null);
            _players[1].GameEnd(endGameState, null);
            return (endGameState, null);
        }

        var api = new TalesOfTributeApi(patrons!, Seed)
        {
            Logger =
            {
                P1LoggerEnabled = P1LoggerEnabled,
                P2LoggerEnabled = P2LoggerEnabled,
                P1LogTarget = P1LogTarget,
                P2LogTarget = P2LogTarget
            }
        };
        _game = new TalesOfTributeGame(_players, api, Timeout);

        var r = _game!.Play();

        return r;
    }
}
