using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ModuleTests;

public class GameStateTests
{   
    private PatronId[] _patronsSetUp = {PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.TREASURY, PatronId.HLAALU, PatronId.DUKE_OF_CROWS};
    [Fact]
    void SeededGameStateShouldAllowToPlayAsEnemy()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyHand = new List<UniqueCard> { GlobalCardDatabase.Instance.GetCard(CardId.GOLD) };
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, enemyHand, new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new GameState(new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123));

        var (newState, possibleMoves) = board.ApplyMove(Move.EndTurn(), 123);
        (newState, possibleMoves) = newState.ApplyMove(Move.PlayCard(enemyHand[0]));
        Assert.Equal(1, newState.CurrentPlayer.Coins);
    }

    [Fact]
    void HandsShouldHaveDifferentUniqueCards()
    {
        ulong seed = 42;

        var api = new ScriptsOfTributeApi(_patronsSetUp, seed);

        var currentHand = api.GetPlayer(PlayerEnum.PLAYER1).Hand;
        var enemyHand = api.GetPlayer(PlayerEnum.PLAYER2).Hand;

        var currentHandIds = currentHand.Select(c => c.UniqueId).ToHashSet();
        var enemyHandIds = enemyHand.Select(c => c.UniqueId).ToHashSet();

        bool anyOverlap = currentHandIds.Overlaps(enemyHandIds);
        Assert.False(anyOverlap, "Players should not share cards with the same UniqueId.");
    }

    [Fact]
    void SetUpGame_ShouldGiveEachPlayer5CardsInHand()
    {
        var api = new ScriptsOfTributeApi(_patronsSetUp, 123);
        var p1 = api.GetPlayer(PlayerEnum.PLAYER1);
        var p2 = api.GetPlayer(PlayerEnum.PLAYER2);
        Assert.Equal(5, p1.Hand.Count);
        Assert.Equal(5, p2.Hand.Count);
    }

    [Fact]
    void SetUpGame_ShouldGiveSecondPlayerOneCoin()
    {
        var api = new ScriptsOfTributeApi(_patronsSetUp, 123);
        var p1 = api.GetPlayer(PlayerEnum.PLAYER1);
        var p2 = api.GetPlayer(PlayerEnum.PLAYER2);
        Assert.Equal(0, p1.Coins);
        Assert.Equal(1, p2.Coins);
    }

    [Fact]
    void SetUpGame_ShouldReduceDrawPileBy5Cards()
    {
        var api = new ScriptsOfTributeApi(_patronsSetUp, 123);
        var p1 = api.GetPlayer(PlayerEnum.PLAYER1);
        Assert.True(p1.DrawPile.Count > 0);
        Assert.Equal(10, p1.Hand.Count + p1.DrawPile.Count);
    }

    [Fact]
    void PlayCard_ShouldMoveCardFromHandToPlayed()
    {
        var api = new ScriptsOfTributeApi(_patronsSetUp, 123);
        var cardToPlay = api.GetPlayer(PlayerEnum.PLAYER1).Hand.First();
        
        api.PlayCard(cardToPlay);
        
        var updated = api.GetPlayer(PlayerEnum.PLAYER1);
        Assert.DoesNotContain(cardToPlay, updated.Hand);
        Assert.Contains(cardToPlay, updated.Played);
    }

    [Fact]
    void BuyCard_ShouldAddCardToCooldownAndRemoveFromTavern()
    {
        var board = new BoardManager(_patronsSetUp, 123);
        var player = board.CurrentPlayer;
        board.Tavern.AvailableCards.Add(GlobalCardDatabase.Instance.GetCard(CardId.THE_ARMORY));
        var cheapCard = board.Tavern.AvailableCards[0];
        
        player.CoinsAmount += cheapCard.Cost;

        board.BuyCard(cheapCard);

        var newPlayer = board.CurrentPlayer;

        Assert.DoesNotContain(cheapCard, board.GetAvailableTavernCards());
        Assert.Contains(cheapCard.UniqueId, newPlayer.CooldownPile.Select(c => c.UniqueId));
    }

    [Fact]
    void IsMoveLegal_ShouldReturnTrueOnlyForLegalMoves()
    {
        var api = new ScriptsOfTributeApi(_patronsSetUp, 123);
        var possible = api.GetListOfPossibleMoves();
        
        foreach (var move in possible)
        {
            Assert.True(api.IsMoveLegal(move));
        }

        var enemyCard = api.GetPlayer(PlayerEnum.PLAYER2).Hand.First();
        Assert.False(api.IsMoveLegal(Move.PlayCard(enemyCard)));
    }
}
