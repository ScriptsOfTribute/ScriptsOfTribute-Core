using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public class RandomBot : AI
{
    Random random = new Random(); 

    public List<T> SelectKRandomElement<T>(SerializedChoice<T> choice){
        int amountOfChoices = random.Next(choice.MinChoices, choice.MaxChoices);
        //Console.WriteLine(choice.PossibleChoices);
        Console.WriteLine('1');
        return (List<T>) choice.PossibleChoices.OrderBy(x => random.Next()).Take(amountOfChoices).ToList();
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round){
        Console.WriteLine('2');
        return availablePatrons[random.Next(availablePatrons.Count)];
    }

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves){
        Move result = possibleMoves[random.Next(possibleMoves.Count)];
        Console.WriteLine('3');
        Console.WriteLine(result);
        return result;
    }

    public override List<EffectType> HandleEffectChoice(SerializedBoard serializedBoard, SerializedChoice<EffectType> choice){
        Console.WriteLine('4');
        return SelectKRandomElement(choice);
    }

    public override List<Card> HandleCardChoice(SerializedBoard serializedBoard, SerializedChoice<Card> choice){
        Console.WriteLine('5');
        return SelectKRandomElement(choice);
    }

    public override List<Card> HandleStartOfTurnChoice(SerializedBoard serializedBoard, SerializedChoice<Card> choice){
        Console.WriteLine('6');
        return SelectKRandomElement(choice);
    }

    public override void GameEnd(EndGameState state){
        Console.WriteLine('7');
    }
}
