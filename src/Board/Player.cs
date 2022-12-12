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
        public ExecutionChain? StartOfTurnEffectsChain { get; private set; }

        public uint ForcedDiscard;
        public uint PatronCalls { get; set; }
        public long ShuffleSeed;
        public ExecutionChain? PendingExecutionChain { get; private set; }

        private ComboContext _comboContext = new ComboContext();
        private Random _rnd = new Random();

        public Player(PlayerEnum iD)
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
        }

        public Player(
            PlayerEnum iD, int coinsAmount, int prestigeAmount, int powerAmount,
            List<Card> hand, List<Card> drawPile, List<Card> played, List<Agent> agents, List<Card> cooldownPile
        )
        {
            CoinsAmount = coinsAmount;
            PrestigeAmount = prestigeAmount;
            PowerAmount = powerAmount;
            Hand = hand;
            DrawPile = new List<Card>(drawPile);
            Played = played;
            Agents = agents;
            CooldownPile = cooldownPile;
            ID = iD;
        }

        public ExecutionChain PlayCard(Card card, IPlayer other, ITavern tavern)
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

            return PlayCardWithoutChecks(card, other, tavern);
        }

        private ExecutionChain PlayCardWithoutChecks(Card card, IPlayer other, ITavern tavern, bool replacePendingExecutionChain = true)
        {
            var result = _comboContext.PlayCard(card, this, other, tavern);

            if (!replacePendingExecutionChain) return result;

            PendingExecutionChain = result;
            PendingExecutionChain.AddCompleteCallback(() => PendingExecutionChain = null);

            return result;
        }

        public void HandleAcquireDuringExecutionChain(Card card, IPlayer other, ITavern tavern)
        {
            var result = AcquireCard(card, other, tavern, false);
            if (PendingExecutionChain == null)
            {
                throw new Exception("This shouldn't happen - there is a bug in the app!");
            }

            PendingExecutionChain.MergeWith(result);
        }

        public void InitDrawPile(List<Card> starterCards)
        {
            DrawPile = starterCards.OrderBy(x => this._rnd.Next(0, starterCards.Count)).ToList();
        }

        public void HealAgent(UniqueId uniqueId, int amount)
        {
            var agent = Agents.First(agent => agent.RepresentingCard.UniqueId == uniqueId);
            agent.Heal(amount);
        }

        public void Discard(Card card)
        {
            AssertCardIn(card, Hand);
            Hand.Remove(card);
        }

        public void Refresh(Card card)
        {
            AssertCardIn(card, CooldownPile);
            DrawPile.Add(card);
            CooldownPile.Remove(card);
        }

        public void Draw()
        {
            if (DrawPile.Count == 0)
            {
                RefreshDrawPile();
            }

            Hand.Add(DrawPile.First());
            DrawPile.RemoveAt(0);
        }

        private void RefreshDrawPile()
        {
            CooldownPile.OrderBy(x => this._rnd.Next(0, CooldownPile.Count)).ToList();
            DrawPile.AddRange(CooldownPile);
            CooldownPile = new List<Card>();
        }

        public void EndTurn()
        {
            _comboContext.Reset();
            CooldownPile.AddRange(this.Hand);
            CooldownPile.AddRange(this.Played);
            Played = new List<Card>();
            Hand = new List<Card>();
            PatronCalls = 1;
            Agents.ForEach(agent => agent.Refresh());
        }

        public ExecutionChain AcquireCard(Card card, IPlayer enemy, ITavern tavern, bool replacePendingExecutionChain = true)
        {
            switch (card.Type)
            {
                case CardType.CONTRACT_AGENT:
                    var agent = Agent.FromCard(card);
                    agent.MarkActivated();
                    Agents.Add(agent);
                    break;
                case CardType.CONTRACT_ACTION:
                    {
                        var result = PlayCardWithoutChecks(
                            card, enemy, tavern, replacePendingExecutionChain
                        );
                        result.AddCompleteCallback(() => tavern.Cards.Add(card));
                        return result;
                    }
                default:
                    CooldownPile.Add(card);
                    break;
            }

            return new ExecutionChain(
                this,
                enemy,
                tavern
            );
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
            Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
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
                Agents.RemoveAll(agent => agent.RepresentingCard.UniqueId == card.UniqueId);
            }
            else
            {
                throw new Exception($"Can't destroy card {card.CommonId} - it's not in Hand or on Board!");
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

        public void AddStartOfTurnEffects(ExecutionChain chain)
        {
            if (StartOfTurnEffectsChain != null)
            {
                StartOfTurnEffectsChain.MergeWith(chain);
            }
            else
            {
                StartOfTurnEffectsChain = chain;
                StartOfTurnEffectsChain.AddCompleteCallback(() => StartOfTurnEffectsChain = null);
            }
        }

        public ExecutionChain ActivateAgent(Card card, IPlayer enemy, ITavern tavern)
        {
            AssertCardIn(card, AgentCards);
            var agent = Agents.First(agent => agent.RepresentingCard.UniqueId == card.UniqueId);

            if (!agent.Activated)
            {
                agent.MarkActivated();
                return PlayCardWithoutChecks(card, enemy, tavern);
            }

            return ExecutionChain.Failed("Picked agent has been already activated in your turn", this, enemy, tavern);
        }

        public ISimpleResult AttackAgent(Card card, IPlayer enemy)
        {
            if (!enemy.AgentCards.Contains(card))
            {
                return new Failure("Agent you are trying to attack doesn't exist!");
            }

            var agent = enemy.Agents.First(agent => agent.RepresentingCard == card);
            var attackValue = Math.Min(agent.CurrentHp, PowerAmount);
            PowerAmount -= attackValue;
            agent.Damage(attackValue);
            if (agent.CurrentHp <= 0)
            {
                enemy.Agents.Remove(agent);
                enemy.CooldownPile.Add(agent.RepresentingCard);
            }

            return new Success();
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
    }
}
