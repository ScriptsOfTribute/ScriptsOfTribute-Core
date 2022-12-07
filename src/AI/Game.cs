using System;
using System.Collections.Generic;
using System.Text;

namespace TalesOfTribute.src.AI
{
    public class Game
    {
        private AI _player1;
        private AI _player2;
        private AI[] _players = new AI[2];
        private TalesOfTributeApi _api;
        private AI CurrentPlayer => _players[(int)_api.CurrentPlayerId];
        public Game(AI player1, AI player2)
        {
            _player1 = player1;
            _player2 = player2;
            _players[0] = _player1;
            _players[1] = _player2;
            _player1.PlayerID = PlayerEnum.PLAYER1;
            _player2.PlayerID = PlayerEnum.PLAYER2;

            _api = new TalesOfTributeApi(PatronSelection());
        }

        private PatronId[] PatronSelection()
        {
            List<PatronId> patrons = Enum.GetValues(typeof(PatronId)).Cast<PatronId>()
                .Where(patronId => patronId != PatronId.TREASURY).ToList();

            List<PatronId> patronsSelected = new List<PatronId>();
            PatronId patron = _player1.SelectPatron(patrons, 1);
            VerifyPatronSelection(patron, patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patron = _player2.SelectPatron(patrons, 1);
            VerifyPatronSelection(patron, patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patronsSelected.Add(PatronId.TREASURY);

            patron = _player2.SelectPatron(patrons, 2);
            VerifyPatronSelection(patron, patrons);
            patronsSelected.Add(patron);
            patrons.Remove(patron);

            patron = _player1.SelectPatron(patrons, 2);
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

        public void Play()
        {
            while (_api.CheckWinner() == PlayerEnum.NO_PLAYER_SELECTED)
            {
                HandleStartOfTurnChoices();

                Move move;
                do
                {
                    move = CurrentPlayer.Play(_api.GetSerializer());
                    HandleMove(move);
                } while (move.Command != CommandEnum.END_TURN);
            }
        }


        private void HandleStartOfTurnChoices()
        {
            var startOfTurnChoices = _api.HandleStartOfTurnChoices();

            if (startOfTurnChoices is null) return;

            foreach (var choice in startOfTurnChoices.Consume())
            {
                if (choice is not Choice<Card> realChoice)
                {
                    throw new Exception(
                        "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
                }
                
                HandleStartOfTurnChoice(realChoice);
            }
        }

        private void HandleStartOfTurnChoice(Choice<Card> choice)
        {
            var playersChoice = CurrentPlayer.HandleStartOfTurnChoice(_api.GetSerializer(),
                SerializedChoice<Card>.FromChoice(choice));

            PlayResult result;
            do
            {
                result = choice.Choose(playersChoice);

                if (result is Failure f)
                {
                    CurrentPlayer.HandleChoiceFailure(f.Reason);
                }
            } while (result is Failure);

            if (result is not Success)
            {
                throw new Exception(
                    "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
            }
        }

        private void HandleMove(Move move)
        {
            if (!_api.IsMoveLegal(move))
            {
                throw new Exception("Illegal move!");
            }
            
            switch (move.Command)
            {
                case CommandEnum.PLAY_CARD:
                    HandlePlayCard(move.Value);
                    break;
                case CommandEnum.ATTACK:
                    HandleAttack(move.Value);
                    break;
                case CommandEnum.BUY_CARD:
                    HandleBuyCard(move.Value);
                    break;
                case CommandEnum.END_TURN:
                    break;
                case CommandEnum.PATRON:
                    HandleCallPatron(move.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandlePlayCard(int uniqueId)
        {
            var chain = _api.PlayCard(uniqueId);

            foreach (var result in chain.Consume())
            {
                HandleTopLevelResult(result);
            }
        }

        private void HandleAttack(int uniqueId)
        {
            
        }

        private void HandleBuyCard(int uniqueId)
        {
            
        }

        private void HandleCallPatron(int id)
        {
            
        }

        private void HandleTopLevelResult(PlayResult result)
        {
            switch (result)
            {
                case Success:
                    return;
                case Failure failure:
                    // TODO: Top-level Failure (in ExecutionChain) should mean that it's the last element in
                    // the chain. If there are more elements, something is broken. Make sure that's true
                    // (mid chain failure should occur only after Choice, in other cases it doesn't make sense as we
                    // don't have anything to retry).
                    CurrentPlayer.HandleChoiceFailure(failure.Reason);
                    break;
                default:
                    HandleChoice(result);
                    break;
            }
        }

        private void HandleChoice(PlayResult result)
        {
            PlayResult newResult = result;
            
            do
            {
                switch (newResult)
                {
                    case Choice<Card> choice:
                    {
                        var c = CurrentPlayer.HandleCardChoice(_api.GetSerializer(), SerializedChoice<Card>.FromChoice(choice));
                        result = newResult;
                        newResult = choice.Choose(c);
                        break;
                    }
                    case Choice<Effect> choice:
                    {
                        var c= CurrentPlayer.HandleEffectChoice(_api.GetSerializer(), SerializedChoice<Effect>.FromChoice(choice));
                        result = newResult;
                        newResult = choice.Choose(c);
                        break;
                    }
                    // That means Failure occured after Choice, so we repeat previous Choice.
                    case Failure failure:
                    {
                        CurrentPlayer.HandleChoiceFailure(failure.Reason);
                        newResult = result;
                        break;
                    }
                }
            }
            while (newResult is not Success);
        }
    }
}
