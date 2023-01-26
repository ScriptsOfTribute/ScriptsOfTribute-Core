using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using System.Diagnostics;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Board.Cards;
//using NUnit.Framework;

namespace SimpleBots;

public class Node{

    private int patronFavour = 50;
    private int patronNeutral = 10;
    private int patronUnfavour = -50;
    private int coinsValue = 1;
    private int powerValue = 40;
    private int prestigeValue = 50;
    private int agentOnBoardValue = 30;
    private int hpValue = 3;
    private int opponentAgentsPenaltyValue = 40;
    private int potentialComboValue = 3;
    private int cardValue = 10;
    private int penaltyForHighTierInTavern = 2;
    private int numberOfDrawsValue = 10;
    private int enemyPotentialComboPenalty = 1;
    static double c = Math.Sqrt(2);
    public List<Node>? childs;
    public Node? father;
    
    public GameState nodeGameState;
    public Move? move;
    public List<Move> possibleMoves;
    
    public double wins;
    public ulong visits;

    private int heuristicMax = 160;//40000;
    private int heuristicMin  = 0;//-10000;

    public Node(GameState fatherGameState, Move? nodeMove, Node? fatherOrig, List<Move> possibleMoves = null){
        this.wins = 0;
        this.visits = 0;
        
        this.move = nodeMove;
        if (nodeMove is not null && nodeMove.Command != CommandEnum.END_TURN){
            var (newGameState, newMoves) = fatherGameState.ApplyState(nodeMove);
            this.nodeGameState = newGameState;
            this.possibleMoves = newMoves;
        }
        else{
            this.nodeGameState = fatherGameState;
            this.possibleMoves = possibleMoves;
        }

        this.father = fatherOrig;

        this.childs = new List<Node>();
    }

    public void CreateChilds() {
        foreach(Move childMove in this.possibleMoves){
            this.childs.Add(new Node(this.nodeGameState, childMove, this));
        }
    }

    public double UCBscore(){
        if (this.visits < 1){
            return int.MaxValue;
        }
        double tmpWins = this.wins;
        ulong tmpVisits = this.visits;

        //if (this.move.Command == CommandEnum.END_TURN) tmpWins -= 1000000; // TODO remove this line

        if (this.father is not null){
           return (tmpWins/tmpVisits) + c*Math.Sqrt((Math.Log(this.father.visits))/tmpVisits);
        }
        else{
            return (tmpWins/tmpVisits) + c*Math.Sqrt((Math.Log(tmpVisits))/tmpVisits);
        }
    }

    public Node SelectBestChild() {
        Node bestChild = this.childs[0];

        double bestScore = 0;
        
        foreach(Node child in this.childs){
            double tmpWins = child.wins;
            ulong tmpVisits = child.visits;
            double tmpScore = tmpWins;// + c*Math.Sqrt((Math.Log(this.visits))/tmpVisits);

            //if (child.move.Command == CommandEnum.END_TURN) tmpScore -= 1000000; // TODO remove this line

            if (tmpScore >= bestScore){
                bestScore = tmpScore;
                bestChild = child;
            }
        }
        return bestChild;
    } 

    private List<Move> NotEndTurnPossibleMoves(List<Move> possibleMoves){
        return possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    }
    public double Simulate(SeededRandom rng) {
        
        if (this.move.Command == CommandEnum.END_TURN){
            return Heuristic(this.nodeGameState);
        }

        /*Console.WriteLine("\nSIMULATION");
        try {
            Console.WriteLine("GRANDFATHER " + this.father.father.move);
        } catch {}
        
        try {
            Console.WriteLine("FATHER " + this.father.move);
        } catch {}

        Console.WriteLine("ME " + this.move);*/

        List<Move> notEndTurnPossibleMoves = NotEndTurnPossibleMoves(this.possibleMoves);
        Move nextMove;
        if (notEndTurnPossibleMoves.Count > 0){
            if ((this.nodeGameState.BoardState == BoardState.NORMAL) && (SimpleBots.Extensions.RandomK(0, 100000, rng)==0)){
                nextMove = Move.EndTurn();
            }
            else{
                nextMove = notEndTurnPossibleMoves.PickRandom(rng);
            }
        }
        else{
            nextMove = Move.EndTurn();
        }
        
        GameState gameState = this.nodeGameState;

        //Console.WriteLine(nextMove);

        while (nextMove.Command != CommandEnum.END_TURN) {

            var (newGameState, newMoves) = gameState.ApplyState(nextMove);

            notEndTurnPossibleMoves = NotEndTurnPossibleMoves(newMoves);
            if (notEndTurnPossibleMoves.Count > 0){
                if ((newGameState.BoardState == BoardState.NORMAL) && (SimpleBots.Extensions.RandomK(0, 10000, rng)==0)){
                    nextMove = Move.EndTurn();
                }
                else{
                    nextMove = notEndTurnPossibleMoves.PickRandom(rng);
                }
            }
            else{
                nextMove = Move.EndTurn();
            }
          //  Console.WriteLine(nextMove);
            
            gameState = newGameState;
        }

        return Heuristic(gameState);
    }

    public double Heuristic(GameState gameState){
        /*
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

            foreach(UniqueCard card in allCards){
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

            foreach(UniqueCard card in allCardsEnemy){
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
        }
        */
        
        int finalValue = gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
        //int finalValue = gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
        double normalizedValue = NormalizeHeuristic(finalValue);

        return normalizedValue;
    }

    private double NormalizeHeuristic(int value) {
        double normalizedValue = ((double)value - (double)heuristicMin) / ((double)heuristicMax - (double)heuristicMin);

        if (normalizedValue < 0){
            return 0.0;
        }

        return normalizedValue;
    }

