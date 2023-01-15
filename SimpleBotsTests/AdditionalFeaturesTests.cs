using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace SimpleBotsTests;

public class AdditionalFeaturesTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AdditionalFeaturesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void HeuristicBotWithEvolutionsTests()
    {
        AdjustParametersByEvolution ewo = new AdjustParametersByEvolution();
        int[] genotype = ewo.Evolution(100, 1000, 2);
        
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new HeuristicBot();
            //bot1.SetGenotype(genotype);
            var bot2 = new RandomBotWithRandomStateExploring();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
        Console.WriteLine("Ewolucja");
        Console.WriteLine(counter.ToString());
    }
    //add Apriori tests   
}