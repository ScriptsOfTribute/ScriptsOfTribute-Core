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

    public PlayResult AcquireTavern(Card choice)
    {
        var card = _tavern.Acquire(choice);
        
        _parent.ImmediatePlayCard(card);
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

    public PlayResult CompleteEffectChoice(Effect left, Effect right, List<EffectType> choices)
    {
        if (choices.First() == left.Type)
        {
            return left.Enact(_currentPlayer, _enemyPlayer, _tavern);
        }

        return right.Enact(_currentPlayer, _enemyPlayer, _tavern);
    }

    public PlayResult CompleteHlaalu(Card card)
    {
        _currentPlayer.Hand.Remove(card);
        _currentPlayer.PrestigeAmount += card.Cost - 1;
        return new Success();
    }

    public PlayResult CompletePelin(Card choice)
    {
        _currentPlayer.CooldownPile.Remove(choice);
        _currentPlayer.DrawPile.Insert(0, choice);
        return new Success();
    }

    public PlayResult CompletePsijic(Card choice)
    {
        _enemyPlayer.Destroy(choice);
        _enemyPlayer.CooldownPile.Add(choice);
        return new Success();
    }

    public PlayResult CompleteTreasury(Card choice)
    {
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
