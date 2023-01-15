using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;

namespace SimpleBots;

public class Node
{
    static double C = Math.Sqrt(2);

    public Node?[] childs;
    public Node? father;
    
    public GameState gameState;
    public Move? prevMove;
    public List<Move> possibleMoves;
    public List<CompletedAction> startOfturnCompletedActions;
    
    public int possibleMovesSize;
    public double wins;
    public ulong visits;
    public int actChildExpanding;

    // heuristic params
    const int patronFavour = 200;
    const int patronNeutral = 100;
    const int patronUnfavour = -200;
    const int coinsValue = 10;
    const int powerValue = 400;
    const int prestigeValue = 1000;
    const int agentOnBoardValue = 125;
    const int hpValue = 20;
    const int opponentAgentsPenaltyValue = 40;
    //was 20 xD
    const int potentialComboValue = 2;
    const int cardValue = 10;
    const int penaltyForHighTierInTavern = 40;
    const int numberOfDrawsValue = 35;

    const double heuristicMax  = 101000;
    const double heuristicMin = -700000;

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
        List<int> indexesToRemove = new List<int>();

        foreach (Move m in moves) {
            File.AppendAllText("xd.txt", "BEFORE: " + m + Environment.NewLine);
        }

        for (int i = 0; i < moves.Count; ++i) {
            if (moves[i].Command == CommandEnum.END_TURN) {
                indexesToRemove.Add(i);
            }

            if
            (
                moves[i].Command == CommandEnum.PLAY_CARD ||
                moves[i].Command == CommandEnum.ACTIVATE_AGENT ||
                moves[i].Command == CommandEnum.BUY_CARD ||
                moves[i].Command == CommandEnum.ATTACK
            )
            {
                SimpleCardMove tmpMove = moves[i] as SimpleCardMove;

                if (tmpMove.Card.CommonId == CardId.UNKNOWN) {
                    indexesToRemove.Add(i);
                }
            }

            if (moves[i].Command == CommandEnum.MAKE_CHOICE && this.gameState.PendingChoice.Context.ChoiceType == ChoiceType.CARD_EFFECT) {
                MakeChoiceMove<UniqueCard> tmpMove = moves[i] as MakeChoiceMove<UniqueCard>;

                foreach(UniqueCard tmpCard in tmpMove.Choices) {
                    if (tmpCard.CommonId == CardId.UNKNOWN) {
                        indexesToRemove.Add(i);
                        break;
                    }
                }
            }
        }

        foreach (int m in indexesToRemove) {
            File.AppendAllText("xd.txt", "AFTER: " + m + Environment.NewLine);
        }

        for (int i = indexesToRemove.Count - 1; i >= 0; i--) {
            moves.RemoveAt(indexesToRemove[i]);
        }

