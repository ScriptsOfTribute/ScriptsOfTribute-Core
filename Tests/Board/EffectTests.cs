using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;
using TalesOfTribute;

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

        _player2.VerifySet(p => p.PrestigeAmount = -1);
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
        Assert.True(result is Choice<CardId>);
        var choice = result as Choice<CardId>;
        Assert.All(choice.PossibleChoices,
            card => Assert.Contains(card, cardsToChooseFrom.Select(card => card.Id)));

        var newResult = choice.Choose(cardsToChooseFrom.Select(card => card.Id).ToList());
        Assert.True(newResult is Success);
        
        _tavernMock.Verify(t => t.ReplaceCard(cardsToChooseFrom[0].Id), Times.Once);
        _tavernMock.Verify(t => t.ReplaceCard(cardsToChooseFrom[1].Id), Times.Once);
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

        var cardIdsReturned = cardsToReturn.Select(card => card.Id);

        _player1.Setup(p => p.DrawPile).Returns(cardsToReturn);

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        
        Assert.True(result is Choice<CardId>);
        
        var choice = result as Choice<CardId>;
        Assert.All(cardIdsReturned, cardId => Assert.Contains(cardId, choice.PossibleChoices));

        var newResult = choice.Choose(cardIdsReturned.ToList());
        Assert.True(newResult is Success);
        
        _player1.Verify(p => p.Toss(It.IsAny<CardId>()), Times.Exactly(2));
    }
    
    [Fact]
    void TestKnockout()
    {
        var effect = new Effect(EffectType.KNOCKOUT, 2);

        var cardsToReturn = new List<Card>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.PECK),
        };

        var cardIdsReturned = cardsToReturn.Select(card => card.Id);

        _player2.Setup(p => p.Agents).Returns(cardsToReturn);

        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        
        Assert.True(result is Choice<CardId>);
        
        var choice = result as Choice<CardId>;
        Assert.All(cardIdsReturned, cardId => Assert.Contains(cardId, choice.PossibleChoices));

        var newResult = choice.Choose(cardIdsReturned.ToList());
        Assert.True(newResult is Success);
        
        _player2.Verify(p => p.KnockOut(It.IsAny<CardId>()), Times.Exactly(2));
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

        _player1.Verify(p => p.AddToCooldownPile(It.Is<Card>(card => card.Id == CardId.MAORMER_BOARDING_PARTY)),
            Times.Exactly(2));
    }
    
    [Fact]
    void TestHeal()
    {
        var guid = Guid.NewGuid();
        var effect = new Effect(EffectType.HEAL, 2, guid);
        var result = effect.Enact(_player1.Object, _player2.Object, _tavernMock.Object);
        Assert.True(result is Success);

        _player1.Verify(p => p.HealAgent(guid, 2),
            Times.Once());
    }
}
