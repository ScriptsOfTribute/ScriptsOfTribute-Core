using Bots;
using ScriptsOfTribute;
using System.Text;
using ScriptsOfTribute.AI;

namespace Bots;

public class AdjustParametersByEvolution
{

    private Random rnd = new Random();
    private StringBuilder evolutionLogger = new StringBuilder();

    private string evolutionLoggerPath = "evolution_DecisionTreeBot.txt";

    private int[] GetRandomIndividual(int length, int minValue, int maxValue)
    {
        int[] genotype = new int[length];

        for (int i = 0; i < length; i++)
        {
            genotype[i] = rnd.Next(minValue, maxValue);
        }
        return genotype;
    }

    private int[] Mutation(int[] genotype, int mutationRate, int minValue, int maxValue)
    {
        for (int i = 0; i < genotype.Length; i++)
        {
            int chance = rnd.Next(minValue, maxValue);
            if (chance <= mutationRate)
            {
                // albo zmiana znaku , albo wyzerowanie, albo przemnożenie przez jakąś stała z randomowym znakiem
                genotype[i] = rnd.Next(minValue, maxValue);
            }
        }
        return genotype;
    }

    private (int[], int[]) Inheritence(int[] parent1, int[] parent2)
    {
        int[] child1 = new int[parent1.Length];
        int[] child2 = new int[parent1.Length];
        for (int i = 0; i < parent1.Length; i++)
        {
            if (rnd.Next(0, 2) == 1)
            {
                child1[i] = parent1[i];
                child2[i] = parent2[i];
            }
            else
            {
                child1[i] = parent2[i];
                child2[i] = parent1[i];
            }
        }
        return (child1, child2);
    }
    //TODO zrobić to dla dowolnego bota - problem z tworzeniem tablic dowolnego typu, musiałabym to robić na switchu
    public int[] Evolution(int sizeOfPopulation, int numberOfGenerations, int mutationRate, int minValue, int maxValue)
    {
        DecisionTreeBot[] population = new DecisionTreeBot[sizeOfPopulation];
        for (int i = 0; i < sizeOfPopulation; i++)
        {
            var bot = new DecisionTreeBot();
            bot.SetGenotype(GetRandomIndividual(5, minValue, maxValue));
            population[i] = bot;
        }
        DecisionTreeBot[] winners = new DecisionTreeBot[sizeOfPopulation / 2];
        DecisionTreeBot[] children = new DecisionTreeBot[sizeOfPopulation / 2];
        Task[] taskArray = new Task[5];
        for (int generation = 0; generation < numberOfGenerations; generation++)
        {
            //Console.WriteLine("generacja: " + generation.ToString());
            for (int j = 0; j < sizeOfPopulation; j += 10)
            {
                for (int i = 0; i < taskArray.Length; i++)
                {
                    taskArray[i] = Task.Factory.StartNew((thread_index_obj) =>
                    {
                        int thread_index = (int)thread_index_obj;
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(population[thread_index * 2 + j], population[thread_index * 2 + 1 + j]);
                        var (endState, endBoardState) = game.Play();

                        if (endState.Winner == PlayerEnum.PLAYER1)
                        {
                            winners[j / 2 + thread_index] = population[thread_index * 2 + j];
                        }
                        else
                        {
                            winners[j / 2 + thread_index] = population[thread_index * 2 + 1 + j];
                        }
                    }, i);
                }
                Task.WaitAll(taskArray);
            }
            for (int j = 0; j < sizeOfPopulation / 2; j += 2)
            {
                (int[] genotype1, int[] genotype2) = Inheritence(winners[j].GetGenotype(), winners[j + 1].GetGenotype());
                genotype1 = Mutation(genotype1, mutationRate, minValue, maxValue);
                genotype2 = Mutation(genotype1, mutationRate, minValue, maxValue);
                children[j] = new DecisionTreeBot();
                children[j + 1] = new DecisionTreeBot();
                children[j].SetGenotype(genotype1);
                children[j + 1].SetGenotype(genotype2);
            }
            population = winners.Concat(children).ToArray();
            population = population.OrderBy(x => rnd.Next()).ToArray();

            evolutionLogger.Append("Generation: " + generation.ToString() + " " + string.Join(",", winners[rnd.Next(sizeOfPopulation / 2)].GetGenotype()) + System.Environment.NewLine);
        }
        File.AppendAllText(evolutionLoggerPath, evolutionLogger.ToString());
        return winners[rnd.Next(sizeOfPopulation / 2)].GetGenotype();
    }
}