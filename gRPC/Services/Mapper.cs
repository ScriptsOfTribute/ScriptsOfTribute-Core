using System;
using System.Reflection;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTributeGRPC;

public class Mapper
{
    public static Card MapCard(UniqueCard card)
    {
        var grpcCard =  new Card
        {
            UniqueId = card.UniqueId.Value,
            Name = card.Name,
            Deck = (PatronId)card.Deck,
            CommonId = (CardId)card.CommonId,
            Cost = card.Cost,
            Type = (CardType)card.Type,
            HP = card.HP,
            Hash = card.Hash,
            Family = (CardId)card.Family,
            Taunt = card.Taunt,
            Copies = card.Copies,
        };
        grpcCard.Effects.AddRange(card.Effects.Select(eff => MapEffect(eff, grpcCard)).ToList());
        return grpcCard;
    }

    private static ComplexEffect MapEffect(ScriptsOfTribute.Board.Cards.UniqueComplexEffect? effect, Card parentCard)
    {
        if (effect == null)
        {
            return new ComplexEffect
            {
                IsEmpty = true
            };
        }
        var complexEffect = new ComplexEffect();
        if (effect is ScriptsOfTribute.Board.Cards.Effect baseEffect)
        {
            complexEffect.SingleEffect = MapSingleEffect(baseEffect, parentCard);
        }
        else if (effect is ScriptsOfTribute.Board.Cards.EffectOr orEffect)
        {
            complexEffect.AlternativeEffect = new EffectOr
            {
                Left = MapSingleEffect(orEffect.GetLeft(), parentCard),
                Right = MapSingleEffect(orEffect.GetRight(), parentCard),
            };
            complexEffect.AlternativeEffect.ParentCard = parentCard;
        }
        else if (effect is ScriptsOfTribute.Board.Cards.EffectComposite andEffect)
        {
            complexEffect.CompositeEffect = new EffectAnd
            {
                Left = MapSingleEffect(andEffect.GetLeft(), parentCard),
                Right = MapSingleEffect(andEffect.GetRight(), parentCard),
            };
            complexEffect.AlternativeEffect.ParentCard = parentCard;
        }

        return complexEffect;
    }

    private static Effect MapSingleEffect(ScriptsOfTribute.Board.Cards.Effect baseEffect, Card parentCard)
    {
        return new Effect
        {
            Type = (EffectType)baseEffect.Type,
            Amount = baseEffect.Amount,
            Combo = baseEffect.Combo,
            ParentCard = parentCard
        };
    }
    
}
