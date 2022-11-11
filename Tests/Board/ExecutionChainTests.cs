using Moq;
using TalesOfTribute;

namespace Tests.Board;

public class ExecutionChainTests
{
    private ExecutionChain _sut;

    Player _player1 = new Player(PlayerEnum.PLAYER1);
    Player _player2 = new Player(PlayerEnum.PLAYER2);
    Tavern _tavern = new Tavern(GlobalCardDatabase.Instance.AllCards);

    [Fact]
    void ShouldNotAllowUnfinishedChoiceToPass()
    {
        _sut = new ExecutionChain(_player1, _player2, _tavern);

        // First - OR choice and immediately after ACQUIRE choice
        var effect1 = new EffectChoice(new Effect(EffectType.GAIN_POWER, 1),
            new Effect(EffectType.ACQUIRE_TAVERN, 5));
        
        _sut.Add(effect1.Decompose().First().Enact);
        _sut.Add(new Effect(EffectType.GAIN_POWER, 1).Enact);

        var consume = _sut.Consume().GetEnumerator();
        consume.MoveNext();
        var result = consume.Current;

        // Advance from OR choice to ACQUIRE choice.
        Assert.True(result is Choice<EffectType>);
        var choice = result as Choice<EffectType>;
        var newResult = choice.Commit(EffectType.ACQUIRE_TAVERN);
        Assert.True(newResult is Choice<CardId>);
        Assert.Throws<Exception>(() => consume.MoveNext());
    }
    
    [Fact]
    void ShouldAllowFinishedChoiceToPass()
    {
        _sut = new ExecutionChain(_player1, _player2, _tavern);

        // First - OR choice and immediately after ACQUIRE choice
        var effect1 = new EffectChoice(new Effect(EffectType.GAIN_POWER, 1),
            new Effect(EffectType.ACQUIRE_TAVERN, 5));
        
        _sut.Add(effect1.Decompose().First().Enact);
        _sut.Add(new Effect(EffectType.GAIN_POWER, 1).Enact);

        var consume = _sut.Consume().GetEnumerator();
        consume.MoveNext();
        var result = consume.Current;

        // Advance from OR choice to GAIN_POWER this time, so no further choice is needed.
        Assert.True(result is Choice<EffectType>);
        var choice = result as Choice<EffectType>;
        var newResult = choice.Commit(EffectType.GAIN_POWER);
        Assert.True(newResult is Success);
        consume.MoveNext();
    }
    
    [Fact]
    void ShouldCorrectlyFinishComplexFlow()
    {
        // SETUP
        _sut = new ExecutionChain(_player1, _player2, _tavern);

        // First - OR choice and immediately ACQUIRE choice
        var effect1 = new EffectChoice(new Effect(EffectType.GAIN_POWER, 1),
            new Effect(EffectType.ACQUIRE_TAVERN, 5));

        // Second - Gain Power
        // Third - ACQUIRE choice
        var effect2 = new EffectComposite(new Effect(EffectType.GAIN_POWER, 1),
            new Effect(EffectType.ACQUIRE_TAVERN, 5));

        var finalChain = new List<BaseEffect>();
        finalChain.AddRange(effect1.Decompose());
        finalChain.AddRange(effect2.Decompose());

        foreach (var effect in finalChain)
        {
            _sut.Add(effect.Enact);
        }

        // TEST
        Assert.Equal(0, _player1.PowerAmount);
        var counter = 0;
        foreach (var result in _sut.Consume())
        {
            switch (counter)
            {
                // Should be OR choice
                case 0:
                {
                    Assert.True(result is Choice<EffectType>);
                    var choice = result as Choice<EffectType>;
                    var newResult = choice.Commit(EffectType.ACQUIRE_TAVERN);
                    Assert.True(newResult is Choice<CardId>);
                    var newChoice = newResult as Choice<CardId>;
                    Assert.True(newChoice.Commit(CardId.GOLD) is Success);
                    break;
                }
                // Should be power gain
                case 1:
                {
                    Assert.Equal(1, _player1.PowerAmount);
                    Assert.True(result is Success);
                    break;
                }
                // Should be standalone ACQUIRE choice
                case 2:
                {
                    Assert.True(result is Choice<CardId>);
                    var choice = result as Choice<CardId>;
                    var newResult = choice.Commit(CardId.GOLD);
                    Assert.True(newResult is Success);
                    break;
                }
            }

            counter += 1;
        }
    }
}