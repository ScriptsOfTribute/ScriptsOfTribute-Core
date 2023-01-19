using TalesOfTribute;
using TalesOfTribute.Board.Cards;
using TalesOfTribute.Serializers;

namespace ModuleTests;

public class UnknownCardTests
{
    [Fact]
    void SimulatingBuyFromTavernShouldInsertUnknownCardThatIsUnavailableToBuy()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var tavernAvailableCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
        };
        var tavernCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
        };
        
        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            tavernAvailableCards, tavernCards, 123);

        var (newState, possibleMoves) = board.ApplyState(Move.BuyCard(tavernAvailableCards[0]));
        Assert.Equal(CardId.UNKNOWN, newState.TavernAvailableCards[0].CommonId);
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.BUY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
    }
    
    [Fact]
    void SimulatingDrawShouldInsertUnknownCardThatIsUnavailableToPlayButCanBeDiscarded()
    {
        var drawPile = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY)
        };

        // Has Draw
        var harvestSeason = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        // Has Destroy
        var ragpicker = GlobalCardDatabase.Instance.GetCard(CardId.RAGPICKER);
        var hand = new List<UniqueCard>
        {
            harvestSeason,
            ragpicker,
        };
        
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(harvestSeason));
        Assert.Contains(CardId.UNKNOWN, newState.CurrentPlayer.Hand.Select(c => c.CommonId));
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.PLAY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
        Assert.Equal(1, possibleMoves.Count(m => m.Command == CommandEnum.PLAY_CARD));

        (newState, _) = newState.ApplyState(Move.PlayCard(ragpicker));
        Assert.Contains(CardId.UNKNOWN, newState.PendingChoice!.PossibleCards.Select(c => c.CommonId));

        (newState, _) =
            newState.ApplyState(
                Move.MakeChoice(newState.PendingChoice!.PossibleCards.First(c => c.CommonId == CardId.UNKNOWN)));
        
        Assert.Empty(newState.CurrentPlayer.Hand);
    }

    [Fact]
    void SimulatingDrawAfterRefreshShouldNotReturnUnknownCard()
    {
        // Refresh 2 (put 2 cards from Cooldown on top of Draw)
        var helShiraHerald = GlobalCardDatabase.Instance.GetCard(CardId.HEL_SHIRA_HERALD);

        var gold1 = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var drawPile = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN),
            gold1,
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN),
        };

        // Toss 2
        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var scryingGlobe2 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);

        var cooldownPile = new List<UniqueCard>
        {
            scryingGlobe1, scryingGlobe2,
        };

        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason3 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var hand = new List<UniqueCard>
        {
            helShiraHerald,
            harvestSeason1,
            harvestSeason2,
            harvestSeason3,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            cooldownPile, new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(helShiraHerald));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard> { scryingGlobe1, scryingGlobe2 }));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason1));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason2));
        Assert.Equal(2, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.SCRYING_GLOBE));

        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason3));
        Assert.Contains(CardId.UNKNOWN, newState.CurrentPlayer.Hand.Select(c => c.CommonId));
    }

    [Fact]
    void TossAfterRefreshShouldAllowToSeeRefreshedCardAndNotReplaceIt()
    {
        // Refresh 1 (put 1 card from Cooldown on top of Draw)
        var noShiraPoet = GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET);

        var drawPile = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN),
        };


        var gold1 = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var cooldownPile = new List<UniqueCard>
        {
            gold1,
        };

        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        // Toss 2
        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var hand = new List<UniqueCard>
        {
            noShiraPoet,
            harvestSeason1,
            harvestSeason2,
            scryingGlobe1,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            cooldownPile, new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(noShiraPoet));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard> { gold1 }));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(scryingGlobe1));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard>()));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason1));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason2));
        Assert.Equal(1, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.GOLD));
        Assert.Contains(CardId.UNKNOWN, newState.CurrentPlayer.Hand.Select(c => c.CommonId));
        Assert.DoesNotContain(CardId.WRIT_OF_COIN, newState.CurrentPlayer.Hand.Select(c => c.CommonId));
    }

    [Fact]
    void RefreshAfterTossShouldAllowToSeeAdditionalCards()
    {
        // Refresh 1 (put 1 card from Cooldown on top of Draw)
        var noShiraPoet = GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET);

        var drawPile = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
            GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN),
        };


        var gold1 = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var cooldownPile = new List<UniqueCard>
        {
            gold1,
        };

        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason3 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason4 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        // Toss 2
        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var hand = new List<UniqueCard>
        {
            noShiraPoet,
            harvestSeason1,
            harvestSeason2,
            harvestSeason3,
            harvestSeason4,
            scryingGlobe1,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            cooldownPile, new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(scryingGlobe1));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard>()));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(noShiraPoet));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard> { gold1 }));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason1));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason2));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason3));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason4));
        Assert.Equal(1, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.GOLD));
        Assert.Equal(3, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.UNKNOWN));
    }

    // TODO: If we change Cycling behavior after testing, this test will likely break.
    [Fact]
    void DeckCycleBehaviourWithTossAndUnknownCards()
    {
        // Refresh 2 (put 2 cards from Cooldown on top of Draw)
        var helShiraHerald = GlobalCardDatabase.Instance.GetCard(CardId.HEL_SHIRA_HERALD);

        var drawPile = new List<UniqueCard>
        {
        };
        
        var gold1 = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var gold2 = GlobalCardDatabase.Instance.GetCard(CardId.GOLD);
        var writ1 = GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN);
        var writ2 = GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN);
        var cooldownPile = new List<UniqueCard>
        {
            gold1,
            gold2,
            writ1,
            writ2,
        };

        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason3 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason4 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        // Toss 2
        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var hand = new List<UniqueCard>
        {
            helShiraHerald,
            harvestSeason1,
            harvestSeason2,
            harvestSeason3,
            harvestSeason4,
            scryingGlobe1,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            cooldownPile, new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(helShiraHerald));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard> { gold1, gold2 }));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason1)); // draw gold
        // This action should Deck Cycle, because there is 1 item in DrawPile, so Toss 2 Cycles.
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(scryingGlobe1));
        (newState, possibleMoves) = newState.ApplyState(Move.MakeChoice(new List<UniqueCard>()));
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason2)); //draw gold because toss add cooldown cards to bottom
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason3)); // unknown draw
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(harvestSeason4)); // unknown draw
        Assert.Equal(2, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.UNKNOWN)); // our writs instantiated above
        Assert.Equal(2, newState.CurrentPlayer.Hand.Count(c => c.CommonId == CardId.GOLD));
    }

    [Fact]
    void DrawingInSimulationModeShouldTurnDrawPileIntoUnknownsThatCanBeProperlyInteractedWith()
    {
        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason3 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason4 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var drawPile = new List<UniqueCard>
        {
            harvestSeason2,
            harvestSeason3,
            harvestSeason4,
        };

        var hand = new List<UniqueCard>
        {
            harvestSeason1,
            scryingGlobe1,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newBoard, _) = board.ApplyState(Move.PlayCard(harvestSeason1));

        Assert.Contains(CardId.UNKNOWN, newBoard.CurrentPlayer.Hand.Select(c => c.CommonId));
        Assert.All(newBoard.CurrentPlayer.DrawPile, c => Assert.Equal(CardId.UNKNOWN, c.CommonId));

        var someUnknown = newBoard.CurrentPlayer.DrawPile[0];

        (newBoard, _) = newBoard.ApplyState(Move.PlayCard(scryingGlobe1));
        (newBoard, _) = newBoard.ApplyState(Move.MakeChoice(new List<UniqueCard> { someUnknown }));
        Assert.Equal(someUnknown, newBoard.CurrentPlayer.CooldownPile[0]);
    }

    [Fact]
    void TossingInSimulationModeShouldTurnDrawPileIntoUnknowns()
    {
        var harvestSeason1 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason2 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason3 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var harvestSeason4 = GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON);
        var drawPile = new List<UniqueCard>
        {
            harvestSeason1,
            harvestSeason2,
            harvestSeason3,
            harvestSeason4,
        };

        var scryingGlobe1 = GlobalCardDatabase.Instance.GetCard(CardId.SCRYING_GLOBE);
        var hand = new List<UniqueCard>
        {
            scryingGlobe1,
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawPile,
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newBoard, _) = board.ApplyState(Move.PlayCard(scryingGlobe1));
        (newBoard, _) = newBoard.ApplyState(Move.MakeChoice(new List<UniqueCard>()));

        Assert.All(newBoard.CurrentPlayer.DrawPile, c => Assert.Equal(CardId.UNKNOWN, c.CommonId));
    }
}
