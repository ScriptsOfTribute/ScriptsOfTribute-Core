using TalesOfTribute;

namespace Tests.utils;

public class BoardManagerTests
{
    [Fact]
    void ShouldMakeChoiceCorrectly()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
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