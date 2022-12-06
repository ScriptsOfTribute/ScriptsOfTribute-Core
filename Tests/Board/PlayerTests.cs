using Moq;
using TalesOfTribute;

namespace Tests.Board;

public class PlayerTests
{
    private Player _sut = new Player(PlayerEnum.PLAYER1);
    private Mock<IPlayer> _enemy = new Mock<IPlayer>();
    private Mock<ITavern> _tavern = new Mock<ITavern>();

    [Fact]
    void TestAcquireCardFlowWithContractAction()
    {
        var contractActionWithCoin2 = GlobalCardDatabase.Instance.GetCard(CardId.KWAMA_EGG_MINE);
        _tavern.Setup(tavern => tavern.GetAffordableCards(It.IsAny<int>())).Returns(new List<Card> { contractActionWithCoin2 });
        _tavern.Setup(tavern => tavern.Acquire(It.IsAny<Card>())).Returns(contractActionWithCoin2);
        _tavern.Setup(tavern => tavern.Cards).Returns(new List<Card>());
        
        // Card with Acquire
        var cardToPlay = GlobalCardDatabase.Instance.GetCard(CardId.CUSTOMS_SEIZURE);
        _sut.Hand.Add(cardToPlay);

        var chain = _sut.PlayCard(cardToPlay, _enemy.Object, _tavern.Object);

        var counter = 0;

        foreach (var result in chain.Consume())
        {
            switch (counter)
            {
                // Should be choice for which card to Acquire
                case 0:
                    {
                        var choice = result as Choice<Card>;
                        var newResult = choice.Choose(contractActionWithCoin2);
                        Assert.True(newResult is Success);
                        break;
                    }
                // Should be contract action action
                case 1:
                    {
                        Assert.True(result is Success);
                        Assert.Equal(2, _sut.CoinsAmount);
                        break;
                    }
            }

            counter += 1;
        }

        Assert.Equal(2, counter);
    }

    [Fact]
    void DestroyShouldCorrectlyDestroyInHandOrAgents()
    {
        var agentInHand = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var agentInPlay = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN));

        _sut.Hand.Add(agentInHand);
        _sut.Agents.Add(agentInPlay);

        _sut.Destroy(agentInHand);
        Assert.Empty(_sut.Hand);
        Assert.Single(_sut.Agents);

        _sut.Destroy(agentInPlay.RepresentingCard);
        Assert.Empty(_sut.Agents);
    }
    
    [Fact]
    void ActivateAgentOnceShouldWorkTwiceShouldBeFailed()
    {
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        _sut.Agents.Add(Agent.FromCard(oathman));

        var chain = _sut.ActivateAgent(oathman, _enemy.Object, _tavern.Object);

        var counter = 0;
        foreach (var result in chain.Consume())
        {
            Assert.True(result is Success);
            counter += 1;
        }
        
        Assert.Equal(1, counter);
        Assert.Equal(2, _sut.CoinsAmount);
        
        chain = _sut.ActivateAgent(oathman, _enemy.Object, _tavern.Object);
        counter = 0;
        foreach (var result in chain.Consume())
        {
            Assert.True(result is Failure);
            counter += 1;
        }
        
        Assert.Equal(1, counter);
        Assert.Equal(2, _sut.CoinsAmount);
    }

    [Fact]
    void AgentsShouldBeDestroyedCorrectly()
    {
        _sut.PowerAmount = 10;
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var enemyAgents = new List<Agent> { Agent.FromCard(oathman) };
        var enemyCooldownPile = new List<Card>();
        _enemy.Setup(enemy => enemy.Agents).Returns(enemyAgents);
        _enemy.Setup(enemy => enemy.AgentCards).Returns(new List<Card> { oathman });
        _enemy.Setup(enemy => enemy.CooldownPile).Returns(enemyCooldownPile);
        Assert.True(_sut.AttackAgent(oathman, _enemy.Object) is Success);
        Assert.Empty(enemyAgents);
        Assert.Contains(oathman, enemyCooldownPile);
    }
    
    [Fact]
    void CantDestroyAgentThatDoesntExist()
    {
        _sut.PowerAmount = 10;
        var oathman = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
        var councilor = GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_COUNCILOR);
        var enemyAgents = new List<Card> { oathman };
        _enemy.Setup(enemy => enemy.AgentCards).Returns(enemyAgents);
        Assert.True(_sut.AttackAgent(councilor, _enemy.Object) is Failure);
    }
}
