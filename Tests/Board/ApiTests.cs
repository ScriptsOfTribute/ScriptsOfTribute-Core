using TalesOfTribute;
using Xunit.Abstractions;

namespace Tests.Board;

public class ApiTests
{
    private readonly ITestOutputHelper output;
    private BoardManager _boardManager = new BoardManager(new[] { PatronId.PELIN, PatronId.TREASURY });

    public ApiTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    void TestGetListOfPossibleMoves()
    {
        _boardManager.SetUpGame();
        TalesOfTributeApi api = new TalesOfTributeApi(_boardManager);

        List<Move> possibleMoves = api.GetListOfPossibleMoves();
        Assert.Contains(new Move(CommandEnum.END_TURN), possibleMoves);
        Assert.DoesNotContain(new Move(CommandEnum.PATRON, (int)PatronId.PELIN), possibleMoves);

        var conquest = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        _boardManager.CurrentPlayer.CooldownPile.Add(conquest);
        _boardManager.CurrentPlayer.PowerAmount = 6;
        
        possibleMoves = api.GetListOfPossibleMoves();
        Assert.Contains(new Move(CommandEnum.PATRON, (int)PatronId.PELIN), possibleMoves);

        _boardManager.CurrentPlayer.PowerAmount = 1;
        possibleMoves = api.GetListOfPossibleMoves();
        Assert.DoesNotContain(new Move(CommandEnum.PATRON, (int)PatronId.PELIN), possibleMoves);

    }
}