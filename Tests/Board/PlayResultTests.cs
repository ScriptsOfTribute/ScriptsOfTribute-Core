using TalesOfTribute;

namespace Tests.Board;

public class PlayResultTests
{
    [Fact]
    void ShouldNotAllowIncorrectChoice()
    {
        var sut = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => new Success());

        var result = sut.Choose(EffectType.HEAL);
        Assert.True(result is Failure);
    }

    [Fact]
    void ShouldNotAllowTooManyChoices()
    {
        var sut = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW, EffectType.HEAL, EffectType.TOSS }, 1,
            _ => new Success());

        var result = sut.Choose(new List<EffectType> { EffectType.HEAL, EffectType.TOSS });
        Assert.True(result is Failure);
    }

    [Fact]
    void ShouldNotAllowIncorrectChoices()
    {
        var sut = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW, EffectType.HEAL, EffectType.TOSS }, 2,
            _ => new Success());

        var result = sut.Choose(new List<EffectType> { EffectType.HEAL, EffectType.DESTROY_CARD });
        Assert.True(result is Failure);
    }

    [Fact]
    void ShouldPassCorrectSingleChoice()
    {
        var sut = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => new Success());

        var result = sut.Choose(EffectType.DRAW);
        Assert.True(result is Success);
    }

    [Fact]
    void ShouldPassCorrectMultipleChoice()
    {
        var sut = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW, EffectType.HEAL, EffectType.TOSS }, 2,
            _ => new Success());

        var result = sut.Choose(new List<EffectType> { EffectType.HEAL, EffectType.TOSS });
        Assert.True(result is Success);
    }
}
