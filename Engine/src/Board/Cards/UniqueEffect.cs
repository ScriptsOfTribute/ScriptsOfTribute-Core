namespace ScriptsOfTribute.Board.Cards;

public enum EffectType
{
    GAIN_COIN,
    GAIN_POWER,
    GAIN_PRESTIGE,
    OPP_LOSE_PRESTIGE,
    REPLACE_TAVERN,
    ACQUIRE_TAVERN,
    DESTROY_CARD,
    DRAW,
    OPP_DISCARD,
    RETURN_TOP,
    RETURN_AGENT_TOP,
    TOSS,
    KNOCKOUT,
    PATRON_CALL,
    CREATE_SUMMERSET_SACKING,
    HEAL,
    KNOCKOUT_ALL,
    DONATE,
}

public interface UniqueBaseEffect
{
    public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern);
}

public interface UniqueComplexEffect
{
    public List<UniqueBaseEffect> Decompose();
}

public class UniqueEffect : Effect, UniqueBaseEffect, UniqueComplexEffect
{
    public readonly UniqueCard ParentCard;

    public UniqueEffect(EffectType type, int amount, int combo, UniqueCard parentCard) : base(type, amount, combo)
    {
        ParentCard = parentCard;
    }

    public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern)
    {
        ChoiceContext context;
        switch (Type)
        {
            case EffectType.GAIN_POWER:
                player.PowerAmount += Amount;
                return (new Success(),
                    new List<CompletedAction>
                    {
                        new(player.ID, CompletedActionType.GAIN_POWER,
                            ParentCard, Amount)
                    });

            case EffectType.ACQUIRE_TAVERN:
            {
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(tavern.GetAffordableCards(Amount),
                        ChoiceFollowUp.ACQUIRE_CARDS,
                        context),
                    new List<CompletedAction>());
            }
            case EffectType.GAIN_COIN:
                player.CoinsAmount += Amount;
                return (new Success(),
                    new List<CompletedAction>
                    {
                        new(player.ID, CompletedActionType.GAIN_COIN,
                            ParentCard, Amount)
                    });
            case EffectType.GAIN_PRESTIGE:
                player.PrestigeAmount += Amount;
                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.GAIN_PRESTIGE,
                        ParentCard, Amount)
                });
            case EffectType.OPP_LOSE_PRESTIGE:
                if (enemy.PrestigeAmount - Amount >= 0)
                    enemy.PrestigeAmount -= Amount;
                else
                    enemy.PrestigeAmount = 0;
                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.OPP_LOSE_PRESTIGE,
                        ParentCard, Amount)
                });
            case EffectType.REPLACE_TAVERN:
            {
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    tavern.AvailableCards.ToList(),
                    ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN,
                    context,
                    Amount
                ), new List<CompletedAction>());
            }
            case EffectType.DESTROY_CARD:
            {
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    player.Hand.Concat(player.AgentCards).Concat(player.Played).ToList(),
                    ChoiceFollowUp.DESTROY_CARDS,
                    context,
                    Amount
                ), new List<CompletedAction>());
            }
            case EffectType.DRAW:
                player.Draw(Amount);
                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.DRAW,
                        ParentCard, Amount)
                });
            case EffectType.OPP_DISCARD:
            {
                var howManyToDiscard = Amount > player.Hand.Count ? player.Hand.Count : Amount;

                if (howManyToDiscard == 0)
                {
                    return (new Success(), new List<CompletedAction>());
                }

                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);

                return (new Choice(
                    player.Hand.ToList(),
                    ChoiceFollowUp.DISCARD_CARDS,
                    context,
                    howManyToDiscard,
                    howManyToDiscard
                ), new List<CompletedAction>());
            }
            case EffectType.RETURN_TOP:
            {
                var amount = player.CooldownPile.Count < Amount ? player.CooldownPile.Count : Amount;
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    player.CooldownPile,
                    ChoiceFollowUp.REFRESH_CARDS,
                    context,
                    amount,
                    0
                ), new List<CompletedAction>());
            }
            case EffectType.TOSS:
            {
                var tossableCards = player.PrepareToss(Amount);
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    tossableCards,
                    ChoiceFollowUp.TOSS_CARDS,
                    context,
                    tossableCards.Count
                ), new List<CompletedAction>());
            }
            case EffectType.KNOCKOUT:
            {
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    enemy.AgentCards,
                    ChoiceFollowUp.KNOCKOUT_AGENTS,
                    context,
                    Amount > enemy.AgentCards.Count ? enemy.AgentCards.Count : Amount,
                    Amount > enemy.AgentCards.Count ? enemy.AgentCards.Count : Amount
                ), new List<CompletedAction>());
            }
            case EffectType.PATRON_CALL:
                player.PatronCalls += (uint)Amount;
                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.ADD_PATRON_CALLS,
                        ParentCard, Amount)
                });
            case EffectType.CREATE_SUMMERSET_SACKING:
                for (var i = 0; i < Amount; i++)
                    player.AddToCooldownPile(GlobalCardDatabase.Instance.GetCard(CardId.SUMMERSET_SACKING));
                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.ADD_SUMMERSET_SACKING,
                        ParentCard, Amount)
                });
            case EffectType.HEAL:
                var healAmount = player.HealAgent(ParentCard, Amount);
                if (healAmount < 0)
                {
                    return (new Success(), new List<CompletedAction>());
                }

                return (new Success(), new List<CompletedAction>
                {
                    new(player.ID, CompletedActionType.HEAL_AGENT,
                        ParentCard, healAmount, ParentCard)
                });
            case EffectType.KNOCKOUT_ALL:
                player.KnockOutAll(tavern);
                enemy.KnockOutAll(tavern);
                return (
                    new Success(), new List<CompletedAction>
                    {
                        new(player.ID, CompletedActionType.KNOCKOUT_ALL, ParentCard)
                    }
                );
            case EffectType.RETURN_AGENT_TOP:
                var agentsList = player.CooldownPile.Where(card => card.Type == CardType.AGENT).ToList();
                var agentsCount = agentsList.Count < Amount ? agentsList.Count : Amount;
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    agentsList,
                    ChoiceFollowUp.REFRESH_CARDS,
                    context,
                    agentsCount,
                    0
                ), new List<CompletedAction>());
            case EffectType.DONATE:
            {
                context = new ChoiceContext(this.ParentCard, ChoiceType.CARD_EFFECT, Combo);
                return (new Choice(
                    player.Hand,
                    ChoiceFollowUp.DONATE,
                    context,
                    Amount
                ), new List<CompletedAction>());
            }
        }

        throw new EngineException($"Unknown effect - {Type}.");
    }

    public override string ToString()
    {
        return Type switch
        {
            EffectType.GAIN_COIN => $"Coin +{Amount}",
            EffectType.GAIN_PRESTIGE => $"Prestige +{Amount}",
            EffectType.GAIN_POWER => $"Power +{Amount}",
            EffectType.OPP_LOSE_PRESTIGE => $"Enemy prestige -{Amount}",
            EffectType.REPLACE_TAVERN => $"Replace {Amount} cards in tavern",
            EffectType.ACQUIRE_TAVERN => $"Get card from tavern with cost up to {Amount}",
            EffectType.DESTROY_CARD => $"Destroy {Amount} cards in play",
            EffectType.DRAW => $"Draw {Amount} cards",
            EffectType.OPP_DISCARD => $"Enemy discards {Amount} cards from hand at start of their turn",
            EffectType.RETURN_TOP => $"Put {Amount} cards from cooldown pile on top of draw pile",
            EffectType.TOSS => $"Choose up to {Amount} of draw pile cards to move to your cooldown pile.",
            EffectType.KNOCKOUT => $"Knockout {Amount} enemy agents",
            EffectType.PATRON_CALL => $"Get {Amount} patron calls",
            EffectType.CREATE_SUMMERSET_SACKING => $"Create {Amount} Summerset Sacking cards and place it in CD pile",
            EffectType.HEAL => $"Heal this agent by {Amount}",
            EffectType.DONATE => $"Discard up to {Amount} cards from hand, draw {Amount} cards",
            EffectType.KNOCKOUT_ALL => $"Knockout all agents",
            _ => ""
        };
    }

    public List<UniqueBaseEffect> Decompose()
    {
        return new List<UniqueBaseEffect> { this };
    }

}

