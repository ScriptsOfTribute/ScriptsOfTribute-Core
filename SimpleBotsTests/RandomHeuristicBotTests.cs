using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;
using System.Text;

namespace SimpleBotsTests;

public class RandomHeuristicBotTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private StringBuilder log = new StringBuilder();

    public RandomHeuristicBotTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void RandomHeuristicGameShouldEndWithoutErrors()
    {
        //AdjustParametersByEvolution ewo = new AdjustParametersByEvolution();
        //int[] genotype = ewo.Evolution(100, 1000, 1, -100, 100);
        //int[] genotype = new int[]{17,77,77,18,87,22,15,25,99,4,23,34,19};
        const int testAmount = 100;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new NewMCTSBot();
            //bot1.SetGenotype(genotype);
            var bot2 = new RandomMaximizePrestigeBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, endBoardState) = game.Play();
            //Console.WriteLine(endState.Reason.ToString());
            //Console.WriteLine(endState.Winner.ToString());
            
            if (endState.Reason == GameEndReason.INCORRECT_MOVE || endState.Reason == GameEndReason.INTERNAL_ERROR || endState.Reason == GameEndReason.BOT_EXCEPTION)
            {
                Console.WriteLine(endState.AdditionalContext);
                //Console.WriteLine(endBoardState.)
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);

            counter.Add(endState);

            //_testOutputHelper.WriteLine(string.Join('\n', endBoardState.CompletedActions.Select(a => a.ToString())));
        }
        Console.WriteLine(counter.ToString());
        //Console.WriteLine("[{0}]", string.Join(", ", genotype));
        //ClashEveryBotTogether clash = new ClashEveryBotTogether();
        //clash.BotClash();
        //log.Append(counter.ToString() + System.Environment.NewLine);
        //File.AppendAllText("log_with_results.txt", log.ToString());
        //log.Clear();
        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    /*
    [Fact]
    public void MaxPrestigeVSHeuristicRandomTest()
    {
        //ClashEveryBotTogether cl = new ClashEveryBotTogether();
        //cl.BotClash();
        AdjustParametersByEvolution ewo = new AdjustParametersByEvolution();
        int[] genotype = ewo.Evolution(100, 1000, 2);
        
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SemiRandomBot();
            bot1.SetGenotype(genotype);
            var bot2 = new RandomMaximizePrestigeBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();
            if (endState.Reason == GameEndReason.INCORRECT_MOVE || endState.Reason == GameEndReason.INTERNAL_ERROR)
            {
                Console.WriteLine(endState.Winner);
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);

            counter.Add(endState);
        }
        Console.WriteLine(counter.ToString());
        log.Append(counter.ToString() + System.Environment.NewLine);
        File.AppendAllText("log_with_results.txt", log.ToString());
        log.Clear();
        _testOutputHelper.WriteLine(counter.ToString());
        
    }
    
    
}
*/
}