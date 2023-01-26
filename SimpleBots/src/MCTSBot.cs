using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;

namespace SimpleBots;

public class OldNode
{
    static double C = Math.Sqrt(2);

    public List<OldNode>? childs;
    public OldNode? father;
    
    public GameState gameState;
    public Move? prevMove;
    public List<Move> possibleMoves;
    public List<CompletedAction> startOfturnCompletedActions;
    
    public int possibleMovesSize;
    public double wins;
    public ulong visits;
    public int actChildExpanding;


    // heuristic params
    private int patronFavour ;//= 50;
    private int patronNeutral ;//= 10;
    private int patronUnfavour ;//= -50;
    private int coinsValue ;//= 1;
    private int powerValue ;//= 20;
    private int prestigeValue ;//= 50;
    private int agentOnBoardValue ;//= 30;
    private int hpValue ;//= 3;
    private int opponentAgentsPenaltyValue ;//= 40;
    private int potentialComboValue ;//= 3;
    private int cardValue ;//= 10;
    private int penaltyForHighTierInTavern ;//= 2;
    private int numberOfDrawsValue ;// 10;
    private int enemyPotentialComboPenalty ;//= 1;
    private int heuristicMax  ;//= 10000;
    private int heuristicMin ;//= -4000;

    public int[] GetGenotype(){
        return new int[] {patronFavour, patronNeutral, patronNeutral, coinsValue, powerValue, prestigeValue, agentOnBoardValue, hpValue, opponentAgentsPenaltyValue, potentialComboValue, cardValue, penaltyForHighTierInTavern, numberOfDrawsValue, enemyPotentialComboPenalty, heuristicMax, heuristicMin};
    }
    public void SetGenotype(int[] values){
        patronFavour = values[0];
        patronNeutral = values[1];
        patronNeutral = values[2];
        coinsValue = values[3];
        powerValue = values[4];
        prestigeValue = values[5]; 
        agentOnBoardValue = values[6];
        hpValue = values[7];
        opponentAgentsPenaltyValue = values[8];
        potentialComboValue = values[9];
        cardValue = values[10];
        penaltyForHighTierInTavern = values[11];
        numberOfDrawsValue = values[12];
        enemyPotentialComboPenalty = values[13];
        heuristicMax = values[14];
        heuristicMin = values[15];
    }

    private List<Move> copyMoveList(List<Move> moves, GameState state) {
        List<Move> result = new List<Move>();

        SimpleCardMove simpleCardMove;
        SimplePatronMove simplePatronMove;

        foreach (Move move in moves) {
            switch (move.Command) {
                case CommandEnum.ACTIVATE_AGENT:
                    simpleCardMove = move as SimpleCardMove;
                    result.Add(Move.ActivateAgent(simpleCardMove.Card));
                    break;

                case CommandEnum.ATTACK:
                    simpleCardMove = move as SimpleCardMove;
                    result.Add(Move.Attack(simpleCardMove.Card));
                    break;

                case CommandEnum.BUY_CARD:
                    simpleCardMove = move as SimpleCardMove;
                    result.Add(Move.BuyCard(simpleCardMove.Card));
                    break;

                case CommandEnum.CALL_PATRON:
                    simplePatronMove = move as SimplePatronMove;
                    result.Add(Move.CallPatron(simplePatronMove.PatronId));
                    break;
                
                case CommandEnum.END_TURN:
                    result.Add(Move.EndTurn());
                    break;

                case CommandEnum.PLAY_CARD:
                    simpleCardMove = move as SimpleCardMove;
                    result.Add(Move.PlayCard(simpleCardMove.Card));
                    break;

                case CommandEnum.MAKE_CHOICE:
                    switch (state.PendingChoice.Context.ChoiceType){
                        case ChoiceType.CARD_EFFECT:
                            MakeChoiceMove<UniqueCard> tmpMove = move as MakeChoiceMove<UniqueCard>;
                            result.Add(Move.MakeChoice(tmpMove.Choices));
                            break;
                        case ChoiceType.EFFECT_CHOICE:
                            MakeChoiceMove<UniqueEffect> tmpMove_2 = move as MakeChoiceMove<UniqueEffect>;
                            result.Add(Move.MakeChoice(tmpMove_2.Choices));
                            break;
                        case ChoiceType.PATRON_ACTIVATION:
                            MakeChoiceMove<UniqueCard> tmpMove_3 = move as MakeChoiceMove<UniqueCard>;
                            result.Add(Move.MakeChoice(tmpMove_3.Choices));
                            break;
                    }
                    break;
            }
        }

        return result;
    }

