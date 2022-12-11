using Moq;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace Tests.AI;

public class RandomBotTests
{
    private readonly Mock<ITalesOfTributeApi> _api = new();
    private readonly Mock<TalesOfTribute.AI.RandomBot> _player1 = new();
    private readonly Mock<TalesOfTribute.AI.RandomBot> _player2 = new();
    private BoardManager _boardManager = new BoardManager(new[] { PatronId.PELIN, PatronId.TREASURY });
    private RandomBot r1 = new RandomBot();
    private RandomBot r2 = new RandomBot();

    /*
    [Fact]
    void ShouldEndAfterAnInvalidMoveDuringStartOfTurnChoices()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        #EndGameState state = sut.Play();
        _player1
        Console.WriteLine(state.Winner);
        Console.WriteLine(state.Reason);
        Console.WriteLine(state.AdditionalContext);
    }
    */
    [Fact]
    void ShouldEndAfterAnInvalidMoveDuringStartOfTurnChoices()
    {
        _boardManager.SetUpGame();
        TalesOfTributeApi api = new TalesOfTributeApi(_boardManager);

        //SerializedChoice<Card> xxx = SerializedChoice<Card>.FromChoice(dummyChoice);

        var sut = new TalesOfTributeGame(new []{ r1, r2}, api);
        var result = sut.Play();
        Console.WriteLine(result.Winner);
        Console.WriteLine(result.Reason);
        Console.WriteLine(result.AdditionalContext);
/*
        _player1.Setup(player => player.HandleStartOfTurnChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN) });

        var result = sut.Play();
        Console.WriteLine(result.Winner);
        Console.WriteLine(result.Reason);
        Console.WriteLine(result.AdditionalContext);
        */
    }
}