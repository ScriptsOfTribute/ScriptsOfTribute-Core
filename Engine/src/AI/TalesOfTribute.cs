﻿using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public class TalesOfTribute
{
    private AI[] _players = new AI[2];

    private TalesOfTributeGame? _game;

    public ulong Seed = (ulong)Environment.TickCount;

    public TalesOfTribute(AI player1, AI player2)
    {
        _players[0] = player1;
        _players[1] = player2;
        player1.Id = PlayerEnum.PLAYER1;
        player2.Id = PlayerEnum.PLAYER2;
    }

    private Task<PatronId> SelectPatronTask(AI currentPlayer, List<PatronId> availablePatrons, int round)
    {
        return Task.Run(() => currentPlayer.SelectPatron(availablePatrons, round));
    }

    private (EndGameState?, PatronId?) SelectPatronWithTimeout(PlayerEnum playerToWin, AI currentPlayer, List<PatronId> availablePatrons, int round)
    {
        var timeout = currentPlayer.MoveTimeout;
        var task = SelectPatronTask(currentPlayer, availablePatrons, round);

        if (task.Wait(timeout))
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

    public (EndGameState, SerializedBoard?) Play()
    {
        var (endGameState, patrons) = PatronSelection();

        if (endGameState is not null)
        {
            _players[0].GameEnd(endGameState);
            _players[1].GameEnd(endGameState);
            return (endGameState, null);
        }

        _game = new TalesOfTributeGame(_players, new TalesOfTributeApi(patrons!, Seed));

        var r = _game!.Play();

        return r;
    }
}
