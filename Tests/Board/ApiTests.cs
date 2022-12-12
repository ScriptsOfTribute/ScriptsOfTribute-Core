using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace Tests.Board;

public class ApiTests
{
    private BoardManager _boardManager = new BoardManager(new[] { PatronId.PELIN, PatronId.TREASURY });

    [Fact]
    void TestGetListOfPossibleMoves()
    {
        _boardManager.SetUpGame();
        TalesOfTributeApi api = new TalesOfTributeApi(_boardManager);

        List<Move> possibleMoves = api.GetListOfPossibleMoves();
        Assert.Contains(Move.EndTurn(), possibleMoves);
        Assert.DoesNotContain(Move.CallPatron(PatronId.PELIN), possibleMoves);

        var conquest = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        _boardManager.CurrentPlayer.CooldownPile.Add(conquest);
        _boardManager.CurrentPlayer.PowerAmount = 6;

        possibleMoves = api.GetListOfPossibleMoves();
        Assert.Contains(Move.CallPatron(PatronId.PELIN), possibleMoves);

        _boardManager.CurrentPlayer.PowerAmount = 1;
        possibleMoves = api.GetListOfPossibleMoves();
        Assert.DoesNotContain(Move.CallPatron(PatronId.PELIN), possibleMoves);
    }
}
