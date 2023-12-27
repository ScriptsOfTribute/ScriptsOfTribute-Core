using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Diagnostics;
using System.IO.Pipes;

namespace ScriptsOfTribute.AI;

public class ExternalAIAdapter : AI
{
    private Process botProcess;
    private StreamWriter sw;
    private StreamReader sr;

    private string EOT = "EOT";
    public ExternalAIAdapter(string programName, string fileName)
    {
        botProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = programName,
                Arguments = fileName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        botProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine($"Debug: {e.Data}");
            }
        };

        botProcess.Start();
        botProcess.BeginErrorReadLine();
        sw = botProcess.StandardInput;
        sr = botProcess.StandardOutput;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        // TODO: parse EndGameSTate and FullGameState to JSON object and send it
        sw.WriteLine("FINISHED" + " " + state.ToSimpleString());
        sw.WriteLine(EOT);
        sw.Flush();
        botProcess.CloseMainWindow();
        botProcess.WaitForExit();
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var obj = gameState.SerializeGameState();
        sw.WriteLine(obj.ToString());
        sw.WriteLine(EOT);
        string botOutput;
        botOutput = sr.ReadLine();
        //Console.WriteLine($"Bot response: {botOutput}");
        return MapStringToMove(botOutput, gameState);
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        sw.WriteLine(string.Join(",", availablePatrons.Select(patron => patron.ToString())) + " " + round.ToString());
        string botOutput;
        botOutput = sr.ReadLine();
        Enum.TryParse(botOutput.ToUpper(), out PatronId patronPicked);
        //Console.WriteLine($"Bot response: {botOutput}");
        return patronPicked;
    }

    private Move MapStringToMove(string move, GameState gameState)
    {
        Console.WriteLine(move);
        string[] tokens = move.Split(" ");
        if (tokens.Length < 2)
            return Move.EndTurn();

        // We parse patron first since it's the only one with string args, rest uses integer values that represent UniqueId
        if (tokens[0] == "PATRON" && tokens.Length == 2 && Enum.TryParse(tokens[1], out PatronId patron))
        {
            return Move.CallPatron(patron);
        }

        List<UniqueId> args = new List<UniqueId>();
        for (int i = 1; i < tokens.Length; i++)
        {
            args.Add(UniqueId.FromExisting(Convert.ToInt32(tokens[i])));
        }

        if (tokens[0] == "PLAY" && args.Count == 1)
        {
            UniqueCard card = gameState.CurrentPlayer.Hand.First(c => c.UniqueId == args[0]);
            return Move.PlayCard(card);
        }
        else if (tokens[0] == "BUY" && args.Count == 1)
        {
            UniqueCard card = gameState.TavernAvailableCards.First(c => c.UniqueId == args[0]);
            return Move.BuyCard(card);
        }
        else if (tokens[0] == "ATTACK" && args.Count == 1)
        {
            UniqueCard card = gameState.EnemyPlayer.Agents.First(c => c.RepresentingCard.UniqueId == args[0]).RepresentingCard;
            return Move.Attack(card);
        }
        else if (tokens[0] == "ACTIVATE" && args.Count == 1)
        {
            UniqueCard card = gameState.CurrentPlayer.Agents.First(c => c.RepresentingCard.UniqueId == args[0]).RepresentingCard;
            return Move.ActivateAgent(card);
        }
        
        // In case of no match or something else treat it as giving up a turn.
        return Move.EndTurn();
    }

}
