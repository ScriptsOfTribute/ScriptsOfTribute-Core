# Scripts of Tribute
Implementation of Tales of Tribute card game simulator for AI competition.

# Tales of Tribute
More info about the game itself here: https://eso-hub.com/en/tales-of-tribute-card-game

SoT created for 8.1.5 patch at the moment.

# Project structure
* ScriptsOfTribute Engine
    
    Core simulator engine all the functions needed for simulations are here, in ScriptsOfTributeAPI to be precise.
* GameRunner
    
    CLI game runner with referee, game stats etc. Provides tools such as debbuging, threading or setting game seed. User can also specify amount of games to be played. Use `--help`/`-h` flag to get all information.
* Bots
    
    Several AIs created by us on our engine
* Tests and BotTests
    
    Tests for engine and bots
* ScriptsOfTributeUI (Not in this repository)
    
    GUI app created with Unity3D framework. It is a tool that allows user to play with AIs created on this engine. Introduces many tools to debug and improve bots. More info here: https://github.com/ScriptsOfTributeProject/ScriptsOfTribute-UI
