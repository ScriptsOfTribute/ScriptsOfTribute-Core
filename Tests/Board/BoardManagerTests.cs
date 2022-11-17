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
                new[] { PatronId.ANSEI }
            );
        board.SetUpGame();

        Assert.Equal(PlayerEnum.PLAYER1, board.CurrentPlayerId);
        Assert.Equal(1, board.Players[(int)PlayerEnum.PLAYER2].CoinsAmount);

        Assert.Contains(
            CardId.GOLD,
            board.Players[(int)PlayerEnum.PLAYER2].DrawPile.Select(card => card.Id)
        );

        Assert.Equal(6, board.Players[(int)PlayerEnum.PLAYER2].DrawPile.Count(card => card.Id == CardId.GOLD));

    }

    [Fact]
    void TestBasicExecutionChainInteraction()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
        var conquest = GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST);
        sut.Players[0].Hand.Add(conquest);
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

        var card = sut.Tavern.AvailableCards.First();
        sut.BuyCard(card);

        Assert.Contains(
            card.Id,
            sut.Players[(int)sut.CurrentPlayerId].CooldownPile.Select(card => card.Id)
        );

        Assert.DoesNotContain(
            card.Id,
            sut.Tavern.AvailableCards.Select(card => card.Id)
        );

        Assert.Equal(
            5,
            sut.Tavern.AvailableCards.Count
        );
    }
    
    [Fact]
    void ShouldThrowWhenNotEnoughCoinsForCard()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.SetUpGame();

        var card = sut.Tavern.AvailableCards.First();
        Assert.Throws<Exception>(() => sut.BuyCard(card));
    }

    [Fact]
    void TestPlayerDraw()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.SetUpGame();

        Assert.Empty(sut.Players[0].Hand);
        var cardsAmount = sut.Players[0].DrawPile.Count;

        sut.DrawCards();

        Assert.Equal(5, sut.Players[0].Hand.Count);
        Assert.Equal(cardsAmount - 5, sut.Players[0].DrawPile.Count);
    }

    [Fact]
    void TestPlayerDrawWhenNotEnoughCards()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC });
        sut.Players[0].DrawPile.AddRange(new List<Card>() {
            GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST),
            GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET)
        });

        sut.Players[0].CooldownPile.AddRange(new List<Card>() {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
            GlobalCardDatabase.Instance.GetCard(CardId.PSIJIC_APPRENTICE),
            GlobalCardDatabase.Instance.GetCard(CardId.TIME_MASTERY),
            GlobalCardDatabase.Instance.GetCard(CardId.CEPORAHS_INSIGHT),
        });
        Assert.Empty(sut.Players[0].Hand);
        Assert.Equal(2, sut.Players[0].DrawPile.Count);
        Assert.Equal(4, sut.Players[0].CooldownPile.Count);
        sut.DrawCards();
        Assert.Equal(5, sut.Players[0].Hand.Count);
        Assert.Single(sut.Players[0].DrawPile);
        Assert.Empty(sut.Players[0].CooldownPile);
    }
}