    private Move copyMove(Move move, GameState state) {
        Move result = Move.EndTurn();

        SimpleCardMove simpleCardMove;
        SimplePatronMove simplePatronMove;

        switch (move.Command) {
            case CommandEnum.ACTIVATE_AGENT:
                simpleCardMove = move as SimpleCardMove;
                result = Move.ActivateAgent(simpleCardMove.Card);
                break;

            case CommandEnum.ATTACK:
                simpleCardMove = move as SimpleCardMove;
                result = Move.Attack(simpleCardMove.Card);
                break;

            case CommandEnum.BUY_CARD:
                simpleCardMove = move as SimpleCardMove;
                result = Move.BuyCard(simpleCardMove.Card);
                break;

            case CommandEnum.CALL_PATRON:
                simplePatronMove = move as SimplePatronMove;
                result = Move.CallPatron(simplePatronMove.PatronId);
                break;
            
            case CommandEnum.END_TURN:
                result = Move.EndTurn();
                break;

            case CommandEnum.PLAY_CARD:
                simpleCardMove = move as SimpleCardMove;
                result = Move.PlayCard(simpleCardMove.Card);
                break;

            case CommandEnum.MAKE_CHOICE:
                switch (state.PendingChoice.Context.ChoiceType){
                    case ChoiceType.CARD_EFFECT:
                        MakeChoiceMove<UniqueCard> tmpMove = move as MakeChoiceMove<UniqueCard>;
                        result = Move.MakeChoice(tmpMove.Choices);
                        break;
                    case ChoiceType.EFFECT_CHOICE:
                        MakeChoiceMove<UniqueEffect> tmpMove_2 = move as MakeChoiceMove<UniqueEffect>;
                        result = Move.MakeChoice(tmpMove_2.Choices);
                        break;
                    case ChoiceType.PATRON_ACTIVATION:
                        MakeChoiceMove<UniqueCard> tmpMove_3 = move as MakeChoiceMove<UniqueCard>;
                        result = Move.MakeChoice(tmpMove_3.Choices);
                        break;
                }
                break;
        }
        

        return result;
    }

    private List<Move> clearMoves(List<Move> moves) {
        for (int i = 0; i < moves.Count; ++i) {
            if (moves[i].Command == CommandEnum.END_TURN) {
                moves.RemoveAt(i);
                break;
            }
        }
        
        return moves;
    }

    public OldNode(GameState gameState, Move prevMoveOrig, OldNode? father, List<CompletedAction> startOfturnCompletedActions, int[] heuristicValues) {
        this.SetGenotype(heuristicValues);
        Move prevMove = copyMove(prevMoveOrig, gameState);

        this.wins = 0;
        this.visits = 0;
        this.actChildExpanding = 0;

        this.prevMove = prevMove;
        var (newGameState, newMoves) = gameState.ApplyState(prevMove);
        this.gameState = newGameState;

        newMoves = this.clearMoves(newMoves);
        
        this.possibleMovesSize = newMoves.Count;
        this.possibleMoves = newMoves;

        this.father = father;

        this.childs = new List<OldNode>();

        this.startOfturnCompletedActions = startOfturnCompletedActions;
    }

    public void UpdateChilds(List<Move> newPossibleMoves, GameState newGameState) {
        List<Move> newPossibleMovesCopy = copyMoveList(newPossibleMoves, newGameState);
        this.gameState = newGameState;
        newPossibleMovesCopy = this.clearMoves(newPossibleMovesCopy);
        List<Move> childsMoves = this.childs.Select(x => x.prevMove).ToList();
        foreach(Move newMove in newPossibleMovesCopy) {
            if (!childsMoves.Contains(newMove)) {
                this.possibleMoves.Add(newMove);
                this.possibleMovesSize++;
                this.childs.Add(new OldNode(this.gameState, newMove, this, this.startOfturnCompletedActions, this.GetGenotype()));
            }
        }
    }

    public bool checkChagnedChilds(List<Move> newPossibleMoves, GameState newGameState) {
        List<Move> newPossibleMovesCopy = copyMoveList(newPossibleMoves, newGameState);
        this.gameState = newGameState;
        newPossibleMovesCopy = this.clearMoves(newPossibleMovesCopy);
        List<Move> childsMoves = this.childs.Select(x => x.prevMove).ToList();
        foreach(Move newMove in newPossibleMovesCopy) {
            if (!childsMoves.Contains(newMove)) {
                return true;
            }
        }

        return false;
    }

