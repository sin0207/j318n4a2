using GameBoard;
using System.Reflection;

namespace Notakto;

    public class NotaktoBoard : GameBoard.GameBoard
    {
        protected override int PlayerCount => 2;
        protected override string GameRecordFileName => _recordFileName;
        public override string GameName => "Notakto";

        private readonly string _recordFileName = "notakto_save.json";
        private readonly bool[] BoardWasDead = new bool[3];

        static NotaktoBoard()
            => GameBoardFactory.RegisterGame(() => new NotaktoBoard());

        public NotaktoBoard()
        {
            RowSize = 9;
            ColSize = 3;
        }

        private bool IsBoardDeadSegment(int idx)
        {
            int baseRow = idx * 3 + 1;
            for (int r = 0; r < 3; r++)
                for (int c = 1; c <= 3; c++)
                    if (CheckWin(baseRow + r, c, "X"))
                        return true;
            return false;
        }

        public bool IsBoardDead(int boardIndex)
            => IsBoardDeadSegment(boardIndex);

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

        public override void DisplayCurrentInformation()
        {
            // 1) Kill reminders
            for (int b = 0; b < 3; b++)
            {
                bool dead = IsBoardDead(b);
                if (dead && !BoardWasDead[b])
                {
                    Console.WriteLine($"Board {b + 1} is [KILLED]! Please choose a non-dead board.");
                    BoardWasDead[b] = true;
                }
            }

            // 2) Player turn + segments
            var player = GetCurrentPlayer();
            Console.WriteLine(player.IsHumanPlayer()
                ? $"Player {player.PlayerNumber}'s turn:"
                : "Computer's turn:");
            Console.WriteLine("The current game board is:");

            for (int b = 0; b < 3; b++)
            {
                string status = IsBoardDead(b) ? " [KILLED]" : "";
                Console.WriteLine($"Board {b + 1}{status}:");
                PrintSegment(b);
                Console.WriteLine();
            }
        }

protected override void RefreshGameStatus(int row, int col, object? value)
{
    // record the move so the built-in history advances
    AppendMove(row, col, value!);

    // let the base class update remaining positions
    base.RefreshGameStatus(row, col, value);

    // if *all* three 3×3 segments have been killed, fire game-over
    if (Enumerable.Range(0, 3).All(b => IsBoardDead(b)))
    {
        // display final result
        HandleGameOver();
        // exit immediately so main loop stops
        Environment.Exit(0);
    }
}
    private void PrintSegment(int boardIndex)
    {
        int baseRow = boardIndex * 3 + 1;
        // Columns A, B, C
        Console.WriteLine("   | A | B | C |");
        Console.WriteLine("---+---+---+---");

        for (int r = 0; r < 3; r++)
        {
            int rowNum = baseRow + r;
            Console.Write(rowNum.ToString().PadLeft(2) + " |");
            for (int c = 1; c <= 3; c++)
            {
                var val = Board[rowNum, c] == NotPlacedFlag
                    ? "." : Board[rowNum, c].ToString();
                Console.Write(" " + val + " |");
            }
            Console.WriteLine();
            Console.WriteLine("---+---+---+---");
        }
    }

        protected override void ShowResult()
{
    // Pull in the private _moveHistory via reflection
    var historyField = typeof(GameBoard.GameBoard)
        .GetField("_moveHistory", BindingFlags.Instance | BindingFlags.NonPublic)!;
    var history = (List<Move>)historyField.GetValue(this)!;

    // The last move’s PlayerIndex is the one who caused the final kill
    int loserIndex  = history.Last().PlayerIndex;
    int winnerIndex = loserIndex == 0 ? 1 : 0;

    Console.WriteLine("All boards are completed!");
    Console.WriteLine($"Player {winnerIndex + 1} wins!");
}
    protected override HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
    {
        return new NotaktoHumanPlayer(boardSize, playerNumber);
    }

protected override ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
{
    return new NotaktoComputerPlayer(boardSize, playerNumber);
}
    }
