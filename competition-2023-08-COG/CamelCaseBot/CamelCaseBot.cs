using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace Bots;

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// This was added by the competition organizers to solve the problem with agents
// using this internal `List` extension.
public static class Extensions
{
    public static T PickRandom<T>(this List<T> source, SeededRandom rng)
    {
        return source[rng.Next() % source.Count];
    }
}
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////


internal class Action
{
    public string Type { get; }
    public Move Move { get; }

    public Action(string type, Move move)
    {
        Type = type;
        Move = move;
    }
}

public class CamelCaseBot : AI
{
    private readonly SeededRandom rng = new(42);

    private int currentTurn = 1;
    private int currentMove = 1;

    private readonly List<EffectType> basicEffectTypes = new()
    {
        EffectType.GAIN_PRESTIGE,
        EffectType.GAIN_COIN,
        EffectType.GAIN_POWER,
        EffectType.DRAW,
        EffectType.OPP_LOSE_PRESTIGE,
        EffectType.KNOCKOUT,
        EffectType.OPP_DISCARD,
        EffectType.PATRON_CALL,
        EffectType.HEAL
    };

    // Based on Effect.ToString()
    private readonly Dictionary<EffectType, string> effectTypePrefixes = new()
    {
        { EffectType.GAIN_COIN, "Coin +" },
        { EffectType.GAIN_PRESTIGE, "Prestige +" },
        { EffectType.GAIN_POWER, "Power +" },
        { EffectType.OPP_LOSE_PRESTIGE, "Enemy prestige -" },
        { EffectType.REPLACE_TAVERN, "Replace " },
        { EffectType.ACQUIRE_TAVERN, "Get card from tavern with cost up to " },
        { EffectType.DESTROY_CARD, "Destroy " },
        { EffectType.DRAW, "Draw " },
        { EffectType.OPP_DISCARD, "Enemy discards " },
        { EffectType.RETURN_TOP, "Put " },
        { EffectType.TOSS, "Choose up to " },
        { EffectType.KNOCKOUT, "Knockout " },
        { EffectType.PATRON_CALL, "Get " },
        { EffectType.CREATE_BOARDINGPARTY, "Create " },
        { EffectType.HEAL, "Heal this agent by " }
    };