    public OldNode(GameState gameState, List<Move> possibleMovesOrig, OldNode? father, List<CompletedAction> startOfturnCompletedActions, int[] heuristicValues) {
        this.SetGenotype(heuristicValues);
        List<Move> possibleMoves = copyMoveList(possibleMovesOrig, gameState);

        this.gameState = gameState;
        possibleMoves = this.clearMoves(possibleMoves);

        this.wins = 0;
        this.visits = 0;
        this.actChildExpanding = 0;

        this.prevMove = null;
        
        this.possibleMovesSize = possibleMoves.Count;

        this.possibleMoves = possibleMoves;
        this.father = father;

        this.childs = new List<OldNode>();

        this.startOfturnCompletedActions = startOfturnCompletedActions;
    }

    public bool IsEnd() {
        return this.possibleMovesSize <= 0;
    }

    public OldNode Expand() {
        this.actChildExpanding++;
        this.childs[this.actChildExpanding - 1].CreateChilds();

        return this.childs[this.actChildExpanding - 1];
    }

    public void CreateChilds() {
        for (int i = 0; i < this.possibleMovesSize; i++) {
            Move move = this.possibleMoves[i];

            this.childs.Add(new OldNode(this.gameState, move, this, this.startOfturnCompletedActions, this.GetGenotype()));
        }
    }

    public Move BestChildMove() {
        if (this.possibleMovesSize == 0) {
            return Move.EndTurn();
        }

        Move bestChildMove = this.childs[0].prevMove;

        double bestScore = 0;
        
        for (int i = 0; i < this.possibleMovesSize; i++) {
            double tmpWins = this.childs[i].wins;
            ulong tmpVisits = this.childs[i].visits;
            double tmpScore = tmpWins;//(tmpWins / tmpVisits);// (tmpWins / tmpVisits) + C * Math.Sqrt( (2 * Math.Log(tmpVisits)) / tmpVisits); // TODO maybe just tmpVisits

            if (tmpScore >= bestScore) {
                bestScore = tmpScore;
                bestChildMove = this.childs[i].prevMove;
            }    
        }

        return bestChildMove;
    } 

    public int BestChildIndex() {
        if (this.possibleMovesSize == 0) {
            return -1;
        }

        int bestIndex = 0;

        double bestScore = 0;
        
        for (int i = 0; i < this.possibleMovesSize; i++) {
            double tmpWins = this.childs[i].wins;
            ulong tmpVisits = this.childs[i].visits;
            double tmpScore = tmpWins;//(tmpWins / tmpVisits);// (tmpWins / tmpVisits) + C * Math.Sqrt( (2 * Math.Log(tmpVisits)) / tmpVisits); // TODO maybe just tmpVisits

            if (tmpScore >= bestScore) {
                bestScore = tmpScore;
                bestIndex = i;
            }    
        }

        return bestIndex;
    } 

    public OldNode BestChild(SeededRandom rng) {
        OldNode bestChild = childs[0];
        double bestScore = Double.NegativeInfinity;
        
        return childs[Extensions.RandomK(0,childs.Count, rng)];

        for (int i = 0; i < possibleMovesSize; i++) {
            double tmpWins = childs[i].wins;
            ulong tmpVisits = childs[i].visits;

            double tmpScore = 0;

            if (tmpVisits > 0 && visits > 0) {
                tmpScore = (tmpWins / tmpVisits) + C * Math.Sqrt( (Math.Log(visits)) / tmpVisits);
            }

            if (tmpScore >= bestScore) {
                bestScore = tmpScore;
                bestChild = childs[i];
            }
        }

        return bestChild;
    }

    private int CountNumberOfDrawsInTurn(){
        List<CompletedAction> completedActions = this.gameState.CompletedActions;

        int numberOfLastActions = completedActions.Count - this.startOfturnCompletedActions.Count;
        List<CompletedAction> lastCompltedActions = completedActions.TakeLast(numberOfLastActions).ToList();
        int counter = 0;
        foreach (CompletedAction action in lastCompltedActions){
            if (action.Type == CompletedActionType.DRAW){
                counter += 1;
            }
        }
        return counter;
    }

    private double NormalizeHeuristic(int value) {
        double normalizedValue = ((double)value - (double)heuristicMin) / ((double)heuristicMax - (double)heuristicMin);

        if (normalizedValue < 0){
            return 0.0;
        }

        return normalizedValue;
    }

