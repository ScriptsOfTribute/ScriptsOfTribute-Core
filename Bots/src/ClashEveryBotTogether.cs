using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using System.Text;

namespace Bots;

public class ClashEveryBotTogether
{

    private StringBuilder log = new StringBuilder();

    private string loggerPath = "allBotsStatistics.txt";
    private string matrixPath = "statisticInMatrix.txt";

    private int numberOfTrials = 100;

    private AI[] bots = new AI[] { new RandomBot(), new RandomWithoutEndTurnBot(), new PatronFavorsBot(), new MaxPrestigeBot(), new MaxAgentsBot(), new BeamSearchBot(), new MCTSBot(), new DecisionTreeBot(), new RandomSimulationBot() };
    private double[] botsWinRatio = new double[9];
    private int[] botsWinCounter = new int[9];

    private (int, int)[,] winMatrix = new (int, int)[9, 9];

    private void ClearBotsWinCounter()
    {
        for (int i = 0; i < botsWinCounter.Length; i++)
        {
            botsWinCounter[i] = 0;
        }
    }

    private void ReadWinMatrix(string result, int k = 9)
    {
        string[] splittedResult = result.Split("#");
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++)
            {
                string cell = splittedResult[i * k + j];
                int a = Int32.Parse(cell.Replace(@"(", "").Replace(@")", "").Split(",")[0]);
                int b = Int32.Parse(cell.Replace(@"(", "").Replace(@")", "").Split(",")[1]);
                winMatrix[i, j] = (a, b);
            }
        }
    }
    public void BotClash()
    {
        string names = "";
        foreach (var bot in bots)
        {
            names += bot.ToString() + ',';
        }

        names += System.Environment.NewLine;
        File.AppendAllText(loggerPath, names);
        int k = bots.Length;

        try
        {
            string oldMatrix = File.ReadAllText(matrixPath);
            ReadWinMatrix(oldMatrix, k);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during reading from file, probably file does not exist");
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    winMatrix[i, j] = (0, 0);
                }
            }
        }

        ClearBotsWinCounter();
        for (int trial = 0; trial < numberOfTrials; trial++)
        {
            Console.WriteLine("Iteration: " + trial.ToString());
            for (int i = 0; i < k; i++)
            {
                Task[] taskArray = new Task[k - (i + 1)];
                for (int j = i + 1; j < k; j++)
                {
                    taskArray[j - (i + 1)] = Task.Factory.StartNew((j_thread) =>
                    {
                        var instance = (AI)Activator.CreateInstance(bots[i].GetType());
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(instance, bots[(int)j_thread]);
                        var (endState, _) = game.Play();
                        if (endState.Winner == PlayerEnum.PLAYER1)
                        {
                            Interlocked.Increment(ref botsWinCounter[i]);
                            var (wins, counter) = winMatrix[(int)j_thread, i];
                            winMatrix[(int)j_thread, i] = (wins + 1, counter + 1);
                            winMatrix[i, (int)j_thread].Item2 = counter + 1;
                        }
                        if (endState.Winner == PlayerEnum.PLAYER2)
                        {
                            Interlocked.Increment(ref botsWinCounter[(int)j_thread]);
                            var (wins, counter) = winMatrix[i, (int)j_thread];
                            winMatrix[i, (int)j_thread] = (wins + 1, counter + 1);
                            winMatrix[(int)j_thread, i].Item2 = counter + 1;
                        }

                    }, j);
                }
                Task.WaitAll(taskArray);
            }
            for (int i = 0; i < botsWinCounter.Length; i++)
            {
                botsWinRatio[i] = (double)botsWinCounter[i] / (double)(botsWinCounter.Length - 1);
            }
            File.AppendAllText(loggerPath, string.Join(",", botsWinRatio) + System.Environment.NewLine);
            string result = "";
            foreach ((int, int) x in winMatrix)
            {
                result += string.Join(",", x) + "#";
            }
            File.WriteAllText(matrixPath, result);
            ClearBotsWinCounter();
        }
    }

}
