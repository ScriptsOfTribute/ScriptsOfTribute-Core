using ScriptsOfTribute;
using ScriptsOfTribute.Board;
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
    
    [Fact]
    void ShouldGenerateAllPossibleChoicesIfChoiceIsPending()
    {
        _boardManager.SetUpGame();
        var api = new TalesOfTributeApi(_boardManager);
        var currencyExchange = GlobalCardDatabase.Instance.GetCard(CardId.CURRENCY_EXCHANGE);
        var gold = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var party = GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY);
        _boardManager.CurrentPlayer.Hand.Add(currencyExchange);
        _boardManager.Tavern.AvailableCards = new List<Card>
        {
            currencyExchange, gold, party
        };
        
        // Play a card containing a choice and leave the choice hanging.
        var chain = api.PlayCard(currencyExchange);
        foreach (var result in chain.Consume())
        {
            if (result is BaseChoice)
            {
                break;
            }
        }

        var possibleMoves = api.GetListOfPossibleMoves();
        Assert.All(possibleMoves, move => Assert.True(move is BaseMakeChoiceMove));

        var expectedResult = new List<Move>
        {
            Move.MakeChoice(new List<Card> { }),
            Move.MakeChoice(new List<Card> { currencyExchange }),
            Move.MakeChoice(new List<Card> { gold }),
            Move.MakeChoice(new List<Card> { party }),
            Move.MakeChoice(new List<Card> { currencyExchange, gold }),
            Move.MakeChoice(new List<Card> { currencyExchange, party }),
            Move.MakeChoice(new List<Card> { gold, party }),
        };
        
        Assert.Equal(expectedResult, possibleMoves);
    }
}
