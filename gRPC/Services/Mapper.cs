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
            Family = card.Family != null ? (CardId)card.Family : CardId.WritOfCoin,
            Taunt = card.Taunt,
            Copies = card.Copies,
        };
        var effs = card.Effects.Select(eff => MapEffect(eff, card.UniqueId.Value)).ToList();
        grpcCard.Effects.AddRange(effs);
        return grpcCard;
    }

    private static ComplexEffect MapEffect(ScriptsOfTribute.Board.Cards.UniqueComplexEffect? effect, int parentCardID)
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
            complexEffect.SingleEffect = MapSingleEffect(baseEffect, parentCardID);
        }
        else if (effect is ScriptsOfTribute.Board.Cards.EffectOr orEffect)
        {
            complexEffect.AlternativeEffect = new EffectOr
            {
                Left = MapSingleEffect(orEffect.GetLeft(), parentCardID),
                Right = MapSingleEffect(orEffect.GetRight(), parentCardID),
            };
            complexEffect.AlternativeEffect.ParentCardID = parentCardID;
        }
        else if (effect is ScriptsOfTribute.Board.Cards.EffectComposite andEffect)
        {
            complexEffect.CompositeEffect = new EffectAnd
            {
                Left = MapSingleEffect(andEffect.GetLeft(), parentCardID),
                Right = MapSingleEffect(andEffect.GetRight(), parentCardID),
            };
            complexEffect.AlternativeEffect.ParentCardID = parentCardID;
        }

        return complexEffect;
    }

    private static Effect MapSingleEffect(ScriptsOfTribute.Board.Cards.Effect baseEffect, int parentCardID)
    {
        return new Effect
        {
            Type = (EffectType)baseEffect.Type,
            Amount = baseEffect.Amount,
            Combo = baseEffect.Combo,
            ParentCardID = parentCardID
        };
    }
    
}
