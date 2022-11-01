using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfTribute
{
    public class Player
    {
        public int Id;
        public uint CoinsAmount;
        public uint PrestigeAmount;
        public uint PowerAmount;
        public List<Card> Hand;
        public List<Card> Draw;
        public List<Card> Played;
        public List<Card> Agents;
        public List<Card> CooldownPile;
        public uint ForcedDiscard;
        public uint PatronCalls;
        public long ShuffleSeed;

        public Player()
        {
            Hand = new List<Card>();
            Draw = new List<Card>();
            Played = new List<Card>();
            Agents = new List<Card>();
            CooldownPile = new List<Card>();
        }

        public Player(int id, uint coinsAmount, uint prestigeAmount, uint powerAmount, List<Card> hand, List<Card> draw, List<Card> played, List<Card> agents, List<Card> cooldownPile)
        {
            Id = id;
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            Draw = draw;
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
        }

        public Player(uint coinsAmount, uint prestigeAmount, uint powerAmount, List<Card> hand, List<Card> draw, List<Card> played, List<Card> agents, List<Card> cooldownPile, uint forcedDiscard, uint patronCalls, long shuffleSeed)
        {
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            Draw = draw;
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
            ForcedDiscard = forcedDiscard;
            PatronCalls = patronCalls;
            ShuffleSeed = shuffleSeed;
        }

        public override string ToString()
        {
            return String.Format("Player: ({0}, {1}, {2})", this.CoinsAmount, this.PrestigeAmount, this.PowerAmount);
        }
    }
}
