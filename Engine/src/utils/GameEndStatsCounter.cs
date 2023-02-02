using System.Text;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;

namespace Bots;

public class GameEndStatsCounter
{
    private int _statesChecked;
    private int _p1Wins;
    private int _p2Wins;
    private int _draws;
    private int _prestigeOver40Ends;
    private int _prestigeOver80Ends;
    private int _patronFavorEnds;
    private int _turnLimitEnds;
    private int _otherEnds;
    
    public double P1WinPercentage => 100.0 * _p1Wins / _statesChecked;
    public double P2WinPercentage => 100.0 * _p2Wins / _statesChecked;
    public double DrawPercentage => 100.0 * _draws / _statesChecked;
    
    public void Add(EndGameState state)
    {
        switch (state.Winner)
        {
            case PlayerEnum.PLAYER1:
                _p1Wins++;
                break;
            case PlayerEnum.PLAYER2:
                _p2Wins++;
                break;
            case PlayerEnum.NO_PLAYER_SELECTED:
                _draws++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (state.Reason)
        {
            case GameEndReason.TURN_LIMIT_EXCEEDED:
                _turnLimitEnds++;
                break;
            case GameEndReason.PRESTIGE_OVER_40_NOT_MATCHED:
                _prestigeOver40Ends++;
                break;
            case GameEndReason.PRESTIGE_OVER_80:
                _prestigeOver80Ends++;
                break;
            case GameEndReason.PATRON_FAVOR:
                _patronFavorEnds++;
                break;
            case GameEndReason.TURN_TIMEOUT:
            case GameEndReason.PATRON_SELECTION_TIMEOUT:
            case GameEndReason.PATRON_SELECTION_FAILURE:
            case GameEndReason.INCORRECT_MOVE:
            case GameEndReason.INTERNAL_ERROR:
            case GameEndReason.BOT_EXCEPTION:
                _otherEnds++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _statesChecked++;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Final amount of draws: {_draws}/{_statesChecked} ({DrawPercentage}%)");
        sb.AppendLine($"Final amount of P1 wins: {_p1Wins}/{_statesChecked} ({P1WinPercentage}%)");
        sb.AppendLine($"Final amount of P2 wins: {_p2Wins}/{_statesChecked} ({P2WinPercentage}%)");
        sb.AppendLine($"Ends due to Prestige>40: {_prestigeOver40Ends}/{_statesChecked} ({100.0*_prestigeOver40Ends/_statesChecked}%)");
        sb.AppendLine($"Ends due to Prestige>80: {_prestigeOver80Ends}/{_statesChecked} ({100.0*_prestigeOver80Ends/_statesChecked}%)");
        sb.AppendLine($"Ends due to Patron Favor: {_patronFavorEnds}/{_statesChecked} ({100.0*_patronFavorEnds/_statesChecked}%)");
        sb.AppendLine($"Ends due to Turn Limit: {_turnLimitEnds}/{_statesChecked} ({100.0*_turnLimitEnds/_statesChecked}%)");
        sb.AppendLine($"Ends due to other factors: {_otherEnds}/{_statesChecked} ({100.0*_otherEnds/_statesChecked}%)");
        return sb.ToString();
    }
}
