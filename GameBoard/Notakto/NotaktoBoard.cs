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
    private bool[]? _boardHasLine;

    static NotaktoBoard()
    {
        GameBoardFactory.RegisterGame(() => new NotaktoBoard());
    }

    protected override void SetupGameBoard()
    {
        ColSize = BoardSize;
        RowSize = BoardSize * BoardCount;
        _boardHasLine = new bool[BoardCount];
        base.SetupGameBoard();
    }

    protected override void DisplayBoard(string subject) {
        for (int i = 0; i < BoardCount; i++)
        {
            string status = _boardHasLine[i] ? "[Dead]" : "";
            WriteLine("Board {0}{1}", i + 1, status);
            int rowStart = (i * BoardSize) + 1;
            int rowEnd = rowStart + BoardSize - 1;
            PrintBoard(rowStart, 1, rowEnd, BoardSize);
        }
    }

    public bool IsBoardDead(int row)
    {
        int boardIndex = (row - 1) / BoardSize;
        return _boardHasLine[boardIndex];
    }

    public void SetTempMove(int row, int col, object? value)
    {
        Board[row, col] = value;
    }

    public bool CauseLineDetect(int row, int col, object symbol)
    {
        int boardIndex = (row - 1) / BoardSize;
        int rowStart = boardIndex * BoardSize + 1;
        int rowEnd = rowStart + BoardSize - 1;

        // row checking
        bool rowWin = true;
        for (int i = 1; i <= BoardSize; i++)
        {
            if (Board[row, i]?.ToString() != symbol.ToString())
            {
                rowWin = false;
                break;
            }
        }

        // col checking
        bool colWin = true;
        for (int i = rowStart; i <= rowEnd; i++)
        {
            if (Board[i, col]?.ToString() != symbol.ToString())
            {
                colWin = false;
                break;
            }
        }

        // diagonal (top-left to bottom-right) \
        bool diag1Win = true;
        if ((row - rowStart) == (col - 1))
        {
            for (int i = 0; i < BoardSize; i++)
            {
                if (Board[rowStart + i, i + 1]?.ToString() != symbol.ToString())
                {
                    diag1Win = false;
                    break;
                }
            }
        }
        else diag1Win = false;

        // diagonal (top-right to bottom-left) /
        bool diag2Win = true;
        if ((row - rowStart) == (BoardSize - col))
        {
            for (int i = 0; i < BoardSize; i++)
            {
                if (Board[rowStart + i, BoardSize - i]?.ToString() != symbol.ToString())
                {
                    diag2Win = false;
                    break;
                }
            }
        }
        else diag2Win = false;

        return rowWin || colWin || diag1Win || diag2Win;
    }

    public override bool CheckWin(int row, int col, object? value = null)
    {
        int boardIndex = (row - 1) / BoardSize;
        string symbol = value?.ToString() ?? "X";

        _boardHasLine[boardIndex] = CauseLineDetect(row, col, symbol);

        return false;
    }

    protected override bool IsGameCompleted(bool isPlayerWin)
    {
        bool isGameCompleted = _boardHasLine.All(b => b);

        if (isGameCompleted)
        {
            // the last player who made the line losed
            winnerId = (currentPlayerIndex == 0) ? 1 : 0;   
        }
        
        return isGameCompleted;
    }

    protected override void ShowResult()
    {
        WriteLine("All boards are complete!");
        base.ShowResult();
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
        return new NotaktoHumanPlayer(boardSize, playerNumber);
    }
}