    public double Heuristic(){
        int finalValue = 0;
        int enemyPatronFavour = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All) {
            if (entry.Key == PatronId.TREASURY){
                continue;
            }
            if (entry.Value == gameState.CurrentPlayer.PlayerID) {
                finalValue += patronFavour;
            }
            else if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED){
                finalValue += patronNeutral;
            }
            else{
                finalValue += patronUnfavour;
                enemyPatronFavour += 1;
            }
        }
        if (enemyPatronFavour>=2){
            finalValue -= 100;
        }
        if (gameState.EnemyPlayer.Prestige >=20)
        {
            finalValue += gameState.CurrentPlayer.Power * powerValue;
            finalValue += gameState.CurrentPlayer.Prestige * prestigeValue;
        }

        if (gameState.CurrentPlayer.Prestige<30){
            TierEnum tier = TierEnum.UNKNOWN;
            foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents){
                tier = CardTierList.GetCardTier(agent.RepresentingCard.Name);
                finalValue += agentOnBoardValue * (int)tier + agent.CurrentHp * hpValue;
            }

            foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents){
                tier = CardTierList.GetCardTier(agent.RepresentingCard.Name);
                finalValue -= agentOnBoardValue * (int)tier + agent.CurrentHp * hpValue + opponentAgentsPenaltyValue;
            }

            List<UniqueCard> allCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
            Dictionary<PatronId, int> potentialComboNumber = new Dictionary<PatronId, int>();
            List<UniqueCard> allCardsEnemy = gameState.EnemyPlayer.HandAndDraw.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile)).ToList();
            Dictionary<PatronId, int> potentialComboNumberEnemy = new Dictionary<PatronId, int>();

            foreach(Card card in allCards){
                finalValue += (int)tier * cardValue;
                if (card.Deck != PatronId.TREASURY){
                    if (potentialComboNumber.ContainsKey(card.Deck)){
                        potentialComboNumber[card.Deck] +=1;
                    }
                    else{
                        potentialComboNumber[card.Deck] = 1;
                    }
                }
            }

            foreach(Card card in allCardsEnemy){
                if (card.Deck != PatronId.TREASURY){
                    if (potentialComboNumberEnemy.ContainsKey(card.Deck)){
                        potentialComboNumberEnemy[card.Deck] +=1;
                    }
                    else{
                        potentialComboNumberEnemy[card.Deck] = 1;
                    }
                }
            }

            foreach (KeyValuePair<PatronId, int> entry in potentialComboNumber){
                finalValue += (int)Math.Pow(entry.Value, potentialComboValue);
            }
            foreach(Card card in gameState.TavernAvailableCards){
                tier = CardTierList.GetCardTier(card.Name);
                finalValue -= penaltyForHighTierInTavern * (int)tier;
                
                if (potentialComboNumberEnemy.ContainsKey(card.Deck) && (potentialComboNumberEnemy[card.Deck]>4) && (tier > TierEnum.B)){
                    finalValue -= enemyPotentialComboPenalty*(int)tier;
                }
                
                
            }

            finalValue += CountNumberOfDrawsInTurn() * numberOfDrawsValue;
        }
        
        //int finalValue = gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
        double normalizedValue = this.NormalizeHeuristic(finalValue);

        var (newGameState, newMoves) = gameState.ApplyState(Move.EndTurn());

        if (newGameState.GameEndState is not null)
        {
            return double.MaxValue;
        }

        return normalizedValue;
    }

    public double Simulate(SeededRandom rng) {
        GameState gameStateSave = this.gameState;
        List<Move> possibleMovesSave = copyMoveList(this.possibleMoves, this.gameState);

        int possibleMovesSizeSave = this.possibleMovesSize;

        Move prevMoveSave = Move.EndTurn();

        if (this.prevMove is not null && this.father is not null) {
            prevMoveSave = copyMove(this.prevMove, this.father.gameState);
        }

        double score;
        double winsSave = this.wins;

        int actChildExpandingSave = this.actChildExpanding;
        ulong visitsSave = this.visits;

        OldNode v = this.father;
        //Console.WriteLine("\nSTART");
        List<Move> prevMoves = new List<Move>();
        prevMoves.Add(this.prevMove);
        while (v is not null && v.prevMove is not null) {
            prevMoves.Add(v.prevMove);
            v=v.father;
        }
        
        /*for (int i = prevMoves.Count-1; i >=0; i--) {
            Console.WriteLine("PREV MOVE " + prevMoves[i]);
        }*/

        while (!this.IsEnd()) {
            Move move = this.possibleMoves[SimpleBots.Extensions.RandomK(0, (int)this.possibleMovesSize, rng)];
            //Console.WriteLine("SIMULATED MOVE " + move);
            this.prevMove = move;

            var (newGameState, newMoves) = gameState.ApplyState(move);
            this.gameState = newGameState;
            newMoves = clearMoves(newMoves);

            this.possibleMoves = newMoves;
            this.possibleMovesSize = this.possibleMoves.Count;
        }
        //Console.WriteLine("END\n");

        score = this.Heuristic();

        this.gameState = gameStateSave;
        this.possibleMoves = possibleMovesSave;

        this.possibleMovesSize = possibleMovesSizeSave;

        if (this.prevMove is not null && this.father is not null) {
            this.prevMove = prevMoveSave;
        }

        this.wins = winsSave;

        this.actChildExpanding = actChildExpandingSave;
        this.visits = visitsSave;

        return score;
    }
}

