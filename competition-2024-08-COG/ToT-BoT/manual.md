# Setup the Tales of Tribute framework

- Download the Tales of Tribute repository and extract it to some folder, from now on called "root"
- Install Visual Studion 2022
- Install .Net Package
- Open the root folder and its contained TalesOfTribute.sln file
- Build Solution inside Visual Studio and test GameRunner
    - The project will be built in several subfolders. The most relevant for us will be: "GameRunner/bin/Release/net7.0"
    - Open a terminal and navigate to the "GameRunner/bin/Release/net7.0" folder
    - from here, we cann open the python example bot using the following command './GameRunner "cmd:python ../../../../Bots/ExternalLanguageBotsUtils/Python/example-bot.py" "cmd:python ../../../../Bots/ExternalLanguageBotsUtils/Python/example-bot.py" -n 10 -t 1'
        - The game will start and the bots will play against each other n times within t threads
        - more threads may improve performance, but I wouldn't use it for your RL-based bot to avoid conflicts


# Modifying the Tales of Tribute framework to also parse actions to the external bots

- in ExternalAIAdapapter.cs update the following method accordingly:
        public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {

        var obj = gameState.SerializeGameState();
        sw.WriteLine("{ \"State\":");        
        sw.WriteLine(obj.ToString());
        sw.WriteLine(", \"Actions\": [");
        //var obj2 = possibleMoves.Select(m => m.Command.ToString()).ToList();
        sw.WriteLine(string.Join(',', possibleMoves.Select(m => "\"" + m.ToString() + "\"").ToList()));
        sw.WriteLine("]}");
        sw.WriteLine(EOT);


        string botOutput;
        botOutput = sr.ReadLine();
        // Console.WriteLine(string.Join(",", possibleMoves.Select(m => m.ToString()).ToList()));

        // Console.WriteLine($"Bot response: {botOutput}");
        return possibleMoves[int.Parse(botOutput)];
        //return MapStringToMove(botOutput, gameState, possibleMoves);
    }

- this breaks compatibility with the example-bot.py, but it is necessary to make the bots work with later to be implemented RL-based bots and we are going to fix this in a few seconds
