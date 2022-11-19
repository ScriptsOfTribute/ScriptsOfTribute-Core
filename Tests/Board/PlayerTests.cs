using TalesOfTribute;

namespace Tests.Board;

public class PlayerTests
{
    private Player _enemy = new Player(PlayerEnum.PLAYER2);
    private Player _sut = new Player(PlayerEnum.PLAYER1);
    private Tavern _tavern = new Tavern(GlobalCardDatabase.Instance.AllCards);

    [Fact]
    void TestAcquireCardFlowWithContractAction()
    {
        var contractActionWithCoin2 = GlobalCardDatabase.Instance.GetCard(CardId.KWAMA_EGG_MINE);
        _tavern.AvailableCards.Add(contractActionWithCoin2);

        // Card with Acquire
        var cardToPlay = GlobalCardDatabase.Instance.GetCard(CardId.CUSTOMS_SEIZURE);
        _sut.Hand.Add(cardToPlay);

        var chain = _sut.PlayCard(cardToPlay, _enemy, _tavern);

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
        var agentInPlay = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);

        _sut.Hand.Add(agentInHand);
        _sut.Agents.Add(agentInPlay);

        _sut.Destroy(agentInHand);
        Assert.Empty(_sut.Hand);
        Assert.Single(_sut.Agents);

        _sut.Destroy(agentInPlay);
        Assert.Empty(_sut.Agents);
    }
}
