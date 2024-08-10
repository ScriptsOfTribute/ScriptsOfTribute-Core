using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace Bots;

using QKey = Tuple<CardId, int>;

public class PatronsCount
{
    public int player_patrons = 0;
    public int neutral_patron = 0;
    public int enemy_patrons = 0;

    public PatronsCount(int p, int n, int e)
    {
        player_patrons = p;
        neutral_patron = n;
        enemy_patrons = e;
    }
}

public class QL
{
    private int turn_counter = 0;

    private static double explorationChance = 0.5;
    private static double learningRate = 0.1;
    private static double discountFactor = 0.5;

    // action - card to buy, state = stages and combo
    // key = {0 - card id, 1 - enemy prestige + turns finished, 3 - combo for card's deck}

    // action - played action or activated agent from specific deck
    // state - number of cards from specific deck
    // value - reward for playing that move
    private Dictionary<QKey, double> qTable = new Dictionary<QKey, double>();

    public int[] deck_cards_counters = new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0};

    public int actions_before_this_turn_count = 0;
    public HashSet<UniqueCard> played_cards_this_turn = new HashSet<UniqueCard>();
    public SortedSet<CardId> gained_cards = new SortedSet<CardId>();

    public QL()
    {
        using (var sw = new StreamWriter(FilePaths.errorFile, append: true))
        {
            // sw.WriteLine("Start of game");
            // sw.WriteLine(DateTime.Now + "\n\n");
        }

        using (var sw = new StreamWriter(FilePaths.tmpFile, append: false))
        {
            // sw.WriteLine("Start of game");
            // sw.WriteLine(DateTime.Now + "\n");
        }

        ReadQTableFromFile();
    }

    public void ReadQTableFromFile()
    {
        using (var streamReader = new StreamReader(FilePaths.qTablePath))
        {
            string raw_line;
            while ((raw_line = streamReader.ReadLine()) != null)
            {
                string key_value_str = String.Join("", raw_line.Split('(', ')', ' '));
                string[] action_state_value = key_value_str.Split(':');

                CardId action = (CardId)int.Parse(action_state_value[0]);
                int state = int.Parse(action_state_value[1]);
                double value = double.Parse(action_state_value[2].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);

                if(!qTable.TryAdd(Tuple.Create(action, state), value))
                {
                    // ErrorFileWriteLine($"Didn't add key to qTable");
                    // ErrorFileWriteLine($"action = {action}");
                    // ErrorFileWriteLine($"state = {state}");
                    // ErrorFileWriteLine($"value = {value}");
                }
            }
        }
    }

    public void SaveQTableToFile()
    {
        using (var sw = new StreamWriter(FilePaths.qTablePath))
        {
            foreach (var (key, value) in qTable)
            {
                sw.WriteLine(((int)key.Item1).ToString() + " : " + key.Item2.ToString() + " : " + value.ToString());
            }
        }
    }

    public void ErrorFileWriteLine(string msg)
    {
        using (var sw = new StreamWriter(FilePaths.errorFile, append: true))
        {
            sw.WriteLine(msg);
        }
    }

    public void TmpFileWriteLine(string msg)
    {
        using (var sw = new StreamWriter(FilePaths.tmpFile, append: true))
        {
            sw.WriteLine(msg);
        }
    }

    public double TryToGetQValue(CardId played_card, int deck_cards)
    {
        if (deck_cards > Consts.max_deck_cards)
        {
            deck_cards = Consts.max_deck_cards;
        }
        QKey key = Tuple.Create(played_card, deck_cards);
        double q_value = 0;
        if (qTable.TryGetValue(key, out q_value))
        {
            return q_value;
        }
        else
        {
            // ErrorFileWriteLine("Didn't get key value from qTable");
            // ErrorFileWriteLine($"action = {played_card}");
            // ErrorFileWriteLine($"state = {deck_cards}");

            return 0;
        }
    }

    public int TryToGetQValueToInt(CardId played_card, int deck_cards)
    {
        int q_value = (int)TryToGetQValue(played_card, deck_cards);
        if (q_value == 0)
        {
            q_value = 1;
        }

        return q_value;
    }

    public Stage TransformGameStateToStages(SeededGameState sgs)
    {
        Func<int, Stage> FindStage = x =>
        {
            switch (x)
            {
                case >= 0 and < 10:
                    return Stage.Start;
                case >= 10 and < 20:
                    return Stage.Early;
                case >= 20 and < 30:
                    return Stage.Middle;
                case >= 30:
                    return Stage.Late;
                default:
                    using (var sw = new StreamWriter(FilePaths.errorFile, append: true))
                    {
                        // sw.WriteLine("Unexpected prestige value in TransfromGameStateToGrade() = " + x.ToString());
                        // sw.WriteLine(DateTime.Now + "\n");
                    }
                    return Stage.Late;
            }
        };

        int sum = sgs.EnemyPlayer.Prestige + turn_counter;

        Stage stage = FindStage(sum);

        return stage;
    }

    public void IncrementTurnCounter()
    {
        ++turn_counter;
    }

    public void UpdateDeckCardsCounter(SeededGameState sgs)
    {
        List<UniqueCard> all_cards = sgs.CurrentPlayer.Hand.Concat(sgs.CurrentPlayer.Played.Concat(sgs.CurrentPlayer.CooldownPile.Concat(sgs.CurrentPlayer.DrawPile))).ToList();
        int[] updated_counter = new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0};
        foreach (var card in all_cards)
        {
            if (card.Type != CardType.STARTER)
            updated_counter[(int)card.Deck]++;
        }

        for (int i = 0; i < updated_counter.Count(); ++i)
        {
            if (updated_counter[i] > Consts.max_deck_cards)
            {
                updated_counter[i] = Consts.max_deck_cards;
            }
        }
        deck_cards_counters = updated_counter;
    }

    public void ResetVariables()
    {
        turn_counter = 0;

        deck_cards_counters = new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0};

        actions_before_this_turn_count = 0;
        played_cards_this_turn = new HashSet<UniqueCard>();
        gained_cards = new SortedSet<CardId>();
    }

    public double MaxQValueFromNewState(SeededGameState sgs, int deck_id)
    {
        double result = 0;

        var tavern_cards = sgs.TavernCards.Concat(sgs.TavernAvailableCards);

        HashSet<CardId> deck_cards = new HashSet<CardId>();

        foreach (var card in tavern_cards)
        {
            if (card.Deck == PatronId.PELIN)
            {
                deck_cards.Add(card.CommonId);
            }
        }

        foreach (var possible_card in deck_cards)
        {
            // +1 counter because of possible card in new state
            double possible_value = TryToGetQValue(possible_card, deck_cards_counters[deck_id] + 1);
            if (possible_value > result)
            {
                result = possible_value;
            }
        }

        return result;
    }

    public void CalculateNewQValue(SeededGameState sgs, CardId played_card, int card_score, int deck_id)
    {
        double q_value = TryToGetQValue(played_card, deck_cards_counters[deck_id]);

        double new_q_value = (1.0 - learningRate) * q_value + learningRate * (card_score + discountFactor * MaxQValueFromNewState(sgs, deck_id));

        QKey q_key = Tuple.Create(played_card, deck_cards_counters[deck_id]);
        qTable[q_key] = new_q_value;
    }

    // Pick best by value move or explore other move
    // Return weakest move, if didn't pick any
    // public Move PickBuyMove(SeededGameState sgs, List<Move> buy_moves)
    // {
    //     Random random = new Random();

    //     List<Tuple<int, double>> moves_values = new List<Tuple<int, double>>();
    //     for (int i = 0; i < buy_moves.Count; ++i)
    //     {
    //         QKey key = ConstructQTableKey(sgs, buy_moves[i]);
    //         double q_value = TryToGetQValue(key);
    //         moves_values.Add(Tuple.Create(i, q_value));
    //     }

    //     // Sort in descending order
    //     moves_values.Sort((x, y) => y.Item2.CompareTo(x.Item2));

    //     Move result = buy_moves[moves_values.First().Item1];

    //     // if (random.NextDouble() < explorationChance)
    //     // {
    //     //     result = buy_moves[random.Next(buy_moves.Count)];
    //     // }

    //     var card_move = (SimpleCardMove)result;
    //     IncrementDeckCardsCounter(card_move.Card.Deck);

    //     return result;
    // }

    // qltodo: change to more reasonable weights
    public int ScoreForCompletedAction(CompletedAction action, SeededGameState sgs)
    {
        int result = 0;

        int power_prestige_w = 2;
        int coin_w = 1;
        int average_card_w = 3;
        int max_cost = 10;
        int patron_w = 6;

        switch (action.Type)
        {
            case CompletedActionType.ACQUIRE_CARD:
            {
                if (action.TargetCard is not null)
                {
                    result += action.TargetCard.Cost;
                }
                break;
            }
            case CompletedActionType.GAIN_COIN:
            {
                result += action.Amount * coin_w;
                break;
            }
            case CompletedActionType.GAIN_POWER:
            {
                result += action.Amount * power_prestige_w;
                break;
            }
            case CompletedActionType.GAIN_PRESTIGE:
            {
                result += action.Amount * power_prestige_w;
                break;
            }
            case CompletedActionType.OPP_LOSE_PRESTIGE:
            {
                result += Math.Abs(action.Amount) * power_prestige_w;
                break;
            }
            case CompletedActionType.REPLACE_TAVERN:
            {
                // combo numbers?
                break;
            }
            case CompletedActionType.DESTROY_CARD:
            {
                if (action.TargetCard is not null)
                {
                    result += (max_cost - action.TargetCard.Cost) / 2;
                }
                break;
            }
            case CompletedActionType.DRAW:
            {
                result += average_card_w;
                break;
            }
            case CompletedActionType.DISCARD:
            {
                result += average_card_w;
                break;
            }
            case CompletedActionType.REFRESH:
            {
                if (action.TargetCard is not null)
                {
                    result += action.TargetCard.Cost;
                }
                break;
            }
            case CompletedActionType.KNOCKOUT:
            {
                // if (action.TargetCard is not null)
                // {
                //     if (action.TargetCard.HP > 0)
                //     {
                //         result += action.TargetCard.HP * power_prestige_w;
                //     }
                // }
                break;
            }
            case CompletedActionType.ADD_PATRON_CALLS:
            {
                result += patron_w;
                break;
            }
            case CompletedActionType.ADD_SUMMERSET_SACKING:
            {
                result += 2;
                break;
            }
            case CompletedActionType.HEAL_AGENT:
            {
                result += action.Amount * power_prestige_w;
                break;
            }
            default:
            {
                // Other actions type don't bring value from played card.
                break;
            }
        }

        return result;
    }

    public void SaveGainedCards(SeededGameState sgs)
    {
        List<UniqueCard> all_cards = sgs.CurrentPlayer.Hand.Concat(sgs.CurrentPlayer.Played.Concat(sgs.CurrentPlayer.CooldownPile.Concat(sgs.CurrentPlayer.DrawPile))).ToList();

        foreach (var card in all_cards)
        {
            if (card.Type != CardType.STARTER && card.Type != CardType.CURSE)
            {
                if (!gained_cards.Contains(card.CommonId))
                {
                    gained_cards.Add(card.CommonId);
                }
            }
        }
    }

    public void SavePlayedCardIfApplicable(Move move)
    {
        if (move is SimpleCardMove card && gained_cards.Contains(card.Card.CommonId))
        {
            played_cards_this_turn.Add(card.Card);
        }
    }

    public void UpdateQValuesForPlayedCardsAtEndOfTurn(SeededGameState sgs)
    {
        Dictionary<CardId, int> card_id_to_turn_score = new Dictionary<CardId, int>();

        List<CompletedAction> actions_completed_this_turn = sgs.CompletedActions.Skip(Math.Max(0, actions_before_this_turn_count)).ToList();

        foreach (var action in actions_completed_this_turn)
        {
            if (action.SourceCard is not null && gained_cards.Contains(action.SourceCard.CommonId))
            {
                int action_score = ScoreForCompletedAction(action, sgs);
                if (card_id_to_turn_score.ContainsKey(action.SourceCard.CommonId))
                {
                    card_id_to_turn_score[action.SourceCard.CommonId] += action_score;
                }
                else
                {
                    card_id_to_turn_score[action.SourceCard.CommonId] = action_score;
                }
            }
        }

        foreach (var (card_id, score) in card_id_to_turn_score)
        {
            var unique_card = GlobalCardDatabase.Instance.GetCard(card_id);
            CalculateNewQValue(sgs, card_id, score, (int)unique_card.Deck);
        }

        played_cards_this_turn = new HashSet<UniqueCard>();
        actions_before_this_turn_count = sgs.CompletedActions.Count;
    }

    public int Heuristic(SeededGameState sgs)
    {
        int stage = (int)TransformGameStateToStages(sgs);
        int player_score = 0;
        int enemy_score = 0;

        player_score += sgs.CurrentPlayer.Prestige * Consts.prestige_weight[stage];
        player_score += sgs.CurrentPlayer.Power * Consts.power_weight[stage];
        player_score += sgs.CurrentPlayer.Coins * Consts.coins_weight[stage];

        enemy_score += sgs.EnemyPlayer.Prestige * Consts.prestige_weight[stage];
        enemy_score += sgs.EnemyPlayer.Power * Consts.power_weight[stage];
        enemy_score += sgs.EnemyPlayer.Coins * Consts.coins_weight[stage];

        int enemy_patrons = 0;

        foreach (var (key, value) in sgs.PatronStates.All)
        {
            if (key == PatronId.TREASURY)
            {
                continue;
            }
            if (value == sgs.CurrentPlayer.PlayerID)
            {
                player_score += Consts.patron_weight[stage];
                switch (key)
                {
                    case PatronId.ANSEI:
                        player_score += Consts.ansei_weight[stage];
                        break;
                    case PatronId.DUKE_OF_CROWS:
                        player_score += Consts.crow_weight[stage];
                        break;
                    case PatronId.ORGNUM:
                        player_score += Consts.orgnum_weight[stage];
                        break;
                    default:
                        break;
                }
            }
            else if (value == sgs.EnemyPlayer.PlayerID)
            {
                ++enemy_patrons;
                enemy_score += Consts.patron_weight[stage];
                switch (key)
                {
                    case PatronId.ANSEI:
                        enemy_score += Consts.ansei_weight[stage];
                        break;
                    case PatronId.DUKE_OF_CROWS:
                        enemy_score += Consts.crow_weight[stage];
                        break;
                    case PatronId.ORGNUM:
                        enemy_score += Consts.orgnum_weight[stage];
                        break;
                    default:
                        break;
                }
            }
        }

        if (enemy_patrons == 2)
        {
            enemy_score += Consts.patron_weight[stage] * 2;
        }
        else if (enemy_patrons == 3)
        {
            enemy_score += Consts.patron_weight[stage] * 4;
        }
        else if (enemy_patrons == 4)
        {
            enemy_score += Consts.patron_weight[stage] * 100;
        }

        List<UniqueCard> all_cards = sgs.CurrentPlayer.Hand.Concat(sgs.CurrentPlayer.Played.Concat(sgs.CurrentPlayer.CooldownPile.Concat(sgs.CurrentPlayer.DrawPile))).ToList();
        int[] player_combo = new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0};
        List<UniqueCard> all_enemy_cards = sgs.EnemyPlayer.Hand.Concat(sgs.EnemyPlayer.DrawPile).Concat(sgs.CurrentPlayer.Played.Concat(sgs.CurrentPlayer.CooldownPile)).ToList();
        int[] enemy_combo = new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0};

        foreach (var card in all_cards)
        {
            if (card.CommonId == CardId.GOLD)
            {
                player_score += Consts.gold_card_weight[stage];
            }
            else
            {
                int q_value = TryToGetQValueToInt(card.CommonId, deck_cards_counters[(int)card.Deck]);
                player_score += q_value * Consts.card_weight[stage];
                player_combo[(int)card.Deck]++;
            }
        }

        foreach (var card in all_enemy_cards)
        {
            enemy_combo[(int)card.Deck]++;
        }

        foreach (var combo in player_combo)
        {
            player_score += Consts.combo_weight[stage] * combo;
        }
        foreach (var combo in enemy_combo)
        {
            enemy_score += Consts.combo_weight[stage] * combo;
        }

        foreach (var agent in sgs.CurrentPlayer.Agents)
        {
            int q_value = TryToGetQValueToInt(agent.RepresentingCard.CommonId, deck_cards_counters[(int)agent.RepresentingCard.Deck]);
            player_score += Consts.active_agent_weight[stage] * q_value + agent.CurrentHp * Consts.hp_weight[stage];
        }

        foreach (var agent in sgs.EnemyPlayer.Agents)
        {
            int q_value = TryToGetQValueToInt(agent.RepresentingCard.CommonId, deck_cards_counters[(int)agent.RepresentingCard.Deck]);
            enemy_score += Consts.active_agent_weight[stage] * q_value + agent.CurrentHp * Consts.hp_weight[stage];
        }

        return player_score - enemy_score;
    }
}