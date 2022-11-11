using TalesOfTribute;

namespace Tests.Board;

public class BoardManagerTests
{
    [Fact]
    void TestBoardManagerSetUp()
    {
        var board = new BoardManager(
                new[] { PatronId.ANSEI }
            );
        board.SetUpGame();

        Assert.Equal(PlayerEnum.PLAYER1, board.CurrentPlayer);
        Assert.Equal(1, board.Players[(int)PlayerEnum.PLAYER2].CoinsAmount);

        Assert.Contains(
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            board.Players[(int)PlayerEnum.PLAYER2].DrawPile
        );

        Assert.Equal(6, board.Players[(int)PlayerEnum.PLAYER2].DrawPile.Count(card => card.Id == CardId.GOLD));

        Assert.NotEqual(
            board.Players[(int)PlayerEnum.PLAYER1].DrawPile,
            board.Players[(int)PlayerEnum.PLAYER2].DrawPile
        );
    }
    
    [Fact]
    void TestBasicExecutionChainInteraction()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
        sut.Players[0].Hand.Add(GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST));
        var chain = sut.PlayCard(CardId.CONQUEST);
        var flag = 0;

        foreach (var result in chain.Consume())
        {
            flag += 1;
            Assert.True(result is Choice<EffectType>);
            
            var choice = result as Choice<EffectType>;
            Assert.Contains(EffectType.GAIN_POWER, choice.Choices);
            Assert.Contains(EffectType.ACQUIRE_TAVERN, choice.Choices);
            var newResult = choice.Commit(EffectType.GAIN_POWER);
            Assert.True(newResult is Success);
        }
        
        // Loop only executed once.
        Assert.Equal(1, flag);
    }
}
