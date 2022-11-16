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

        public ComplexEffect MakeUniqueCopy(Guid guid);
    }

    public class Effect : BaseEffect, ComplexEffect
    {
        public readonly EffectType Type;
        public readonly int Amount;
        public Guid Guid { get; } = Guid.Empty;

        public Effect(EffectType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
        
        public Effect(EffectType type, int amount, Guid guid)
        {
            Type = type;
            Amount = amount;
            Guid = guid;
        }

        public PlayResult Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            switch (Type)
            {
                case EffectType.GAIN_POWER:
                    player.PowerAmount += Amount;
                    break;
                
                case EffectType.ACQUIRE_TAVERN:
                    return new Choice<CardId>(tavern.GetAffordableCards(Amount).Select(card => card.Id).ToList(),
                        choiceList =>
                        {
                            var choice = choiceList.First();
                            var card = tavern.Acquire(choice);
                            player.HandleAcquireDuringExecutionChain(card, enemy, tavern);
                            return new Success();
                        });
                case EffectType.GAIN_COIN:
                    player.CoinsAmount += Amount;
                    break;
                case EffectType.GAIN_PRESTIGE:
                    player.PrestigeAmount += Amount;
                    break;
                case EffectType.OPP_LOSE_PRESTIGE:
                    enemy.PrestigeAmount -= Amount;
                    break;
                case EffectType.REPLACE_TAVERN:
                    return new Choice<CardId>(
                        tavern.AvailableCards.Select(card => card.Id).ToList(),
                        Amount,
                        choices =>
                        {
                            choices.ForEach(tavern.ReplaceCard);
                            return new Success();
                        });
                case EffectType.DESTROY_CARD:
                    // TODO: Fix this in the future when we decide how to Guid etc.
                    return new Choice<Location>(
                        new List<Location> { Location.HAND, Location.BOARD },
                        choiceList =>
                        {
                            var choice = choiceList.First();
                            if (choice == Location.HAND)
                            {
                                return new Choice<CardId>(
                                    player.Hand.Select(card => card.Id).ToList(),
                                    Amount,
                                    choices =>
                                    {
                                        choices.ForEach(player.DestroyInHand);
                                        return new Success();
                                    }
                                );
                            }
                            
                            // TODO: This is actually not correct, because there can be multiple
                            // agents with the same card id. Fix this when agents are implemented.
                            return new Choice<CardId>(
                                player.Agents.Select(card => card.Id).ToList(),
                                Amount,
                                choices =>
                                {
                                    choices.ForEach(player.DestroyAgent);
                                    return new Success();
                                }
                            );
                        });
                case EffectType.DRAW:
                    for (var i = 0; i < Amount; i++)
                        player.Draw();
                    break;
                case EffectType.OPP_DISCARD:
                    // TODO: Implement this.
                    break;
                case EffectType.RETURN_TOP:
                    return new Choice<CardId>(
                        player.CooldownPile.Select(card => card.Id).ToList(),
                        Amount,
                        choices =>
                        {
                            choices.ForEach(player.Refresh);
                            return new Success();
                        }
                        );
                case EffectType.TOSS:
                    return new Choice<CardId>(
                        player.DrawPile.Select(card => card.Id).Take(Amount).ToList(),
                        Amount > player.DrawPile.Count ? player.DrawPile.Count : Amount,
                        choices =>
                        {
                            choices.ForEach(player.Toss);
                            return new Success();
                        }
                    );
                case EffectType.KNOCKOUT:
                    return new Choice<CardId>(
                        enemy.Agents.Select(card => card.Id).ToList(),
                        Amount > enemy.Agents.Count ? enemy.Agents.Count : Amount,
                        choices =>
                        {
                            choices.ForEach(enemy.KnockOut);
                            return new Success();
                        }
                    );
                case EffectType.PATRON_CALL:
                    player.PatronCalls += (uint)Amount;
                    break;
                case EffectType.CREATE_BOARDINGPARTY:
                    for (var i = 0; i < Amount; i++)
                        player.AddToCooldownPile(GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY));
                    break;
                case EffectType.HEAL:
                    if (Guid == Guid.Empty)
                    {
                        throw new Exception("This shouldn't happen - there is a bug in the engine!");
                    }

                    player.HealAgent(Guid, Amount);
                    break;
                default:
                    throw new Exception("Not implemented yet!");
            }

            return new Success();
        }

        public override string ToString()
        {
            return $"Effect: {this.Type} {this.Amount}";
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public ComplexEffect MakeUniqueCopy(Guid guid)
        {
            return new Effect(Type, Amount, guid);
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

    public class EffectChoice : ComplexEffect, BaseEffect
    {
        private readonly Effect _left;
        private readonly Effect _right;
        public Guid Guid { get; } = Guid.Empty;

        public EffectChoice(Effect left, Effect right)
        {
            _left = left;
            _right = right;
        }

        public EffectChoice(Effect left, Effect right, Guid guid)
        {
            _left = left;
            _right = right;
            Guid = guid;
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { this };
        }

        public ComplexEffect MakeUniqueCopy(Guid guid)
        {
            return new EffectChoice(
                _left.MakeUniqueCopy(guid) as Effect ?? throw new InvalidOperationException(),
                    _right.MakeUniqueCopy(guid) as Effect ?? throw new InvalidOperationException(),
                guid
                );
        }

        public PlayResult Enact(IPlayer player, IPlayer enemy, ITavern tavern)
        {
            return new Choice<EffectType>(new List<EffectType> { _left.Type, _right.Type },
                choice =>
                {
                    if (choice.First() == _left.Type)
                    {
                        return _left.Enact(player, enemy, tavern);
                    }

                    return _right.Enact(player, enemy, tavern);
                });
        }
    }

    public class EffectComposite : ComplexEffect
    {
        private readonly Effect _left;
        private readonly Effect _right;

        public Guid Guid { get; } = Guid.Empty;
        
        public EffectComposite(Effect left, Effect right)
        {
            _left = left;
            _right = right;
        }

        public EffectComposite(Effect left, Effect right, Guid guid)
        {
            _left = left;
            _right = right;
            Guid = guid;
        }

        public List<BaseEffect> Decompose()
        {
            return new List<BaseEffect> { _left, _right };
        }

        public ComplexEffect MakeUniqueCopy(Guid guid)
        {
            return new EffectComposite(
                _left.MakeUniqueCopy(guid) as Effect ?? throw new InvalidOperationException(),
                _right.MakeUniqueCopy(guid) as Effect ?? throw new InvalidOperationException(),
                guid
            );
        }
    }
}
