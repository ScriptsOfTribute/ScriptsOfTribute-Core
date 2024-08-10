
using System;
using System.Linq;

using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// This was added by the competition organizers to solve the problem with agents
// using this internal `List` extension.
public static class Extensions
{
    public static T PickRandom<T>(this List<T> source, SeededRandom rng)
    {
        return source[rng.Next() % source.Count];
    }
}

public enum TierEnum
{
    /*
    S = 1000,
    A = 400,
    B = 200,
    C = 90,
    D = 40,
    */
    S = 50,
    A = 25,
    B = 10,
    C = 5,
    D = 1,
    UNKNOWN = 0,
}

public class CardTier
{
    public string Name;
    public PatronId Deck;
    public TierEnum Tier;

    public CardTier(string name, PatronId deck, TierEnum tier)
    {
        Name = name;
        Deck = deck;
        Tier = tier;
    }
}

public class CardTierList
{
    private static CardTier[] CardTierArray = {
        new CardTier("Currency Exchange", PatronId.HLAALU, TierEnum.S),
        new CardTier("Luxury Exports", PatronId.HLAALU, TierEnum.S),
        new CardTier("Oathman", PatronId.HLAALU, TierEnum.A),
        new CardTier("Ebony Mine", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Councilor", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Kinsman", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Embassy", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Marketplace", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hireling", PatronId.HLAALU, TierEnum.C),
        new CardTier("Hostile Takeover", PatronId.HLAALU, TierEnum.C),
        new CardTier("Kwama Egg Mine", PatronId.HLAALU, TierEnum.C),
        new CardTier("Customs Seizure", PatronId.HLAALU, TierEnum.D),
        new CardTier("Goods Shipment", PatronId.HLAALU, TierEnum.D),
        new CardTier("Midnight Raid", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Blood Sacrifice", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bloody Offering", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bonfire", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Briarheart Ritual", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Clan-Witch", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Elder Witch", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven Matron", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Plunder", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Spoils", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Karth Man-Hunter", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("War Song", PatronId.RED_EAGLE, TierEnum.D),
        new CardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Plunder", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Pilfer", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Scratch", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Peck", PatronId.DUKE_OF_CROWS, TierEnum.D),
        new CardTier("Conquest", PatronId.ANSEI, TierEnum.S),
        new CardTier("Grand Oratory", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hira's End", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hel Shira Herald", PatronId.ANSEI, TierEnum.A),
        new CardTier("March on Hattu", PatronId.ANSEI, TierEnum.A),
        new CardTier("Shehai Summoning", PatronId.ANSEI, TierEnum.A),
        new CardTier("Warrior Wave", PatronId.ANSEI, TierEnum.A),
        new CardTier("Ansei Assault", PatronId.ANSEI, TierEnum.B),
        new CardTier("Ansei's Victory", PatronId.ANSEI, TierEnum.B),
        new CardTier("Battle Meditation", PatronId.ANSEI, TierEnum.B),
        new CardTier("No Shira Poet", PatronId.ANSEI, TierEnum.C),
        new CardTier("Way of the Sword", PatronId.ANSEI, TierEnum.D),
        new CardTier("Prophesy", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Scrying Globe", PatronId.PSIJIC, TierEnum.S),
        new CardTier("The Dreaming Cave", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Augur's Counsel", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Relicmaster", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Sage Counsel", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Prescience", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Apprentice", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Ceporah's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Psijic's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Time Mastery", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Mainland Inquiries", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Rally", PatronId.PELIN, TierEnum.S),
        new CardTier("Siege Weapon Volley", PatronId.PELIN, TierEnum.S),
        new CardTier("The Armory", PatronId.PELIN, TierEnum.S),
        new CardTier("Banneret", PatronId.PELIN, TierEnum.A),
        new CardTier("Knight Commander", PatronId.PELIN, TierEnum.A),
        new CardTier("Reinforcements", PatronId.PELIN, TierEnum.A),
        new CardTier("Archers' Volley", PatronId.PELIN, TierEnum.B),
        new CardTier("Legion's Arrival", PatronId.PELIN, TierEnum.B),
        new CardTier("Shield Bearer", PatronId.PELIN, TierEnum.B),
        new CardTier("Bangkorai Sentries", PatronId.PELIN, TierEnum.C),
        new CardTier("Knights of Saint Pelin", PatronId.PELIN, TierEnum.C),
        new CardTier("The Portcullis", PatronId.PELIN, TierEnum.D),
        new CardTier("Fortify", PatronId.PELIN, TierEnum.D),
        new CardTier("Bag of Tricks", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Bewilderment", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Grand Larceny", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Jarring Lullaby", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Jeering Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Moonlit Illusion", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Pounce and Profit", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Prowling Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Ring's Guile", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Shadow's Slumber", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Slight of Hand", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Stubborn Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Swipe", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Twilight Revelry", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Ghostscale Sea Serpent", PatronId.ORGNUM, TierEnum.B),
        new CardTier("King Orgnum's Command", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Maormer Boarding Party", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Maormer Cutter", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Pyandonean War Fleet", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Sea Elf Raid", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Raider's Glory", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Serpent Colossus", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Serpentguard Rider", PatronId.ORGNUM, TierEnum.A),
        new CardTier("Serpentprow Schooner", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Snakeskin Freebooter", PatronId.ORGNUM, TierEnum.S),
        new CardTier("Storm Shark Wavecaller", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Summerset Sacking", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Ambush", PatronId.TREASURY, TierEnum.B),
        new CardTier("Barterer", PatronId.TREASURY, TierEnum.C),
        new CardTier("Black Sacrament", PatronId.TREASURY, TierEnum.B),
        new CardTier("Blackmail", PatronId.TREASURY, TierEnum.B),
        new CardTier("Gold", PatronId.TREASURY, TierEnum.UNKNOWN),
        new CardTier("Harvest Season", PatronId.TREASURY, TierEnum.C),
        new CardTier("Imprisonment", PatronId.TREASURY, TierEnum.C),
        new CardTier("Ragpicker", PatronId.TREASURY, TierEnum.C),
        new CardTier("Tithe", PatronId.TREASURY, TierEnum.C),
        new CardTier("Writ of Coin", PatronId.TREASURY, TierEnum.D),
        new CardTier("Unknown", PatronId.TREASURY, TierEnum.UNKNOWN)
    };

    public static TierEnum GetCardTier(string cardName)
    {
        return Array.Find(CardTierArray, x => x.Name == cardName).Tier;
    }
}
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

public class Sakkirin : AI
{
    private string patrons;
    private readonly SeededRandom rng = new(12345679);
    private bool startOfGame = true;

    public Sakkirin()
    {
        this.PrepareForGame();
    }

    private void PrepareForGame()
    {
        startOfGame = true;
    }

    private double PatronValue(PatronId patronId)
    {
        if (patronId == PatronId.ANSEI)
        {
            return 0.4;
        }
        else if (patronId == PatronId.DUKE_OF_CROWS)
        {
            return 3.5;
        }
        else if (patronId == PatronId.RAJHIN)
        {
            return 0.3;
        }
        else if (patronId == PatronId.PSIJIC)
        {
            return 2.2;
        }
        else if (patronId == PatronId.ORGNUM)
        {
            return 0.8;
        }
        else if (patronId == PatronId.HLAALU)
        {
            return 0.4;
        }
        else if (patronId == PatronId.PELIN)
        {
            return 1.0;
        }
        else if (patronId == PatronId.RED_EAGLE)
        {
            return 2.0;
        }
        else if (patronId == PatronId.TREASURY)
        {
            return 0.1;
        }

        return 0.0;
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        PatronId bestPatronId = availablePatrons[0];
        double bestScore = -10000;

        foreach (PatronId patronId in availablePatrons)
        {
            double v = this.PatronValue(patronId);
            double randomValue = (double)rng.Next() * v;
            if (randomValue > bestScore)
            {
                bestScore = randomValue;
                bestPatronId = patronId;
            }
        }
        return bestPatronId;
    }

    private double CardValue(GameState gameState, Card card) {
        double s = 0;

        TierEnum tier = CardTierList.GetCardTier(card.Name);
        //s -= (double)tier * 0.03;

        if (card.Type == CardType.ACTION)
        {
            s += 0.3;
        }
        else if (card.Type == CardType.AGENT)
        {
            s += 1.5;
            s += card.HP * 0.1;
            s -= card.Cost * 0.2;
        }
        else if (card.Type == CardType.CONTRACT_ACTION)
        {
            s += 0.1;
        }
        else if (card.Type == CardType.CONTRACT_AGENT)
        {
            s += 0.1;
        }
        else if (card.Type == CardType.STARTER)
        {
            s += 0;
        }
        else if (card.Type == CardType.CURSE)
        {
            s += 5;
        }
        return s;
    }

    private double Scoring(GameState gameState, Move move) {
        // score move
        double s = 0.0;

        var myPlayer = gameState.CurrentPlayer;
        var opponentPlayer = gameState.EnemyPlayer;

        if (move.Command == CommandEnum.PLAY_CARD)
        {
            s += 5.0;
            var cardMove = (SimpleCardMove)move;
            var card = cardMove.Card;

            if (card.CommonId == CardId.GOLD)
            {
                s += 20.0;
            }

            if (card.Effects.Length > 0)
            {
                s += 5.0;
                //Console.WriteLine(card.Effects[0].GetType());
                foreach (UniqueEffect effect in card.Effects[0].Decompose()) {
                    if (effect.Type == EffectType.GAIN_COIN)
                    {
                        s += effect.Amount * 1.0;
                    }
                    else if (effect.Type == EffectType.GAIN_POWER)
                    {
                        s += effect.Amount * 0.1;
                    }
                    else if (effect.Type == EffectType.GAIN_PRESTIGE)
                    {
                        s += effect.Amount * 0.2;
                    }
                    else if (effect.Type == EffectType.OPP_LOSE_PRESTIGE)
                    {
                        s += effect.Amount * 0.2;
                    }
                    else if (effect.Type == EffectType.REPLACE_TAVERN)
                    {
                        s += 0.01;
                    }
                    else if (effect.Type == EffectType.ACQUIRE_TAVERN)
                    {
                        s += 0.01;
                    }
                    else if (effect.Type == EffectType.DESTROY_CARD)
                    {
                        s += 0.3;
                    }
                    else if (effect.Type == EffectType.DRAW)
                    {
                        s += 1.2;
                    }
                    else if (effect.Type == EffectType.OPP_DISCARD)
                    {
                        s += 0.2;
                    }
                    else if (effect.Type == EffectType.RETURN_TOP)
                    {
                        s += 0.2;
                    }
                    else if (effect.Type == EffectType.TOSS)
                    {
                        s += 0.2;
                    }
                    else if (effect.Type == EffectType.KNOCKOUT)
                    {
                        s += 0.1;
                    }
                    else if (effect.Type == EffectType.PATRON_CALL)
                    {
                        s += 0.2;
                    }
                    else if (effect.Type == EffectType.CREATE_SUMMERSET_SACKING)
                    {
                        s += 0.02;
                    }
                    else if (effect.Type == EffectType.HEAL)
                    {
                        s += effect.Amount * 0.04;
                    }
                    s += effect.Combo * 0.5;
                }
            }
        }
        else if (move.Command == CommandEnum.ACTIVATE_AGENT)
        {
            s += 2.0;
            var cardMove = (SimpleCardMove)move;
            var card = cardMove.Card;
            s += card.HP * 0.2;
        }
        else if (move.Command == CommandEnum.ATTACK)
        {
            var cardMove = (SimpleCardMove)move;
            s += 30.0;
        }
        else if (move.Command == CommandEnum.BUY_CARD)
        {
            s += 2.0;
            var cardMove = (SimpleCardMove)move;
            var card = cardMove.Card;
            s += this.CardValue(gameState, card);
        }
        else if (move.Command == CommandEnum.CALL_PATRON)
        {
            s += 1.0;

            var patronFavours = gameState.PatronStates.All;
            var patronMove = (SimplePatronMove)move;
            if (patronFavours[patronMove.PatronId] != myPlayer.PlayerID && patronMove.PatronId != PatronId.TREASURY)
            {
                s += 30.0;
            }
        }
        else if (move.Command == CommandEnum.MAKE_CHOICE)
        {
            s += 0.4;
        }

        var (newState, newPossibleMoves) = gameState.ApplyMove(move, 0);
        if (newState.GameEndState?.Winner == Id)
        {
            //Console.WriteLine("win");
            return 10000;
        }

        var newMovesToCheck = newPossibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        var currentVal = gameState.CurrentPlayer.Prestige + gameState.CurrentPlayer.Power;
        var newVal = currentVal;

        foreach (var newMove in newMovesToCheck)
        {
            var (newestState, _) = newState.ApplyMove(newMove);
            if (newestState.GameEndState?.Winner == Id)
            {
                //Console.WriteLine("win2");
                return 10000;
            }

            newVal = Math.Max(newVal, newestState.CurrentPlayer.Prestige + newestState.CurrentPlayer.Power);
        }

        s += 0.1 * (newVal - currentVal);

        return s;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        if (startOfGame)
        {
            patrons = string.Join(",", gameState.Patrons.FindAll(x => x != PatronId.TREASURY).Select(n => n.ToString()).ToArray());
            startOfGame = false;
        }

        var movesWithoutEndTurn = possibleMoves.Where(move => move.Command != CommandEnum.END_TURN).ToList();
        if (movesWithoutEndTurn.Count == 0)
        {
            return Move.EndTurn();
        }

        double bestScore = -10000;
        Move bestMove = Move.EndTurn();

        movesWithoutEndTurn = movesWithoutEndTurn.OrderBy(a => Guid.NewGuid()).ToList();

        foreach (Move move in movesWithoutEndTurn) {
            double s = 0;
            try
            {
                // Code to try goes here.
                s = this.Scoring(gameState, move);
            }
            catch (Exception ex) {}

            //Console.WriteLine("{0} {1}", move, s);
            if (s > bestScore) {
                bestMove = move;
                bestScore = s;
            }
        }
        //Console.WriteLine(bestMove);
        return bestMove;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        this.PrepareForGame();
    }
}
