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
            PatronStates = ToPatronStatesProto(gameState.PatronStates),
            TavernAvailableCards = { gameState.TavernAvailableCards.Select(ToUniqueCardProto) },
            BoardState = ToBoardStateProto(gameState.BoardState),
            UpcomingEffects = { gameState.UpcomingEffects.Select(EffectSerializer.ParseEffectToString)},
            StartOfNextTurnEffects = {gameState.StartOfNextTurnEffects.Select(EffectSerializer.ParseEffectToString)},
            CurrentPlayer = ToCurrentPlayerProto(gameState.CurrentPlayer),
            EnemyPlayer = ToEnemyPlayerProto(gameState.EnemyPlayer),
            TavernCards = { gameState.TavernCards.Select(ToUniqueCardProto) },
            CompletedActions = {gameState.CompletedActions.Select(act => act.SimpleString()) },
            PendingChoice = ToChoiceProto(gameState.PendingChoice)
        };
    }

    public static SeededGameStateProto ToProto(SeededGameState gameState)
    {
        return new SeededGameStateProto
        {
            PatronStates = ToPatronStatesProto(gameState.PatronStates),
            TavernAvailableCards = { gameState.TavernAvailableCards.Select(ToUniqueCardProto) },
            BoardState = ToBoardStateProto(gameState.BoardState),
            UpcomingEffects = { gameState.UpcomingEffects.Select(EffectSerializer.ParseEffectToString)},
            StartOfNextTurnEffects = {gameState.StartOfNextTurnEffects.Select(EffectSerializer.ParseEffectToString)},
            CurrentPlayer = ToPlayerProto(gameState.CurrentPlayer),
            EnemyPlayer = ToPlayerProto(gameState.EnemyPlayer),
            TavernCards = { gameState.TavernCards.Select(ToUniqueCardProto) },
            CompletedActions = {gameState.CompletedActions.Select(act => act.SimpleString()) },
            PendingChoice = ToChoiceProto(gameState.PendingChoice)
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
            var newMove = new Move();
            newMove.Basic = new BasicMove
            {
                Command = MoveEnum.EndTurn
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.SimpleCardMove cardMove)
        {
            var newMove = new Move();
            newMove.CardMove = new SimpleCardMove
            {
                Command = (MoveEnum)((int)cardMove.Command),
                CardUniqueId = cardMove.Card.UniqueId.Value
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.SimplePatronMove patronMove)
        {
            var newMove = new Move();
            newMove.PatronMove = new SimplePatronMove
            {
                Command = MoveEnum.CallPatron,
                PatronId = (PatronIdProto)(patronMove.PatronId)
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueCard choiceCardMove || move is ScriptsOfTribute.MakeChoiceMove<UniqueCard> genericChoiceCardMove)
        {
            var newMove = new Move();
            newMove.CardChoiceMove = new MakeChoiceMoveUniqueCard
            {
                Command = MoveEnum.MakeChoice,
            };

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueCard m
                ? m.Choices
                : ((ScriptsOfTribute.MakeChoiceMove<UniqueCard>)move).Choices;

            newMove.CardChoiceMove.CardsUniqueIds.AddRange(choices.Select(card => card.UniqueId.Value).ToArray());
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect choiceEffectMove || move is ScriptsOfTribute.MakeChoiceMove<UniqueEffect> genericChoiceEffectMove)
        {
            var newMove = new Move();
            newMove.EffectChoiceMove = new MakeChoiceMoveUniqueEffect
            {
                Command = MoveEnum.MakeChoice,
            };

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect m
                ? m.Choices
                : ((ScriptsOfTribute.MakeChoiceMove<UniqueEffect>)move).Choices;

            newMove.EffectChoiceMove.Effects.AddRange(choices.Select(eff => eff.ToSimpleString()).ToArray());
            return newMove;
        }

        throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
    }

    public static ScriptsOfTribute.Move MapMove(Move move, SerializedChoice? PendingChoice)
    {
        if (move == null)
        {
            return ScriptsOfTribute.Move.EndTurn();
        }

        if (move.Basic != null && move.Basic.Command == MoveEnum.EndTurn)
        {
            return ScriptsOfTribute.Move.EndTurn();
        }

        if (move.CardMove != null)
        {
            var cardReferencedTo = GlobalCardDatabase.Instance.AllCards.First(card => move.CardMove.CardUniqueId == card.UniqueId.Value);
            return move.CardMove.Command switch
            {
                MoveEnum.PlayCard => ScriptsOfTribute.Move.PlayCard(cardReferencedTo),
                MoveEnum.ActivateAgent => ScriptsOfTribute.Move.ActivateAgent(cardReferencedTo),
                MoveEnum.BuyCard => ScriptsOfTribute.Move.BuyCard(cardReferencedTo),
                MoveEnum.Attack => ScriptsOfTribute.Move.Attack(cardReferencedTo),
                _ => throw new ArgumentException("Undefined behaviour in AIService.MapMove method")
            };
        }

        if (move.PatronMove != null)
        {
            var patronReferenced = (ScriptsOfTribute.PatronId)((int)move.PatronMove.PatronId);
            return ScriptsOfTribute.Move.CallPatron(patronReferenced);
        }

        if (move.CardChoiceMove != null)
        {
            var cardsReferencedTo = GlobalCardDatabase.Instance.AllCards.Where(card => move.CardChoiceMove.CardsUniqueIds.Contains(card.UniqueId.Value)).ToList();
            return ScriptsOfTribute.Move.MakeChoice(cardsReferencedTo);
        }

        if (move.EffectChoiceMove != null && PendingChoice != null)
        {
            var referencedEffects = move.EffectChoiceMove.Effects;
            var pickedEffects = PendingChoice.PossibleEffects.Where(eff => referencedEffects.Contains(eff.ToSimpleString())).ToList();
            return ScriptsOfTribute.Move.MakeChoice(pickedEffects);
        }

        throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
    }

}

