using TalesOfTribute.Board;

namespace TalesOfTribute
{
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
        TOSS,
        KNOCKOUT,
        PATRON_CALL,
        CREATE_BOARDINGPARTY,
        HEAL,
    }

    public interface BaseEffect
    {
        public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern);
    }

    public interface ComplexEffect
    {
        public List<BaseEffect> Decompose();

        public ComplexEffect MakeUniqueCopy(UniqueId uniqueId);
    }

    public class Effect : BaseEffect, ComplexEffect
    {
        public readonly EffectType Type;
        public readonly int Amount;
        public readonly int Combo;
        public UniqueId UniqueId { get; } = UniqueId.Empty;

        public Effect(EffectType type, int amount, int combo)
        {
            Type = type;
            Amount = amount;
            Combo = combo;
        }

        public Effect(EffectType type, int amount, int combo, UniqueId uniqueId)
        {
            Type = type;
            Amount = amount;
            Combo = combo;
            UniqueId = uniqueId;
        }

        public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            ChoiceContext? context;
            switch (Type)
            {
                case EffectType.GAIN_POWER:
                    player.PowerAmount += Amount;
                    return (new Success(),
                        new List<CompletedAction>
                        {
                            new(CompletedActionType.GAIN_POWER,
                                GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                        });

                case EffectType.ACQUIRE_TAVERN:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
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
                            new(CompletedActionType.GAIN_COIN,
                                GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                        });
                case EffectType.GAIN_PRESTIGE:
                    player.PrestigeAmount += Amount;
                    return (new Success(), new List<CompletedAction>
                    {
                        new(CompletedActionType.GAIN_PRESTIGE,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
                case EffectType.OPP_LOSE_PRESTIGE:
                    if (enemy.PrestigeAmount - Amount >= 0)
                        enemy.PrestigeAmount -= Amount;
                    else
                        enemy.PrestigeAmount = 0;
                    return (new Success(), new List<CompletedAction>
                    {
                        new(CompletedActionType.OPP_LOSE_PRESTIGE,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
                case EffectType.REPLACE_TAVERN:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return (new Choice(
                            tavern.AvailableCards,
                            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN,
                            context,
                            Amount
                        ), new List<CompletedAction>());
                    }
                case EffectType.DESTROY_CARD:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return (new Choice(
                            player.Hand.Concat(player.AgentCards).ToList(),
                            ChoiceFollowUp.DESTROY_CARDS,
                            context,
                            Amount
                        ), new List<CompletedAction>());
                    }
                case EffectType.DRAW:
                    for (var i = 0; i < Amount; i++)
                        player.Draw();
                    return (new Success(), new List<CompletedAction>
                    {
                        new(CompletedActionType.DRAW,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
                case EffectType.OPP_DISCARD:
                    {
                        var howManyToDiscard = Amount > player.Hand.Count ? player.Hand.Count : Amount;

                        if (howManyToDiscard == 0)
                        {
                            return (new Success(), new List<CompletedAction>());
                        }

                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;

                        return (new Choice(
                            player.Hand,
                            ChoiceFollowUp.DISCARD_CARDS,
                            context,
                            howManyToDiscard,
                            howManyToDiscard
                        ), new List<CompletedAction>());
                    }
                case EffectType.RETURN_TOP:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return (new Choice(
                            player.CooldownPile,
                            ChoiceFollowUp.REFRESH_CARDS,
                            context,
                            Amount
                        ), new List<CompletedAction>());
                    }
                case EffectType.TOSS:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return (new Choice(
                            player.DrawPile,
                            ChoiceFollowUp.TOSS_CARDS,
                            context,
                            Amount > player.DrawPile.Count ? player.DrawPile.Count : Amount
                        ), new List<CompletedAction>());
                    }
                case EffectType.KNOCKOUT:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
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
                        new(CompletedActionType.ADD_PATRON_CALLS,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
                case EffectType.CREATE_BOARDINGPARTY:
                    for (var i = 0; i < Amount; i++)
                        player.AddToCooldownPile(GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY));
                    return (new Success(), new List<CompletedAction>
                    {
                        new(CompletedActionType.ADD_BOARDING_PARTY,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
                case EffectType.HEAL:
                    if (UniqueId == UniqueId.Empty)
                    {
                        throw new Exception("This shouldn't happen - there is a bug in the engine!");
                    }

                    player.HealAgent(UniqueId, Amount);
                    return (new Success(), new List<CompletedAction>
                    {
                        new(CompletedActionType.HEAL_AGENT,
                            GlobalCardDatabase.Instance.GetExistingCard(UniqueId), Amount)
                    });
            }

            throw new Exception($"Unknown effect - {Type}.");
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
                EffectType.CREATE_BOARDINGPARTY => $"Create {Amount} Maormer Boarding Patry cards and place it in CD pile",
                EffectType.HEAL => $"Heal selected agent by {Amount}",
                _ => ""
            };
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public ComplexEffect MakeUniqueCopy(UniqueId uniqueId)
        {
            return new Effect(Type, Amount, Combo, uniqueId);
        }

        public static EffectType MapEffectType(string effect)
        {
            return effect switch
            {
                "Coin" => EffectType.GAIN_COIN,
                "Power" => EffectType.GAIN_POWER,
                "Prestige" => EffectType.GAIN_PRESTIGE,
                "OppLosePrestige" => EffectType.OPP_LOSE_PRESTIGE,
                "Remove" => EffectType.REPLACE_TAVERN,
                "Acquire" => EffectType.ACQUIRE_TAVERN,
                "Destroy" => EffectType.DESTROY_CARD,
                "Draw" => EffectType.DRAW,
                "Discard" => EffectType.OPP_DISCARD,
                "Return" => EffectType.RETURN_TOP,
                "Toss" => EffectType.TOSS,
                "KnockOut" => EffectType.KNOCKOUT,
                "Patron" => EffectType.PATRON_CALL,
                "Create" => EffectType.CREATE_BOARDINGPARTY,
                "Heal" => EffectType.HEAL,
                _ => throw new Exception("Invalid effect type.")
            };
        }
    }

    public class EffectOr : ComplexEffect, BaseEffect
    {
        private readonly Effect _left;
        private readonly Effect _right;
        public UniqueId UniqueId { get; } = UniqueId.Empty;
        public readonly int Combo;

        public EffectOr(Effect left, Effect right, int combo)
        {
            _left = left;
            _right = right;
            Combo = combo;
        }

        public EffectOr(Effect left, Effect right, int combo, UniqueId uniqueId)
        {
            _left = left;
            _right = right;
            UniqueId = uniqueId;
            Combo = combo;
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public ComplexEffect MakeUniqueCopy(UniqueId uniqueId)
        {
            return new EffectOr(
                _left.MakeUniqueCopy(uniqueId) as Effect ?? throw new InvalidOperationException(),
                    _right.MakeUniqueCopy(uniqueId) as Effect ?? throw new InvalidOperationException(),
                Combo,
                uniqueId
                );
        }

        public (PlayResult, List<CompletedAction>) Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            var context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.EFFECT_CHOICE, Combo) : null;
            return (new Choice(new List<Effect> { _left, _right },
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

    public class EffectComposite : ComplexEffect
    {
        private readonly Effect _left;
        private readonly Effect _right;

        public UniqueId UniqueId { get; } = UniqueId.Empty;

        public EffectComposite(Effect left, Effect right)
        {
            _left = left;
            _right = right;
        }

        public EffectComposite(Effect left, Effect right, UniqueId uniqueId)
        {
            _left = left;
            _right = right;
            UniqueId = uniqueId;
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { _left, _right };
        }

        public ComplexEffect MakeUniqueCopy(UniqueId uniqueId)
        {
            return new EffectComposite(
                _left.MakeUniqueCopy(uniqueId) as Effect ?? throw new InvalidOperationException(),
                _right.MakeUniqueCopy(uniqueId) as Effect ?? throw new InvalidOperationException(),
                uniqueId
            );
        }

        public override string ToString()
        {
            return $"{this._left} AND {this._right}";
        }
    }
}
