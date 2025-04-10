using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;

namespace ModuleTests;

public class BoardManagerTests
{
    [Fact]
    void HealEffectShouldWorkCorrectlyWithDeadAgents()
    {
        var br = new BoardManager(new[] { PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU }, 123);
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
        br.CardActionManager.MakeChoice(new List<UniqueCard> { knightCommander });
        br.ActivateAgent(knightsOfSaintPelin);
    }

    [Fact]
    void AttackOnAgentWithoutDeath()
    {
        var br = new BoardManager(new[] { PatronId.DUKE_OF_CROWS, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU }, 123);
        var agent = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.BLACKFEATHER_KNIGHT)); // 3hp agent
        br.EnemyPlayer.Agents.Add(agent);
        Assert.Contains(agent, br.EnemyPlayer.Agents);
        br.CurrentPlayer.PowerAmount = 1;
        br.AttackAgent(agent.RepresentingCard);
        Assert.Contains(agent, br.EnemyPlayer.Agents);
        Assert.DoesNotContain(CompletedActionType.AGENT_DEATH, br.CardActionManager.CompletedActions.Select(a => a.Type).ToList());
    }

    [Fact]
    void StarterCardsTest()
    {
        var br = new BoardManager(new[] { PatronId.DUKE_OF_CROWS, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU }, 123);
        br.SetUpGame();
        Assert.Contains(CardId.PECK, br.EnemyPlayer.Hand.Concat(br.EnemyPlayer.DrawPile).Select(c => c.CommonId).ToList());
    }
}