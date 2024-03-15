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


## External Language Adapter Docs
Here is the description and breakdown of the JSON file which is being sent to external (different language than C#) bots.
First let's list basic objects which are being used to build whole JSON input:
* *PatronStates*:
  * It's JSON object where keys are Patron's names in stringified [PatronId](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Patrons/Patron.cs#L5) format, for example "ANSEI", "DUKE_OF_CROWS". Values are stringified [PlayerEnum](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/Player.cs#L6) values suggesting which Player is favored by certain Patron.
  * Object contains 4 key-value pairs like that, only for Patrons picked for current game.
* *Effects*:
  * We describe effects here because they are used for serialization of *Cards* below.
  * Effect is just a *string* but may contain 3 formats depending on type: Normal effect, Composite of effects and Alternative of effects
  * Normal effect is just a string in format: "{Type} {Amount}"
  * Composite of effects is a string in format: "{Type} {Amount} AND {Type} {Amount}"
  * Alternative of effects is a string in format: {Type} {Amount} OR {Type} {Amount}"
  * *Type* is stringified [EffectType](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/Cards/UniqueEffect.cs#L3), *Amount* is a number
* *Cards*:
  * Basic object representing card
  * This object contains few fields with basic types:
    * `"name"` - representing name of the card, known from the Tales of Tribute game. **Type**: *string*
    * `"deck"` - representing name of the deck this card belongs to. **Type**: *string*
    * `"type"` - Type of this card, one of the [CardType](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/Cards/Card.cs#L3) values, obviosly stringified. **Type**: *string*
    * `"HP"` - Health status of the card, Non-agent cards have -1 value in this field. **Type**: *int*
    * `"taunt"` - boolean field that can be True only for Agent-type cards. **Type**: *bool*
    * `"UniqueId"` - unique identifier for this **instance** of card in engine's database. **Type**: *int*
    * `"effects"` - list of *Effect* objects. Index is corresponding to the Combo type of the card, more precisely:
      * 0th index corresponds to "On activation"
      * 1st index corresponds to "Combo 2"
      * 2nd index and 3rd analogically to "Combo 3" and "Combo 4"
  * Examples can be found in [tests](Tests/utils/JSONSerializeTests.cs)
* *Agent*:
  * Contains 3 keys:
    * `"CurrentHP"` - current health of this agent. **Type**: *int*
    * `"Card"` - *Card* object representing this Agent. **Type**: *Card*
    * `"Activated"` - was this agent activated in this round. **Type**": *bool*
* *Choice*:
  * Describes currently pending choise player has to handle. Contains 6 keys:
    * `"MaxChoices"` - maximum number of picks player can make in current choice. **Type**: *int*
    * `"MinChoices"` - minimum number of picks player has to make in current choice. **Type**: *int*
    * `"Context"` - value that informs player what exactly enacted this choice. It can be Patron, Effect or another Choice. **Type**: *string*
    * `"ChoiceFollowUp"` - stringified [ChoiceFollowUp](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/PlayResult.cs#L15C13-L15C27) explaining what happens next.
    * `"Type"` - saying either "CARD" or "EFFECTS" explaining what type of objects is now being chosen. **Type**: *string*
    * Now depending on "Type" value we can have two different keys:
      * `"PossibleCards"` - list of possible *Cards* to choose from
      * `"PossibleEffects"` - list of possible *Effects* to choose from
* *CurrentPlayer*:
  * JSON object describing player with following keys:
    * `"Player"` - stringified [PlayerEnum](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/Player.cs#L6). Identified of player for this game.
    * `"Hand"` - array of *Cards* that are in Player's hand right now.
    * `"Cooldown"` - array of *Cards* that are in Player's cooldown pile right now.
    * `"Played"` - array of *Cards* that are in Player's played pile right now.
    * `"KnownPile"` -  - array of *Cards* from player's draw pile that Player has knowledge about their order.
    * `"Agents"` - array of *Agent* cards currently on the board.
    * `"Power"` - amount of power Player has. **Type**: *int*
    * `"PatronCalls"` - amount of Patron Calls Player can still make this turn. **Type**: *int*
    * `"Coins"` - amount of coin Player has. **Type**: *int*
    * `"Prestige"` - amount of prestige Player has. **Type**: *int*
    * `"DrawPile"` - array of *Cards* that are in Player's draw pile right now.
   
* *EnemyPlayer*:
  * Similar to *CurrentPlayer* with some small differences.
    * `"Player"` - stringified [PlayerEnum](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/Player.cs#L6). Identified of player for this game.
    * `"HandAndDraw"` - array of *Cards* that are in Enemy's hand and draw pile right now. We do this since our Bot doesn't know what cards will enemy have in his hand in the next turn.
    * `"Cooldown"` - array of *Cards* that are in Enemy's cooldown pile right now.
    * `"Played"` - array of *Cards* that are in Enemy's played pile right now.
    * `"Agents"` - array of *Agent* cards currently on the board on Enemy side.
    * `"Power"` - amount of power Enemy has. **Type**: *int*
    * `"Coins"` - amount of coin Enemy has. **Type**: *int*
    * `"Prestige"` - amount of prestige Enemy has. **Type**: *int*
   
* *CompletedAction*
  * That's a simple string that explains every action that happened in history of the game. I'm not gonna explain every possible output here, please check [this](Engine/src/Board/CompletedAction.cs) file and CompletedAction's method `ToSimpleString()`.

Now we can build our JSON object that will describe GameState. That's the object which engine sends to external bots.
Root object contains following keys:
* `"PatronStates"` - *PatronState* object
* `"TavernAvailableCards"` - array of *Cards* which are available in the tavern to buy.
* `"BoardState"` - stringified [BoardState](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/CardAction/CardActionManager.cs#L6) enum. It explains in what state board is currently in.
* `"UpcomingEffects"` - array of *Effects* which are about to be enacted.
* `"StartOfNextTurnEffects"` - array of *Effects* to be enacted on the start of the next turn.
* `"GameEndState"` - string explaining how the game ended. Only valid after game is over. 
  * Format is: "{Winner} {Reason} {AdditionalContext}" Where *Winner* is stringified PlayerEnum, *Reason* is stringified [GameEndReason](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core/blob/c92f394483a9b2ba2fff0a52194bace6a4b7114d/Engine/src/Board/GameEndReason.cs#L3C13-L3C26). *AdditionalContent* is a string that can contain additional info.
* `"CurrentPlayer"` - *CurrentPlayer* object giving info about bot status in current turn.
* `"EnemyPlayer"` - *EnemyPlayer* object giving info about enemy status.
* `"CompletedActions"` - array of *CompletedActions*
* `"TavernCards"` - array of *Cards* that are in (whole) tavern.
* `"PendingChoice"` - *Choice* pending which bot has to handle.
