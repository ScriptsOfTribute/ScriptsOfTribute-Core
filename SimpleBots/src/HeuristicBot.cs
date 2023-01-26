using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Text;

namespace SimpleBots;

public class HeuristicBot : AI
{
    Random random = new Random(); 

    private bool startOfGame = true;

    private bool startOfTurn = true;

    private int knockoutAgent = 6498;

    private int heuristicPatronAmountCardValue = 2;

    private int heuristicConstAttackValue =3542;

    private int heuristicWinMoveValue = -2254;

    private int heuristicValueOfPatronActivations = -2739;

    private int bigHeuristicValue = 5000;

    private int coinsNeed = 0;

    private int powerNeed = 0;

    private int coinsValue = 3928;
    private int powerValue = 10;
    private int prestigeValue = 3;

    private PatronId deckInPlay;

    private UniqueCard? chosenCard;

    private PatronId[] orderOfPlay = new PatronId[5];

    private HashSet<UniqueCard> cardsInHandAndOnBoard= new HashSet<UniqueCard>();
    private StringBuilder bugLog = new StringBuilder();

    private int allCompletedActions;

    public class DuplicateKeyComparer<TKey>: IComparer<TKey> where TKey:IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0){
                return 1;
            }
            else{
                return result;
            }
        }
    }

    public int[] GetGenotype(){
        return new int[] {knockoutAgent, heuristicPatronAmountCardValue, heuristicConstAttackValue, heuristicWinMoveValue, heuristicValueOfPatronActivations, bigHeuristicValue, coinsValue, powerValue, prestigeValue};
    }

    public void SetGenotype(int[] values){
        knockoutAgent = values[0];
        heuristicPatronAmountCardValue = values[1];
        heuristicConstAttackValue = values[2];
        heuristicWinMoveValue = values[3];
        heuristicValueOfPatronActivations = values[4];
        bigHeuristicValue = values[5]; 
        coinsValue = values[6];
        powerValue = values[7];
        prestigeValue = values[8];
    }

    public class PatronDeckCards{
        public int SafeToPlayMaxCombo;
        public int LeftToPlay;
        public List<UniqueCard>[] SortedByMaxCombo;

        public PatronDeckCards(){
            SafeToPlayMaxCombo = 0;
            LeftToPlay = 0;
            SortedByMaxCombo = new List<UniqueCard>[] {new List<UniqueCard>(), new List<UniqueCard>(), new List<UniqueCard>(), new List<UniqueCard>()};
        }
    }

    private Dictionary<PatronId, PatronDeckCards> sortedByPatronCardsInHand = new Dictionary<PatronId, PatronDeckCards>();

    private Dictionary<PatronId, int> numberOfPatronCards = new Dictionary<PatronId, int>();

    private List<Move> GetAllMoveOfType(List<Move> possibleMoves, CommandEnum commandType){
        return possibleMoves.FindAll(x => x.Command == commandType);
    }

    private void UpdateAmountOfPatronsCards(GameState gameState){
        numberOfPatronCards = new Dictionary<PatronId, int>();
        foreach (PatronId patron in gameState.Patrons){
            numberOfPatronCards[patron] = 0;
        }
        List<UniqueCard> allPlayerCards = GetAllPlayerCards(gameState);
        foreach (UniqueCard card in allPlayerCards){
            numberOfPatronCards[card.Deck] += 1;
        }
    }

    private void HandleStartOfGame(List<PatronId> patrons){
        sortedByPatronCardsInHand = new Dictionary<PatronId, PatronDeckCards>();
        
        orderOfPlay[0] = PatronId.TREASURY;
        sortedByPatronCardsInHand.Add(PatronId.TREASURY, new PatronDeckCards());

        List<PatronId> patronsWithoutTresury = patrons.FindAll(x => x != PatronId.TREASURY).ToList();
        for (int i = 0; i < 4; i++){
            sortedByPatronCardsInHand.Add(patronsWithoutTresury[i], new PatronDeckCards());
            orderOfPlay[i+1] = patronsWithoutTresury[i];
        }
    }

    private void ClearPatronDeckCards(){
        foreach(PatronId patron in orderOfPlay){
            sortedByPatronCardsInHand[patron] = new PatronDeckCards();
        }
    }

    private void RemoveDestroyedCards(GameState gameState, List<Move> playCardMoves){
        List<UniqueCard> playableCard = new List<UniqueCard>();
        foreach (Move m in playCardMoves){
            SimpleCardMove move = m as SimpleCardMove;
            playableCard.Add(move.Card);
        }
        int numberOfLastActions = gameState.CompletedActions.Count - allCompletedActions;
        List<CompletedAction> lastCompltedActions = gameState.CompletedActions.TakeLast(numberOfLastActions).ToList();
        allCompletedActions = gameState.CompletedActions.Count;
        foreach (CompletedAction action in lastCompltedActions){
            //Console.WriteLine(action.ToString());
            if ((action.Type == CompletedActionType.DESTROY_CARD || action.Type ==CompletedActionType.DISCARD || action.Type ==CompletedActionType.TOSS || action.Type ==CompletedActionType.REFRESH) && action.TargetCard is not null){
                UniqueCard destroyedCard = action.TargetCard;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < sortedByPatronCardsInHand[destroyedCard.Deck].SortedByMaxCombo[i].Count; j++)
                    {
                        if (sortedByPatronCardsInHand[destroyedCard.Deck].SortedByMaxCombo[i][j].UniqueId == destroyedCard.UniqueId){
                            sortedByPatronCardsInHand[destroyedCard.Deck].SortedByMaxCombo[i].RemoveAt(j);
                            sortedByPatronCardsInHand[destroyedCard.Deck].LeftToPlay -=1;
                            break;
                        }
                    }
                }
            }
        }
        foreach(UniqueCard card in playableCard){
            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < sortedByPatronCardsInHand[card.Deck].SortedByMaxCombo[i].Count; j++)
                {
                    if (sortedByPatronCardsInHand[card.Deck].SortedByMaxCombo[i][j].UniqueId == card.UniqueId){
                        found = true;
                    }
                }
            }
            if (!found){
                HandleCard(card);
            }
        }
    }

    private int GetMaxComboFromCard(UniqueCard card){
        for (int i = 3; i >= 0; i--){
            if (card.Effects[i] != null){
                return i;
            }
        }
        return 0;
    }

    private void HandleCard(UniqueCard card){
        int maxCombo = GetMaxComboFromCard(card);
        sortedByPatronCardsInHand[card.Deck].SortedByMaxCombo[maxCombo].Add(card);
        sortedByPatronCardsInHand[card.Deck].LeftToPlay += 1;
    }

    private void HandleAllCardsInHand(List<UniqueCard> cards){
        foreach (UniqueCard card in cards){
            if (!cardsInHandAndOnBoard.Contains(card) && card.Type != CardType.CONTRACT_AGENT){
                HandleCard(card);
                cardsInHandAndOnBoard.Add(card);
            }
        }
    }

    private List<UniqueCard> GetAllPlayerCards(GameState gameState){
        return gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
    }

    private int CoinNeed(GameState gameState){
        List<UniqueCard> notAffordableCards = gameState.TavernAvailableCards.FindAll(x=> x.Cost==1+gameState.CurrentPlayer.Coins);
        UpdateAmountOfPatronsCards(gameState);
        foreach (UniqueCard card in notAffordableCards){
            if (CardTierList.GetCardTier(card.Name)== TierEnum.A || numberOfPatronCards[card.Deck]>5){
                return 1;
            }
        }
        return 0;
    }

    private (UniqueCard, int) BuyCard(List<UniqueCard> tavern, GameState gameState, List<Move> possibleMoves) {
        List<UniqueCard> affordableCards = tavern.FindAll(x=> x.Cost<=gameState.CurrentPlayer.Coins);
        UniqueCard chosenCard = affordableCards[0];
        int bestHeuristicScore = 0;
        UpdateAmountOfPatronsCards(gameState);

        foreach(UniqueCard card in affordableCards) {
            int cardHeuristicScore = 0;
            int tier = 0;
            int numberOfCardsOfThisPatron = numberOfPatronCards[card.Deck];

            switch (card.Deck) {
                case PatronId.TREASURY:
                    int opponnetAgents = gameState.EnemyPlayer.Agents.Count;
                    tier = 0;
                    // really situational deck, you don't want to waste money unless boardstate need that
                    switch (card.Name) {
                        case "Tithe":
                            int numberOfPatronWhichFavoursMe = 0;

                            foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All) {
                                if (entry.Value == gameState.CurrentPlayer.PlayerID) {
                                    numberOfPatronWhichFavoursMe += 1;
                                }
                                else {
                                    if (!possibleMoves.Contains(Move.CallPatron(entry.Key))) {
                                        continue;
                                    }
                                }
                            }

                            if (numberOfPatronWhichFavoursMe + gameState.CurrentPlayer.PatronCalls + 1 == 4) {
                                cardHeuristicScore = heuristicWinMoveValue;
                            }
                            break;

                        case "Black Sacrament":
                            foreach(SerializedAgent agent in gameState.EnemyPlayer.Agents) {
                                tier += (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
                            }

                            cardHeuristicScore = opponnetAgents * heuristicConstAttackValue + tier;
                            break;

                        case "Ambush":
                            foreach(SerializedAgent agent in gameState.EnemyPlayer.Agents) {
                                tier += (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
                            }

                            cardHeuristicScore = opponnetAgents * heuristicConstAttackValue * 2 + tier;
                            break;

                        default:
                            cardHeuristicScore = 0;
                            break;
                    }
                    break;

                default:
                    tier = (int)CardTierList.GetCardTier(card.Name);
                    int combo = 0;

                    for (int i =0; i < 4; i++) {
                        if (card.Effects[i] is not null){
                            combo += i*(i+1);
                        }
                    }

                    cardHeuristicScore = (int)Math.Pow((double)numberOfCardsOfThisPatron, (double)heuristicPatronAmountCardValue) + tier + combo - card.Cost; 
                    break;    
            }

            if (cardHeuristicScore > bestHeuristicScore) {
                bestHeuristicScore = cardHeuristicScore;
                chosenCard = card;
            }
        }
        
        return (chosenCard, bestHeuristicScore);
    }

    private UniqueCard PlayCard(){
        if (sortedByPatronCardsInHand[deckInPlay].LeftToPlay <=0){
            for (int i = 0; i < 5; i++){
                if (sortedByPatronCardsInHand[orderOfPlay[i]].LeftToPlay > 0){
                    deckInPlay = orderOfPlay[i];
                    break;
                }
            }
        }

        for (int i = 0; i < Math.Min(sortedByPatronCardsInHand[deckInPlay].SafeToPlayMaxCombo, 4); i++){
            if (sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[i].Any()){
                UniqueCard chosenCard = sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[i][0];
                sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[i].RemoveAt(0);
                sortedByPatronCardsInHand[deckInPlay].LeftToPlay -=1;
                sortedByPatronCardsInHand[deckInPlay].SafeToPlayMaxCombo +=1;
                return chosenCard;
            }
        }

        int minComboNumerAtTheMoment = 4;
        int minComboValue = 4;
        int maxPossibleCombo = sortedByPatronCardsInHand[deckInPlay].LeftToPlay + sortedByPatronCardsInHand[deckInPlay].SafeToPlayMaxCombo - 1;
    
        int cardToPlayMaxCombo = -1;
        int cardToPlayIndex = -1;

        for (int i = 0; i < 4; i++){
            int cardIndex = -1;
            foreach (UniqueCard card in sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[i]){
                cardIndex++;

                int combo = 0;
                int sumCombo = 0;

                for (int j = 1; j < 4; j++){
                    if (card.Effects[j] != null && j <= maxPossibleCombo){
                        combo++;
                        sumCombo += j;
                    }
                }

                if (combo < minComboNumerAtTheMoment || (combo == minComboNumerAtTheMoment && sumCombo < minComboValue)) {
                    minComboNumerAtTheMoment = combo;
                    minComboValue = sumCombo;

                    cardToPlayMaxCombo = i;
                    cardToPlayIndex = cardIndex;
                }
            }
        }

        UniqueCard cardToSacrifice = sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[cardToPlayMaxCombo][cardToPlayIndex];
        sortedByPatronCardsInHand[deckInPlay].SortedByMaxCombo[cardToPlayMaxCombo].RemoveAt(cardToPlayIndex);
        sortedByPatronCardsInHand[deckInPlay].LeftToPlay -=1;
        sortedByPatronCardsInHand[deckInPlay].SafeToPlayMaxCombo +=1;
        return cardToSacrifice;
    }

    private List<SerializedAgent> OpponentAgents(GameState gameState){
        List<SerializedAgent> opponentAgents = gameState.EnemyPlayer.Agents;
        List<SerializedAgent> tauntOpponentAgents = gameState.EnemyPlayer.Agents.FindAll(agent => agent.RepresentingCard.Taunt);
        if (tauntOpponentAgents.Any()){
            return tauntOpponentAgents;
        }
        return opponentAgents;
    }
    private UniqueCard AttackAgent(GameState gameState){
        List<SerializedAgent> opponentAgents = OpponentAgents(gameState);
        int myPower = gameState.CurrentPlayer.Power;
        int idxAgentToKill = -1;
        int tierAgentToKill = 0;
        int idxAgentToHurt = -1;
        int tierAgentToHurt = 0;
        for (int i = 0; i < opponentAgents.Count(); i++){
            int tier = (int)CardTierList.GetCardTier(opponentAgents[i].RepresentingCard.Name);

            if (opponentAgents[i].CurrentHp <= myPower){
                if (tier > tierAgentToKill){
                    idxAgentToKill = i;
                    tierAgentToKill = tier;
                }
            }
            else{
                if (tier > tierAgentToHurt){
                    idxAgentToHurt = i;
                    tierAgentToHurt = tier;
                }
            }
        }
        if (idxAgentToKill != -1){
            return opponentAgents[idxAgentToKill].RepresentingCard;
        }
        return opponentAgents[idxAgentToHurt].RepresentingCard;
    }

    private int HeuristicValueOfPatronActivations(GameState gameState, PatronId patron){
        List<UniqueCard> drawPileCards = gameState.CurrentPlayer.DrawPile;
        List<UniqueCard> handCards = gameState.CurrentPlayer.Hand;
        List<UniqueCard> playedCards = gameState.CurrentPlayer.Played;
        List<UniqueCard> coolDownPileCards = gameState.CurrentPlayer.CooldownPile;


        switch (patron){
            case PatronId.ANSEI:
                if (coinsNeed > 0){
                    return bigHeuristicValue;
                }
                else{
                    return 0;
                }
            case PatronId.DUKE_OF_CROWS:
                if (gameState.CurrentPlayer.Coins>=10){
                    return (gameState.CurrentPlayer.Coins/gameState.CurrentPlayer.Prestige) * bigHeuristicValue;
                }
                if (gameState.CurrentPlayer.Coins-1+gameState.CurrentPlayer.Prestige>40){
                    return bigHeuristicValue;
                }
                return 0;
            case PatronId.RAJHIN:
                int opponentNumberOfCards = gameState.EnemyPlayer.HandAndDraw.Concat(gameState.EnemyPlayer.CooldownPile.Concat(gameState.EnemyPlayer.Played)).ToList().Count();
                if (opponentNumberOfCards == 0){
                    return 0;
                }
                return bigHeuristicValue * (5/opponentNumberOfCards);
            case PatronId.PSIJIC:
                int agentsTierValue = 0;
                foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents){
                    agentsTierValue += (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
                }
                return agentsTierValue * heuristicValueOfPatronActivations;
            case PatronId.ORGNUM:
                if (powerNeed > 0){
                    return heuristicValueOfPatronActivations * powerNeed * GetAllPlayerCards(gameState).Count;
                }
                return 0;
            case PatronId.HLAALU:
                if (gameState.CurrentPlayer.Prestige>30){
                    return bigHeuristicValue;
                }
                int cardsValue = 0;
                foreach(UniqueCard card in gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile).ToList()){
                    cardsValue = Math.Max(cardsValue, card.Cost * 20 - (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue) - (int)CardTierList.GetCardTier(card.Name));
                }
                return cardsValue;
            case PatronId.PELIN:
                return gameState.CurrentPlayer.CooldownPile.FindAll(x => x.Type == CardType.AGENT).Count() * heuristicValueOfPatronActivations;
            case PatronId.RED_EAGLE:
                return heuristicValueOfPatronActivations*5;
            case PatronId.TREASURY:
                int dTierCardsAmount = playedCards.Concat(coolDownPileCards).ToList().FindAll(x => x.Name != "Gold" && CardTierList.GetCardTier(x.Name) == TierEnum.D).Count * 2;
                dTierCardsAmount += drawPileCards.Concat(handCards).ToList().FindAll(x => x.Name != "Gold" && CardTierList.GetCardTier(x.Name) == TierEnum.D).Count;

                int goldCardsAmount = playedCards.Concat(coolDownPileCards).ToList().FindAll(x => x.Name == "Gold").Count * 2;
                goldCardsAmount += drawPileCards.Concat(handCards).ToList().FindAll(x => x.Name == "Gold").Count;
                
                return (dTierCardsAmount + goldCardsAmount)* heuristicValueOfPatronActivations;
            default:
                return 0;
        }
    }

    private PatronId ActivatePatron(GameState gameState, List<Move> possibleMoves){
        List<Move> patronMovess = possibleMoves.FindAll(x => x.Command == CommandEnum.CALL_PATRON);
        List<SimplePatronMove> patronMoves = new List<SimplePatronMove>();
        SimplePatronMove patronMove;
        foreach(Move move in patronMovess){
            patronMove = move as SimplePatronMove;
            patronMoves.Add(patronMove);
        }
        List<PatronId> patronThatCanBeActivated = patronMoves.Select(x => x.PatronId).ToList();
        List<UniqueCard> coolDownPile = gameState.CurrentPlayer.CooldownPile;
        List<UniqueCard> played = gameState.CurrentPlayer.Played;
        List<UniqueCard> cursed = gameState.CurrentPlayer.Played.FindAll(card => card.Type==CardType.CURSE);
        int numberOfPatronWhichFavoursMe = 0;
        int numberOfPatronWhichFavoursOpponent = 0;
        int myNumberOfPatronActivationToWin = 0;
        int opppnentNumberOfPatronActivationToWin = 0;

        foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All) {
            if (PatronId.TREASURY == entry.Key){
                continue;
            }
            if (PatronId.ANSEI == entry.Key){
                if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED){
                    myNumberOfPatronActivationToWin += 1;
                    opppnentNumberOfPatronActivationToWin += 1;
                }
                if (entry.Value == gameState.CurrentPlayer.PlayerID){
                    opppnentNumberOfPatronActivationToWin += 1;
                }
                else{
                    myNumberOfPatronActivationToWin += 1;
                }
            }
            else{
                if (entry.Value == gameState.CurrentPlayer.PlayerID) {
                    opppnentNumberOfPatronActivationToWin += 2;
                    numberOfPatronWhichFavoursMe += 1;
                }
                if(entry.Value == gameState.EnemyPlayer.PlayerID){
                    myNumberOfPatronActivationToWin += 2;
                    numberOfPatronWhichFavoursOpponent +=1;
                }
                else{
                    myNumberOfPatronActivationToWin += 1;
                    opppnentNumberOfPatronActivationToWin += 1;
                }
            }
        }

        if (myNumberOfPatronActivationToWin <= gameState.CurrentPlayer.PatronCalls){
            foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All){
                if (entry.Value != gameState.CurrentPlayer.PlayerID && patronThatCanBeActivated.Contains(entry.Key)){
                    return entry.Key;
                }
            }
        }

        if (opppnentNumberOfPatronActivationToWin<= 2){
            foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All){
                if (entry.Value == gameState.EnemyPlayer.PlayerID && patronThatCanBeActivated.Contains(entry.Key)){
                    if(entry.Key == PatronId.DUKE_OF_CROWS){
                        if (opppnentNumberOfPatronActivationToWin==1){
                            return entry.Key;
                        }
                    }
                    else{
                        return entry.Key;
                    }
                }
            }
        }
        
        if (cursed.Any() && patronThatCanBeActivated.Contains(PatronId.TREASURY)){
            chosenCard = cursed[0];
            return PatronId.TREASURY;
        }
        /*
        if (gameState.CurrentPlayer.Played.FindAll(x => x.Name== "Gold").Any() && patronThatCanBeActivated.Contains(PatronId.TREASURY)){
            return PatronId.TREASURY;
        }*/

        int maxHeuristicValueOfActivation = -10000;
        int patronHeuristicActivationValue;
        PatronId selectedPatron = patronThatCanBeActivated[0];
        List<PatronId> allPatrons = gameState.PatronStates.All.Keys.ToList();
        allPatrons.Add(PatronId.TREASURY);
        foreach (PatronId patron in patronThatCanBeActivated){
            patronHeuristicActivationValue = HeuristicValueOfPatronActivations(gameState, patron);
            if (patronHeuristicActivationValue > maxHeuristicValueOfActivation){
                maxHeuristicValueOfActivation = patronHeuristicActivationValue;
                selectedPatron = patron;
            }
        }
        return selectedPatron;
    }

    private UniqueCard PatronActivationMove(GameState gameState, List<Move> possibleMoves){
        UpdateAmountOfPatronsCards(gameState);
        Dictionary <UniqueCard, int> cardValue = new Dictionary<UniqueCard, int>();
        switch (gameState.PendingChoice.Context.PatronSource){
            case PatronId.TREASURY:
                foreach(Move m in possibleMoves){
                    MakeChoiceMove<UniqueCard> move = m as MakeChoiceMove<UniqueCard>;
                    foreach (UniqueCard card in move.Choices){
                        if (card.Name == "Gold"){
                            cardValue[card] = 0;
                            continue;
                        }
                        if (card.Type == CardType.CURSE){
                            cardValue[card] = -1000;
                            continue;
                        }
                        else{
                            cardValue[card] = (int)CardTierList.GetCardTier(card.Name) + numberOfPatronCards[card.Deck] * heuristicPatronAmountCardValue;
                            continue;
                        }
                    }
                }
                return cardValue.OrderBy(x => x.Value).First().Key;
            case PatronId.HLAALU:
                foreach(Move m in possibleMoves){
                    MakeChoiceMove<UniqueCard> move = m as MakeChoiceMove<UniqueCard>;
                    foreach (UniqueCard card in move.Choices){
                        if (gameState.EnemyPlayer.Prestige >= gameState.CurrentPlayer.Prestige){
                            cardValue[card] = card.Cost -1;
                        }
                        else{
                            cardValue[card] = card.Cost * 20 - (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue) - (int)CardTierList.GetCardTier(card.Name);
                        }
                    }
                }
                return cardValue.OrderBy(x => x.Value).Last().Key;
            case PatronId.PELIN:
                foreach(Move m in possibleMoves){
                    MakeChoiceMove<UniqueCard> move = m as MakeChoiceMove<UniqueCard>;
                    foreach (UniqueCard card in move.Choices){
                        cardValue[card] = (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue) + (int)CardTierList.GetCardTier(card.Name);
                    }
                }
                return cardValue.OrderBy(x => x.Value).Last().Key;
            default:
            //case PatronId.PSIJIC:
                foreach(Move m in possibleMoves){
                    MakeChoiceMove<UniqueCard> move = m as MakeChoiceMove<UniqueCard>;
                    foreach (UniqueCard card in move.Choices){
                        cardValue[card] = (int)CardTierList.GetCardTier(card.Name) + gameState.EnemyPlayer.Agents.Find(agent => agent.RepresentingCard.UniqueId == card.UniqueId).CurrentHp;
                    }
                }
                return cardValue.OrderBy(x => x.Value).Last().Key;
        }
    }
    
    private int GetEffectChoiceValue(UniqueEffect effect, GameState gameState){
        UpdateAmountOfPatronsCards(gameState);
        int counter = 0;
        switch (effect.Type){
            case EffectType.GAIN_COIN:
                foreach(UniqueCard card in gameState.TavernAvailableCards){
                    if (CardTierList.GetCardTier(card.Name)>=TierEnum.B || numberOfPatronCards[card.Deck]>=3){
                        if (card.Cost>= gameState.CurrentPlayer.Coins && card.Cost <= gameState.CurrentPlayer.Coins+effect.Amount){
                            counter +=1;
                        }
                    }
                }
                if (gameState.EnemyPlayer.Prestige <= 15){
                    return bigHeuristicValue;
                }
                return counter*100;
            case EffectType.GAIN_POWER:
                if (gameState.EnemyPlayer.Prestige>=25){
                    return bigHeuristicValue;
                }
                return effect.Amount * powerValue;
            case EffectType.GAIN_PRESTIGE:
                return effect.Amount * prestigeValue;
            case EffectType.ACQUIRE_TAVERN:
                int maxHeuristicValue = -1;
                foreach(UniqueCard card in gameState.TavernAvailableCards){
                    maxHeuristicValue = Math.Max(maxHeuristicValue,  (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue) + (int)CardTierList.GetCardTier(card.Name));
                }
                return maxHeuristicValue;
            case EffectType.OPP_LOSE_PRESTIGE:
                if (gameState.EnemyPlayer.Prestige>=gameState.CurrentPlayer.Prestige){
                    return bigHeuristicValue;
                }
                return (int)((double)gameState.EnemyPlayer.Prestige/(double)gameState.CurrentPlayer.Prestige)*1000;
            case EffectType.REPLACE_TAVERN:
                int cardLeftInHand = gameState.CurrentPlayer.Hand.Count();
                if (cardLeftInHand >=1 ){
                    List<UniqueCard> uselessCards = gameState.TavernAvailableCards.FindAll(card => card.Cost > gameState.CurrentPlayer.Coins || CardTierList.GetCardTier(card.Name) < TierEnum.B);
                    return uselessCards.Count * 100;
                }
                List<UniqueCard> tooGoodCards = gameState.TavernAvailableCards.FindAll(card => CardTierList.GetCardTier(card.Name) >= TierEnum.B);
                return tooGoodCards.Count * 150;
            case EffectType.DESTROY_CARD:
                if (GetAllPlayerCards(gameState).Count <= 5){
                    return -1000000000;
                }
                foreach (UniqueCard card in GetAllPlayerCards(gameState)){
                    if (CardTierList.GetCardTier(card.Name)<= TierEnum.C || numberOfPatronCards[card.Deck]<=3){
                        counter +=1;
                    }
                }
                return 100*counter;     
            case EffectType.DRAW:
                return 500 * gameState.CurrentPlayer.Hand.Count();
            case EffectType.OPP_DISCARD:
                return bigHeuristicValue;
            case EffectType.RETURN_TOP:
                int value = 0;
                foreach (UniqueCard card in gameState.CurrentPlayer.CooldownPile.TakeLast(effect.Amount)){
                    value += (int)CardTierList.GetCardTier(card.Name)+ (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue);
                }
                return value;                       
            case EffectType.TOSS:
                /*
                foreach(ComboState comboState in gameState.ComboStates.All.Values){
                    foreach(UniqueBaseEffect e in comboState.All){
                        Console.WriteLine(e.ToString());
                    }
                    //Console.WriteLine('')
                }
                */
                return 200 * gameState.CurrentPlayer.Hand.Count();
            case EffectType.KNOCKOUT:
            case EffectType.PATRON_CALL:
            case EffectType.CREATE_BOARDINGPARTY:
            //Heal
            default:
                return 500;
        }
    }

    private List<UniqueCard>? SelectKBestCards(List<UniqueCard>? cards, int k){
        SortedList<int, UniqueCard>? cardsValue = new SortedList<int, UniqueCard>(new DuplicateKeyComparer<int>());
        foreach (UniqueCard? card in cards){
            cardsValue.Add((int)CardTierList.GetCardTier(card.Name), card);
        }
        return cardsValue.TakeLast(k).Select(x => x.Value).ToList();
    }

    private List<UniqueCard>? SelectKWorstCards(List<UniqueCard>? cards, int k){
        SortedList<int, UniqueCard>? cardsValue = new SortedList<int, UniqueCard>(new DuplicateKeyComparer<int>());
        foreach (UniqueCard? card in cards){
            cardsValue.Add((int)CardTierList.GetCardTier(card.Name), card);
        }
        return cardsValue.Take(k).Select(x => x.Value).ToList();
    }

    private List<UniqueCard>? CardsSelection(GameState gameState){
    /*
        ENACT_CHOSEN_EFFECT,
    
    DISCARD_CARDS,
    
    TOSS_CARDS,
    
    COMPLETE_HLAALU,
    
    COMPLETE_TREASURY,
    */
        switch (gameState.PendingChoice.ChoiceFollowUp){
            case ChoiceFollowUp.REPLACE_CARDS_IN_TAVERN:
                if (gameState.CurrentPlayer.Hand.Count()>=1){
                    return SelectKWorstCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MaxChoices);
                }
                return SelectKBestCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MaxChoices);
            case ChoiceFollowUp.ENACT_CHOSEN_EFFECT:
                Console.WriteLine(gameState.PendingChoice.Context.ToString());
                return SelectKBestCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MaxChoices);
            case ChoiceFollowUp.COMPLETE_HLAALU:
                UpdateAmountOfPatronsCards(gameState);
                UniqueCard selectedCard = gameState.PendingChoice.PossibleCards[0];
                int value = 1000000;
                foreach(UniqueCard card in gameState.PendingChoice.PossibleCards){
                    int v = (int)Math.Pow((double)numberOfPatronCards[card.Deck], (double)heuristicPatronAmountCardValue) + (int)CardTierList.GetCardTier(card.Name);
                    if (v < value){
                        value = v;
                        selectedCard = card;
                    }
                }
                return new List<UniqueCard> {selectedCard};
            case ChoiceFollowUp.ACQUIRE_CARDS:
            case ChoiceFollowUp.KNOCKOUT_AGENTS:
            case ChoiceFollowUp.COMPLETE_PSIJIC:
            case ChoiceFollowUp.COMPLETE_PELLIN:
            case ChoiceFollowUp.REFRESH_CARDS:
                return SelectKBestCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MaxChoices);
            case ChoiceFollowUp.DESTROY_CARDS:
                if (GetAllPlayerCards(gameState).Count <= 10){
                    return SelectKWorstCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MinChoices);
                }
                return SelectKWorstCards(gameState.PendingChoice.PossibleCards, Math.Min(gameState.PendingChoice.MaxChoices, 1));
            default:
                return SelectKWorstCards(gameState.PendingChoice.PossibleCards, gameState.PendingChoice.MaxChoices);
        }
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round){
        return availablePatrons[random.Next(availablePatrons.Count)];
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves){

        if (startOfGame){
            startOfGame = false;
            HandleStartOfGame(gameState.PatronStates.All.Keys.ToList());
        }

        List<Move> playCardMoves = possibleMoves.FindAll(x => x.Command ==CommandEnum.PLAY_CARD || x.Command ==CommandEnum.ACTIVATE_AGENT);
        HandleAllCardsInHand(gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Agents.Select(agent => agent.RepresentingCard)).ToList());

        if (startOfTurn){
            startOfTurn = false;
            deckInPlay = PatronId.TREASURY;
            coinsNeed = 0;
            powerNeed = 0;

            List<SerializedAgent> contractAgents = gameState.CurrentPlayer.Agents.FindAll(agent => agent.RepresentingCard.Type == CardType.CONTRACT_AGENT);
            foreach (SerializedAgent contractAgent in contractAgents){
                HandleCard(contractAgent.RepresentingCard);
                cardsInHandAndOnBoard.Add(contractAgent.RepresentingCard);
            }
            allCompletedActions = gameState.CompletedActions.Count;
        }
        else{
            RemoveDestroyedCards(gameState, playCardMoves);
        }
        /*
        foreach(ComboState comboState in gameState.ComboStates.All.Values){
            foreach(UniqueBaseEffect e in comboState.All){
                Console.WriteLine(e.ToString());
            }
            //Console.WriteLine('')
        }
        */
       
        if (playCardMoves.Count > 0){
            chosenCard = PlayCard();
            if ((chosenCard.Type == CardType.AGENT  || chosenCard.Type==CardType.CONTRACT_AGENT)&& gameState.CurrentPlayer.Agents.Any(x => x.RepresentingCard.UniqueId == chosenCard.UniqueId)){
                return Move.ActivateAgent(chosenCard);
            }
            return Move.PlayCard(chosenCard);
            //playCardMoves.PickRandom(Rng);
        }
        if (gameState.EnemyPlayer.Prestige > 40 && gameState.CurrentPlayer.Prestige < gameState.EnemyPlayer.Prestige){
            if (possibleMoves.Contains(Move.CallPatron(PatronId.DUKE_OF_CROWS))){
                return Move.CallPatron(PatronId.DUKE_OF_CROWS);
            }
            if (possibleMoves.Contains(Move.CallPatron(PatronId.ORGNUM))){
                return Move.CallPatron(PatronId.ORGNUM);
            }
            List<Move> buyCardMoves = GetAllMoveOfType(possibleMoves, CommandEnum.BUY_CARD);
            foreach (Move m in buyCardMoves){
                SimpleCardMove move = m as SimpleCardMove;
                if (move.Card.Name == "Imprisonment" || move.Card.Name == "Blackmail"){
                    return m;
                }
            }
        }
        coinsNeed = CoinNeed(gameState);
        if (coinsNeed == 1 && possibleMoves.Contains(Move.CallPatron(PatronId.ANSEI))){
            return Move.CallPatron(PatronId.ANSEI);
        }
        /*
        if (gameState.CurrentPlayer.Coins == 2 && possibleMoves.Contains(Move.CallPatron(PatronId.TREASURY))){
            if (gameState.CurrentPlayer.Played.FindAll(x => x.Name =="Gold").Count>0 && gameState.CurrentPlayer.Prestige<20){
                return Move.CallPatron(PatronId.TREASURY);
            }
        }
        */
        
        if (GetAllMoveOfType(possibleMoves, CommandEnum.BUY_CARD).Count > 0){
            (UniqueCard card, int heuristicValue) = BuyCard(gameState.TavernAvailableCards, gameState, possibleMoves);
            if (heuristicValue>0){
                if (CardTierList.GetCardTier(card.Name) <= TierEnum.B && numberOfPatronCards[card.Deck]<3){
                    if (possibleMoves.Contains(Move.CallPatron(PatronId.ORGNUM))){
                        return Move.CallPatron(PatronId.ORGNUM);
                    }
                    if (possibleMoves.Contains(Move.CallPatron(PatronId.RAJHIN))){
                        return Move.CallPatron(PatronId.RAJHIN);
                    }
                    else{
                        return Move.BuyCard(card);
                    }
                }
                else{
                    return Move.BuyCard(card);
                }
            }
        }
        if (GetAllMoveOfType(possibleMoves, CommandEnum.CALL_PATRON).Count>0){
            return Move.CallPatron(ActivatePatron(gameState, possibleMoves));
        }
        if (GetAllMoveOfType(possibleMoves, CommandEnum.ATTACK).Count >0){
            return Move.Attack(AttackAgent(gameState));
        }
        if (GetAllMoveOfType(possibleMoves, CommandEnum.MAKE_CHOICE).Count>0){
            switch (gameState.PendingChoice.Context.ChoiceType){
                case ChoiceType.CARD_EFFECT:
                    return Move.MakeChoice(CardsSelection(gameState));
                case ChoiceType.EFFECT_CHOICE:
                    Move selectedMove = possibleMoves[0];
                    int maxValue = -1;
                    foreach(Move m in possibleMoves){
                        MakeChoiceMove<UniqueEffect> move = m as MakeChoiceMove<UniqueEffect>;
                        int moveValue = GetEffectChoiceValue(move.Choices[0], gameState);
                        if (moveValue > maxValue){
                            maxValue = moveValue;
                            selectedMove = m;
                        }
                    }
                    return selectedMove;
                case ChoiceType.PATRON_ACTIVATION:
                    //Console.WriteLine("weszło");
                    UniqueCard selectedCard = PatronActivationMove(gameState, possibleMoves);
                    return Move.MakeChoice(new List<UniqueCard> {selectedCard});
            }
        }
        cardsInHandAndOnBoard.Clear();
        startOfTurn = true;
        ClearPatronDeckCards();
        return Move.EndTurn();
    }

    public override void GameEnd(EndGameState state){
        startOfGame = true;
    }
}