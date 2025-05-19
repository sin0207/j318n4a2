using GameBoard;

namespace Notakto
{
    /// <summary>
    /// Human player for Notakto.  Prompts for board (1–3) and cell (A1–C3) in one method.
    /// </summary>
    public class NotaktoHumanPlayer : HumanPlayer
    {
        private const int Boards = 3;

        public NotaktoHumanPlayer(int boardSize, int playerNumber)
            : base(boardSize, playerNumber)
        {
            RemainingHoldings = Enumerable
                .Repeat((object)'X', boardSize * Boards)
                .ToArray();
        }

        protected override object GetValueForNextMove() => 'X';

        public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
        {
            var nb = (NotaktoBoard)gameBoard;
            int b, r, c;

            // Prompt board
            do
            {
                Console.Write("Select board (1-3): ");
            }
            while (!int.TryParse(Console.ReadLine(), out b)
                   || b < 1 || b > Boards
                   || nb.IsBoardDead(b - 1));

            // Prompt cell
            do
            {
                Console.Write("Select cell (A1-C3): ");
                var s = (Console.ReadLine() ?? "").ToUpper();
                if (s.Length == 2
                    && s[0] is >= 'A' and <= 'C'
                    && s[1] is >= '1' and <= '3')
                {
                    r = s[0] - 'A' + 1;
                    c = s[1] - '1' + 1;
                    if (gameBoard.IsAvailablePosition(r, c))
                        break;
                }
                Console.WriteLine("Invalid or occupied. Try again.");
            }
            while (true);

            return (b * 100 + r, c, GetValueForNextMove());
        }
    }
}