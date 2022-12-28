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
        public PlayResult Enact(IPlayer player, IPlayer enemy, ITavern tavern);
    }

    public interface ComplexEffect
    {
        public List<BaseEffect> Decompose();

        public ComplexEffect MakeUniqueCopy(UniqueId uniqueId);
    }

    public class Effect : BaseEffect, ComplexEffect, IChoosable
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

        public PlayResult Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            ChoiceContext? context;
            switch (Type)
            {
                case EffectType.GAIN_POWER:
                    player.PowerAmount += Amount;
                    break;

                case EffectType.ACQUIRE_TAVERN:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(tavern.GetAffordableCards(Amount),
                            ChoiceFollowUp.ACQUIRE_CARDS,
                            context);
                    }
                case EffectType.GAIN_COIN:
                    player.CoinsAmount += Amount;
                    break;
                case EffectType.GAIN_PRESTIGE:
                    player.PrestigeAmount += Amount;
                    break;
                case EffectType.OPP_LOSE_PRESTIGE:
                    if (enemy.PrestigeAmount - Amount >= 0)
                        enemy.PrestigeAmount -= Amount;
                    else
                        enemy.PrestigeAmount = 0;
                    break;
                case EffectType.REPLACE_TAVERN:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(
                            tavern.AvailableCards,
                            ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN,
                            context,
                            Amount
                        );
                    }
                case EffectType.DESTROY_CARD:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(
                            player.Hand.Concat(player.AgentCards).ToList(),
                            ChoiceFollowUp.DESTROY_CARDS,
                            context,
                            Amount
                        );
                    }
                case EffectType.DRAW:
                    for (var i = 0; i < Amount; i++)
                        player.Draw();
                    break;
                case EffectType.OPP_DISCARD:
                    {
                        var howManyToDiscard = Amount > player.Hand.Count ? player.Hand.Count : Amount;

                        if (howManyToDiscard == 0)
                        {
                            return new Success();
                        }

                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;

                        return new Choice<Card>(
                            player.Hand,
                            ChoiceFollowUp.DISCARD_CARDS,
                            context,
                            howManyToDiscard,
                            howManyToDiscard
                        );
                    }
                case EffectType.RETURN_TOP:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(
                            player.CooldownPile,
                            ChoiceFollowUp.REFRESH_CARDS,
                            context,
                            Amount
                        );
                    }
                case EffectType.TOSS:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(
                            player.DrawPile,
                            ChoiceFollowUp.TOSS_CARDS,
                            context,
                            Amount > player.DrawPile.Count ? player.DrawPile.Count : Amount
                        );
                    }
                case EffectType.KNOCKOUT:
                    {
                        context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.CARD_EFFECT, Combo) : null;
                        return new Choice<Card>(
                            enemy.AgentCards,
                            ChoiceFollowUp.KNOCKOUT_AGENTS,
                            context,
                            Amount > enemy.AgentCards.Count ? enemy.AgentCards.Count : Amount
                        );
                    }
                case EffectType.PATRON_CALL:
                    player.PatronCalls += (uint)Amount;
                    break;
                case EffectType.CREATE_BOARDINGPARTY:
                    for (var i = 0; i < Amount; i++)
                        player.AddToCooldownPile(GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY));
                    break;
                case EffectType.HEAL:
                    if (UniqueId == UniqueId.Empty)
                    {
                        throw new Exception("This shouldn't happen - there is a bug in the engine!");
                    }

                    player.HealAgent(UniqueId, Amount);
                    break;
                default:
                    throw new Exception("Not implemented yet!");
            }

            return new Success();
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

        public PlayResult Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            var context = this.UniqueId != UniqueId.Empty ? new ChoiceContext(this.UniqueId, ChoiceType.EFFECT_CHOICE, Combo) : null;
            return new Choice<Effect>(new List<Effect> { _left, _right },
                ChoiceFollowUp.ENACT_CHOSEN_EFFECT,
                context,
                1, 1); // OR choice should always result in one choice
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