    private readonly List<PatronId> preferredPatrons = new()
    {
        PatronId.DUKE_OF_CROWS,
        PatronId.RED_EAGLE,
        PatronId.ANSEI,
        PatronId.HLAALU
    };

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return preferredPatrons.First(availablePatrons.Contains);
    }

    public override Move Play(GameState state, List<Move> possibleMoves)
    {
        var action = GetNextAction(state, possibleMoves);

        Log($"[{currentMove:00}/{currentTurn:00}] {SummarizeState(state)} {action.Type}: {action.Move}");

        if (action.Move.Command == CommandEnum.END_TURN)
        {
            currentTurn++;
            currentMove = 1;
        }
        else
        {
            currentMove++;
        }

        return action.Move;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        currentTurn = 1;
        currentMove = 1;
    }

    private Action GetNextAction(GameState state, List<Move> possibleMoves)
    {
        var basicAction = GetBasicAction(state, possibleMoves);
        if (basicAction != null)
        {
            return basicAction;
        }

        var prestigeEndGame = state.EnemyPlayer.Prestige >= 20;
        var patronEndGame = state.PatronStates.All.Count(kvp => kvp.Value == state.EnemyPlayer.PlayerID) == 3;

        var phaseAction = prestigeEndGame || patronEndGame
            ? GetPrestigeAction(state, possibleMoves)
            : GetIncomeAction(state, possibleMoves);
        if (phaseAction != null)
        {
            return phaseAction;
        }

        if (state.CurrentPlayer.Coins > 5)
        {
            var prestigeAction = GetPrestigeAction(state, possibleMoves);
            if (prestigeAction != null)
            {
                return prestigeAction;
            }
        }

        var buyContractCard = possibleMoves
            .Where(move => move.Command == CommandEnum.BUY_CARD && move is SimpleCardMove)
            .Select(move => (move as SimpleCardMove)!)
            .Where(move => move.Card.Type is CardType.CONTRACT_ACTION or CardType.CONTRACT_AGENT)
            .OrderBy(move =>
            {
                var hasPowerGain = move.Card.Effects
                    .SelectMany(effect => effect?.Decompose() ?? Enumerable.Empty<UniqueBaseEffect>())
                    .Any(effect => effect is UniqueEffect { Type: EffectType.GAIN_POWER });

                return hasPowerGain ? 0 : 1;
            })
            .FirstOrDefault();
        if (buyContractCard != null)
        {
            return new Action("Buy contract card", buyContractCard);
        }

        var endTurn = possibleMoves.FirstOrDefault(move => move.Command == CommandEnum.END_TURN);
        if (endTurn != null)
        {
            return new Action("End turn", endTurn);
        }

        return new Action("Random", possibleMoves.PickRandom(rng));
    }

    private Action? GetBasicAction(GameState state, List<Move> possibleMoves)
    {
        var curseCard = possibleMoves
            .Where(move => move.Command == CommandEnum.PLAY_CARD && move is SimpleCardMove)
            .Select(move => (move as SimpleCardMove)!)
            .FirstOrDefault(move => move.Card.Type == CardType.CURSE);
        if (curseCard != null)
        {
            return new Action("Curse", curseCard);
        }

        var activateAgent = possibleMoves.FirstOrDefault(move => move.Command == CommandEnum.ACTIVATE_AGENT);
        if (activateAgent != null)
        {
            return new Action("Activate agent", activateAgent);
        }

        var attack = possibleMoves.FirstOrDefault(move => move.Command == CommandEnum.ATTACK);
        if (attack != null)
        {
            return new Action("Attack", attack);
        }

        var basicMove = possibleMoves.Where(move => move.Command == CommandEnum.PLAY_CARD && move is SimpleCardMove)
            .Select(move => (move as SimpleCardMove)!)
            .FirstOrDefault(move => IsBasicCard(move.Card));
        if (basicMove != null)
        {
            return new Action("Basic card", basicMove);
        }

        var chooseEffect = possibleMoves
            .Where(move => move.Command == CommandEnum.MAKE_CHOICE && move is MakeChoiceMove<UniqueEffect>)
            .Select(move => (move as MakeChoiceMove<UniqueEffect>)!)
            .Where(move => move.Choices.All(choice => basicEffectTypes.Contains(choice.Type)))
            .OrderBy(move => move.Choices.Sum(choice => basicEffectTypes.IndexOf(choice.Type)))
            .FirstOrDefault();
        if (chooseEffect != null)
        {
            return new Action("Choose effect", chooseEffect);
        }

        return null;
    }

    private Action? GetPrestigeAction(GameState state, List<Move> possibleMoves)
    {
        var dukeOfCrows = possibleMoves.FirstOrDefault(move =>
            move is SimplePatronMove
            {
                PatronId: PatronId.DUKE_OF_CROWS
            });
        if (dukeOfCrows != null && state.CurrentPlayer.Coins > 1)
        {
            return new Action("Duke of Crows", dukeOfCrows);
        }

        return GetIncomeAction(state, possibleMoves);
    }

    private Action? GetIncomeAction(GameState state, List<Move> possibleMoves)
    {
        var buyCard = possibleMoves
            .Where(move => move.Command == CommandEnum.BUY_CARD && move is SimpleCardMove)
            .Select(move => (move as SimpleCardMove)!)
            .Where(move => IsBasicCard(move.Card))
            .OrderByDescending(move => move.Card.Cost)
            .FirstOrDefault();
        if (buyCard != null)
        {
            return new Action("Buy card", buyCard);
        }

        var treasury = possibleMoves.FirstOrDefault(move =>
            move is SimplePatronMove
            {
                PatronId: PatronId.TREASURY
            });
        if (treasury != null && state.CurrentPlayer.Hand.Concat(state.CurrentPlayer.Played).Any(IsConvertibleCard))
        {
            return new Action("Treasury", treasury);
        }

        var chooseCard = possibleMoves
            .Where(move => move.Command == CommandEnum.MAKE_CHOICE && move is MakeChoiceMove<UniqueCard>)
            .Select(move => (move as MakeChoiceMove<UniqueCard>)!)
            .Where(move => move.Choices.All(IsConvertibleCard))
            .OrderBy(move => move.Choices.Sum(card => card.Cost))
            .FirstOrDefault();
        if (chooseCard != null)
        {
            return new Action("Choose card", chooseCard);
        }

        return null;
    }

    private bool IsBasicCard(UniqueCard card)
    {
        if (card.Type != CardType.STARTER && card.Type != CardType.ACTION && card.Type != CardType.AGENT)
        {
            return false;
        }

        return card.Effects
            .SelectMany(effect => effect?.Decompose() ?? Enumerable.Empty<UniqueBaseEffect>())
            .All(effect =>
            {
                if (effect is UniqueEffect uniqueEffect)
                {
                    return basicEffectTypes.Contains(uniqueEffect.Type);
                }

                // UniqueEffectOr's left and right effect are unfortunately private
                // The only way to figure out their types are by their string representation
                if (effect is UniqueEffectOr)
                {
                    return effect
                        .ToString()
                        .Split(" OR ")
                        .Any(part => basicEffectTypes.Any(type => part.StartsWith(effectTypePrefixes[type])));
                }

                return false;
            });
    }

    private bool IsConvertibleCard(UniqueCard card)
    {
        return card.Type is CardType.STARTER or CardType.CURSE && card.CommonId != CardId.WRIT_OF_COIN;
    }

    private string SummarizeState(GameState state)
    {
        var me = state.CurrentPlayer;
        var enemy = state.EnemyPlayer;

        var mePatrons = state.PatronStates.All.Values.Count(player => player == me.PlayerID);
        var enemyPatrons = state.PatronStates.All.Values.Count(player => player == enemy.PlayerID);

        var meSummary = $"[{me.Prestige}/{me.Coins}/{me.Power}/{mePatrons}]";
        var enemySummary = $"[{enemy.Prestige}/{enemy.Coins}/{enemy.Power}/{enemyPatrons}]";

        return $"{meSummary} {enemySummary}";
    }
}
