using TalesOfTribute;

namespace Tests.Board
{
    public class BoardManagerTests
    {
        [Fact]
        public void TestBoardManagerSetUp()
        {
            var board = new BoardManager(
                    new[] { PatronId.ANSEI }
                );
            board.SetUpGame();

            Assert.Equal(PlayerEnum.PLAYER1, board.currentPlayer);
            Assert.Equal(1, board.players[(int)PlayerEnum.PLAYER2].CoinsAmount);

            Assert.Contains(
                GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
                board.players[(int)PlayerEnum.PLAYER2].DrawPile
            );

            Assert.Equal(6, board.players[(int)PlayerEnum.PLAYER2].DrawPile.Where(card => card.Id == CardId.GOLD).Count());

            Assert.NotEqual(
                board.players[(int)PlayerEnum.PLAYER1].DrawPile,
                board.players[(int)PlayerEnum.PLAYER2].DrawPile
            );
        }
    }
}