public class MCTSBot : AI
{
    OldNode? actRoot;
    OldNode? OldactNode;
    bool startOfTurn;

    int possibleMovesSize;

    // heuristic params
    private int patronFavour = 50;
    private int patronNeutral = 10;
    private int patronUnfavour = -50;
    private int coinsValue = 1;
    private int powerValue = 20;
    private int prestigeValue = 50;
    private int agentOnBoardValue = 30;
    private int hpValue = 3;
    private int opponentAgentsPenaltyValue = 40;
    private int potentialComboValue = 3;
    private int cardValue = 10;
    private int penaltyForHighTierInTavern = 2;
    private int numberOfDrawsValue = 10;
    private int enemyPotentialComboPenalty = 1;
    private int heuristicMax  = 160;
    private int heuristicMin = 0;

    public int[] GetGenotype(){
        return new int[] {patronFavour, patronNeutral, patronNeutral, coinsValue, powerValue, prestigeValue, agentOnBoardValue, hpValue, opponentAgentsPenaltyValue, potentialComboValue, cardValue, penaltyForHighTierInTavern, numberOfDrawsValue, enemyPotentialComboPenalty, heuristicMax, heuristicMin};
    }

    public void SetGenotype(int[] values){
        patronFavour = values[0];
        patronNeutral = values[1];
        patronNeutral = values[2];
        coinsValue = values[3];
        powerValue = values[4];
        prestigeValue = values[5]; 
        agentOnBoardValue = values[6];
        hpValue = values[7];
        opponentAgentsPenaltyValue = values[8];
        potentialComboValue = values[9];
        cardValue = values[10];
        penaltyForHighTierInTavern = values[11];
        numberOfDrawsValue = values[12];
        enemyPotentialComboPenalty = values[13];
        heuristicMax = values[14];
        heuristicMin = values[15];
    }

    public MCTSBot() {
        this.PrepareForGame();
    }

    private void PrepareForGame() {
        actRoot = null;
        OldactNode = null;

        startOfTurn = true;
        possibleMovesSize = 0;
    }

    private OldNode TreePolicy(OldNode v, SeededRandom rng) {
        int i = 0;
        while (!v.IsEnd()) {
            if (v.actChildExpanding < v.possibleMovesSize) {
                return v.Expand();
            }
            v = v.BestChild(rng);
        }
        return v;
    }

    private void BackUp(OldNode? v, double delta) {
        while (v != null) {
            v.visits += 1;

            v.wins = Math.Max(delta, v.wins);

            v = v.father;
        }
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (true || startOfTurn || actRoot.checkChagnedChilds(possibleMoves, gameState)){
            actRoot = new OldNode(gameState, possibleMoves, null, gameState.CompletedActions, this.GetGenotype());
            actRoot.CreateChilds();

            startOfTurn = false;
        }
        else {
            //actRoot.UpdateChilds(possibleMoves, gameState);
        }

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN) {
            startOfTurn = true;
            //return Move.EndTurn();
        }

        actRoot.father = null;

        int actionCounter = 0;

        //Console.WriteLine("POSSIBLE MOVES ");
        //foreach (Move m in possibleMoves) {Console.WriteLine(m);}

        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromSeconds(0.1)){
            OldactNode = TreePolicy(actRoot, Rng);
            double delta = OldactNode.Simulate(Rng);
            BackUp(OldactNode, delta);
            actionCounter++;
        }

        Move move = actRoot.BestChildMove();

        if (!possibleMoves.Contains(move)) {
            int bestIndex = actRoot.BestChildIndex();
            
            startOfTurn = true;

            return possibleMoves[bestIndex];
        }

        OldNode tmpRoot = null;

        for (int i = 0; i < actRoot.possibleMovesSize; i++) {
            if (actRoot.childs[i].prevMove == move) {
                tmpRoot = actRoot.childs[i];
            }
            else {
                actRoot.childs[i] = null;
            }
        }

        actRoot = tmpRoot;

        if (move.Command == CommandEnum.END_TURN) {
            startOfTurn = true;
        }

        //Console.WriteLine(move);
        //if (move.Command == CommandEnum.END_TURN) Console.WriteLine();

        return move;
    }

    public override void GameEnd(EndGameState state)
    {
        this.PrepareForGame();
    }
}
