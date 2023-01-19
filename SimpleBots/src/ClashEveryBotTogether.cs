using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using System.Text;

namespace SimpleBotsTests;

public class ClashEveryBotTogether{

    private StringBuilder log = new StringBuilder();

    private string loggerPath = "allBotsStatistics.txt";

    private int numberOfTrials = 1000;

    private AI[] bots = new AI[] {new RandomBot(), new DoEverythingBot(), new WinByPatronFavors(), new RandomMaximizePrestigeBot(), new MaximizeAgentsBot()};//, new SemiRandomBot()}; new RandomHeuristicBot()

    private double[] botsWinCounter = new double[5];

    private void ClearBotsWinCounter(){
        for(int i=0; i< botsWinCounter.Length; i++){
            botsWinCounter[i] = 0;
        }
    }
    public void BotClash(){
        foreach (var bot in bots){
            log.Append(bot.ToString() + ',');
        }
        log.Append(System.Environment.NewLine);
        
        ClearBotsWinCounter();
        for (int trial=0; trial < numberOfTrials; trial++){
            for(int i = 0; i< bots.Length; i++){
                for (int j=i+1; j< bots.Length; j++){
                    var game = new TalesOfTribute.AI.TalesOfTribute(bots[i], bots[j]);
                    var (endState, _) = game.Play();
                    if (endState.Winner == PlayerEnum.PLAYER1){
                        botsWinCounter[i] +=1.0;
                    }
                    else{
                        botsWinCounter[j] +=1.0;
                    }
                }
            }
            for(int i=0; i< botsWinCounter.Length; i++){
                botsWinCounter[i] /= (double)(botsWinCounter.Length-1);
            }
            log.Append(string.Join(",", botsWinCounter));
            log.Append(System.Environment.NewLine);
            ClearBotsWinCounter();
        }
        File.AppendAllText(loggerPath, log.ToString());
    }

}
