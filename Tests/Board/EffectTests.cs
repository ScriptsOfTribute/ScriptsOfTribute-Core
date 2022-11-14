using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using TalesOfTribute;

namespace Tests.Board;

public class EffectTests
{
    Player _player1 = new Player(PlayerEnum.PLAYER1);
    Player _player2 = new Player(PlayerEnum.PLAYER2);
    Tavern _tavern = new Tavern(GlobalCardDatabase.Instance.AllCards);
    
    [Fact]
    void TestGainPower()
    {
        var card = GlobalCardDatabase.Instance.GetCard(CardId.ARCHERS_VOLLEY);
        var powerEfect = card.Effects.First();
        var result = powerEfect.Decompose().First().Enact(_player1, _player2, _tavern);
        Assert.True(result is Success);
        
        Assert.Equal(3, _player1.PowerAmount);
    }
    
    [Fact]
    void TestGainCoin()
    {
        var card = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var effect = card.Effects.First();
        var result = effect.Decompose().First().Enact(_player1, _player2, _tavern);
        Assert.True(result is Success);
        
        Assert.Equal(1, _player1.CoinsAmount);
    }

    [Fact]
    void TestGainPrestige()
    {
        var card = GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY);
        var effect = card.Effects.First();
        var result = effect.Decompose().First().Enact(_player1, _player2, _tavern);
        Assert.True(result is Success);
        
        Assert.Equal(2, _player1.PrestigeAmount);
    }

    [Fact]
    void TestOpponentLosePrestige()
    {
        var card = GlobalCardDatabase.Instance.GetCard(CardId.JEERING_SHADOW);
        var effect = card.Effects[1];
        var result = effect.Decompose().First().Enact(_player1, _player2, _tavern);
        Assert.True(result is Success);
        
        Assert.Equal(-1, _player2.PrestigeAmount);
    }
    
    [Fact]
    void TestAcquireTavern()
    {
        // Acquire 6
        var card = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var effect = card.Effects[1];
        
        _tavern.AvailableCards.Add(GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN));
        _tavern.AvailableCards.Add(GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_COUNCILOR));
        var result = effect.Decompose().First().Enact(_player1, _player2, _tavern);
        Assert.True(result is Choice<CardId>);
        var choice = result as Choice<CardId>;
        Assert.Single(choice.PossibleChoices);

        Assert.Equal(-1, _player2.PrestigeAmount);
    }
}
