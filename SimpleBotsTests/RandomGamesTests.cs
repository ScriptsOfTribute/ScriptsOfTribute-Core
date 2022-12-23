using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace SimpleBotsTests;

public class RandomGamesTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RandomGamesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RandomGameShouldEndWithoutErrors()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.RandomBot();
            var bot2 = new SimpleBots.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void RandomBotWithRandomStateExploringTests()
    {
        const int testAmount = 500;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomBotWithRandomStateExploring();
            var bot2 = new RandomBotWithRandomStateExploring();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void MaxPrestigeTest()
    {
        const int testAmount = 500;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomMaximizePrestigeBot();
            var bot2 = new RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }

    [Fact]
    void Test()
    {
        var br = new BoardManager(new[] { PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU });
        var ritual = GlobalCardDatabase.Instance.GetCard(CardId.BRIARHEART_RITUAL);
        br.Tavern.AvailableCards.Add(ritual);
        var knightCommander = GlobalCardDatabase.Instance.GetCard(CardId.KNIGHT_COMMANDER);
        br.CurrentPlayer.Hand.Add(knightCommander);
        var knightsOfSaintPellin = GlobalCardDatabase.Instance.GetCard(CardId.KNIGHTS_OF_SAINT_PELIN);
        br.CurrentPlayer.Hand.Add(knightsOfSaintPellin);
        
        br.PlayCard(knightsOfSaintPellin);

        br.EndTurn();
        br.EndTurn();

        br.CurrentPlayer.CoinsAmount = 1000;
        br.PlayCard(knightCommander);
        br.BuyCard(ritual);
        br.CardActionManager.MakeChoice(new List<Card> { knightCommander });
        br.ActivateAgent(knightsOfSaintPellin);
    }
}
