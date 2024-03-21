using ScriptsOfTribute.Board;
using ScriptsOfTribute.utils;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute.AI;

public class ScriptsOfTribute
{
    private AI[] _players = new AI[2];

    private ScriptsOfTributeGame? _game;
    private ulong _seed;

    public ulong Seed
    {
        get => _seed;
        set
        {
            _seed = value;
        }
    }
    public TextWriter P1LogTarget { get; set; } = Console.Out;
    public TextWriter P2LogTarget { get; set; } = Console.Out;
    public bool P1LoggerEnabled { get; set; } = false;
    public bool P2LoggerEnabled { get; set; } = false;
    public TimeSpan Timeout { get; set; }

    public ScriptsOfTribute(AI player1, AI player2, TimeSpan timeout)
    {
        _players[0] = player1;
        _players[1] = player2;
        player1.Id = PlayerEnum.PLAYER1;
        player2.Id = PlayerEnum.PLAYER2;
        Seed = (ulong)Environment.TickCount;
        Timeout = timeout;
    }

    public ScriptsOfTribute(AI player1, AI player2) : this(player1, player2, TimeSpan.FromSeconds(30))
    { }

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
            // TODO: Hardcoded banned patrons, improve this.
            .Where(patronId => patronId != PatronId.TREASURY && patronId != PatronId.PSIJIC).ToList();

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
        EndGameState? endGameState;
        PatronId[]? patrons;

        endGameState = PrepareBots(_players[0], _players[1], 5);
        
        if (endGameState is not null)
        {
            _players[0].GameEnd(endGameState, null);
            _players[1].GameEnd(endGameState, null);
            return (endGameState, null);
        }

        (endGameState, patrons) = PatronSelection();

        if (endGameState is not null)
        {
            _players[0].GameEnd(endGameState, null);
            _players[1].GameEnd(endGameState, null);
            return (endGameState, null);
        }

        var api = new ScriptsOfTributeApi(patrons!, Seed)
        {
            Logger =
            {
                P1LoggerEnabled = P1LoggerEnabled,
                P2LoggerEnabled = P2LoggerEnabled,
                P1LogTarget = P1LogTarget,
                P2LogTarget = P2LogTarget
            }
        };
        _game = new ScriptsOfTributeGame(_players, api, Timeout);

        var r = _game!.Play();

        return r;
    }


    private EndGameState? PrepareBots(AI player1, AI player2, int timeout)
    {
        Task task = Task.Run(() =>
        {
            player1.PregamePrepare();
        });

        if (!task.Wait(TimeSpan.FromSeconds(timeout)))
        {
            return new EndGameState(PlayerEnum.PLAYER2, GameEndReason.PREPARE_TIME_EXCEEDED);
        }

        task = Task.Run(() =>
        {
            player2.PregamePrepare();
        });

        if (!task.Wait(TimeSpan.FromSeconds(timeout)))
        {
            return new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PREPARE_TIME_EXCEEDED);
        }

        return null;
    }
}
