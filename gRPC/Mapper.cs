using ScriptsOfTribute;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Board.CardAction;

namespace ScriptsOfTributeGRPC;

public class Mapper
{
    public static GameStateProto ToProto(GameState gameState)
    {
        return new GameStateProto
        {
            StateId = gameState.StateId,
            PatronStates = ToPatronStatesProto(gameState.PatronStates),
            TavernAvailableCards = { gameState.TavernAvailableCards.Select(ToUniqueCardProto) },
            BoardState = ToBoardStateProto(gameState.BoardState),
            UpcomingEffects = { gameState.UpcomingEffects.Select(EffectSerializer.ParseEffectToString)},
            StartOfNextTurnEffects = {gameState.StartOfNextTurnEffects.Select(EffectSerializer.ParseEffectToString)},
            CurrentPlayer = ToCurrentPlayerProto(gameState.CurrentPlayer),
            EnemyPlayer = ToEnemyPlayerProto(gameState.EnemyPlayer),
            TavernCards = { gameState.TavernCards.Select(ToUniqueCardProto) },
            CompletedActions = {gameState.CompletedActions.Select(act => act.SimpleString()) },
            PendingChoice = ToChoiceProto(gameState.PendingChoice),
            EndGameState = ToEndGameStateProto(gameState.GameEndState),
        };
    }

    public static SeededGameStateProto ToProto(SeededGameState gameState)
    {
        return new SeededGameStateProto
        {
            StateId = gameState.StateId,
            PatronStates = ToPatronStatesProto(gameState.PatronStates),
            TavernAvailableCards = { gameState.TavernAvailableCards.Select(ToUniqueCardProto) },
            BoardState = ToBoardStateProto(gameState.BoardState),
            UpcomingEffects = { gameState.UpcomingEffects.Select(EffectSerializer.ParseEffectToString)},
            StartOfNextTurnEffects = {gameState.StartOfNextTurnEffects.Select(EffectSerializer.ParseEffectToString)},
            CurrentPlayer = ToPlayerProto(gameState.CurrentPlayer),
            EnemyPlayer = ToPlayerProto(gameState.EnemyPlayer),
            TavernCards = { gameState.TavernCards.Select(ToUniqueCardProto) },
            CompletedActions = {gameState.CompletedActions.Select(act => act.SimpleString()) },
            PendingChoice = ToChoiceProto(gameState.PendingChoice),
            EndGameState = ToEndGameStateProto(gameState.GameEndState),
        };
    }

    public static PatronStatesProto ToPatronStatesProto(PatronStates patronStates)
    {
        var proto = new PatronStatesProto();
        foreach (var pair in patronStates.All)
        {
            proto.Patrons.Add(pair.Key.ToString(), pair.Value.ToString());
        }
        return proto;
    }

    public static UniqueCardProto ToUniqueCardProto(UniqueCard card)
    {
        return new UniqueCardProto
        {
            Name = card.Name,
            Deck = (PatronIdProto)card.Deck,
            Cost = card.Cost,
            Type = (CardTypeProto)card.Type,
            Hp = card.HP,
            Taunt = card.Taunt,
            UniqueId = card.UniqueId.Value,
            Effects = { card.Effects.Select(EffectSerializer.ParseEffectToString) },
        };
    }

    public static BoardStateProto ToBoardStateProto(BoardState boardState)
    {
        return boardState switch
        {
            BoardState.NORMAL => BoardStateProto.Normal,
            BoardState.CHOICE_PENDING => BoardStateProto.ChoicePending,
            BoardState.START_OF_TURN_CHOICE_PENDING => BoardStateProto.StartOfTurnChoicePending,
            BoardState.PATRON_CHOICE_PENDING => BoardStateProto.PatronChoicePending,
            _ => throw new ArgumentOutOfRangeException(nameof(boardState), boardState, null)
        };
    }

    public static PlayerProto ToCurrentPlayerProto(FairSerializedPlayer player)
    {
        return new PlayerProto
        {
            PlayerId = (PlayerEnumProto)player.PlayerID,
            Hand = { player.Hand.Select(ToUniqueCardProto) },
            CooldownPile = { player.CooldownPile.Select(ToUniqueCardProto) },
            Played = { player.Played.Select(ToUniqueCardProto) },
            KnownUpcomingDraws = { player.KnownUpcomingDraws.Select(ToUniqueCardProto) },
            Agents = { player.Agents.Select(ToSerializedAgentProto) },
            Power = player.Power,
            PatronCalls = player.PatronCalls,
            Coins = player.Coins,
            Prestige = player.Prestige,
            DrawPile = { player.DrawPile.Select(ToUniqueCardProto) }
        };
    }

