using TalesOfTribute.Board.Cards;

namespace TalesOfTribute.Board.CardAction;

public class ComplexEffectExecutor
{
    private CardActionManager _parent;
    private IPlayer _currentPlayer;
    private IPlayer _enemyPlayer;
    private ITavern _tavern;

    public ComplexEffectExecutor(CardActionManager parent, IPlayer currentPlayer, IPlayer enemyPlayer, ITavern tavern)
    {
        _parent = parent;
        _currentPlayer = currentPlayer;
        _enemyPlayer = enemyPlayer;
        _tavern = tavern;
    }

    public (PlayResult, IEnumerable<CompletedAction>) Enact(Choice c, UniqueEffect choice)
    {
        return c.FollowUp switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => CompleteChoice(choice),
        };
    }

    public (PlayResult, IEnumerable<CompletedAction>) Enact(Choice choice, List<UniqueCard> choices)
    {
        return choice.FollowUp switch
        {
            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN => (ReplaceTavern(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.DESTROY_CARDS => (DestroyCard(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.DISCARD_CARDS => (Discard(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.REFRESH_CARDS => (Refresh(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.TOSS_CARDS => (Toss(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.KNOCKOUT_AGENTS => (Knockout(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.ACQUIRE_CARDS => (AcquireTavern(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_HLAALU => (CompleteHlaalu(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_PELLIN => (CompletePelin(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_PSIJIC => (CompletePsijic(choice, choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_TREASURY => (CompleteTreasury(choice, choices), new List<CompletedAction>()),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null)
        };
    }

    public PlayResult AcquireTavern(Choice c, List<UniqueCard> choices)
    {
        if (choices.Count == 0)
        {
            return new Success();
        }

        if (choices.Count > 1)
        {
            throw new Exception("Can't acquire more than 1 card.");
        }

        var choice = choices.First();
        var card = _tavern.Acquire(choice);

        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.ACQUIRE_CARD, c.Context!.CardSource!, card));

        switch (card.Type)
        {
            case CardType.CONTRACT_ACTION:
                _parent.ImmediatePlayCard(card);
                break;
            case CardType.CONTRACT_AGENT:
                var agent = Agent.FromCard(card);
                agent.MarkActivated();
                _currentPlayer.Agents.Add(agent);
                _parent.ImmediatePlayCard(card);
                break;
            default:
                _currentPlayer.CooldownPile.Add(card);
                break;
        }

        return new Success();
    }

    public PlayResult ReplaceTavern(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_tavern.ReplaceCard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REPLACE_TAVERN, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult DestroyCard(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Destroy);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DESTROY_CARD, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Discard(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Discard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DISCARD, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Refresh(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Refresh);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REFRESH, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Toss(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Toss);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.TOSS, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Knockout(Choice choice, List<UniqueCard> choices)
    {
        var contractAgents = choices.FindAll(card => card.Type == CardType.CONTRACT_AGENT);
        var normalAgents = choices.FindAll(card => card.Type == CardType.AGENT);
        contractAgents.ForEach(_enemyPlayer.Destroy);
        _tavern.Cards.AddRange(contractAgents);
        normalAgents.ForEach(_enemyPlayer.KnockOut);
        contractAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, choice.Context!.CardSource!, c)));
        normalAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public (PlayResult, IEnumerable<CompletedAction>) CompleteChoice(UniqueEffect choice)
    {
        return choice.Enact(_currentPlayer, _enemyPlayer, _tavern);
    }

    public PlayResult CompleteHlaalu(Choice sourceChoice, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Hlaalu requires exactly 1 choice.");
        }

        var card = choices.First();
        if (_currentPlayer.Hand.Any(c => c.UniqueId == card.UniqueId))
        {
            _currentPlayer.Hand.Remove(card);
        }
        else // if not in hand, then it must be in a played pile
        {
            _currentPlayer.Played.Remove(card);
        }

        var prestigeGainAmount = card.Cost - 1;
        _currentPlayer.PrestigeAmount += prestigeGainAmount;

        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DESTROY_CARD, PatronId.HLAALU, card));
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.GAIN_PRESTIGE, PatronId.HLAALU, prestigeGainAmount));

        return new Success();
    }

    public PlayResult CompletePelin(Choice sourceChoice, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Pelin requires exactly 1 choice.");
        }

        var choice = choices.First();
        _currentPlayer.CooldownPile.Remove(choice);
        _currentPlayer.DrawPile.Insert(0, choice);

        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REFRESH, PatronId.PELIN, choice));

        return new Success();
    }

    public PlayResult CompletePsijic(Choice sourceChoice, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Psijic requires exactly 1 choice.");
        }

        var choice = choices.First();
        _enemyPlayer.KnockOut(choice);

        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, PatronId.PSIJIC, choice));

        return new Success();
    }

    public PlayResult CompleteTreasury(Choice _, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Treasury requires exactly 1 choice.");
        }

        var writOfCoin = GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN);
        var choice = choices.First();
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DESTROY_CARD, PatronId.TREASURY, choice));
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.ADD_WRIT_OF_COIN, PatronId.TREASURY, 1, writOfCoin));
        if (_currentPlayer.Played.Contains(choice))
        {
            _currentPlayer.Played.Remove(choice);
            _currentPlayer.CooldownPile.Add(writOfCoin);
        }
        else
        {
            _currentPlayer.Hand.Remove(choice);
            _currentPlayer.CooldownPile.Add(writOfCoin);
        }
        return new Success();
    }
}
