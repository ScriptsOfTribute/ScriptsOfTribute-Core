using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public class TalesOfTribute
{
    public EndGameState? EndGameState => _game.EndGameState;
    
    private AI[] _players = new AI[2];

    private TalesOfTributeGame _game;
    
    public TalesOfTribute(AI player1, AI player2)
    {
        _players[0] = player1;
        _players[1] = player2;

        _game = new TalesOfTributeGame(_players, new TalesOfTributeApi(PatronSelection()));
    }

    private PatronId[] PatronSelection()
    {
        List<PatronId> patrons = Enum.GetValues(typeof(PatronId)).Cast<PatronId>()
            .Where(patronId => patronId != PatronId.TREASURY).ToList();

        List<PatronId> patronsSelected = new List<PatronId>();
        PatronId patron = _players[0].SelectPatron(patrons, 1);
        VerifyPatronSelection(patron, patrons);
        patronsSelected.Add(patron);
        patrons.Remove(patron);

        patron = _players[1].SelectPatron(patrons, 1);
        VerifyPatronSelection(patron, patrons);
        patronsSelected.Add(patron);
        patrons.Remove(patron);

        patronsSelected.Add(PatronId.TREASURY);

        patron = _players[1].SelectPatron(patrons, 2);
        VerifyPatronSelection(patron, patrons);
        patronsSelected.Add(patron);
        patrons.Remove(patron);

        patron = _players[0].SelectPatron(patrons, 2);
        VerifyPatronSelection(patron, patrons);
        patronsSelected.Add(patron);
        patrons.Remove(patron);

        return patronsSelected.ToArray();
    }

    private void VerifyPatronSelection(PatronId patron, List<PatronId> patrons)
    {
        if (!patrons.Contains(patron))
        {
            throw new Exception("Invalid patron selected.");
        }
    }

    public async Task<EndGameState> Play()
    {
        return await _game.Play();
    }
}
