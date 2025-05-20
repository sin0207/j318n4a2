using GameBoard;

namespace Notakto
{
    public class NotaktoBoard : GameBoard.GameBoard
    {
        private string _recordFileName = "notakto_save.json";

        static NotaktoBoard() => GameBoardFactory.RegisterGame(() => new NotaktoBoard());

        public NotaktoBoard()
        {
            RowSize = 9;
            ColSize = 3;
        }

        protected override int PlayerCount => 2;
        protected override string GameRecordFileName => _recordFileName;
        public override string GameName => "Notakto";

        public override void DisplayHelpMenu()
        {
            Console.WriteLine("\n=== HELP MENU ===");
            Console.WriteLine("{0} Rules:", GameName);
            Console.WriteLine("1. Notakto is played on three separate 3×3 boards.");
            Console.WriteLine("2. Players take turns placing 'X' on any board.");
            Console.WriteLine("3. If a move creates three-in-a-row on a board, that board is killed and removed.");
            Console.WriteLine("4. The player who kills the last board (i.e., makes the last three-in-a-row) loses.");

            Console.WriteLine("Actions:");
            Console.WriteLine("1. Make a Move: you can choose to make next move.");
            Console.WriteLine("2. Undo move: you can undo your previous move from current game board.");
            Console.WriteLine("3. Redo move: you can redo your previous undo move from current game board.");
            Console.WriteLine("4. Save current game: you can save the current game state and resume later.");
            PauseProgramByReadingKeyPress();
        }

        public override bool CheckWin(int row, int col, object? value = null)
        {
            if (value == null) return false;
            var mark = value.ToString();
            int baseRow = ((row - 1) / 3) * 3 + 1;
            int localRow = ((row - 1) % 3) + 1;

            bool rowWin = Enumerable.Range(1, 3)
                .All(c => Board[baseRow + (localRow - 1), c]?.ToString() == mark);
            bool colWin = Enumerable.Range(1, 3)
                .All(r => Board[baseRow + (r - 1), col]?.ToString() == mark);
            bool diag1 = new[] { (0,0),(1,1),(2,2) }
                .All(o => Board[baseRow + o.Item1, o.Item2 + 1]?.ToString() == mark);
            bool diag2 = new[] { (0,2),(1,1),(2,0) }
                .All(o => Board[baseRow + o.Item1, o.Item2 + 1]?.ToString() == mark);

            return rowWin || colWin || diag1 || diag2;
        }

        public override void DisplayCurrentInformation()
        {
            var player = GetCurrentPlayer();
            Console.WriteLine(player.IsHumanPlayer()
                ? $"Player {player.PlayerNumber}'s turn:"
                : "Computer's turn:");

            Console.WriteLine("The current game board is:");
            for (int b = 0; b < 3; b++)
            {
                int rs = b * 3 + 1;
                char top = (char)('A' + b * 3);
                char bot = (char)('A' + b * 3 + 2);
                Console.WriteLine($"Board {b + 1} (Rows {top}–{bot}):");
                PrintBoard(rs, 1, rs + 2, ColSize);
                Console.WriteLine();
            }
        }

          public new void SaveGame()
        {
            string name;
            do
            {
                Console.Write("Enter filename to save: ");
                name = Console.ReadLine()?.Trim();
            }
            while (string.IsNullOrEmpty(name));

            _recordFileName = name + ".json";
            base.SaveGame();
        }

        protected override HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
            => new NotaktoHumanPlayer(boardSize, playerNumber);

        protected override ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
            => new NotaktoComputerPlayer(boardSize, playerNumber);
    }

        public class NotaktoHumanPlayer : HumanPlayer
        {
        public NotaktoHumanPlayer(int boardSize, int playerNumber)
            : base(boardSize, playerNumber) { }

        public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
        {
            if (!(gameBoard is NotaktoBoard nb))
                throw new ArgumentException("Expected NotaktoBoard", nameof(gameBoard));

            int boardNum;
            while (true)
            {
                Console.Write("Choose board (1-3): ");
                if (int.TryParse(Console.ReadLine(), out boardNum) && boardNum >= 1 && boardNum <= 3)
                    break;
                Console.WriteLine("Invalid — enter 1, 2 or 3.");
            }

            int localRow;
            while (true)
            {
                Console.Write("Choose row (A-C): ");
                var s = Console.ReadLine()?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(s) && s.Length == 1 && s[0] >= 'A' && s[0] <= 'C')
                {
                    localRow = s[0] - 'A' + 1;
                    break;
                }
                Console.WriteLine("Invalid — enter A, B or C.");
            }

            int col;
            while (true)
            {
                Console.Write("Choose column (1-3): ");
                if (int.TryParse(Console.ReadLine(), out col) && col >= 1 && col <= 3)
                    break;
                Console.WriteLine("Invalid — enter 1, 2 or 3.");
            }

            int globalRow = (boardNum - 1) * 3 + localRow;
            return (globalRow, col, GetValueForNextMove());
        }

        protected override object GetValueForNextMove() => "X";
    }

        public class NotaktoComputerPlayer : ComputerPlayer
        {
        public NotaktoComputerPlayer(int boardSize, int playerNumber)
            : base(boardSize, playerNumber) { }

        public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
        {
            if (!(gameBoard is NotaktoBoard nb))
                throw new ArgumentException("Expected NotaktoBoard", nameof(gameBoard));

            int alive = Enumerable.Range(0, 3)
                .Count(i => !BoardIsDead(i, nb));
            var all = new List<(int, int)>();
            var safe = new List<(int, int)>();

            for (int r = 1; r <= nb.RowSize; r++)
                for (int c = 1; c <= nb.ColSize; c++)
                    if (nb.IsAvailablePosition(r, c))
                    {
                        all.Add((r, c));
                        if (!(alive == 1 && nb.CheckWin(r, c, "X")))
                            safe.Add((r, c));
                    }

            var choices = safe.Count > 0 ? safe : all;
            var pick = choices[PickIndexRandomly(choices.Count)];
            return (pick.Item1, pick.Item2, GetValueForNextMove());
        }

        private bool BoardIsDead(int idx, NotaktoBoard nb)
        {
            int baseRow = idx * 3 + 1;
            return Enumerable.Range(baseRow, 3)
                .Any(r => Enumerable.Range(1, 3)
                    .Any(c => nb.CheckWin(r, c, "X")));
        }

        protected override object GetValueForNextMove() => "X";
    }
}
