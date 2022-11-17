﻿namespace TalesOfTribute
{
    public enum CardType
    {
        ACTION,
        AGENT,
        CONTRACT_ACTION,
        CONTRACT_AGENT,
        STARTER,
        CURSE
    }

    public struct Card
    {
        public string Name;
        public PatronId Deck;
        public CardId Id;
        public int Cost;
        public readonly CardType Type;
        public int HP; // >=0 for Agent, -1 for other types
        public int CurrentHP;
        public bool Activated;
        public bool Taunt;
        public ComplexEffect?[] Effects; // 0th - On activation, 1st - combo 2, 2nd - combo 3, 3rd - combo 4
        public int Hash;
        public CardId? Family;

        public Card(string name, PatronId deck, CardId id, int cost, CardType type, int hp, ComplexEffect?[] effects, int hash, CardId? family, bool taunt)
        {
            Name = name;
            Deck = deck;
            Id = id;
            Cost = cost;
            Type = type;
            HP = hp;
            CurrentHP = hp;
            Activated = false;
            Taunt = taunt;
            Effects = effects;
            Hash = hash;
            Family = family;
        }

        public override string ToString()
        {
            return String.Format($"Card: {this.Name}, " +
                $"Deck: {this.Deck}, Cost: {this.Cost}, Type: {this.Type}");
        }


    }
}
