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
        public List<Card> Agents { get; set; }
        public List<Card> CooldownPile { get; set; }
        public ExecutionChain? StartOfTurnEffectsChain { get; private set; }

        public uint ForcedDiscard;
        public uint PatronCalls { get; set; }
        public long ShuffleSeed;
        private ExecutionChain? _pendingExecutionChain;

        private ComboContext _comboContext = new ComboContext();

        public Player(PlayerEnum iD)
        {
            CoinsAmount = 0;
            PrestigeAmount = 0;
            PowerAmount = 0;
            Hand = new List<Card>();
            DrawPile = new List<Card>();
            Played = new List<Card>();
            Agents = new List<Card>();
            CooldownPile = new List<Card>();
            ID = iD;
            PatronCalls = 1;
        }

        public Player(
            PlayerEnum iD, int coinsAmount, int prestigeAmount, int powerAmount,
            List<Card> hand, List<Card> drawPile, List<Card> played, List<Card> agents, List<Card> cooldownPile
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
            Played.Add(card);

            return PlayCardWithoutChecks(card, other, tavern);
        }

        private ExecutionChain PlayCardWithoutChecks(Card card, IPlayer other, ITavern tavern, bool replacePendingExecutionChain = true)
        {
            var result = _comboContext.PlayCard(card, this, other, tavern);

            if (!replacePendingExecutionChain) return result;

            _pendingExecutionChain = result;
            _pendingExecutionChain.AddCompleteCallback(() => _pendingExecutionChain = null);

            return result;
        }

        public void HandleAcquireDuringExecutionChain(Card card, IPlayer other, ITavern tavern)
        {
            var result = AcquireCard(card, other, tavern, false);
            if (_pendingExecutionChain == null)
            {
                throw new Exception("This shouldn't happen - there is a bug in the app!");
            }

            _pendingExecutionChain.MergeWith(result);
        }

        public void HealAgent(Guid guid, int amount)
        {
            // TODO: Implement when agents are implemented.
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
            Hand.Add(DrawPile.First());
            DrawPile.RemoveAt(0);
        }

        public void EndTurn()
        {
            _comboContext.Reset();
            CooldownPile.AddRange(this.Played);
            Played = new List<Card>();
            PatronCalls = 1;
        }

        public ExecutionChain AcquireCard(Card card, IPlayer enemy, ITavern tavern, bool replacePendingExecutionChain = true)
        {
            switch (card.Type)
            {
                case CardType.CONTRACT_AGENT:
                    Agents.Add(card);
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
            AssertCardIn(card, Agents);
            Agents.Remove(card);
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
            else if (Agents.Contains(card))
            {
                Agents.Remove(card);
            }
            else
            {
                throw new Exception($"Can't destroy card {card.Id} - it's not in Hand or on Board!");
            }
        }

        public override string ToString()
        {
            return $"Player: ({this.CoinsAmount}, {this.PrestigeAmount}, {this.PowerAmount})";
        }

        public List<Card> GetAllPlayersCards()
        {
            List<Card> cards = this.Hand.Concat(this.DrawPile)
                .Concat(this.Played)
                .Concat(this.Agents)
                .Concat(this.CooldownPile).ToList();
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

        private void AssertCardIn(Card card, List<Card> list)
        {
            if (!list.Contains(card))
            {
                throw new Exception("Wrong card chosen!");
            }
        }
    }
}
