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
        public int DeckID;
        public int InstanceID;
        public uint Cost;
        public readonly CardType Type;
        public int HP; // >=0 for Agent, -1 for other types
        public bool Taunt;
        public List<Effect> Effects;
        public int Hash;
        public int UpgradeCardID;
        public int Copies;
        public int CopiesUpgraded;

        public Card()
        {
            Name = "null card";
            Effects = new List<Effect>();
        }

        public Card(string name, int deckID, int instanceID, uint cost, CardType type, int hp, bool taunt, List<Effect> effects, int hash)
        {
            Name = name;
            DeckID = deckID;
            InstanceID = instanceID;
            Cost = cost;
            Type = type;
            HP = hp;
            Taunt = taunt;
            Effects = effects;
            Hash = hash;
        }

        public Card(string name, int deckID, int instanceID, uint cost, CardType type, int hp, bool taunt, List<Effect> effects, int hash, int upgradeCardID, int copies, int copiesUpgraded)
        {
            Name = name;
            DeckID = deckID;
            InstanceID = instanceID;
            Cost = cost;
            Type = type;
            HP = hp;
            Taunt = taunt;
            Effects = effects;
            Hash = hash;
            UpgradeCardID = upgradeCardID;
            Copies = copies;
            CopiesUpgraded = copiesUpgraded;
        }

        public override string ToString()
        {
            return String.Format("Card: {0}, Cost: {1}, Type: {2}, Effects: {3}", this.Name, this.Cost, this.Type, String.Join(", ", this.Effects));
        }
    }
}
