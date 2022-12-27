using TalesOfTribute;

namespace ModuleTests;

public class UnitTest1
{
    [Fact]
    void HealEffectShouldWorkCorrectlyWithDeadAgents()
    {
        var br = new BoardManager(new[] { PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU });
        var ritual = GlobalCardDatabase.Instance.GetCard(CardId.BRIARHEART_RITUAL);
        br.Tavern.AvailableCards.Add(ritual);
        var knightCommander = GlobalCardDatabase.Instance.GetCard(CardId.KNIGHT_COMMANDER);
        br.CurrentPlayer.Hand.Add(knightCommander);
        var knightsOfSaintPelin = GlobalCardDatabase.Instance.GetCard(CardId.KNIGHTS_OF_SAINT_PELIN);
        br.CurrentPlayer.Hand.Add(knightsOfSaintPelin);
        
        br.PlayCard(knightsOfSaintPelin);

        br.EndTurn();
        br.EndTurn();

        br.CurrentPlayer.CoinsAmount = 1000;
        br.PlayCard(knightCommander);
        br.BuyCard(ritual);
        br.CardActionManager.MakeChoice(new List<Card> { knightCommander });
        br.ActivateAgent(knightsOfSaintPelin);
    }
}