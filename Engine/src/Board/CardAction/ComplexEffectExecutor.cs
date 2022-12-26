﻿namespace TalesOfTribute.Board.CardAction;

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

    public PlayResult Enact(ChoiceFollowUp choice, List<IChoosable> choices)
    {
        return choice switch
        {
            ChoiceFollowUp.ENACT_CHOSEN_EFFECT => CompleteChoice(ToEffectList(choices)),
            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN => ReplaceTavern(ToCardList(choices)),
            ChoiceFollowUp.DESTROY_CARDS => DestroyCard(ToCardList(choices)),
            ChoiceFollowUp.DISCARD_CARDS => Discard(ToCardList(choices)),
            ChoiceFollowUp.REFRESH_CARDS => Refresh(ToCardList(choices)),
            ChoiceFollowUp.TOSS_CARDS => Toss(ToCardList(choices)),
            ChoiceFollowUp.KNOCKOUT_AGENTS => Knockout(ToCardList(choices)),
            ChoiceFollowUp.ACQUIRE_CARDS => AcquireTavern(ToCardList(choices)),
            ChoiceFollowUp.COMPLETE_HLAALU => CompleteHlaalu(ToCardList(choices)),
            ChoiceFollowUp.COMPLETE_PELLIN => CompletePelin(ToCardList(choices)),
            ChoiceFollowUp.COMPLETE_PSIJIC => CompletePsijic(ToCardList(choices)),
            ChoiceFollowUp.COMPLETE_TREASURY => CompleteTreasury(ToCardList(choices)),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null)
        };
    }

    private List<Effect> ToEffectList(List<IChoosable> l) => l.Select(c => (Effect)c).ToList();
    private List<Card> ToCardList(List<IChoosable> l) => l.Select(c => (Card)c).ToList();

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

    public PlayResult ReplaceTavern(List<Card> choices)
    {
        choices.ForEach(_tavern.ReplaceCard);
        return new Success();
    }

    public PlayResult DestroyCard(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Destroy);
        return new Success();
    }

    public PlayResult Discard(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Discard);
        return new Success();
    }

    public PlayResult Refresh(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Refresh);
        return new Success();
    }

    public PlayResult Toss(List<Card> choices)
    {
        choices.ForEach(_currentPlayer.Toss);
        return new Success();
    }

    public PlayResult Knockout(List<Card> choices)
    {
        var contractAgents = choices.FindAll(card => card.Type == CardType.CONTRACT_AGENT);
        var normalAgents = choices.FindAll(card => card.Type == CardType.AGENT);
        contractAgents.ForEach(_enemyPlayer.Destroy);
        _tavern.Cards.AddRange(contractAgents);
        normalAgents.ForEach(_enemyPlayer.KnockOut);
        return new Success();
    }

    public PlayResult CompleteChoice(List<Effect> choices)
    {
        return choices.First().Enact(_currentPlayer, _enemyPlayer, _tavern);
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
        
        _currentPlayer.PrestigeAmount += card.Cost - 1;
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
        return new Success();
    }

    public PlayResult CompletePsijic(List<Card> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Psijic requires exactly 1 choice.");
        }

        var choice = choices.First();
        _enemyPlayer.Destroy(choice);
        _enemyPlayer.CooldownPile.Add(choice);
        return new Success();
    }

    public PlayResult CompleteTreasury(List<Card> choices)
    {
        if (choices.Count != 1)
        {
            throw new Exception("Treasury requires exactly 1 choice.");
        }

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
