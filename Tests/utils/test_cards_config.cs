namespace Tests.utils;

public class test_cards_config
{
    public const string CARDS_JSON_TWO_DECKS = @"[
    {
       ""id"":1,
       ""Name"":""Luxury Exports"",
       ""Deck"":""Hlaalu"",
       ""Cost"":2,
       ""Type"":""Action"",
       ""HP"":-1,
       ""Activation"":""Coin 3"",
       ""Combo 2"":null,
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Copies"":1
    },
    {
       ""id"":4,
       ""Name"":""Hlaalu Councilor"",
       ""Deck"":""Hlaalu"",
       ""Cost"":10,
       ""Type"":""Agent"",
       ""HP"":2,
       ""Activation"":""Acquire 9"",
       ""Combo 2"":""Remove 1"",
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Family"":5,
       ""Copies"":1
    },
    {
       ""id"":5,
       ""Name"":""Hlaalu Kinsman"",
       ""Deck"":""Hlaalu"",
       ""Cost"":10,
       ""Type"":""Agent"",
       ""HP"":1,
       ""Activation"":""Acquire 9"",
       ""Combo 2"":""Remove 1"",
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Family"":5,
       ""Copies"":1,
       ""PostUpgradeCopies"":1
    }
]";

    public const string CARDS_JSON_PREUPGRADE_CARDS = @"[
    {
       ""id"":1,
       ""Name"":""Luxury Exports"",
       ""Deck"":""Hlaalu"",
       ""Cost"":2,
       ""Type"":""Action"",
       ""HP"":-1,
       ""Activation"":""Coin 3"",
       ""Combo 2"":null,
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Copies"":1
    },
    {
       ""id"":4,
       ""Name"":""Hlaalu Councilor"",
       ""Deck"":""Hlaalu"",
       ""Cost"":10,
       ""Type"":""Agent"",
       ""HP"":2,
       ""Activation"":""Acquire 9"",
       ""Combo 2"":""Remove 1"",
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Family"":5,
       ""Copies"":1
    },
    {
       ""id"":5,
       ""Name"":""Hlaalu Kinsman"",
       ""Deck"":""Hlaalu"",
       ""Cost"":10,
       ""Type"":""Agent"",
       ""HP"":1,
       ""Activation"":""Acquire 9"",
       ""Combo 2"":""Remove 1"",
       ""Combo 3"":null,
       ""Combo 4"":null,
       ""Family"":5,
       ""Copies"":1
    }
]";
}