public class UniqueEffectOr : EffectOr, UniqueComplexEffect, UniqueBaseEffect
{
    private readonly UniqueEffect _left;
    private readonly UniqueEffect _right;
    public readonly UniqueCard ParentCard;

    public UniqueEffectOr(UniqueEffect left, UniqueEffect right, int combo, UniqueCard card) : base(left, right, combo)
    {
        _left = left;
        _right = right;
        ParentCard = card;
    }

    public List<UniqueBaseEffect> Decompose()
    {
        return new List<UniqueBaseEffect> { this };
    }

    public UniqueEffect GetLeft() { return _left; }
    public UniqueEffect GetRight() { return _right; }
    public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern)
    {
        var context = new ChoiceContext(ParentCard, ChoiceType.EFFECT_CHOICE, Combo);
        return (new Choice(new List<UniqueEffect> { _left, _right },
                ChoiceFollowUp.ENACT_CHOSEN_EFFECT,
                context,
                1, 1), // OR choice should always result in one choice
            new List<CompletedAction>());
    }

    public override string ToString()
    {
        return $"{this._left} OR {this._right}";
    }
}

public class UniqueEffectComposite : EffectComposite, UniqueComplexEffect
{
    private readonly UniqueEffect _left;
    private readonly UniqueEffect _right;

    public readonly UniqueCard ParentCard;

    public UniqueEffectComposite(UniqueEffect left, UniqueEffect right, UniqueCard parentCard) : base(left, right)
    {
        _left = left;
        _right = right;
        ParentCard = parentCard;
    }

    public List<UniqueBaseEffect> Decompose()
    {
        return new List<UniqueBaseEffect> { _left, _right };
    }

    public UniqueEffect GetLeft() { return _left; }
    public UniqueEffect GetRight() { return _right; }

    public override string ToString()
    {
        return $"{this._left} AND {this._right}";
    }
}
