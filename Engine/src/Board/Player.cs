using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute
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
        public List<UniqueCard> Hand { get; set; }
        public List<UniqueCard> DrawPile { get; set; }
        public List<UniqueCard> Played { get; set; }
        public List<Agent> Agents { get; set; }
        public List<UniqueCard> AgentCards => Agents.Select(agent => agent.RepresentingCard).ToList();
        public List<UniqueCard> CooldownPile { get; set; }
        public uint PatronCalls { get; set; }
        public int KnownUpcomingDrawsAmount { get; private set; } = 0;
        private readonly SeededRandom _rng;

        public Player(PlayerEnum iD, SeededRandom rng)
        {
            CoinsAmount = 0;
            PrestigeAmount = 0;
            PowerAmount = 0;
            Hand = new List<UniqueCard>();
            DrawPile = new List<UniqueCard>();
            Played = new List<UniqueCard>();
            Agents = new List<Agent>();
            CooldownPile = new List<UniqueCard>();
            ID = iD;
            PatronCalls = 1;
            _rng = rng;
        }

        public void PlayCard(UniqueCard card)
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

        public void InitDrawPile(List<UniqueCard> starterCards)
        {
            DrawPile = starterCards.OrderBy(_ => _rng.Next()).ToList();
        }

        public int HealAgent(UniqueCard card, int amount)
        {
            // It's possible for this agent to already be gone, for example - discarded, so we need to check.
            if (Agents.All(a => a.RepresentingCard != card)) return -1;

            var agent = Agents.First(agent => agent.RepresentingCard == card);
            agent.Heal(amount);
            return amount;
        }

        public void Discard(UniqueCard card)
        {
            AssertCardIn(card, Hand);
            Hand.Remove(card);
            CooldownPile.Add(card);
        }

        public void Refresh(UniqueCard card)
        {
            AssertCardIn(card, CooldownPile);
            DrawPile.Insert(0, card);
            CooldownPile.Remove(card);
            KnownUpcomingDrawsAmount += 1;
        }

        public void Draw(int amount)
        {
            if (DrawPile.Count < amount)
            {
                ShuffleCooldownPileIntoDrawPile();
            }

            for (var i = 0; i < amount; i++)
            {
                if (DrawPile.Count == 0)
                {
                    return;
                }

                Hand.Add(DrawPile.First());
                DrawPile.RemoveAt(0);
                KnownUpcomingDrawsAmount -= 1;
                if (KnownUpcomingDrawsAmount < 0)
                {
                    KnownUpcomingDrawsAmount = 0;
                }
            }
        }

        public List<UniqueCard> PrepareToss(int amount)
        {
            if (DrawPile.Count < amount)
            {
                ShuffleCooldownPileIntoDrawPile();
            }

            var result = DrawPile.Take(amount).ToList();
            KnownUpcomingDrawsAmount = KnownUpcomingDrawsAmount > result.Count ? KnownUpcomingDrawsAmount : result.Count;
            return result;
        }

        private void ShuffleCooldownPileIntoDrawPile()
        {
            var cooldownShuffled = CooldownPile.OrderBy(_ => _rng.Next()).ToList();
            DrawPile.AddRange(cooldownShuffled);
            CooldownPile.Clear();
        }

        public void EndTurn()
        {
            CooldownPile.AddRange(Hand);
            CooldownPile.AddRange(Played);
            Played.Clear();
            Hand.Clear();

            Draw(5);

            PatronCalls = 1;
            Agents.ForEach(agent => agent.Refresh());
        }

        public void Toss(UniqueCard card)
        {
            AssertCardIn(card, DrawPile);
            DrawPile.Remove(card);
            CooldownPile.Add(card);
            KnownUpcomingDrawsAmount -= 1;
            if (KnownUpcomingDrawsAmount < 0)
            {
                KnownUpcomingDrawsAmount = 0;
            }
        }

        public void KnockOut(UniqueCard card, ITavern tavern)
        {
            AssertCardIn(card, AgentCards);
            var removed = Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
            if (removed != 1)
            {
                throw new EngineException($"1 agent should have been removed, actually removed: {removed}.");
            }

            if (card.Type == CardType.AGENT)
            {
                CooldownPile.Add(card);
            }
            else if (card.Type == CardType.CONTRACT_AGENT)
            {
                tavern.Cards.Add(card);
            }
        }

        public void KnockOutAll(ITavern tavern)
        {
            var contract_agents = Agents.Where(agent => agent.RepresentingCard.Type == CardType.CONTRACT_AGENT).Select(agent => agent.RepresentingCard).ToList();
            var normal_agents = Agents.Where(agent => agent.RepresentingCard.Type == CardType.AGENT).Select(agent => agent.RepresentingCard).ToList();
            CooldownPile.AddRange(normal_agents);
            tavern.Cards.AddRange(contract_agents);
            Agents.Clear();
        }

        public void AddToCooldownPile(UniqueCard card)
        {
            CooldownPile.Add(card);
        }

        public void Destroy(UniqueCard card)
        {
            if (Hand.Contains(card))
            {
                Hand.Remove(card);
            }
            else if (Played.Contains(card))
            {
                Played.Remove(card);
            }
            else if (Agents.Any(agent => agent.RepresentingCard.UniqueId == card.UniqueId))
            {
                var removed = Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
                if (removed != 1)
                {
                    throw new EngineException($"1 agent should have been removed, actually removed: {removed}.");
                }
            }
            else
            {
                throw new EngineException($"Can't destroy card {card.CommonId}({card.UniqueId}) - it's not in Hand or on Board!");
            }
        }

        public override string ToString()
        {
            return $"Player: ({this.CoinsAmount}, {this.PrestigeAmount}, {this.PowerAmount})";
        }

        public List<UniqueCard> GetAllPlayersCards()
        {
            List<UniqueCard> cards = this.Hand.Concat(this.DrawPile)
                .Concat(Played)
                .Concat(AgentCards)
                .Concat(CooldownPile).ToList();
            return cards;
        }

        public void ActivateAgent(UniqueCard card)
        {
            AssertCardIn(card, AgentCards);
            var agent = Agents.First(agent => agent.RepresentingCard.UniqueId == card.UniqueId);

            if (!agent.Activated)
            {
                agent.MarkActivated();
            }
            else
            {
                throw new EngineException("Given agent has already been activated!");
            }
        }

        public int AttackAgent(UniqueCard card, IPlayer enemy, ITavern tavern)
        {
            if (!enemy.AgentCards.Contains(card))
            {
                throw new EngineException("Agent you are trying to attack doesn't exist!");
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

        private void AssertCardIn(UniqueCard card, List<UniqueCard> list)
        {
            if (!list.Contains(card))
            {
                throw new EngineException("Wrong card chosen!");
            }
        }

        private Player(PlayerEnum id, int coinsAmount, int prestigeAmount, int powerAmount, List<UniqueCard> hand, List<UniqueCard> drawPile, List<UniqueCard> played, List<Agent> agents, List<UniqueCard> cooldownPile, uint patronCalls, SeededRandom rng, int knownUpcomingDrawsAmount)
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
            _rng = rng;
            KnownUpcomingDrawsAmount = knownUpcomingDrawsAmount;
        }

        public static Player FromSerializedPlayer(SerializedPlayer player, SeededRandom rnd, bool cheats)
        {
            var hand = new List<UniqueCard>(player.Hand.Count);
            hand.AddRange(player.Hand);
            var drawPile = new List<UniqueCard>(player.DrawPile.Count);
            drawPile.AddRange(player.DrawPile);
            var played = new List<UniqueCard>(player.Played.Count);
            played.AddRange(player.Played);
            var cooldownPile = new List<UniqueCard>(player.CooldownPile.Count);
            cooldownPile.AddRange(player.CooldownPile);
            return new Player(player.PlayerID, player.Coins, player.Prestige, player.Power, hand,
                drawPile, played,
                player.Agents.Select(Agent.FromSerializedAgent).ToList(), cooldownPile,
                player.PatronCalls, rnd, player.KnownUpcomingDraws.Count);
        }
    }
}
