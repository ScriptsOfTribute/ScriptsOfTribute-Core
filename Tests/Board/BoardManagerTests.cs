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
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC, PatronId.TREASURY, PatronId.DUKE_OF_CROWS, PatronId.ORGNUM });
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
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC, PatronId.TREASURY, PatronId.DUKE_OF_CROWS, PatronId.ORGNUM });
        sut.SetUpGame();

        var card = sut.GetAvailableTavernCards().First();
        Assert.Throws<Exception>(() => sut.BuyCard(card));
    }

    [Fact]
    void TestPlayerDraw()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI, PatronId.PSIJIC, PatronId.TREASURY, PatronId.HLAALU, PatronId.ORGNUM });

        Assert.Empty(sut.CurrentPlayer.Hand);

        sut.SetUpGame();

        Assert.Equal(5, sut.CurrentPlayer.Hand.Count);
        Assert.Equal(5, sut.CurrentPlayer.DrawPile.Count);
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

    [Fact]
    void TestFullComboPlay()
    {
        var sut = new BoardManager(new[] { PatronId.DUKE_OF_CROWS });
        var drawpile = new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.BLACKFEATHER_KNIGHT) };
        var hand = new List<Card>()
        {
            GlobalCardDatabase.Instance.GetCard(CardId.MURDER_OF_CROWS),
            GlobalCardDatabase.Instance.GetCard(CardId.SCRATCH),
            GlobalCardDatabase.Instance.GetCard(CardId.PECK),
            GlobalCardDatabase.Instance.GetCard(CardId.POOL_OF_SHADOW),
        };
        Assert.Equal(0, sut.CurrentPlayer.PowerAmount);
        Assert.Equal(0, sut.CurrentPlayer.PrestigeAmount);
        Assert.Equal(0, sut.CurrentPlayer.CoinsAmount);

        sut.CurrentPlayer.Hand = hand;
        sut.CurrentPlayer.DrawPile = drawpile;

        var chain = sut.PlayCard(hand.First(c => c.Id == CardId.MURDER_OF_CROWS));
        var counter = 0;
        foreach (var result in chain.Consume())
        {
            switch (counter)
            {
                // Should be coin gain
                case 0:
                    {
                        Assert.Equal(1, sut.CurrentPlayer.CoinsAmount);
                        Assert.True(result is Success);
                        break;
                    }
            }

            counter += 1;
        }
        Assert.Equal(1, counter);
        Assert.Equal(0, sut.CurrentPlayer.PowerAmount);
        Assert.Equal(0, sut.CurrentPlayer.PrestigeAmount);
        Assert.Equal(1, sut.CurrentPlayer.CoinsAmount);

        chain = sut.PlayCard(hand.First(c => c.Id == CardId.SCRATCH));
        counter = 0;
        foreach (var result in chain.Consume())
        {

            Assert.True(result is Success);
            counter += 1;
        }

        Assert.Equal(4, sut.CurrentPlayer.PowerAmount);
        Assert.Equal(0, sut.CurrentPlayer.PrestigeAmount);
        Assert.Equal(6, sut.CurrentPlayer.CoinsAmount);
        Assert.Equal(5, counter); // Scratch Activation, Scratch Combo2 has 2 effect, Murder of crows combo2 has 2 effects

        chain = sut.PlayCard(hand.First(c => c.Id == CardId.PECK));
        counter = 0;
        foreach (var result in chain.Consume())
        {

            Assert.True(result is Success);
            counter += 1;
        }

        Assert.Equal(6, sut.CurrentPlayer.PowerAmount);
        Assert.Equal(0, sut.CurrentPlayer.PrestigeAmount);
        Assert.Equal(7, sut.CurrentPlayer.CoinsAmount);
        Assert.Equal(2, counter); // Peck activ, Murder of Crows Combo3

        Assert.Single(sut.CurrentPlayer.Hand);

        chain = sut.PlayCard(hand.First(c => c.Id == CardId.POOL_OF_SHADOW));
        counter = 0;
        foreach (var result in chain.Consume())
        {

            Assert.True(result is Success);
            counter += 1;
        }

        Assert.Equal(7, sut.CurrentPlayer.PowerAmount);
        Assert.Equal(0, sut.CurrentPlayer.PrestigeAmount);
        Assert.Equal(10, sut.CurrentPlayer.CoinsAmount);
        Assert.Equal(3, counter); // PoolOfShadow activ, combo2 & combo4
        Assert.Single(sut.CurrentPlayer.Hand); // PoolOfShadow has Draw on combo2
        Assert.Contains(CardId.BLACKFEATHER_KNIGHT, sut.CurrentPlayer.Hand.Select(c => c.Id).ToList());
    }
}
