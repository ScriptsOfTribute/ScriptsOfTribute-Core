using TalesOfTribute.Serializers;

namespace TalesOfTribute
{
    public enum PlayerEnum
    {
        PLAYER1 = 0,
        PLAYER2 = 1,
        NO_PLAYER_SELECTED = 2,
    }

    public class Player : IPlayer
    {
        public PlayerEnum ID { get; set; }
        public int CoinsAmount { get; set; }
        public int PrestigeAmount { get; set; }
        public int PowerAmount { get; set; }
        public List<Card> Hand { get; set; }
        public List<Card> DrawPile { get; set; }
        public List<Card> Played { get; set; }
        public List<Agent> Agents { get; set; }
        public List<Card> AgentCards => Agents.Select(agent => agent.RepresentingCard).ToList();
        public List<Card> CooldownPile { get; set; }
        public uint PatronCalls { get; set; }
        private readonly SeededRandom _rnd;

        public Player(PlayerEnum iD, SeededRandom rnd)
        {
            CoinsAmount = 0;
            PrestigeAmount = 0;
            PowerAmount = 0;
            Hand = new List<Card>();
            DrawPile = new List<Card>();
            Played = new List<Card>();
            Agents = new List<Agent>();
            CooldownPile = new List<Card>();
            ID = iD;
            PatronCalls = 1;
            _rnd = rnd;
        }

        public void PlayCard(Card card)
        {
            AssertCardIn(card, Hand);
            Hand.Remove(card);
            if (card.Type == CardType.AGENT)
            {
                var agent = Agent.FromCard(card);
                agent.MarkActivated();
                Agents.Add(agent);
            }
            else
            {
                Played.Add(card);
            }
        }

        public void InitDrawPile(List<Card> starterCards)
        {
            DrawPile = starterCards.OrderBy(_ => _rnd.Next()).ToList();
        }

        public int HealAgent(UniqueId uniqueId, int amount)
        {
            // It's possible for this agent to already be gone, for example - discarded, so we need to check.
            if (Agents.All(a => a.RepresentingCard.UniqueId != uniqueId)) return -1;

            var agent = Agents.First(agent => agent.RepresentingCard.UniqueId == uniqueId);
            agent.Heal(amount);
            return amount;
        }

        public void Discard(Card card)
        {
            AssertCardIn(card, Hand);
            Hand.Remove(card);
            CooldownPile.Add(card);
        }

        public void Refresh(Card card)
        {
            AssertCardIn(card, CooldownPile);
            DrawPile.Insert(0, card);
            CooldownPile.Remove(card);
        }

        public void Draw()
        {
            if (DrawPile.Count == 0)
            {
                RefreshDrawPile();
            }

            if (DrawPile.Count == 0)
            {
                return;
            }

            Hand.Add(DrawPile.First());
            DrawPile.RemoveAt(0);
        }

        private void RefreshDrawPile()
        {
            var mixedCards = CooldownPile.OrderBy(_ => _rnd.Next()).ToList();
            DrawPile.AddRange(mixedCards);
            CooldownPile.Clear();
        }

        public void EndTurn()
        {
            CooldownPile.AddRange(this.Hand);
            CooldownPile.AddRange(this.Played);
            Played = new List<Card>();
            Hand = new List<Card>();
            PatronCalls = 1;
            Agents.ForEach(agent => agent.Refresh());
        }

        public void Toss(Card card)
        {
            AssertCardIn(card, DrawPile);
            DrawPile.Remove(card);
            CooldownPile.Add(card);
        }

        public void KnockOut(Card card)
        {
            AssertCardIn(card, AgentCards);
            var removed = Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
            if (removed != 1)
            {
                throw new Exception($"1 agent should have been removed, actually removed: {removed}.");
            }
            CooldownPile.Add(card);
        }

        public void AddToCooldownPile(Card card)
        {
            CooldownPile.Add(card);
        }

        public void Destroy(Card card)
        {
            if (Hand.Contains(card))
            {
                Hand.Remove(card);
            }
            else if (Agents.Any(agent => agent.RepresentingCard.UniqueId == card.UniqueId))
            {
                var removed = Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
                if (removed != 1)
                {
                    throw new Exception($"1 agent should have been removed, actually removed: {removed}.");
                }
            }
            else
            {
                throw new Exception($"Can't destroy card {card.CommonId}({card.UniqueId}) - it's not in Hand or on Board!");
            }
        }

        public override string ToString()
        {
            return $"Player: ({this.CoinsAmount}, {this.PrestigeAmount}, {this.PowerAmount})";
        }

        public List<Card> GetAllPlayersCards()
        {
            List<Card> cards = this.Hand.Concat(this.DrawPile)
                .Concat(Played)
                .Concat(AgentCards)
                .Concat(CooldownPile).ToList();
            return cards;
        }

        public void ActivateAgent(Card card)
        {
            AssertCardIn(card, AgentCards);
            var agent = Agents.First(agent => agent.RepresentingCard.UniqueId == card.UniqueId);

            if (!agent.Activated)
            {
                agent.MarkActivated();
            }
            else
            {
                throw new Exception("Given agent has already been activated!");
            }
        }

        public int AttackAgent(Card card, IPlayer enemy, ITavern tavern)
        {
            if (!enemy.AgentCards.Contains(card))
            {
                throw new Exception("Agent you are trying to attack doesn't exist!");
            }

            var agent = enemy.Agents.First(agent => agent.RepresentingCard == card);
            var attackValue = Math.Min(agent.CurrentHp, PowerAmount);
            PowerAmount -= attackValue;
            agent.Damage(attackValue);
            if (agent.CurrentHp <= 0)
            {
                enemy.Agents.Remove(agent);
                if (agent.RepresentingCard.Type == CardType.AGENT)
                    enemy.CooldownPile.Add(agent.RepresentingCard);
                else if (agent.RepresentingCard.Type == CardType.CONTRACT_AGENT)
                    tavern.Cards.Add(card);
            }

            return attackValue;
        }

        private void AssertCardIn(Card card, List<Card> list)
        {
            if (!list.Contains(card))
            {
                throw new Exception("Wrong card chosen!");
            }
        }

        public Card GetCardByUniqueId(int uniqueId)
        {
            try
            {
                return GetAllPlayersCards().First(card => (int)card.UniqueId == uniqueId);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Player doesn't have card specified by unique id!");
            }
        }

        private Player(PlayerEnum id, int coinsAmount, int prestigeAmount, int powerAmount, List<Card> hand, List<Card> drawPile, List<Card> played, List<Agent> agents, List<Card> cooldownPile, uint patronCalls, SeededRandom rnd)
        {
            ID = id;
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            DrawPile = drawPile;
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
            PatronCalls = patronCalls;
            _rnd = rnd;
        }

        public static Player FromSerializedPlayer(SerializedPlayer player, SeededRandom rnd)
        {
            return new Player(player.PlayerID, player.Coins, player.Prestige, player.Power, player.Hand.ToList(),
                player.DrawPile.ToList(), player.Played.ToList(),
                player.Agents.Select(Agent.FromSerializedAgent).ToList(), player.CooldownPile.ToList(),
                player.PatronCalls, rnd);
        }
    }
}
