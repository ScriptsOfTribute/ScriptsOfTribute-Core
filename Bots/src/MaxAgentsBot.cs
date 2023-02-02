using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace SimpleBots;

public class MaxAgentsBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    private Move? BuyAgent(List<Move> movesWithoutEndTurn)
    {
        List<Move> contractAgentToBuy = new List<Move>();
        SimpleCardMove? bestContractAgent = null;
        SimpleCardMove? bestAgent = null;
        foreach (Move m in movesWithoutEndTurn)
        {
            if (m.Command == CommandEnum.BUY_CARD)
            {
                SimpleCardMove move = m as SimpleCardMove;
                if (move.Card.Type == CardType.AGENT)
                {
                    if (bestAgent is null || CardTierList.GetCardTier(move.Card.Name) > CardTierList.GetCardTier(bestAgent.Card.Name))
                    {
                        bestAgent = move;
                        continue;
                    }
                }
                if (move.Card.Type == CardType.CONTRACT_AGENT)
                {
                    if (bestContractAgent is null || CardTierList.GetCardTier(move.Card.Name) > CardTierList.GetCardTier(bestContractAgent.Card.Name))
                    {
                        bestContractAgent = move;
                        continue;
                    }
                }
            }
        }
        if (bestAgent is not null)
        {
            return Move.BuyCard(bestAgent.Card);
        }
        if (bestContractAgent is not null)
        {
            return Move.BuyCard(bestContractAgent.Card);
        }
        return null;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var movesWithoutEndTurn = possibleMoves.Where(move => move.Command != CommandEnum.END_TURN).ToList();
        if (movesWithoutEndTurn.Count == 0)
        {
            return Move.EndTurn();
        }

        List<Move> playCardMoves = movesWithoutEndTurn.FindAll(x => x.Command == CommandEnum.PLAY_CARD || x.Command == CommandEnum.ACTIVATE_AGENT);
        if (playCardMoves.Any())
        {
            playCardMoves.PickRandom(Rng);
        }

        Move? agentBuy = BuyAgent(movesWithoutEndTurn);
        if (agentBuy is not null)
        {
            return agentBuy;
        }

        return movesWithoutEndTurn.PickRandom(Rng);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
