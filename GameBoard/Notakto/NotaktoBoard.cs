using static System.Console;
using GameBoard;
using System.Reflection;

namespace Notakto;

public class NotaktoBoard : GameBoard.GameBoard
{
    protected override int PlayerCount => 2;
    protected override string GameRecordFileName => "notakto-record.json";
    public override string GameName => "Notakto";

    // Board Settings
    private const int BoardSize = 3;
    private const int BoardCount = 3;
    private bool[]? BoardHasLine;

    static NotaktoBoard()
    {
        GameBoardFactory.RegisterGame(() => new NotaktoBoard());
    }

    protected override void SetupGameBoard()
    {
        ColSize = BoardSize;
        RowSize = BoardSize * BoardCount;
        BoardHasLine = new bool[BoardCount];
        InitializeBoard();
    }

    protected override void DisplayBoard(string subject) {
        for (int i = 0; i < BoardCount; i++)
        {
            WriteLine("Board {0}", i + 1);
            int rowStart = (i * BoardSize) + 1;
            int rowEnd = rowStart + BoardSize - 1;
            PrintBoard(rowStart, 1, rowEnd, BoardSize);
        }
    }

    public bool IsBoardDead(int row)
    {
        int boardIndex = (row - 1) / BoardSize;
        return BoardHasLine[boardIndex];
    }

    public void SetTempMove(int row, int col, object? value)
    {
        Board[row, col] = value;
    }

    public bool CauseLineDetect(int row, int col, object symbol)
    {
        int boardSize = 3;
        int boardIndex = (row - 1) / boardSize;
        int rowStart = boardIndex * boardSize + 1;
        int rowEnd = rowStart + boardSize - 1;

        // row checking
        bool rowWin = true;
        for (int i = 1; i <= boardSize; i++)
        {
            if (Board[row, i]?.ToString() != symbol)
            {
                rowWin = false;
                break;
            }
        }

        // col checking
        bool colWin = true;
        for (int i = rowStart; i <= rowEnd; i++)
        {
            if (Board[i, col]?.ToString() != symbol)
            {
                colWin = false;
                break;
            }
        }

        // diagonal (top-left to bottom-right) \
        bool diag1Win = true;
        if ((row - rowStart) == (col - 1))
        {
            for (int i = 0; i < boardSize; i++)
            {
                if (Board[rowStart + i, i + 1]?.ToString() != symbol)
                {
                    diag1Win = false;
                    break;
                }
            }
        }
        else diag1Win = false;

        // diagonal (top-right to bottom-left) /
        bool diag2Win = true;
        if ((row - rowStart) == (boardSize - col))
        {
            for (int i = 0; i < boardSize; i++)
            {
                if (Board[rowStart + i, boardSize - i]?.ToString() != symbol)
                {
                    diag2Win = false;
                    break;
                }
            }
        }
        else diag2Win = false;

        return false;
    }

    public override bool CheckWin(int row, int col, object? value = null)
    {
        int boardSize = 3;
        int boardIndex = (row - 1) / boardSize;
        int rowStart = boardIndex * boardSize + 1;
        int rowEnd = rowStart + boardSize - 1;

        string symbol = value?.ToString() ?? "X";

        // row checking
        bool rowWin = true;
        for (int i = 1; i <= boardSize; i++)
        {
            if (Board[row, i]?.ToString() != symbol)
            {
                rowWin = false;
                break;
            }
        }

        // col checking
        bool colWin = true;
        for (int i = rowStart; i <= rowEnd; i++)
        {
            if (Board[i, col]?.ToString() != symbol)
            {
                colWin = false;
                break;
            }
        }

        // diagonal (top-left to bottom-right) \
        bool diag1Win = true;
        if ((row - rowStart) == (col - 1))
        {
            for (int i = 0; i < boardSize; i++)
            {
                if (Board[rowStart + i, i + 1]?.ToString() != symbol)
                {
                    diag1Win = false;
                    break;
                }
            }
        }
        else diag1Win = false;

        // diagonal (top-right to bottom-left) /
        bool diag2Win = true;
        if ((row - rowStart) == (boardSize - col))
        {
            for (int i = 0; i < boardSize; i++)
            {
                if (Board[rowStart + i, boardSize - i]?.ToString() != symbol)
                {
                    diag2Win = false;
                    break;
                }
            }
        }
        else diag2Win = false;

        bool win = rowWin || colWin || diag1Win || diag2Win;
        if (win)
        {
            BoardHasLine[boardIndex] = true;
        }

        return false;
    }

    protected override void RefreshGameStatus(int row, int col, object? value)
    {
        base.RefreshGameStatus(row, col, value);

        // check if all boards are completed
        if (BoardHasLine.All(b => b))
        {
            // trying to set _isGameOver = true on GameBoard's private variable
            typeof(GameBoard.GameBoard)
                .GetField("_isGameOver", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(this, true);
        }
    }

    protected override void ShowResult()
    {
        var moveHistory = typeof(GameBoard.GameBoard)
            .GetField("_moveHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(this) as List<Move>;

        int loserIndex = moveHistory?.Last().PlayerIndex ?? 0;
        int winnerIndex = (loserIndex == 0) ? 1 : 0;

        WriteLine("All boards are complete!");
        WriteLine("Player {0} loses the game.", loserIndex + 1);
        WriteLine("Player {0} wins!", winnerIndex + 1);
    }

    public override void DisplayHelpMenu()
    {
        WriteLine("\n=== HELP MENU ===");
        WriteLine("{0} Rules:", GameName);
        WriteLine("Two players alternate playing Ｘ in any free cell on any live board,");
        WriteLine("once a board has three-in-a-row it is dead and removed from the game,");
        WriteLine("the player that is forced to complete three-in-a-row on the last live board is the loser.");
        PauseProgramByReadingKeyPress();
    }

    protected override ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
    {
        return new NotaktoComputerPlayer(boardSize, playerNumber);
    }

    protected override HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
    {
        return new NotakoHumanPlayer(boardSize, playerNumber);
    }
}