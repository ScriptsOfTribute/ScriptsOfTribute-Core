
- [Announcements: new competition at COG 2025](#announcements-new-competition-at-cog-2025)
- [Scripts of Tribute Project](#scripts-of-tribute-project)
  - [Tales of Tribute](#tales-of-tribute)
  - [Game Version](#game-version)
  - [Contact](#contact)
  - [Authors](#authors)
- [Setup](#setup)
  - [Step 1: Download the engine](#step-1-download-the-engine)
  - [Step 2: Choose your IDE](#step-2-choose-your-ide)
  - [Step 3: .NET SDK](#step-3-net-sdk)
- [Implementing AI Agent](#implementing-ai-agent)
  - [Important Classes](#important-classes)
  - [Overview of Important Objects](#overview-of-important-objects)
  - [Creating a Bot](#creating-a-bot)
  - [External Language](#external-language)
  - [Example Agents](#example-agents)
  - [Console Game Runner](#console-game-runner)
  - [GUI](#gui)
- [Tales of Tribute AI Competition](#tales-of-tribute-ai-competition)
  - [IEEE Conference on Games 2025](#ieee-conference-on-games-2025)
      - [Changes from 2024 edition](#changes-from-2024-edition)
    - [Important Dates](#important-dates)
    - [Submission Rules](#submission-rules)
    - [Evaluation](#evaluation)
    - [Prizes](#prizes)
  - [Organizers](#organizers)
  - [Past Competitions](#past-competitions)
- [References](#references)
- [Legal Notice](#legal-notice)



# Announcements: new competition at COG 2025

- **Tales of Tribute AI Competition has been accepted for [IEEE Conference on Games 2025](https://cog2025.inesc-id.pt/tales-of-tribute/).**
- **Deadline for the agent submission is August 10.**
- **More details about participating, including updates in the game, [here](#ieee-conference-on-games-2025).**
- **In particular, our framework currently [supports multiple programming languages](#external-language).**



<!--**IEEE Conference on Games 2024 Tales of Tribute AI Competition has ended.**

**See the results and all competition data [here](https://github.com/ScriptsOfTribute/ScriptsOfTribute-CompetitionsArchive/blob/main/competition-2024-08-COG/README.md).**-->


<!--**Details regarding 2025 edition will be posted in in the first quarter of the year.**-->



<!--**Prizes for the winners:  $500USD for the first place, $300USD for the second, $200USD for the third.**-->


# Scripts of Tribute Project

Scripts of Tribute (SoT) framework is a Tales of Tribute simulator, implemented in C# .NET Core and allowing to write AI agents and play against them.




A short video describing the 2023 competition is available [HERE](https://www.youtube.com/watch?v=3FxBlZ40l6o) (most info remain up-to-date).


To play against the existing bots, download the most recent [GUI binary release](https://github.com/ScriptsOfTribute/ScriptsOfTribute-GUI/releases) for your OS.

To start developing your own AI agents, check the documentation in [this section](#implementing-ai-agent) and download [SoT-Core project](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core).

Dockerfile for competition environment is available [here](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Dockerfile)

Extended version of the COG 2024 paper describing the competition is available on [arXiv](https://arxiv.org/abs/2305.08234).

<!--Detailed rules of the competition are described in [this section](#ieee-conference-on-games-2024). -->


![](https://i.imgur.com/PFgkFLm.png)




## Tales of Tribute

Tales of Tribute is a deck-building game that launched with [The Elder Scrolls Online](https://www.elderscrollsonline.com/en-us/home) High Isle expansion. 

As a source of information about the game, we find the following links helpful (some information in the descriptions might be outdated due to the game patches):
- [Introducing Tales of Tribute AI Competition](https://arxiv.org/abs/2305.08234), Section IV
- [game rules](https://eso-hub.com/en/guides/tales-of-tribute-guide)
- [list of cards and patrons](https://eso-hub.com/en/tales-of-tribute-card-game) (up-to-date)
- [patron strategies](https://gamerant.com/complete-guide-to-elder-scrolls-online-high-isle-new-gear-bosses-cosmetics-mythics-and-tales-of-tribute/#tales-of-tribute---cards-patrons-and-strategies)
- [deck guides](https://www.youtube.com/@PinkAppleYT/videos)


## Game Version

The current SoT release is compatible with Tales of Tribute from ESO PC/Mac Patch 10.3.5 (10.03.2025). All patron deck cards are fully upgraded.

Cards data used is available in the [cards.json](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/cards.json) file.


## Contact

Come to our [Discord](https://discord.gg/RSZjNHuHGm) and talk to us.

## Authors

Jakub Kowalski, Dominik Budzki, Damian Kowalik, Katarzyna Polak,  Radosław Miernik ([University of Wrocław, Institute of Computer Science](https://ii.uni.wroc.pl/)).


# Setup

As ScriptsOfTribute Engine is based on the .NET framework, we recommend using Windows as a developing platform. 


## Step 1: Download the engine

You can download the source code of the SoT engine from [this repository](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core).


## Step 2: Choose your IDE

Any you like. If you choose Visual Studio, make sure that you choose Visual Studio 2022, which supports .NET 8.


## Step 3: .NET SDK 

*Skip this if you've chosen Visual Studio.*

To build our engine and create bots with it, you need to install .NET 8 SDK compatible with your operating system. Go to the official page [Download .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), download, and then install the latest version.

If you are using Linux - here is the [link](https://tecadmin.net/how-to-install-dotnet-core-on-ubuntu-22-04) to the tested tutorial; just change the version in commands from 6.0 to 8.0.


# Implementing AI Agent

## Important Classes 

You should familiarize yourself with them before implementing a bot.

1. [AI.cs](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/AI/AI.cs) - abstract class from which your agent should inherit. You have to implement the `SelectPatron` and `Play` methods. In the `EndGame` method, you can add code that should be run after the end of the game, and in `Log` method you can add some logs.

2. All files in [Serializers](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/tree/master/Engine/src/Serializers) folder - by using these classes you can gain access to all visible data of the game - board, hand, tavern, etc. You can start with the `GameState` class - the object of this class you get from the `Play` method. 

Useful files if you want to get access to some important information (cost of a card, amount of power of the opponent, ...) and types of cards, effects, etc:

3. [Move.cs](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/Board/Move.cs)
4. [Card.cs](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/Board/Cards/Card.cs)
5. [Agent.cs](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/Board/Cards/Agent.cs)
6. [UniqueEffects.cs](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/Board/Cards/UniqueEffect.cs)

## Overview of Important Objects
This section introduces some objects that are used in the engine and knowing them is important for understanding other sections.
- `EndGameState` – contains information about how the game ended. It has a `Reason` field that indicates why the game ended, which can, for example, be `TURN_TIMEOUT` or `INCORRECT_MOVE`. It also contains `ID` of the winning player (unless the game ended to a reason such as an internal failure) and a string containing additional context about why the game ended, for example, in case of an incorrect move, it contains the move and a list of all other correct moves that were possible and should have been played instead. API functions often return `EndGameState?` – in most cases it is null, but in case the player makes a mistake or his move ends the game, this object is returned to indicate this.
- `Move` – represents a move that a player can make. It contains a `Type` field, which can be, for example, `PLAY_CARD`, `END_TURN`, or `ACTIVATE_PATRON`. Depending on the type, it also contains additional information, such as the card that is to be played in case of `PLAY_CARD` move. Moves can be created using static methods in `Move` class, for example: `Move.PlayCard(card)`.
- `Choice` – represents a choice that the player has to make. It can be, for example, a choice of which card to discard. Either cards or effects can be chosen, depending on the card played. This object also contains some information about the choice, including all possible items to choose, how many items need to be chosen, or what the effect of the choice will trigger (for example: destroy the chosen cards).
- `ChoiceContext` – this object lives inside the `Choice` and holds additional context, including the card and effect, or the patron that triggered the choice.

## Creating a Bot

1. Create a file in `ScriptsOfTribute-Core\Bots\src` folder e.g. `MyFirstAgent.cs.` Please remember that your class (in this case, `MyFirstAgent`) should inherit from AI abstract class.


2. Implement the body of `SelectPatron` method.
Arguments of this method are a list of available patrons and which round of selection of patron it is (first or second). Your method should return `Enum` object of the type `PatronId`.

3. Implement a body of `Play` method
This method will be run in a loop until you don't return a move that will end your turn, or your bot will try to do not allowed move. The method receives a `GameState` and a list of possible moves and should return one move from that list.

4. [Optional] Implement the body of GameEnd
This method is called after the game has ended. The purpose
of this function is to allow the programmer to analyze the data from the `EndGameState` object as they wish.

5. [Optional] Add logs
To add logs to your bot, call the method `Log` with a string that you want to put in your log. Logs can be shown in the GUI during play.

6. Compile your bot
Just run `dotnet build` in `ScriptsOfTribute-Core\Bots` folder. A `Bots.dll` file should be created in the folder - you can use it to test your bot by copying that to the `GameRunner` folder, where you can test your bot against our sample bots or your other bots.

## External Language

### gRPC
Our engine supports gRPC connection for external languages. For now we've prepared Python [pip package](https://pypi.org/project/scripts-of-tribute/) that covers whole communication, for details please check repository with the [source code](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Python) and provided examples there on how to use this library.

### **OBSOLETE** Communication via standart input/output
There's a possibility to use different language than C# to create a bot thanks to [ExternalAIAdadpter](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/AI/ExternalAIAdapter.cs). For now engine is built to work with Python files, but enabling other languages is easy. In such cases please contact us.
If you plan to create a bot in different language you have to parse game state from stdin which will come in json format ending with EOT string as an "End of Transmission" sign. Forming this object is done in [Game State class](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Engine/src/Serializers/GameState.cs) in `SerializeGameState` method. To understand more how these objects look please check [tests](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Tests/utils/JSONSerializeTests.cs). In case of any problem fastest way to get help or any information is through our [discord](https://discord.gg/RSZjNHuHGm).

## Example Agents

Feel free to study our [example agents](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/tree/master/Bots/src), to familiarize yourself more with the infrastructure of the project. You can also use code from them to create your own code. 

Bots whose understanding can help you implement your own agents:
1. [RandomBot](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Bots/src/RandomBot.cs)
2. [MaxPrestigeBot](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Bots/src/MaxPrestigeBot.cs) Focus on the usage of `gameState.ApplyMove()` - this function allows you to simulate a move. If you provide a seed, you can simulate a random playout based on that seed. When running this method without a seed you will receive `GameState` and available moves without any random events (like a card that you could draw, a new card in the tavern after buying/removing the card, etc.)


## Console Game Runner 

Game Runner is a command-line application that allows the user to load bots from DLLs and run games between them.

Usage: `GameRunner <NameOfBot1> <NameOfBot2> <flags>`
for example: `GameRunner RandomBot RandomBot -n 1000 -t 2` will run 1000 games between two `RandomBot`s using two threads

For more flags and usage help, run `GameRunner -h`

To run game with bot made in different language, for example python via gRPC use `grpc:` before bot's name. For example: `GameRunner RandomBot grpc:RandomBot -n 1000 -t 2`

It is available for download [here](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/releases) for Windows, Linux and MacOS

##  GUI

To test your bot, you can also use [our application]((https://github.com/ScriptsOfTribute/ScriptsOfTribute-GUI)) in the Unity framework. 

Just download the [built version]((https://github.com/ScriptsOfTribute/ScriptsOfTribute-GUI/releases/tag/v0.9)) for your operating system and copy `.dll` file with your bots into 
- for Windows: `ScriptsOfTribute_Data\StreamingAssets`
- for Linux: `Linux_Data\StreamingAssets` (also make sure that you make `Linux.x86_64` executable),
- for Mac: `Mac.app\Contents\Resources\Data\StreamingAssets`


# Tales of Tribute AI Competition

The deckbuilding card game *Tales of Tribute* is a special activity recently added to the massively popular multiplayer online role-playing video game [The Elder Scrolls Online](https://www.elderscrollsonline.com). Although the game remains small by CCG standards (about 120 cards and only a few keywords), it is interesting and challenging for humans, with a significant potential for being a good AI testbed.

The players start with the same base cards and build their decks during the game by buying cards from a shared resource pool called the Tavern (a concept similar to Dominion). Tales of Tributes encourages long-term strategic planning - to ensure the deck we build contains enough strong cards and is not clustered with weak ones. In the meantime, because of a huge randomness factor influencing e.g., which cards occur in the Tavern, it is usually hard to stick with a pre-made plan - so it also requires considerable adaptiveness. 

Tales of Tribute AI Competition aims to fill the void after the [Hearthstone AI Competition](https://hearthstoneai.github.io) while being significantly more challenging than a toy problem covered by [Legends of Code and Magic](https://legendsofcodeandmagic.com/) and the [Strategy Card Game AI Competition](https://github.com/acatai/Strategy-Card-Game-AI-Competition).

The competition is running using *ScriptsOfTribute*, an open reimplementation of the original game in .NET framework designed especially for this event. It features the game manager that allows running AI agents implemented as C# classes against each other and a graphical user interface that support human vs. AI games. Current version supports also other programming languages.


## IEEE Conference on Games 2025

Tales of Tribute AI Competition has been accepted as one of the events at [IEEE CoG 2025](https://cog2025.inesc-id.pt/tales-of-tribute/).

#### Changes from 2024 edition

- gRPC communication
- Python ScriptsOfTribute library
- Added Saint Alessia deck
- Applied balance changes compatible with latest ESO patch
- Multiple QoL changes for writing and testing agents


### Important Dates

- **10th August 2025**, 23:59 GMT - **Submission deadline**
- 26th-29th August 2025 - [COG conference](https://cog2025.inesc-id.pt/) and results announcement


### Submission Rules

- Please send a single `.cs` file containing your agent's source code or a zip archive with all the others necessary files to jko@cs.uni.wroc.pl.
- In case of bots made in Python please provide files with source code + `requirements.txt` file archived in zip file.
- In case of agents written in other programming languages please attach compilation/run instructions.
- Additionally, the email should contain:
  - Agent's name.
  - Names (and institutions, if any) of all agent's authors.
  - Short description of the agent. Preferably a few slides or a short note in markdown or PDF; it has to describe what does the agent do, e.g., whether it employs some search algorithms or neural networks.
- Multiple bots can be submitted, but please indicate if a submission should replace an old one or be counted as a new submission (with a different agent's name). Each participant can have up to 2 final submissions. 
- Please be aware that submitted agents are going to be published in this repository after the competition. With the submission, you agree with this procedure.


### Evaluation

Agents will be evaluated using the [SoT-Core Game Runner](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core), on a large number of mirror matches using randomly generated seeds in an all-play-all system. The deciding factor will be the average winrate.

Evaluation environment will be compatible with the one provided by [Dockerfile](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/master/Dockerfile).

Time limit:
- 10 seconds for every turn

Memory limit and other constraints:
- while playing, the bot should not exceed 256 MB of memory. Anytime exceedance of 1024 MB of RAM usage will result in excluding the bot from the contest
- the size of sent file/archive should not exceed 25 MB


Game version:
- compatible with Tales of Tribute from ESO PC/Mac Patch 10.3.5 (10.03.2025) 
- 6 patrons available: [Pelin](https://eso-hub.com/en/tales-of-tribute/saint-pelin), [Crows](https://eso-hub.com/en/tales-of-tribute/duke-of-crows), [Ansei](https://eso-hub.com/en/tales-of-tribute/ansei-frandar-hunding), [Rajhin](https://eso-hub.com/en/tales-of-tribute/rajhin-the-purring-liar), [Orgnum](https://eso-hub.com/en/tales-of-tribute/sorcerer-king-orgnum), and [Alessia](hhttps://eso-hub.com/en/tales-of-tribute/saint-alessia).
- all decks are assumed to be fully upgraded


### Prizes

We will apply for prizes; more info soon.

<!--
- $500USD for the first place
- $300USD for the second place
- $200USD for the third place

Prize founded by the [IEEE CIS Education Competition Subcommittee](https://cis.ieee.org/).

-->



## Organizers

The Tales of Tribute AI competition is organized by Jakub Kowalski, Dominik Budzki, Damian Kowalik, Katarzyna Polak, and Radosław Miernik ([University of Wrocław, Institute of Computer Science](https://ii.uni.wroc.pl/)).

Have any questions or suggestions? Feel free to contact us on [Discord](https://discord.gg/RSZjNHuHGm) or send a [mail](mailto:jko@cs.uni.wroc.pl).


## Past Competitions

Information about the previous Tales of Tribute AI competitions, including submitted agents, can be found [here](https://github.com/ScriptsOfTribute/ScriptsOfTribute-CompetitionsArchive).



# References

The Tales of Tribute AI Competition has been described in [this article](https://arxiv.org/abs/2305.08234).

Please cite as follows:

```
@inproceedings{Kowalski2024IntroducingTales,
  author = {Kowalski, J. and Miernik, R. and Polak, K. and Budzki, D. and Kowalik D.},
  title = {{Introducing Tales of Tribute AI Competition}},
  booktitle = {IEEE Conference on Games},
  pages = {1--8},
  year = {2024},
}
```
Initial version of the ScriptsOfTribute has been described in [engineer's thesis](https://jakubkowalski.tech/Supervising/Budzki2023ImplementingTalesOfTribute.pdf).
```
@mastersthesis{Budzki2023ImplementingTalesOfTribute,
  title={{Implementing Tales of Tribute as a Programming Game}},
  author={Budzki, Dominik and Kowalik, Damian and Polak, Katarzyna},
  type={Engineer's Thesis},
  year={2023},
  school={University of Wroc{\l}aw}
}
```


# Legal Notice

This competition is neither directly nor indirectly related to Bethesda Softworks, ZeniMax Online Studios, nor parent company ZeniMax Media, in any way, shape, or form.

It is based on the Scripts of Tribute framework, which mimics the game Tales of Tribute and provides access points for the development of AI agents. The framework does not allow to play the original game, nor does it connect to the game’s servers in any way.

The Elder Scrolls® Online developed by ZeniMax Online Studios LLC, a ZeniMax Media company. ZeniMax, The Elder Scrolls, ESO, Bethesda, Bethesda Softworks and related logos are registered trademarks or trademarks of ZeniMax Media Inc. in the US and/or other countries. All Rights Reserved.
