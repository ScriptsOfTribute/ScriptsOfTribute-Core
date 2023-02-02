using Moq;
using ScriptsOfTribute;

namespace Tests.Board;

public class EffectTests
{
    Mock<IPlayer> _player1 = new Mock<IPlayer>();
    Mock<IPlayer> _player2 = new Mock<IPlayer>();
    private Mock<ITavern> _tavernMock = new Mock<ITavern>();

    [Fact]
    void TestGainPower()
    {
        var powerEffect = new Effect(EffectType.GAIN_POWER, 3);
        var result = powerEffect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.VerifySet(p => p.PowerAmount = 3);
    }

    [Fact]
    void TestGainCoin()
    {
        var effect = new Effect(EffectType.GAIN_COIN, 1);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.VerifySet(p => p.CoinsAmount = 1);
    }

    [Fact]
    void TestGainPrestige()
    {
        var effect = new Effect(EffectType.GAIN_PRESTIGE, 2);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.VerifySet(p => p.PrestigeAmount = 2);
    }

    [Fact]
    void TestOpponentLosePrestige()
    {
        var effect = new Effect(EffectType.OPP_LOSE_PRESTIGE, 1);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player2.VerifySet(p => p.PrestigeAmount = 0);
    }

    [Fact]
    void TestReplaceTavern()
    {
        // Replace 2
        var effect = new Effect(EffectType.REPLACE_TAVERN, 2);

        var cardsToChooseFrom = new List<Card>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.IMPERIAL_SPOILS),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
        };

        _tavernMock.Setup(t => t.AvailableCards).Returns(cardsToChooseFrom);

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Choice<Card>);
        var choice = result as Choice<Card>;
        Assert.All(choice.PossibleChoices,
            card => Assert.Contains(card, cardsToChooseFrom));

        var newResult = choice.Choose(cardsToChooseFrom);
        Assert.True(newResult is Success);

        _tavernMock.Verify(t => t.ReplaceCard(cardsToChooseFrom[0]), Times.Once);
        _tavernMock.Verify(t => t.ReplaceCard(cardsToChooseFrom[1]), Times.Once);
    }

    [Fact]
    void TestDrawCard()
    {
        var effect = new Effect(EffectType.DRAW, 2);

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);

        Assert.True(result is Success);

        _player1.Verify(p => p.Draw(), Times.Exactly(2));
    }

    [Fact]
    void TestToss()
    {
        var effect = new Effect(EffectType.TOSS, 2);

        var cardsToReturn = new List<Card>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.PECK),
        };

        _player1.Setup(p => p.DrawPile).Returns(cardsToReturn);

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);

        Assert.True(result is Choice<Card>);

        var choice = result as Choice<Card>;
        Assert.All(cardsToReturn, card => Assert.Contains(card, choice.PossibleChoices));

        var newResult = choice.Choose(cardsToReturn);
        Assert.True(newResult is Success);

        _player1.Verify(p => p.Toss(It.IsAny<Card>()), Times.Exactly(2));
    }

    [Fact]
    void TestKnockout()
    {
        var effect = new Effect(EffectType.KNOCKOUT, 2);

        var cardsToReturn = new List<Card>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN),
            GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN),
        };

        _player2.Setup(p => p.AgentCards).Returns(cardsToReturn);
        _tavernMock.Setup(tav => tav.Cards).Returns(new List<Card>());

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);

        Assert.True(result is Choice<Card>);

        var choice = result as Choice<Card>;
        Assert.All(cardsToReturn, card => Assert.Contains(card, choice.PossibleChoices));

        var newResult = choice.Choose(cardsToReturn);
        Assert.True(newResult is Success);

        _player2.Verify(p => p.KnockOut(It.IsAny<Card>()), Times.Exactly(2));
    }

    [Fact]
    void TestPatronCall()
    {
        var effect = new Effect(EffectType.PATRON_CALL, 1);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.VerifySet(p => p.PatronCalls = 1);
    }

    [Fact]
    void TestCreateBoardingParty()
    {
        var effect = new Effect(EffectType.CREATE_BOARDINGPARTY, 2);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.Verify(p => p.AddToCooldownPile(It.Is<Card>(card => card.CommonId == CardId.MAORMER_BOARDING_PARTY)),
            Times.Exactly(2));
    }

    [Fact]
    void TestHeal()
    {
        var uniqueId = UniqueId.Create();
        var effect = new Effect(EffectType.HEAL, 2, uniqueId);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.Verify(p => p.HealAgent(uniqueId, 2),
            Times.Once());
    }

    [Fact]
    void TestDestroy()
    {
        var effect = new Effect(EffectType.DESTROY_CARD, 2);
        var cardInPlay1 = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var cardInPlay2 = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var cardInHand = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        _player1.Setup(p => p.AgentCards).Returns(
            new List<Card> { cardInPlay1, cardInPlay2 });
        _player1.Setup(p => p.Hand).Returns(new List<Card> { cardInHand });

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Choice<Card>);
        var choice = result as Choice<Card>;
        choice.Choose(new List<Card> { cardInPlay1, cardInHand });

        _player1.Verify(p => p.Destroy(It.Is<Card>(card => card == cardInPlay1)), Times.Once);
        _player1.Verify(p => p.Destroy(It.Is<Card>(card => card == cardInHand)), Times.Once);
    }

    [Fact]
    void TestOppDiscard()
    {
        var effect = new Effect(EffectType.OPP_DISCARD, 1);
        List<Effect> startOfTurnEffects = new();
        var cardInHand = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        _player2.Setup(p => p.Hand).Returns(new List<Card> { cardInHand });
        _player2.Setup(p => p.AddStartOfTurnEffect(It.IsAny<Effect>()))
            .Callback<Effect>(e => startOfTurnEffects.Add(e));

        var basicResult = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(basicResult is Success);

        Assert.Single(startOfTurnEffects);
        Assert.Equal(effect, startOfTurnEffects.First());
    }
}