    public static EnemyPlayerProto ToEnemyPlayerProto(FairSerializedEnemyPlayer enemyPlayer)
    {
        return new EnemyPlayerProto
        {
            PlayerId = (PlayerEnumProto)enemyPlayer.PlayerID,
            Agents = { enemyPlayer.Agents.Select(ToSerializedAgentProto) },
            Power = enemyPlayer.Power,
            Coins = enemyPlayer.Coins,
            Prestige = enemyPlayer.Prestige,
            HandAndDraw = { enemyPlayer.HandAndDraw.Select(ToUniqueCardProto) },
            Played = { enemyPlayer.Played.Select(ToUniqueCardProto) },
            CooldownPile = { enemyPlayer.CooldownPile.Select(ToUniqueCardProto) }
        };
    }
    public static PlayerProto ToPlayerProto(SerializedPlayer player)
    {
        return new PlayerProto
        {
            PlayerId = (PlayerEnumProto)player.PlayerID,
            Hand = { player.Hand.Select(ToUniqueCardProto) },
            CooldownPile = { player.CooldownPile.Select(ToUniqueCardProto) },
            Played = { player.Played.Select(ToUniqueCardProto) },
            KnownUpcomingDraws = { player.KnownUpcomingDraws.Select(ToUniqueCardProto) },
            Agents = { player.Agents.Select(ToSerializedAgentProto) },
            Power = player.Power,
            PatronCalls = player.PatronCalls,
            Coins = player.Coins,
            Prestige = player.Prestige,
            DrawPile = { player.DrawPile.Select(ToUniqueCardProto) }
        };
    }

    public static SerializedAgentProto ToSerializedAgentProto(SerializedAgent agent)
    {
        return new SerializedAgentProto
        {
            CurrentHP = agent.CurrentHp,
            RepresentingCard = ToUniqueCardProto(agent.RepresentingCard),
            Activated = agent.Activated
        };
    }

    public static ChoiceProto? ToChoiceProto(SerializedChoice? choice)
    {
        if (choice == null) return null;

        var choiceProto = new ChoiceProto
        {
            MaxChoices = choice.MaxChoices,
            MinChoices = choice.MinChoices,
            Context = choice.Context.ToString(),
            ChoiceFollowUp = choice.ChoiceFollowUp.ToString(),
            Type = (ChoiceDataTypeProto)choice.Type
        };

        switch (choice.Type)
        {
            case Choice.DataType.CARD:
                choiceProto.CardOptions = new CardOptions
                {
                    PossibleCards = { choice.PossibleCards.Select(ToUniqueCardProto) }
                };
                break;

            case Choice.DataType.EFFECT:
                choiceProto.EffectOptions = new EffectOptions
                {
                    PossibleEffects = { choice.PossibleEffects.Select(EffectSerializer.ParseEffectToString) }
                };
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(choice.Type), choice.Type, "Invalid choice type.");
        }

        return choiceProto;
    }

    public static Move MapMove(ScriptsOfTribute.Move move)
    {
        if (move == null)
        {
            return new Move();
        }

        if (move.Command == CommandEnum.END_TURN)
        {
            var newMove = new Move
            {
                Id = move.UniqueId.Value,
                Command = MoveEnum.EndTurn,
            };
            newMove.Basic = new BasicMove();
            return newMove;
        }

        if (move is ScriptsOfTribute.SimpleCardMove cardMove)
        {
            var newMove = new Move
            {
                Id = move.UniqueId.Value,
                Command = (MoveEnum)((int)cardMove.Command),
            };
            newMove.CardMove = new SimpleCardMove
            {
                CardUniqueId = cardMove.Card.UniqueId.Value,
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.SimplePatronMove patronMove)
        {
            var newMove = new Move
            {
                Id = move.UniqueId.Value,
                Command = MoveEnum.CallPatron,
            };
            newMove.PatronMove = new SimplePatronMove
            {
                PatronId = (PatronIdProto)(patronMove.PatronId)
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueCard choiceCardMove || move is MakeChoiceMove<UniqueCard> genericChoiceCardMove)
        {
            var newMove = new Move
            {
                Id = move.UniqueId.Value,
                Command = MoveEnum.MakeChoice,
            };
            newMove.CardChoiceMove = new MakeChoiceMoveUniqueCard();

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueCard m
                ? m.Choices
                : ((MakeChoiceMove<UniqueCard>)move).Choices;

            newMove.CardChoiceMove.CardsUniqueIds.AddRange(choices.Select(card => card.UniqueId.Value).ToArray());
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect choiceEffectMove || move is MakeChoiceMove<UniqueEffect> genericChoiceEffectMove)
        {
            var newMove = new Move
            {
                Id = move.UniqueId.Value,
                Command = MoveEnum.MakeChoice,
            };
            newMove.EffectChoiceMove = new MakeChoiceMoveUniqueEffect();

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect m
                ? m.Choices
                : ((ScriptsOfTribute.MakeChoiceMove<UniqueEffect>)move).Choices;

            newMove.EffectChoiceMove.Effects.AddRange(choices.Select(eff => eff.ToSimpleString()).ToArray());
            return newMove;
        }

        throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
    }

    public static ScriptsOfTribute.Move MapMove(Move move, SerializedChoice? PendingChoice, List<ScriptsOfTribute.Move> availableMoves)
    {
        try
        {
            var moveEquivalent = availableMoves.First(m => m.UniqueId.Value == move.Id);
            return moveEquivalent;
        }
        catch
        {
            throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
        }
    }

    public static EndGameState? ToEndGameStateProto(ScriptsOfTribute.Board.EndGameState? endGameState)
    {
        if (endGameState == null)
            return null;

        return new EndGameState
        {
            Winner = endGameState.Winner.ToString(),
            Reason = endGameState.Reason.ToString(),
            AdditionalContext = endGameState.AdditionalContext
        };
    }

}

