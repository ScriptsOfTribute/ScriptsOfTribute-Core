using SimpleBots;
using TalesOfTribute;
using System.Text;

namespace SimpleBotsTests;

public class AdjustParametersByEvolution{

    private StringBuilder evolutionLogger = new StringBuilder();

    private string evolutionLoggerPath = "evolution.txt";

    private int[] GetRandomIndividual(int length){
        int[] genotype = new int[length]; 

        for (int i = 0; i < length; i++)
        {
            genotype[i] = SimpleBots.Extensions.RandomK(-5000, 10000);
        }
        return genotype;
    }

    private int[] Mutation(int[] genotype, int mutationRate){
        for (int i =0; i < genotype.Length; i++){
            int chance = SimpleBots.Extensions.RandomK(0, 100);
            if (chance <= mutationRate){
                // albo zmiana znaku , albo wyzerowanie, albo przemnożenie przez jakąś stała z randomowym znakiem
                genotype[i] =  SimpleBots.Extensions.RandomK(-5000, 10000);
            }
        }
        return genotype;
    }

    private (int[], int[]) Inheritence(int[] parent1, int[] parent2){
        int[] child1 = new int[parent1.Length];
        int[] child2 = new int[parent1.Length];
        for (int i=0; i < parent1.Length; i++){
            if (SimpleBots.Extensions.RandomK(0, 2)==1){
                child1[i] = parent1[i];
                child2[i] = parent2[i];
            }
            else{
                child1[i] = parent2[i];
                child2[i] = parent1[i];
            }
        }
        return (child1, child2);
    }

    public int[] Evolution(int sizeOfPopulation, int numberOfGenerations, int mutationRate){
        HeuristicBot[] population = new HeuristicBot[sizeOfPopulation];
        for (int i=0; i < sizeOfPopulation; i++){
            HeuristicBot bot = new HeuristicBot();
            bot.SetGenotype(GetRandomIndividual(9));
            population[i] = bot;
        }
        HeuristicBot[] winners = new HeuristicBot[sizeOfPopulation/2];
        HeuristicBot[] children = new HeuristicBot[sizeOfPopulation/2];
        for (int generation =0; generation < numberOfGenerations; generation++){
            for (int j = 0; j< sizeOfPopulation; j+=2){
                var game = new TalesOfTribute.AI.TalesOfTribute(population[j], population[j+1]);
                var (endState, endBoardState) = game.Play();

                if (endState.Winner == PlayerEnum.PLAYER1){
                    winners[j/2] = population[j];
                }
                else{
                    winners[j/2] = population[j+1];
                }
            }
            for (int j =0; j < sizeOfPopulation/2; j+=2){
                (int[] genotype1, int[] genotype2) = Inheritence(winners[j].GetGenotype(), winners[j+1].GetGenotype());
                genotype1 = Mutation(genotype1, mutationRate);
                genotype2 = Mutation(genotype1, mutationRate);
                children[j] = new HeuristicBot();
                children[j+1] = new HeuristicBot();
                children[j].SetGenotype(genotype1);
                children[j+1].SetGenotype(genotype2);
            }
            population = winners.Concat(children).ToArray();
            population = population.OrderBy(x => SimpleBots.Extensions.Rnd.Next()).ToArray();

            evolutionLogger.Append("Generation: " + generation.ToString() + " " + string.Join(",", winners[SimpleBots.Extensions.Rnd.Next(sizeOfPopulation/2)].GetGenotype())+ System.Environment.NewLine);
        }
        File.AppendAllText(evolutionLoggerPath, evolutionLogger.ToString());
        return winners[SimpleBots.Extensions.Rnd.Next(sizeOfPopulation/2)].GetGenotype();
    }
}