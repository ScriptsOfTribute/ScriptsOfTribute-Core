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
    
    public (PlayResult, IEnumerable<CompletedAction>) Enact(ChoiceFollowUp c, Effect choice)
    {
        return c switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => CompleteChoice(choice),
        };
    }

    public (PlayResult, IEnumerable<CompletedAction>) Enact(ChoiceFollowUp choice, List<Card> choices)
    {
        return choice switch
        {
            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN => (ReplaceTavern(choices), new List<CompletedAction>()),
            ChoiceFollowUp.DESTROY_CARDS => (DestroyCard(choices), new List<CompletedAction>()),
            ChoiceFollowUp.DISCARD_CARDS => (Discard(choices), new List<CompletedAction>()),
            ChoiceFollowUp.REFRESH_CARDS => (Refresh(choices), new List<CompletedAction>()),
            ChoiceFollowUp.TOSS_CARDS => (Toss(choices), new List<CompletedAction>()),
            ChoiceFollowUp.KNOCKOUT_AGENTS => (Knockout(choices), new List<CompletedAction>()),
            ChoiceFollowUp.ACQUIRE_CARDS => (AcquireTavern(choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_HLAALU => (CompleteHlaalu(choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_PELLIN => (CompletePelin(choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_PSIJIC => (CompletePsijic(choices), new List<CompletedAction>()),
            ChoiceFollowUp.COMPLETE_TREASURY => (CompleteTreasury(choices), new List<CompletedAction>()),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null)
        };
    }

    public PlayResult AcquireTavern(List<Card> choices)
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
        
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.ACQUIRE_CARD, card));

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
        
        // TODO: Like this, these will be added in wrong order to Completed Actions History...
        // That is a problem, we need to stagger this.
        // Maybe _parent.AddToHistory(...), that would be nicer than to manage all this.
        
        return new Success();
    }

    public PlayResult ReplaceTavern(List<Card> choices)
    {
        choices.ForEach(_tavern.ReplaceCard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REPLACE_TAVERN, c)));
        return new Success();
    }

    public PlayResult DestroyCard(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Destroy);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DESTROY_CARD, c)));
        return new Success();
    }

    public PlayResult Discard(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Discard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DISCARD, c)));
        return new Success();
    }

    public PlayResult Refresh(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Refresh);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REFRESH, c)));
        return new Success();
    }

    public PlayResult Toss(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Toss);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.TOSS, c)));
        return new Success();
    }

    public PlayResult Knockout(List<Card> choices)
    {
        var contractAgents = choices.FindAll(card => card.Type == CardType.CONTRACT_AGENT);
        var normalAgents = choices.FindAll(card => card.Type == CardType.AGENT);
        contractAgents.ForEach(_enemyPlayer.Destroy);
        _tavern.Cards.AddRange(contractAgents);
        normalAgents.ForEach(_enemyPlayer.KnockOut);
        contractAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, c)));
        normalAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, c)));
        return new Success();
    }

    public (PlayResult, IEnumerable<CompletedAction>) CompleteChoice(Effect choice)
    {
        return choice.Enact(_currentPlayer, _enemyPlayer, _tavern);
    }

    public PlayResult CompleteHlaalu(List<Card> choices)
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
        
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.DESTROY_CARD, card));
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.GAIN_PRESTIGE, PatronId.HLAALU, prestigeGainAmount));
        
        return new Success();
    }

    public PlayResult CompletePelin(List<Card> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Pelin requires exactly 1 choice.");
        }

        var choice = choices.First();
        _currentPlayer.CooldownPile.Remove(choice);
        _currentPlayer.DrawPile.Insert(0, choice);

        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.REFRESH, choice));

        return new Success();
    }

    public PlayResult CompletePsijic(List<Card> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Psijic requires exactly 1 choice.");
        }

        var choice = choices.First();
        _enemyPlayer.KnockOut(choice);
        
        _parent.AddToCompletedActionsList(new CompletedAction(CompletedActionType.KNOCKOUT, choice));
        
        return new Success();
    }

    public PlayResult CompleteTreasury(List<Card> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Treasury requires exactly 1 choice.");
        }
        
        // TODO: Add actions here.

        var choice = choices.First();
        if (_currentPlayer.Played.Contains(choice))
        {
            _currentPlayer.Played.Remove(choice);
            _currentPlayer.DrawPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN));
        }
        else
        {
            _currentPlayer.CooldownPile.Remove(choice);
            _currentPlayer.DrawPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN));
        }
        return new Success();
    }
}
