using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Board.CardAction;

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
            ChoiceFollowUp.DONATE => (Donate(choice, choices), new List<CompletedAction>()),
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
            throw new EngineException("Can't acquire more than 1 card.");
        }

        var choice = choices.First();
        
        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.ACQUIRE_CARD, c.Context!.CardSource!, choice));
        var idx = _tavern.RemoveCard(choice);
        switch (choice.Type)
        {
            case CardType.CONTRACT_ACTION:
                _parent.ImmediatePlayCard(choice);
                _tavern.Cards.Add(choice);
                break;
            case CardType.CONTRACT_AGENT:
                var agent = Agent.FromCard(choice);
                agent.MarkActivated();
                _currentPlayer.Agents.Add(agent);
                _parent.ImmediatePlayCard(choice);
                break;
            default:
                _currentPlayer.CooldownPile.Add(choice);
                break;
        }
        _tavern.DrawAt(idx);
        return new Success();
    }

    public PlayResult ReplaceTavern(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_tavern.ReplaceCard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.REPLACE_TAVERN, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult DestroyCard(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Destroy);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DESTROY_CARD, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Discard(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Discard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DISCARD, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Refresh(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Refresh);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.REFRESH, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Toss(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Toss);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.TOSS, choice.Context!.CardSource!, c)));
        return new Success();
    }

    public PlayResult Knockout(Choice choice, List<UniqueCard> choices)
    {
        handleSaintAlessiaTriggers(choices);
        var contractAgents = choices.FindAll(card => card.Type == CardType.CONTRACT_AGENT);
        var normalAgents = choices.FindAll(card => card.Type == CardType.AGENT);
        contractAgents.ForEach(_enemyPlayer.Destroy);
        _tavern.Cards.AddRange(contractAgents);
        normalAgents.ForEach(a => _enemyPlayer.KnockOut(a, _tavern));
        contractAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.KNOCKOUT, choice.Context!.CardSource!, c)));
        normalAgents.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.KNOCKOUT, choice.Context!.CardSource!, c)));
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
            throw new EngineException("Hlaalu requires exactly 1 choice.");
        }

        var card = choices.First();
        if (_currentPlayer.Hand.Any(c => c.UniqueId == card.UniqueId))
        {
            _currentPlayer.Hand.Remove(card);
        }
        else if (_currentPlayer.Agents.Any(agent => agent.RepresentingCard.UniqueId == card.UniqueId))
        {
            var toRemove = _currentPlayer.Agents.First(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
            _currentPlayer.Agents.Remove(toRemove);
        }
        else // if not in hand and agents, then it must be in a played pile
        {
            _currentPlayer.Played.Remove(card);
        }

        var prestigeGainAmount = card.Cost - 1;
        _currentPlayer.PrestigeAmount += prestigeGainAmount;

        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DESTROY_CARD, PatronId.HLAALU, card));
        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.GAIN_PRESTIGE, PatronId.HLAALU, prestigeGainAmount));

        return new Success();
    }

    public PlayResult CompletePelin(Choice sourceChoice, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new EngineException("Pelin requires exactly 1 choice.");
        }

        var choice = choices.First();
        _currentPlayer.CooldownPile.Remove(choice);
        _currentPlayer.DrawPile.Insert(0, choice);

        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.REFRESH, PatronId.PELIN, choice));

        return new Success();
    }

    public PlayResult CompletePsijic(Choice sourceChoice, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new EngineException("Psijic requires exactly 1 choice.");
        }

        var choice = choices.First();
        _enemyPlayer.KnockOut(choice, _tavern);

        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.KNOCKOUT, PatronId.PSIJIC, choice));

        return new Success();
    }

    public PlayResult CompleteTreasury(Choice _, List<UniqueCard> choices)
    {
        if (choices.Count != 1)
        {
            throw new EngineException("Treasury requires exactly 1 choice.");
        }

        var writOfCoin = GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN);
        var choice = choices.First();
        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DESTROY_CARD, PatronId.TREASURY, choice));
        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.ADD_WRIT_OF_COIN, PatronId.TREASURY, 1, writOfCoin));
        if (_currentPlayer.Played.Contains(choice))
        {
            _currentPlayer.Played.Remove(choice);
            _currentPlayer.CooldownPile.Add(writOfCoin);
        }
        else if (_currentPlayer.Agents.Any(agent => agent.RepresentingCard.UniqueId == choice.UniqueId))
        {
            var toRemove = _currentPlayer.Agents.First(agent => agent.RepresentingCard.UniqueId == choice.UniqueId);
            _currentPlayer.Agents.Remove(toRemove);
            _currentPlayer.CooldownPile.Add(writOfCoin);
        }
        else
        {
            _currentPlayer.Hand.Remove(choice);
            _currentPlayer.CooldownPile.Add(writOfCoin);
        }
        return new Success();
    }

    public PlayResult Donate(Choice choice, List<UniqueCard> choices)
    {
        choices.ForEach(_currentPlayer.Discard);
        choices.ForEach(c => _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DISCARD, choice.Context!.CardSource!, c)));
        var cardsToDraw = choices.Count();
        _currentPlayer.Draw(cardsToDraw);
        _parent.AddToCompletedActionsList(new CompletedAction(_currentPlayer.ID, CompletedActionType.DRAW, choice.Context!.CardSource!, cardsToDraw));
        return new Success();
    }
    private void handleSaintAlessiaTriggers(List<UniqueCard> knockoutedCards)
    {
        void HandleForPlayer(IPlayer player)
        {
            var morihausAgents = player.AgentCards
                .Where(c => c.CommonId == CardId.MORIHAUS_SACRED_BULL || c.CommonId == CardId.MORIHAUS_THE_ARCHER)
                .ToList();

            foreach (var triggerCard in morihausAgents)
            {
                if (knockoutedCards.Any(c => c.UniqueId == triggerCard.UniqueId))
                    continue;

                player.CoinsAmount++;
                _parent.AddToCompletedActionsList(new CompletedAction(player.ID, CompletedActionType.GAIN_COIN, triggerCard, 1));
            }
        }

        HandleForPlayer(_currentPlayer);
        HandleForPlayer(_enemyPlayer);
    }
}
