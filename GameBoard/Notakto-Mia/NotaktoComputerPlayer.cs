
using GameBoard;

namespace Notakto;

    public class NotaktoComputerPlayer : ComputerPlayer
    {
        private const int Boards = 3;
        private readonly Random _rand = new();

        public NotaktoComputerPlayer(int boardSize, int playerNumber)
            : base(boardSize, playerNumber)
        {
            RemainingHoldings = Enumerable
                .Repeat((object)'X', boardSize * Boards).ToArray();
        }

        protected override object GetValueForNextMove() => 'X';

        public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
        {
            // look for immediate kill
            for (int r = 1; r <= gameBoard.Size; r++)
            for (int c = 1; c <= gameBoard.Size; c++)
                if (gameBoard.IsAvailablePosition(r, c)
                    && gameBoard.CheckWin(r, c, 'X'))
                    return (r, c, 'X');

            // else random
            var avail = new List<(int,int)>();
            for (int r = 1; r <= gameBoard.Size; r++)
            for (int c = 1; c <= gameBoard.Size; c++)
                if (gameBoard.IsAvailablePosition(r, c))
                    avail.Add((r, c));

            var (rr, cc) = avail[_rand.Next(avail.Count)];
            return (rr, cc, 'X');
        }
    }
