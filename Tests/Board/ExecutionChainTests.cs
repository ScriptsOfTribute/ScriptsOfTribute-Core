using Moq;
using TalesOfTribute;

namespace Tests.Board;

public class ExecutionChainTests
{

    private ExecutionChain _sut;

    [Fact]
    void ShouldCorrectlyFinishSimpleFlow()
    {
        var _player1 = new Mock<Player>();
        var _player2 = new Mock<Player>();
        var _tavern = new Mock<Tavern>();
        
        _sut = new ExecutionChain(_player1.Object, _player2.Object, _tavern.Object);
    }
}