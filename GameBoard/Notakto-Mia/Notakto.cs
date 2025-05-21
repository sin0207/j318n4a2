using GameBoard;

namespace Notakto
{
    public class NotaktoBoard : GameBoard.GameBoard
    {
        protected override int PlayerCount => 2;
        protected override string GameRecordFileName => _recordFileName;
        public override string GameName => "Notakto";
        
        private string _recordFileName = "notakto_save.json";

        static NotaktoBoard() => GameBoardFactory.RegisterGame(() => new NotaktoBoard());

        public NotaktoBoard()
        {
            RowSize = 9;
            ColSize = 3;
        }

        /// Check if a 3×3 segment forms a three-in-a-row for 'X'.
        private bool IsBoardDeadSegment(int idx)
        {
            int baseRow = idx * 3 + 1;
            // if any triple exists on this board, it's dead
            for (int r = 0; r < 3; r++)
            {
                for (int c = 1; c <= 3; c++)
                {
                    if (CheckWin(baseRow + r, c, "X"))
                        return true;
                }
            }
            return false;
        }


        public bool IsBoardDead(int boardIndex) => IsBoardDeadSegment(boardIndex);

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
            bool diag1 = new[] { (0, 0), (1, 1), (2, 2) }
                .All(o => Board[baseRow + o.Item1, o.Item2 + 1]?.ToString() == mark);
            bool diag2 = new[] { (0, 2), (1, 1), (2, 0) }
                .All(o => Board[baseRow + o.Item1, o.Item2 + 1]?.ToString() == mark);

            return rowWin || colWin || diag1 || diag2;
        }

        public override void DisplayHelpMenu()
        {
            Console.WriteLine("\n=== HELP MENU ===");
            Console.WriteLine("{0} Rules:", GameName);
            Console.WriteLine("1. Three separate 3x3 boards.");
            Console.WriteLine("2. Players take turns placing 'X' on any alive board.");
            Console.WriteLine("3. Creating a three-in-a-row kills that board (removed from play).");
            Console.WriteLine("4. The player who kills the last board loses.\n");

            Console.WriteLine("Actions:");
            Console.WriteLine("1. Make a Move");
            Console.WriteLine("2. Undo move");
            Console.WriteLine("3. Redo move");
            Console.WriteLine("4. Save current game");
            Console.WriteLine("5. View help menu");
            PauseProgramByReadingKeyPress();
        }

        public override void DisplayCurrentInformation()
        {
            var player = GetCurrentPlayer();
            Console.WriteLine(player.IsHumanPlayer()
                ? $"Player {player.PlayerNumber}'s turn:"
                : "Computer's turn:");
            Console.WriteLine("The current game board is:");

            // For each 3×3 board segment
            for (int b = 0; b < 3; b++)
            {
                string status = IsBoardDead(b) ? " [KILLED]" : string.Empty;
                Console.WriteLine($"Board {b + 1} {status}:");
                PrintSegment(b);
                Console.WriteLine();
            }
        }

        private void PrintSegment(int boardIndex)
        {
            int baseRow = boardIndex * 3 + 1;
            Console.WriteLine("   | 1 | 2 | 3 |");
            Console.WriteLine("---+---+---+---");
    
            for (int r = 0; r < 3; r++)
            {
                char rowLetter = (char)('A' + r);  // Always A, B, C for each board
                Console.Write(rowLetter + "  |");
                for (int c = 1; c <= 3; c++)
                {
                    var val = Board[baseRow + r, c] == NotPlacedFlag ? "." : Board[baseRow + r, c].ToString();
                    Console.Write(" " + val + " |");
                }
                Console.WriteLine();
                Console.WriteLine("---+---+---+---");
            }
        }


        protected override HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
            => new NotaktoHumanPlayer(boardSize, playerNumber);

        protected override ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
            => new NotaktoComputerPlayer(boardSize, playerNumber);



        public class NotaktoHumanPlayer : HumanPlayer
    {
        public NotaktoHumanPlayer(int boardSize, int playerNumber)
            : base(boardSize, playerNumber) { }

        public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
        {
            if (!(gameBoard is NotaktoBoard nb))
                throw new ArgumentException("Expected NotaktoBoard", nameof(gameBoard));

            // Choose a non-dead board
            int boardNum;
            while (true)
            {
                Console.Write("Choose board (1–3): ");
                if (int.TryParse(Console.ReadLine(), out boardNum) && boardNum >= 1 && boardNum <= 3)
                {
                    // if this board is dead, prompt and retry
                    if (nb.IsBoardDead(boardNum - 1))
                    {
                        Console.WriteLine("This board is killed! Please play on one of the other alive boards.");
                        continue;
                    }
                    break;
                }
                Console.WriteLine("Invalid — enter 1, 2 or 3.");
            }

            //  Choose a row within selected board
            int localRow;
            while (true)
            {
                Console.Write("Choose row (A–C): ");
                var s = Console.ReadLine()?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(s) && s.Length == 1 && s[0] >= 'A' && s[0] <= 'C')
                {
                    localRow = s[0] - 'A' + 1;
                    break;
                }
                Console.WriteLine("Invalid — enter A, B or C.");
            }

            // Choose column within selected board
            int col;
            while (true)
            {
                Console.Write("Choose column (1–3): ");
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

            // filter moves only on alive boards
            var all = new List<(int, int)>();
            foreach (var b in Enumerable.Range(0, 3))
            {
                if (nb.IsBoardDead(b)) continue;
                int baseRow = b * 3 + 1;
                for (int r = 0; r < 3; r++)
                for (int c = 1; c <= 3; c++)
                {
                    int gr = baseRow + r;
                    if (nb.IsAvailablePosition(gr, c))
                        all.Add((gr, c));
                }
            }

            // among alive positions, avoid final kill
            int aliveCount = Enumerable.Range(0, 3).Count(i => !nb.IsBoardDead(i));
            var safe = all.Where(pos => !
                (aliveCount == 1 && nb.CheckWin(pos.Item1, pos.Item2, "X"))
            ).ToList();

            var choices = safe.Any() ? safe : all;
            var pick = choices[PickIndexRandomly(choices.Count)];
            return (pick.Item1, pick.Item2, GetValueForNextMove());
        }


            protected override object GetValueForNextMove() => "X";
        }
    } }
