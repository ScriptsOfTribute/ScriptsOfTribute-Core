using TalesOfTribute;
using Xunit.Abstractions;

namespace Tests.Board;

public class BoardManagerTests
{
    private readonly ITestOutputHelper output;

    public BoardManagerTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    void TestBoardManagerSetUp()
    {
        var board = new BoardManager(
                new[] { PatronId.ANSEI, PatronId.TREASURY }
            );
        board.SetUpGame();

        Assert.Equal(PlayerEnum.PLAYER1, board.CurrentPlayer.ID);
        Assert.Equal(1, board.EnemyPlayer.CoinsAmount);

        Assert.Contains(
            CardId.GOLD,
            board.EnemyPlayer.DrawPile.Select(card => card.Id)
        );

        Assert.Equal(6, board.EnemyPlayer.DrawPile.Count(card => card.Id == CardId.GOLD));

    }

    [Fact]
    void TestBasicExecutionChainInteraction()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
        var conquest = GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST);
        sut.CurrentPlayer.Hand.Add(conquest);
        var chain = sut.PlayCard(conquest);
        var flag = 0;

        foreach (var result in chain.Consume())
        {
            flag += 1;
            Assert.True(result is Choice<EffectType>);

            var choice = result as Choice<EffectType>;
            Assert.Contains(EffectType.GAIN_POWER, choice.PossibleChoices);
            Assert.Contains(EffectType.ACQUIRE_TAVERN, choice.PossibleChoices);
            var newResult = choice.Choose(EffectType.GAIN_POWER);
            Assert.True(newResult is Success);
        }

        // Loop only executed once.
        Assert.Equal(1, flag);
    }

    [Fact]
    void TestBuyCard()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.SetUpGame();

        sut.CurrentPlayer.CoinsAmount = 100;

        var card = sut.GetAvailableTavernCards().First(c => c.Type == CardType.AGENT || c.Type == CardType.ACTION);
        sut.BuyCard(card);

        Assert.Contains(
            card.Id,
            sut.CurrentPlayer.CooldownPile.Select(card => card.Id)
        );

        Assert.DoesNotContain(
            card.Id,
            sut.GetAvailableTavernCards().Select(card => card.Id)
        );

        Assert.Equal(
            5,
            sut.GetAvailableTavernCards().Count
        );
    }

    [Fact]
    void ShouldThrowWhenNotEnoughCoinsForCard()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.SetUpGame();

        var card = sut.GetAvailableTavernCards().First();
        Assert.Throws<Exception>(() => sut.BuyCard(card));
    }

    [Fact]
    void TestPlayerDraw()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC, PatronId.TREASURY });
        sut.SetUpGame();

        Assert.Empty(sut.CurrentPlayer.Hand);
        var cardsAmount = sut.CurrentPlayer.DrawPile.Count;

        sut.DrawCards();

        Assert.Equal(5, sut.CurrentPlayer.Hand.Count);
        Assert.Equal(cardsAmount - 5, sut.CurrentPlayer.DrawPile.Count);
    }

    [Fact]
    void TestPlayerDrawWhenNotEnoughCards()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.CurrentPlayer.DrawPile.AddRange(new List<Card>() {
            GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST),
            GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET)
        });

        sut.CurrentPlayer.CooldownPile.AddRange(new List<Card>() {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
            GlobalCardDatabase.Instance.GetCard(CardId.PSIJIC_APPRENTICE),
            GlobalCardDatabase.Instance.GetCard(CardId.TIME_MASTERY),
            GlobalCardDatabase.Instance.GetCard(CardId.CEPORAHS_INSIGHT),
        });
        Assert.Empty(sut.CurrentPlayer.Hand);
        Assert.Equal(2, sut.CurrentPlayer.DrawPile.Count);
        Assert.Equal(4, sut.CurrentPlayer.CooldownPile.Count);
        sut.DrawCards();
        Assert.Equal(5, sut.CurrentPlayer.Hand.Count);
        Assert.Single(sut.CurrentPlayer.DrawPile);
        Assert.Empty(sut.CurrentPlayer.CooldownPile);
    }
}
