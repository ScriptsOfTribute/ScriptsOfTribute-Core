using ScriptsOfTribute;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Tests.Board;

public class SaintAlessiaTests
{
    [Fact]
    void BaseCardPlays()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        var alessianRebel = GlobalCardDatabase.Instance.GetCard(CardId.ALESSIAN_REBEL);
        br.CurrentPlayer.Hand.Add(alessianRebel);
        br.PlayCard(alessianRebel);

        /*
        Expected:
            CurrentPlayer has 1 agent activated
        */
        Assert.Contains(CardId.ALESSIAN_REBEL, br.CurrentPlayer.AgentCards.Select(card => card.CommonId).ToList());
        Assert.Equal(0, br.CurrentPlayer.CoinsAmount);

        var sote = GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE);
        br.CurrentPlayer.Hand.Add(sote);
        br.PlayCard(sote);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        var coin_choice = br.CardActionManager.PendingChoice!.PossibleEffects.First(eff => eff.Type == EffectType.GAIN_COIN);
        br.CardActionManager.MakeChoice(coin_choice);

        /*
        Expected: 3 coin -> 2 coin from choice, 1 coin from Combo 2 (Alessian Rebel)
        */
        Assert.Equal(3, br.CurrentPlayer.CoinsAmount);
    }

    [Fact]
    void InvokeSaintAlessiaPatron()
    {
        var br = new BoardManager(new[] { PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA }, 123);
        br.CurrentPlayer.CoinsAmount = 99;
        br.CurrentPlayer.PatronCalls = 10;
        Assert.Equal(PlayerEnum.NO_PLAYER_SELECTED, br.GetPatronFavorism(PatronId.SAINT_ALESSIA));
        br.PatronCall(PatronId.SAINT_ALESSIA);
        Assert.Equal(br.CurrentPlayer.ID, br.GetPatronFavorism(PatronId.SAINT_ALESSIA));
        Assert.Contains(CardId.SOLDIER_OF_THE_EMPIRE, br.CurrentPlayer.CooldownPile.Select(c => c.CommonId).ToList());
        br.PatronCall(PatronId.SAINT_ALESSIA);
        Assert.Equal(br.CurrentPlayer.ID, br.GetPatronFavorism(PatronId.SAINT_ALESSIA));
        Assert.Contains(CardId.CHAINBREAKER_SERGEANT, br.CurrentPlayer.CooldownPile.Select(c => c.CommonId).ToList());
        br.EndTurn();
        br.CurrentPlayer.CoinsAmount = 99;
        Assert.Equal(0, br.CurrentPlayer.PowerAmount);
        br.PatronCall(PatronId.SAINT_ALESSIA);
        Assert.Equal(PlayerEnum.NO_PLAYER_SELECTED, br.GetPatronFavorism(PatronId.SAINT_ALESSIA));
        Assert.Equal(2, br.CurrentPlayer.PowerAmount);
    }

    [Fact]
    void TestDonateEffect()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        var priestess = GlobalCardDatabase.Instance.GetCard(CardId.PRIESTESS_OF_THE_EIGHT);
        var alessianRebel = GlobalCardDatabase.Instance.GetCard(CardId.ALESSIAN_REBEL);
        var sote = GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE);
        var armory = GlobalCardDatabase.Instance.GetCard(CardId.THE_ARMORY);
        br.CurrentPlayer.Hand.Add(alessianRebel);
        br.CurrentPlayer.Hand.Add(priestess);
        br.CurrentPlayer.Hand.Add(sote);
        br.CurrentPlayer.Hand.Add(armory);

        var chainbreaker = GlobalCardDatabase.Instance.GetCard(CardId.CHAINBREAKER_CAPTAIN);
        var morihaus = GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_SACRED_BULL);
        br.CurrentPlayer.DrawPile.Add(chainbreaker);
        br.CurrentPlayer.DrawPile.Add(morihaus);

        br.PlayCard(priestess);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.EFFECT_CHOICE, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        var donateEffect = br.CardActionManager.PendingChoice!.PossibleEffects.First(eff => eff.Type == EffectType.DONATE);
        br.CardActionManager.MakeChoice(donateEffect);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.CARD_EFFECT, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        br.CardActionManager.MakeChoice(new List<UniqueCard> { sote, armory });
        Assert.Equal(BoardState.NORMAL, br.CardActionManager.State);

        Assert.DoesNotContain(sote, br.CurrentPlayer.Hand);
        Assert.DoesNotContain(armory, br.CurrentPlayer.Hand);
        Assert.Contains(morihaus, br.CurrentPlayer.Hand);
        Assert.Contains(chainbreaker, br.CurrentPlayer.Hand);
        Assert.Contains(armory, br.CurrentPlayer.CooldownPile);
    }

    [Fact]
    void TestKnockoutAllEffect()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.HEL_SHIRA_HERALD)));
        br.CurrentPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET)));
        var defector = GlobalCardDatabase.Instance.GetCard(CardId.AYLEID_DEFECTOR);
        br.CurrentPlayer.Hand.Add(defector);
        br.PlayCard(defector);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.EFFECT_CHOICE, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        var knockoutAllEff = br.CardActionManager.PendingChoice!.PossibleEffects.First(eff => eff.Type == EffectType.KNOCKOUT_ALL);
        br.CardActionManager.MakeChoice(knockoutAllEff);
        Assert.Empty(br.EnemyPlayer.Agents);
        Assert.Empty(br.CurrentPlayer.Agents);
        Assert.Contains(CardId.HEL_SHIRA_HERALD, br.EnemyPlayer.CooldownPile.Select(c => c.CommonId).ToArray());
        Assert.Contains(CardId.NO_SHIRA_POET, br.CurrentPlayer.CooldownPile.Select(c => c.CommonId).ToArray());
    }

    [Fact]
    void TestKnockoutAllEffectDoesNotHappenIfNotChosen()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.HEL_SHIRA_HERALD)));
        br.CurrentPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.NO_SHIRA_POET)));
        var defector = GlobalCardDatabase.Instance.GetCard(CardId.AYLEID_DEFECTOR);
        br.CurrentPlayer.Hand.Add(defector);
        br.CurrentPlayer.DrawPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.GOLD));
        br.PlayCard(defector);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.EFFECT_CHOICE, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        var draw = br.CardActionManager.PendingChoice!.PossibleEffects.First(eff => eff.Type == EffectType.DRAW);
        br.CardActionManager.MakeChoice(draw);
        Assert.NotEmpty(br.EnemyPlayer.Agents);
        Assert.NotEmpty(br.CurrentPlayer.Agents);
        Assert.Contains(CardId.HEL_SHIRA_HERALD, br.EnemyPlayer.AgentCards.Select(c => c.CommonId).ToArray());
        Assert.Contains(CardId.NO_SHIRA_POET, br.CurrentPlayer.AgentCards.Select(c => c.CommonId).ToArray());
        Assert.Contains(CardId.GOLD, br.CurrentPlayer.Hand.Select(c => c.CommonId).ToArray());
    }

    [Fact]
    void TestMorihausTriggers()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_SACRED_BULL)));
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE)));
        var black_sacrament = GlobalCardDatabase.Instance.GetCard(CardId.BLACK_SACRAMENT);
        br.CurrentPlayer.Hand.Add(black_sacrament);
        Assert.Equal(BoardState.NORMAL, br.CardActionManager.State);
        br.PlayCard(black_sacrament);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.CARD_EFFECT, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        Assert.Equal(0, br.EnemyPlayer.CoinsAmount);
        var knockout = br.CardActionManager.PendingChoice!.PossibleCards.Where(card => card.CommonId == CardId.SOLDIER_OF_THE_EMPIRE).ToList();
        br.CardActionManager.MakeChoice(knockout);
        Assert.Equal(1, br.EnemyPlayer.CoinsAmount);
    }

    [Fact]
    void TestTwoMorihausTriggers()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_SACRED_BULL)));
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_THE_ARCHER)));
        br.EnemyPlayer.Agents.Add(Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE)));
        var black_sacrament = GlobalCardDatabase.Instance.GetCard(CardId.BLACK_SACRAMENT);
        var black_sacrament2 = GlobalCardDatabase.Instance.GetCard(CardId.BLACK_SACRAMENT);
        br.CurrentPlayer.Hand.Add(black_sacrament);
        br.CurrentPlayer.Hand.Add(black_sacrament2);
        Assert.Equal(BoardState.NORMAL, br.CardActionManager.State);
        br.PlayCard(black_sacrament);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.CARD_EFFECT, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        Assert.Equal(0, br.EnemyPlayer.CoinsAmount);
        var knockout = br.CardActionManager.PendingChoice!.PossibleCards.Where(card => card.CommonId == CardId.SOLDIER_OF_THE_EMPIRE).ToList();
        br.CardActionManager.MakeChoice(knockout);
        Assert.Equal(2, br.EnemyPlayer.CoinsAmount);

        br.PlayCard(black_sacrament2);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.CARD_EFFECT, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        knockout = br.CardActionManager.PendingChoice!.PossibleCards.Where(card => card.CommonId == CardId.MORIHAUS_THE_ARCHER).ToList();
        br.CardActionManager.MakeChoice(knockout);
        Assert.Equal(3, br.EnemyPlayer.CoinsAmount);
    }

    [Fact]
    void TestRefreshTopAgents()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);
        var chainbreaker_sergeant = GlobalCardDatabase.Instance.GetCard(CardId.CHAINBREAKER_SERGEANT);
        var whitestrake_ascendant = GlobalCardDatabase.Instance.GetCard(CardId.WHITESTRAKE_ASCENDANT);
        br.CurrentPlayer.CooldownPile.Add(chainbreaker_sergeant);
        br.CurrentPlayer.Hand.Add(whitestrake_ascendant);

        Assert.Equal(BoardState.NORMAL, br.CardActionManager.State);
        br.PlayCard(whitestrake_ascendant);
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.EFFECT_CHOICE, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        br.CardActionManager.MakeChoice(
            br.CardActionManager.PendingChoice!.PossibleEffects.First(choice => choice.Type == EffectType.RETURN_AGENT_TOP)
        );
        Assert.Equal(BoardState.CHOICE_PENDING, br.CardActionManager.State);
        Assert.Equal(ChoiceType.CARD_EFFECT, br.CardActionManager.PendingChoice!.Context.ChoiceType);
        br.CardActionManager.MakeChoice(
            br.CardActionManager.PendingChoice!.PossibleCards.Where(choice => choice.CommonId == CardId.CHAINBREAKER_SERGEANT).ToList()
        );
        Assert.Equal(BoardState.NORMAL, br.CardActionManager.State);
        Assert.Contains(chainbreaker_sergeant, br.CurrentPlayer.DrawPile);
    }
    
    [Fact]
    void TestMorihausTriggersBothPlayers()
    {
        var br = new BoardManager([PatronId.TREASURY, PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.SAINT_ALESSIA], 123);

        var morihaus1 = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_SACRED_BULL));
        var morihaus2 = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.MORIHAUS_THE_ARCHER));
        var soldier1 = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE));
        var soldier2 = Agent.FromCard(GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE));

        br.CurrentPlayer.Agents.Add(morihaus1);
        br.CurrentPlayer.Agents.Add(soldier1);

        br.EnemyPlayer.Agents.Add(morihaus2);
        br.EnemyPlayer.Agents.Add(soldier2);

        var black_sacrament = GlobalCardDatabase.Instance.GetCard(CardId.BLACK_SACRAMENT);
        br.CurrentPlayer.Hand.Add(black_sacrament);

        br.PlayCard(black_sacrament);
        
        var knockout = br.CardActionManager.PendingChoice!.PossibleCards
            .Where(card => card.CommonId == CardId.SOLDIER_OF_THE_EMPIRE && br.EnemyPlayer.Agents.Any(a => a.RepresentingCard.UniqueId == card.UniqueId))
            .ToList();

        br.CardActionManager.MakeChoice(knockout);

        Assert.Equal(1, br.EnemyPlayer.CoinsAmount);
        Assert.Equal(1, br.CurrentPlayer.CoinsAmount);
    }
}
