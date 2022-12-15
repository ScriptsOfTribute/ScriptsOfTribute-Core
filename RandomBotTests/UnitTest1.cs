using Xunit.Abstractions;

namespace RandomBotTests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RandomGameShouldEndWithoutErrors()
    {
        var bot1 = new RandomBot.RandomBot();
        var bot2 = new RandomBot.RandomBot();

        var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
        var endState = game.Play();
        _testOutputHelper.WriteLine(endState.ToString());
    }
}