        return moves;
    }

    public Node(GameState gameState, Move prevMoveOrig, Node? father, List<CompletedAction> startOfturnCompletedActions) {
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

        this.childs = new Node[possibleMovesSize];

        this.startOfturnCompletedActions = startOfturnCompletedActions;
    }

    public Node(GameState gameState, List<Move> possibleMovesOrig, Node? father, List<CompletedAction> startOfturnCompletedActions) {
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

        this.childs = new Node[possibleMovesSize];

        this.startOfturnCompletedActions = startOfturnCompletedActions;
    }

    public bool IsEnd() {
        return this.possibleMovesSize <= 0;
    }

    public Node Expand() {
        this.actChildExpanding++;
        this.childs[this.actChildExpanding - 1].CreateChilds();

        return this.childs[this.actChildExpanding - 1];
    }

    public void CreateChilds() {
        File.AppendAllText("xd.txt", Environment.NewLine);

        if (this.prevMove is not null) {
            File.AppendAllText("xd.txt", "FATHERS MOVE: " + prevMove + Environment.NewLine);
        } else {
            File.AppendAllText("xd.txt", "I HAVE NO FATHER" + Environment.NewLine);
        }

        for (int i = 0; i < this.possibleMovesSize; i++) {
            Move move = this.possibleMoves[i];

            File.AppendAllText("xd.txt", "CHILD " + i + " MOVE: " + move + Environment.NewLine);

            this.childs[i] = new Node(this.gameState, move, this, this.startOfturnCompletedActions);
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
            double tmpScore = tmpVisits;// (tmpWins / tmpVisits) + C * Math.Sqrt( (2 * Math.Log(tmpVisits)) / tmpVisits); // TODO maybe just tmpVisits

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
            double tmpScore = tmpVisits;// (tmpWins / tmpVisits) + C * Math.Sqrt( (2 * Math.Log(tmpVisits)) / tmpVisits); // TODO maybe just tmpVisits

            if (tmpScore >= bestScore) {
                bestScore = tmpScore;
                bestIndex = i;
            }    
        }

        return bestIndex;
    } 

    public Node BestChild() {
        Node bestChild = childs[0];
        double bestScore = Double.NegativeInfinity;
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
        double normalizedValue = ((double)value - heuristicMin) / (heuristicMax - heuristicMin);

        if (normalizedValue < 0){
            return 0.0;
        }

        return Math.Min(1.0, normalizedValue);
    }

    public double Heuristic(){
        int finalValue = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All) {
            if (entry.Value == gameState.CurrentPlayer.PlayerID) {
                finalValue += patronFavour;
            }
            else if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED){
                finalValue += patronNeutral;
            }
            else{
                finalValue += patronUnfavour;
            }
        }

        finalValue += gameState.CurrentPlayer.Coins * coinsValue;
        finalValue += gameState.CurrentPlayer.Power * powerValue;
        finalValue += gameState.CurrentPlayer.Prestige * prestigeValue;
        
        int tier = -10000;
        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue += agentOnBoardValue * tier + agent.CurrentHp * hpValue;
        }

        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue -= agentOnBoardValue * tier + agent.CurrentHp * hpValue + opponentAgentsPenaltyValue;
        }

        List<UniqueCard> allCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
        Dictionary<PatronId, int> potentialComboNumber = new Dictionary<PatronId, int>();

        foreach(Card card in allCards){
            finalValue += (int)CardTierList.GetCardTier(card.Name) * cardValue;
            if (card.Deck != PatronId.TREASURY){
                if (potentialComboNumber.ContainsKey(card.Deck)){
                    potentialComboNumber[card.Deck] +=1;
                }
                else{
                    potentialComboNumber[card.Deck] = 1;
                }
            }
        }
        foreach (KeyValuePair<PatronId, int> entry in potentialComboNumber){
            finalValue += (int)Math.Pow(entry.Value, potentialComboValue);
        }
        foreach(Card card in gameState.TavernCards){
            finalValue -= penaltyForHighTierInTavern * (int)CardTierList.GetCardTier(card.Name);
        }

        finalValue += CountNumberOfDrawsInTurn() * numberOfDrawsValue;

        double normalizedValue = this.NormalizeHeuristic(finalValue);

        return normalizedValue;
    }

    public double Simulate() {
        GameState gameStateSave = this.gameState;
        List<Move> possibleMovesSave = copyMoveList(this.possibleMoves, this.gameState);

        //Console.WriteLine("\nSTART");

        int possibleMovesSizeSave = this.possibleMovesSize;

        Move prevMoveSave = Move.EndTurn();

        if (this.prevMove is not null && this.father is not null) {
            prevMoveSave = copyMove(this.prevMove, this.father.gameState);
        }

        double score;
        double winsSave = this.wins;

        int actChildExpandingSave = this.actChildExpanding;
        ulong visitsSave = this.visits;

        /*
        Console.WriteLine("\nSTART SIMULATION!");
        Console.WriteLine();
        Console.WriteLine("BEFORE MOVE: " + this.prevMove ?? "NULL MOVE");
        Console.WriteLine("BEFORE GOLD: " + gameState.CurrentPlayer.Coins);
        Console.WriteLine("BEFORE POWER: " + gameState.CurrentPlayer.Power);
        Console.WriteLine("BEFORE PRESTIGE: " + gameState.CurrentPlayer.Prestige);
        */

        while (!this.IsEnd()) {
            Move move = this.possibleMoves[SimpleBots.Extensions.RandomK(0, (int)this.possibleMovesSize)];

            this.prevMove = move;

            var (newGameState, newMoves) = gameState.ApplyState(move);
            this.gameState = newGameState;
            newMoves = clearMoves(newMoves);

            /*
            Console.WriteLine();
            Console.WriteLine("MOVE: " + this.prevMove);
            Console.WriteLine("GOLD: " + gameState.CurrentPlayer.Coins);
            Console.WriteLine("POWER: " + gameState.CurrentPlayer.Power);
            Console.WriteLine("PRESTIGE: " + gameState.CurrentPlayer.Prestige);
            */

            this.possibleMoves = newMoves;
            this.possibleMovesSize = this.possibleMoves.Count;
        }

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
    Node? actRoot;
    Node? actNode;
    bool startOfTurn;

    int possibleMovesSize;

    public MCTSBot() {
        this.PrepareForGame();
    }

    private void PrepareForGame() {
        actRoot = null;
        actNode = null;

        startOfTurn = true;
        possibleMovesSize = 0;

        File.AppendAllText("xd.txt", "\n\n GAME STARTED!!!! \n" + Environment.NewLine);
    }

    private Node TreePolicy(Node v) {
        while (!v.IsEnd()) {
            if (v.actChildExpanding < v.possibleMovesSize) {
                return v.Expand();
            }

            v = v.BestChild();
        }
        return v;
    }

    private void BackUp(Node? v, double delta) {
        while (v != null) {
            v.visits += 1;

            v.wins += delta;

            v = v.father;
        }
    }


    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn){
            File.Delete("xd.txt");
            File.AppendAllText("xd.txt", "NEW TURN" + Environment.NewLine);

            foreach (Move m in possibleMoves) {
                File.AppendAllText("xd.txt", "NEW TURN MOVE: " + m + Environment.NewLine);
            }

            actRoot = new Node(gameState, possibleMoves, null, gameState.CompletedActions);
            actRoot.CreateChilds();

            startOfTurn = false;
        }

        foreach (Move m in possibleMoves) {
            File.AppendAllText("xd.txt", "MOVE: " + m + Environment.NewLine);
        }
        foreach (Node c in actRoot.childs) {
            File.AppendAllText("xd.txt", "CHILD: " + c.prevMove + Environment.NewLine);
        }
        File.AppendAllText("xd.txt", "ACT CHILD EXPANDING: " + actRoot.actChildExpanding + Environment.NewLine);
        File.AppendAllText("xd.txt", "CHILDS LENGTH: " + actRoot.childs.Length + Environment.NewLine);
        File.AppendAllText("xd.txt", "VISITS: " + actRoot.visits + Environment.NewLine);
        File.AppendAllText("xd.txt", "POSSIBLE CHILDS: " + actRoot.possibleMovesSize + Environment.NewLine);

        if (possibleMoves.Count == 1) {
            startOfTurn = true;
        }

        actRoot.father = null;

        int actionCounter = 0;

        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromSeconds(0.25)){
            actNode = TreePolicy(actRoot);
            double delta = actNode.Simulate();
            BackUp(actNode, delta);
            actionCounter++;
        }

        Move move = actRoot.BestChildMove();

        if (!possibleMoves.Contains(move)) {
            int bestIndex = actRoot.BestChildIndex();
            
            startOfTurn = true;

            return possibleMoves[bestIndex];
        }

        Node tmpRoot = null;

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

        File.AppendAllText("xd.txt", "SIMULATIONS COUNTER: " + actionCounter + Environment.NewLine);
        return move;
    }

    public override void GameEnd(EndGameState state)
    {
        this.PrepareForGame();
    }
}
