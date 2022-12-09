using Moq;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace Tests.AI;

public class TalesOfTributeGameTests
{
    private readonly Mock<ITalesOfTributeApi> _api = new();
    private readonly Mock<TalesOfTribute.AI.AI> _player1 = new();
    private readonly Mock<TalesOfTribute.AI.AI> _player2 = new();

    [Fact]
    void ShouldEndAfterAnInvalidMoveDuringStartOfTurnChoices()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);

        var dummyChoice = new Choice<Card>(new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_KINSMAN) },
            _ => new Failure(""));
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => dummyChoice);

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns(dummyExecutionChain);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);

        _player1.Setup(player => player.HandleStartOfTurnChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN) });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidMove()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(false);

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PATRON, 3));

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidCardChoice()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);

        var dummyChoice = new Choice<Card>(new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_KINSMAN) },
            _ => new Failure(""));
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => dummyChoice);

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3));
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN) });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidEffectChoice()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);

        var dummyChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => new Failure(""));
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => dummyChoice);

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3));
        _player1.Setup(player => player.HandleEffectChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        ).Returns(new List<EffectType> { EffectType.HEAL });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidAttackTargetChoice()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.AttackAgent(It.IsAny<int>())).Returns(new Failure(""));

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.ATTACK, 3));

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidBuyCardChoice()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => new Failure(""));

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.BuyCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.BUY_CARD, 3));

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldEndAfterAnInvalidChoiceAfterBuyCard()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => new Failure(""));

        _api.Setup(api => api.CheckWinner()).Returns<EndGameState?>(null);
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.BuyCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.Setup(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.BUY_CARD, 3));
        _player1.Setup(player => player.HandleEffectChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        ).Returns(new List<EffectType> { EffectType.HEAL });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }

    [Fact]
    void ShouldPassSingleComplexFlowWithChoicesCorrectly()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var dummyChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => dummyChoice);

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns(dummyExecutionChain);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.SetupSequence(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3))
            .Returns(new Move(CommandEnum.END_TURN, 3));
        _player1.Setup(player => player.HandleStartOfTurnChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
        Assert.Equal(GameEndReason.PATRON_FAVOR, result.Reason);
    }
    
    [Fact]
    void ShouldPassSingleComplexFlowWithChoiceReturningChoice()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var effectChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var cardChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => effectChoice);
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => cardChoice);

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.SetupSequence(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3))
            .Returns(new Move(CommandEnum.END_TURN, 3));
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });
        _player1.Setup(player => player.HandleEffectChoice(
                It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        )
        .Returns(new List<EffectType> { EffectType.DRAW });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
        Assert.Equal(GameEndReason.PATRON_FAVOR, result.Reason);
    }
    
    [Fact]
    void ChoiceReturningChoiceShouldFailStartOfTurnHandler()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var effectChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var cardChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => effectChoice);
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => cardChoice);

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns(dummyExecutionChain);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);

        _player1.Setup(player => player.HandleStartOfTurnChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });

        Assert.Throws<Exception>(() => sut.Play());
    }
    
    [Fact]
    void ShouldFailSingleComplexFlowWithChoiceReturningChoiceIfSecondChoiceFails()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var effectChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var cardChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => effectChoice);
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => cardChoice);

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.SetupSequence(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3))
            .Returns(new Move(CommandEnum.END_TURN, 3));
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });
        _player1.Setup(player => player.HandleEffectChoice(
                It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        )
        .Returns(new List<EffectType> { EffectType.HEAL });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
    
    [Fact]
    void ShouldPassSingleComplexFlowWithChoiceChainAndSuccess()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var effectChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var cardChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => new Success());
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => cardChoice);
        dummyExecutionChain.Add((_, _, _) => effectChoice);
        dummyExecutionChain.Add((_, _, _) => new Success());

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.SetupSequence(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3))
            .Returns(new Move(CommandEnum.END_TURN, 3));
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });
        _player1.Setup(player => player.HandleEffectChoice(
                It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        )
        .Returns(new List<EffectType> { EffectType.DRAW });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
        Assert.Equal(GameEndReason.PATRON_FAVOR, result.Reason);
    }
    
        [Fact]
    void ShouldFailSingleComplexFlowWithChoiceChainWhereSecondChoiceFails()
    {
        var sut = new TalesOfTributeGame(new []{ _player1.Object, _player2.Object }, _api.Object);
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        var effectChoice = new Choice<Card>(new List<Card> { oathman },
            _ => new Success());
        var cardChoice = new Choice<EffectType>(new List<EffectType> { EffectType.DRAW },
            _ => new Success());
        var dummyExecutionChain = new ExecutionChain(null!, null!, null!);
        dummyExecutionChain.Add((_, _, _) => cardChoice);
        dummyExecutionChain.Add((_, _, _) => effectChoice);
        dummyExecutionChain.Add((_, _, _) => new Success());

        EndGameState? nullEndGameState = null;
        _api.SetupSequence(api => api.CheckWinner())
            .Returns(nullEndGameState)
            .Returns(new EndGameState(PlayerEnum.PLAYER1, GameEndReason.PATRON_FAVOR));
        _api.Setup(api => api.HandleStartOfTurnChoices()).Returns<ExecutionChain?>(null);
        _api.Setup(api => api.GetSerializer()).Returns<SerializedBoard>(null!);
        _api.Setup(api => api.EnemyPlayerId).Returns(PlayerEnum.PLAYER2);
        _api.Setup(api => api.IsMoveLegal(It.IsAny<Move>())).Returns(true);
        _api.Setup(api => api.PlayCard(It.IsAny<int>())).Returns(dummyExecutionChain);

        _player1.SetupSequence(player => player.Play(It.IsAny<SerializedBoard>(), It.IsAny<List<Move>>()))
            .Returns(new Move(CommandEnum.PLAY_CARD, 3))
            .Returns(new Move(CommandEnum.END_TURN, 3));
        _player1.Setup(player => player.HandleCardChoice(
            It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<Card>>())
        ).Returns(new List<Card> { oathman });
        _player1.Setup(player => player.HandleEffectChoice(
                It.IsAny<SerializedBoard>(), It.IsAny<SerializedChoice<EffectType>>())
        )
        .Returns(new List<EffectType> { EffectType.HEAL });

        var result = sut.Play();
        Assert.Equal(PlayerEnum.PLAYER2, result.Winner);
        Assert.Equal(GameEndReason.INCORRECT_MOVE, result.Reason);
    }
}
