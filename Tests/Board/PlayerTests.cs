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
        // Contract Action with Coin 2
        _tavern.AvailableCards.Add(GlobalCardDatabase.Instance.GetCard(CardId.KWAMA_EGG_MINE));

        // Card with Acquire
        _sut.Hand.Add(GlobalCardDatabase.Instance.GetCard(CardId.CUSTOMS_SEIZURE));
        
        var chain = _sut.PlayCard(CardId.CUSTOMS_SEIZURE, _enemy, _tavern);

        var counter = 0;

        foreach (var result in chain.Consume())
        {
            switch (counter)
            {
                // Should be choice for which card to Acquire
                case 0:
                {
                    var choice = result as Choice<CardId>;
                    var newResult = choice.Choose(CardId.KWAMA_EGG_MINE);
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
}
