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
    private string programName;
    private string fileName;

    private string EOT = "EOT";
    public ExternalAIAdapter(string programName, string fileName)
    {
        this.programName = programName;
        this.fileName = fileName;
    }

    public override void PregamePrepare()
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
        sw.Close();
        sr.Close();
        botProcess.CloseMainWindow();
        botProcess.WaitForExit();
        botProcess.Dispose();
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
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
        //Console.WriteLine(move);
        string[] tokens = move.Split(" ");
        if (tokens.Length < 2)
            return Move.EndTurn();

        // We parse patron here because it uses string value as arg
        // Similar case with CHOICE which is choice of effect -> it has to be an OR so we can use args as LEFT or RIGHT to simplify things.
        // TODO: Think if there's better way to parse effect choices
        if (tokens[0] == "PATRON" && tokens.Length == 2 && Enum.TryParse(tokens[1], out PatronId patron))
        {
            return Move.CallPatron(patron);
        }
        else if (tokens[0] == "CHOICE" && gameState.PendingChoice is not null && gameState.PendingChoice.Type == Choice.DataType.EFFECT)
        {
            var left = gameState.PendingChoice.PossibleEffects[0];
            var right = gameState.PendingChoice.PossibleEffects[1];

            return tokens[1] == "LEFT" ? Move.MakeChoice(left) : Move.MakeChoice(right);
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
        else if (tokens[0] == "CHOICE" && gameState.PendingChoice is not null && gameState.PendingChoice.Type == Choice.DataType.CARD)
        {
            var cards = gameState.PendingChoice.PossibleCards.Where(c => args.Contains(c.UniqueId)).ToList();
            return Move.MakeChoice(cards);
        }
        
        // In case of no match or something else treat it as giving up a turn.
        return Move.EndTurn();
    }

}
