# Scripts of Tribute (SoT)
Implementation of Tales of Tribute card game simulator for AI competition.

## Tales of Tribute
More info about the game itself is available at [eso-hub.com](https://eso-hub.com/en/tales-of-tribute-card-game). SoT was created for patch 8.1.5 and this is the only supported version at the moment.

## Project structure
* **`Engine`**, is the core of the engine. All the functions needed for simulations are here (mostly in the `ScriptsOfTributeAPI` class).
* **`GameRunner`**, is a CLI game runner with referee, game stats, etc. It comes with some tools useful for debbuging as well as a multithreaded or fixed game seed modes. Use `--help`/`-h` flag to get all information.
* **`Bots`**, is a set of a couple example AIs created by us.
* **`BotTests`** and **`Tests`**, contain the tests for the bots and the engine respectively.
* **`ScriptsOfTributeUI`** ([repo](https://github.com/ScriptsOfTribute/ScriptsOfTribute-GUI)), is a GUI app created with Unity3D framework. It is a tool that allows user to play with AIs created on this engine. Introduces many tools to debug and improve bots.
