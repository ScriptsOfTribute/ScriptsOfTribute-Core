using ScriptsOfTribute;

namespace Tests.Board;

public class ComboContextTests
{
    Player _player1 = new Player(PlayerEnum.PLAYER1);
    Player _player2 = new Player(PlayerEnum.PLAYER2);
    Tavern _tavern = new Tavern(GlobalCardDatabase.Instance.AllCards);

    private ComboContext _sut = new ComboContext();

    [Fact]
    void ShouldPlaySingleCardWithoutComboCorrectly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOLD), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);
    }

    [Fact]
    void ShouldPlayTwoCardsWithoutComboCorrectly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOLD), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOLD), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);
    }

    [Fact]
    void ShouldPlayCardWithComboCorrectly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);
    }

    [Fact]
    void ShouldTriggerSingleComboCorrectly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.WAR_SONG), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Equal(2, newChainList.Count);
    }

    [Fact]
    void ShouldTriggerSingleComboCorrectlyInDifferentOrder()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.WAR_SONG), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Equal(2, newChainList.Count);
    }

    [Fact]
    void ShouldNotTriggerSingleComboForDifferentPatrons()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);
    }

    [Fact]
    void ShouldTriggerTwoCombosCorrectly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Equal(3, newChainList.Count);

        newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.WAR_SONG), _player1, _player2, _tavern);
        newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);
    }

    [Fact]
    void ShouldTriggerCombo3Correctly()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);

        newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.EBONY_MINE), _player1, _player2, _tavern);
        newChainList = newChain.Consume().ToList();
        Assert.Equal(2, newChainList.Count);
    }

    [Fact]
    void ShouldTriggerCombo3CorrectlyInOrder2()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.EBONY_MINE), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);

        newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        newChainList = newChain.Consume().ToList();
        Assert.Equal(2, newChainList.Count);
    }

    [Fact]
    void ShouldTriggerCombo3CorrectlyInOrder3()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.EBONY_MINE), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);

        newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.GOODS_SHIPMENT), _player1, _player2, _tavern);
        newChainList = newChain.Consume().ToList();
        Assert.Equal(2, newChainList.Count);
    }

    [Fact]
    void ShouldNotTriggerComboOnFollowingTurn()
    {
        var chain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.WAR_SONG), _player1, _player2, _tavern);
        var chainList = chain.Consume().ToList();
        Assert.Single(chainList);

        _sut.Reset();

        var newChain = _sut.PlayCard(GlobalCardDatabase.Instance.GetCard(CardId.MIDNIGHT_RAID), _player1, _player2, _tavern);
        var newChainList = newChain.Consume().ToList();
        Assert.Single(newChainList);
    }
}
