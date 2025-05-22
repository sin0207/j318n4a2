using GameBoard;

namespace Notakto;

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
            Console.WriteLine("Two players alternate playing X in any free cell on any live board.");
            Console.WriteLine("Once a board has three-in-a-row it is dead and removed from the game.");
            Console.WriteLine("The player that is forced to complete three-in-a-row on the last live board is the loser.\n");
            PauseProgramByReadingKeyPress();
        }

        public new void DisplayCurrentInformation()
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
    }