    public void UpdateAllPossibleMoves(){   
        foreach(Node child in this.childs){
            if (child.move.Command == CommandEnum.END_TURN) {
                continue;
            }
            
            (child.nodeGameState, child.possibleMoves) = this.nodeGameState.ApplyState(child.move);
            /*
            List<Move> grandchildsMoves = new List<Move>();
            foreach(Node grandchild in child.childs) {
                grandchildsMoves.Add(grandchild.move);
            }
            */

            if (child.childs.Count>0){
                child.UpdateAllPossibleMoves();
            }
            /*
            foreach(Move possibleMove in child.possibleMoves) {
                if (!grandchildsMoves.Contains(possibleMove)) {
                    child.childs.Add(new Node(child.nodeGameState, possibleMove, child));
                }
            }
            for(int i=0; i<child.childs.Count; i++){
                if (! possibleMoves.Contains(child.childs[i].move)){
                    
                }
            }
            */
        }

        //Console.WriteLine("\n\n\nPOSSIBLE MOVES:");
        foreach (Move m in this.possibleMoves) {
            //Console.WriteLine(m);
        }

        List<Move> childsMoves = new List<Move>();
        foreach(Node child in this.childs) {
            childsMoves.Add(child.move);
        }

        List<Move> moveToRemove = new List<Move>();
        foreach(Move childMove in childsMoves){
            if (!possibleMoves.Contains(childMove)){
                moveToRemove.Add(childMove);
            }
        }

        foreach(Move possibleMove in this.possibleMoves) {
            if (!childsMoves.Contains(possibleMove)) {
                this.childs.Add(new Node(this.nodeGameState, possibleMove, this));
            }
        }
        foreach (Move removeMove in moveToRemove){
            //Console.WriteLine("Weszło");
            this.childs = this.childs.Where(x=>x.move != removeMove).ToList();
        }

        //Console.WriteLine("\nMOVES TO REMOVE:");
        foreach (Move m in moveToRemove) {
            //Console.WriteLine(m);
        }

        //Console.WriteLine("\nNEW CHILDS MOVES:");
        foreach (Node n in this.childs) {
            //Console.WriteLine(n.move);
        }
    }
}

public class NewMCTSBot : AI{

    Node? actRoot;
    Node? actNode;
    bool startOfTurn;

    Move endTurnMove = Move.EndTurn();


    public NewMCTSBot() {
        this.PrepareForGame();
    }

    private void PrepareForGame() {
        actRoot = null;
        actNode = null;

        startOfTurn = true;
    }

    private bool CheckIfPossibleMovesAreTheSame(List<Move> possibleMoves1, List<Move> possibleMoves2){
        if (possibleMoves1.Count != possibleMoves2.Count){
            return false;
        }
        foreach(Move move in possibleMoves1){
            if(! possibleMoves2.Contains(move)){
                return false;
            }
        }
        return true;
    }

    private Node TreePolicy(Node v, SeededRandom rng) {
        //Node v = vOrig;
        if (v.childs.Count()>0){
            double maxValue = -100000000;
            int selectedChild = 0;
            int index = 0;
            foreach(Node childNode in v.childs){
                if (childNode.UCBscore()>maxValue){
                    maxValue = childNode.UCBscore();
                    selectedChild = index;
                }
                index++;
            }
            return TreePolicy(v.childs[selectedChild], rng);
        }
        
        if (v.move.Command==CommandEnum.END_TURN){
            return v;
        }
        v.CreateChilds();
        return v;
    }

    private void BackUp(Node? v, double delta) {
        if (v is not null) {
            v.visits += 1;

            v.wins = Math.Max(delta, v.wins);//delta;//

            BackUp(v.father, delta);
        }
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn){
            Log("Nowa tura się zaczyna");
            actRoot = new Node(gameState, null, null, possibleMoves);
            actRoot.CreateChilds();
            startOfTurn = false;
        }
        else {
            if (!CheckIfPossibleMovesAreTheSame(actRoot.possibleMoves, possibleMoves)){
                Console.WriteLine("Zmiana w possible movsach");
                Console.WriteLine("Stare possible movsy");
                foreach(Move move_ in actRoot.possibleMoves){
                    Console.WriteLine(move_.ToString());
                }
                Console.WriteLine("Nove possible movsy");
                foreach(Move move_ in possibleMoves){
                    Console.WriteLine(move_.ToString());
                }
                actRoot.nodeGameState = gameState;
                /*
                foreach (Move possibleMove in possibleMoves) {
                    if (!actRoot.possibleMoves.Contains(possibleMove)) {
                        actRoot.possibleMoves.Add(possibleMove);
                    }
                }
                */
                actRoot.possibleMoves = possibleMoves;
                actRoot.UpdateAllPossibleMoves();
            }
        }

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN) {
            startOfTurn = true;
            return endTurnMove;
        }

        actRoot.father = null;

        int actionCounter = 0;

        //Console.WriteLine("POSSIBLE MOVES ");
        //foreach (Move m in possibleMoves) {Console.WriteLine(m);}

        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromSeconds(0.1)){
            actNode = TreePolicy(actRoot, Rng);
            double delta = actNode.Simulate(Rng);
            BackUp(actNode, delta);
            actionCounter++;
        }
    
        if (actRoot is null){
            //Console.WriteLine("root jest NULLEM");
        }
        
        actRoot = actRoot.SelectBestChild();
        Move move = actRoot.move;

        if (move.Command == CommandEnum.END_TURN) {
            startOfTurn = true;
        }
        Log(move.ToString());
        return move;
    }

    public override void GameEnd(EndGameState state)
    {
        this.PrepareForGame();
        Console.WriteLine("NEW GAME");
    }
}