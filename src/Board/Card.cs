using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfTribute
{   
    public enum CardType
    {
        Action,
        Agent,
        ContractAction,
        ContractAgent,
        Starter,
        Curse
    }
    public class Card
    {
        public string Name;
        public string? Deck;
        public int InstanceID;
        public int Cost;
        public readonly CardType Type;
        public int HP; // >=0 for Agent, -1 for other types
        public Effect[] Effects; // 0th - On activation, 1st - combo 2, 2nd - combo 3, 3rd - combo 4
        public int Hash;
        public int UpgradeCardID;
        public int Copies;
        public int CopiesUpgraded;
        public int Family;

        public Card()
        {
            Name = "null card";
            Effects = new Effect[4];
        }

        public Card(string name, string? deck, int instanceID, int cost, CardType type, int hp, Effect[] effects, int hash, int family)
        {
            Name = name;
            Deck = deck;
            InstanceID = instanceID;
            Cost = cost;
            Type = type;
            HP = hp;
            Effects = effects;
            Hash = hash;
            Family = family;
        }

        public Card(string name, string? deck, int instanceID, int cost, CardType type, int hp, Effect[] effects, int hash, int upgradeCardID, int copies, int copiesUpgraded)
        {
            Name = name;
            Deck = deck;
            InstanceID = instanceID;
            Cost = cost;
            Type = type;
            HP = hp;
            Effects = effects;
            Hash = hash;
            UpgradeCardID = upgradeCardID;
            Copies = copies;
            CopiesUpgraded = copiesUpgraded;
        }

        public override string ToString()
        {
            return String.Format($"Card: {this.Name}, " +
                $"Deck: {this.Deck}, Cost: {this.Cost}, Type: {this.Type}, " +
                $"Effects: {String.Join(", ", this.Effects.Select(p => p.ToString()).ToArray())}");
        }
    }
}
