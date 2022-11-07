using TalesOfTribute;

namespace Tests.utils;

public class PatronTests
{
    [Fact]
    public void AnseiTest()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            cards.Add(new Card());
        }
        Player player1 = new Player(0, 5, 2, 7, cards, cards, cards, cards, cards);
        Player player2 = new Player(1, 8, 1, 5, cards, cards, cards, cards, cards);
        Patron ansei = new Ansei();

        //Default is neutral state
        Assert.Equal(-1, ansei.FavoredPlayer);

        //Test patron's activation
        ansei.PatronActivation(player1, player2);
        Assert.Equal(5, player1.PowerAmount);
        Assert.Equal(6, player1.CoinsAmount);
        Assert.Equal(player1.ID, ansei.FavoredPlayer);

        //If player is favored, activation doesnt work
        ansei.PatronActivation(player1, player2);
        Assert.Equal(6, player1.CoinsAmount);

        //Test patron's power
        ansei.PatronPower(player1, player2);
        Assert.Equal(7, player1.CoinsAmount);

        // Change favor state and check patron's power
        ansei.PatronActivation(player2, player1);
        ansei.PatronPower(player1, player2);
        Assert.Equal(7, player1.CoinsAmount);

        //Test patron's activation if player doesn't have enough power
        player1.PowerAmount = 1;
        Assert.False(ansei.PatronActivation(player1, player2));
        Assert.Equal(1, player1.PowerAmount);
        Assert.Equal(7, player1.CoinsAmount);
        Assert.Equal(player2.ID, ansei.FavoredPlayer);

    }

    [Fact]
    public void DukeOfCrowsTest()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            cards.Add(new Card());
        }
        Player player1 = new Player(0, 5, 2, 7, cards, cards, cards, cards, cards);
        Player player2 = new Player(1, 8, 1, 5, cards, cards, cards, cards, cards);
        Patron duke = new DukeOfCrows();

        //Default is neutral state
        Assert.Equal(-1, duke.FavoredPlayer);

        //Test patron's activation
        Assert.True(duke.PatronActivation(player1, player2));
        Assert.Equal(11, player1.PowerAmount);
        Assert.Equal(0, player1.CoinsAmount);
        Assert.Equal(player1.ID, duke.FavoredPlayer);

        //If player is favored, activation doesnt work
        Assert.True(duke.PatronActivation(player1, player2));
        Assert.Equal(11, player1.PowerAmount);
        Assert.Equal(0, player1.CoinsAmount);
        Assert.Equal(player1.ID, duke.FavoredPlayer);


        Assert.True(duke.PatronActivation(player2, player1));
        Assert.Equal(12, player2.PowerAmount);
        Assert.Equal(0, player2.CoinsAmount);
        Assert.Equal(-1, duke.FavoredPlayer);

        //If player has no money, activation doesnt work
        Assert.False(duke.PatronActivation(player1, player2));
    }

    [Fact]
    public void RajhinTest()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < 10; i++)
        {
            cards.Add(new Card());
        }
        Player player1 = new Player(0, 5, 2, 7, cards, cards, cards, cards, new List<Card>());
        Player player2 = new Player(1, 1, 1, 5, cards, cards, cards, cards, cards);
        Patron rajhin = new Rajhin();

        //Default is neutral state
        Assert.Equal(-1, rajhin.FavoredPlayer);

        //Test patron's activation
        Assert.True(rajhin.PatronActivation(player1, player2));
        Assert.Equal(2, player1.CoinsAmount);
        Assert.Equal(player1.ID, rajhin.FavoredPlayer);
        Assert.Contains(player2.CooldownPile, card => card.Name == "Bewilderment");

        // Not enough gold
        Assert.False(rajhin.PatronActivation(player2, player1));
        Assert.Equal(1, player2.CoinsAmount);
        Assert.DoesNotContain(player1.CooldownPile, card => card.Name == "Bewilderment");
    }

    [Fact]
    public void OrgnumTest()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < 4; i++)
        {
            cards.Add(new Card());
        }
        Player player1 = new Player(0, 5, 2, 7, cards, cards, cards, cards, new List<Card>());
        Player player2 = new Player(1, 7, 1, 5, cards, cards, cards, cards, cards);
        Patron orgnum = new Orgnum();

        //Default is neutral state
        Assert.Equal(-1, orgnum.FavoredPlayer);

        //Test patron's activation
        Assert.True(orgnum.PatronActivation(player1, player2));
        Assert.Equal(2, player1.CoinsAmount);
        Assert.Equal(player1.ID, orgnum.FavoredPlayer);
        Assert.Equal(9, player1.PowerAmount);

        player1.CoinsAmount = 6;

        //Test patron's activation when favored
        Assert.True(orgnum.PatronActivation(player1, player2));
        Assert.Equal(3, player1.CoinsAmount);
        Assert.Equal(player1.ID, orgnum.FavoredPlayer);
        Assert.Equal(13, player1.PowerAmount);
        Assert.Contains(player1.CooldownPile, card => card.Name == "Maormer Boarding Party");

        //Test patron's activation when not favored
        Assert.True(orgnum.PatronActivation(player2, player1));
        Assert.Equal(4, player2.CoinsAmount);
        Assert.Equal(-1, orgnum.FavoredPlayer);
        Assert.Equal(7, player2.PowerAmount);
    }
}
