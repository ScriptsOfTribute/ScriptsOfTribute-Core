using TalesOfTribute;

namespace Tests.utils;

public class BoardManagerTests
{
    [Fact]
    void ShouldMakeChoiceCorrectly()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
        var result = sut.PlayCard(CardId.CONQUEST);
        Assert.True(result is Choice<EffectType>);

        var choice = result as Choice<EffectType>;
        Assert.Contains(EffectType.GAIN_POWER, choice.Choices);
        Assert.Contains(EffectType.ACQUIRE_TAVERN, choice.Choices);

        Assert.Equal(0, sut.players[0].PowerAmount);
        result = choice.Commit(EffectType.GAIN_POWER);
        Assert.True(result is Success);
        
        Assert.Equal(3, sut.players[0].PowerAmount);
    }
}