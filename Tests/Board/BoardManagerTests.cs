using TalesOfTribute;

namespace Tests.utils;

public class BoardManagerTests
{
    [Fact]
    void ShouldMakeChoiceCorrectly()
    {
        var sut = new BoardManager(new[] { PatronId.ANSEI });
        var chain = sut.PlayCard(CardId.CONQUEST);

        foreach (var result in chain.Consume())
        {
            if (result is Success)
            {
                // Super lecimy dalej!
            }

            if (result is Failure)
            {
                // Failure powinno pojawić się tylko po podjęciu
                // złej decyzji przez użytkownika. Czyli próbujemy naprawić decyzję...
            }

            if (result is Choice<EffectType> choice)
            {
                // Jeszcze pomyślę jak to ogarnąć i dodać tutaj więcej info o tym
                // w zasadzie o czym użytkownik decyduje,
                // to będzie potrzebne.
                 
                Assert.Contains(EffectType.GAIN_POWER, choice.Choices);
                Assert.Contains(EffectType.ACQUIRE_TAVERN, choice.Choices);
                var newResult = choice.Commit(EffectType.GAIN_POWER);
                // Jeśli trzeba podjąć kolejną decyzję, czyli jeśli newResult to Choice,
                // to musimy to zhandlować, bo ExecutionChain nie pozwoli iść dalej.
                Assert.True(newResult is Success);
            }
        }
    }
}