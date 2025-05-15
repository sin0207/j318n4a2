using System.Text.Json;

namespace TicTacToe;

public class TicTacToeBoard : GameBoard
{
    protected override int PLAYER_COUNT => 2;
    protected override string GAME_RECORD_FILE_NAME => "record.json";
    protected override string GAMEBOARD_NAME => "TicTacToe";
    private int targetNumber;

    // for cache
    private Dictionary<string, int> currentSumMap;
    private Dictionary<string, int> currentFilledCountMap;

    protected override void SetupGameBoard()
    {
        base.SetupGameBoard();

        targetNumber = CalculateTargetNumber();
        InitializeCacheMap();
    }

    protected override void InitializeNewGameBoard()
    {
        RequestUserToInputBoardSize();

        base.InitializeNewGameBoard();
    }

    private void RequestUserToInputBoardSize()
    {
        int boardSize;
        while (true)
        {
            Console.Write("Enter the board size of the board: ");
            if (int.TryParse(Console.ReadLine(), out boardSize) && boardSize > 0)
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }

        Size = boardSize;
    }

    private int CalculateTargetNumber()
    {
        return Size * (Size * Size + 1) / 2;
    }

    private void InitializeCacheMap()
    {
        // using cached map to improve the performance by reducing traverse the entire board every time.
        currentSumMap = new Dictionary<string, int>()
        {
            { "diagonal_tl_to_br", 0 }, // top left to bottom right
            { "diagonal_tr_to_bl", 0 }, // top right to bottom left
        };
        currentFilledCountMap = new Dictionary<string, int>()
        {
            { "diagonal_tl_to_br", 0 }, // top left to bottom right
            { "diagonal_tr_to_bl", 0 }, // top right to bottom left
        };

        for (int i = 1; i <= Size; i++)
        {
            currentSumMap["row_" + i] = 0;
            currentSumMap["col_" + i] = 0;
            currentFilledCountMap["row_" + i] = 0;
            currentFilledCountMap["col_" + i] = 0;
        }
    }

    private int CalculateMaxRowIdentifierWidth()
    {
        int currentWidth = 0;
        int size = Size;
        while (size > 1)
        {
            currentWidth++;
            size /= 10;
        }

        return currentWidth;
    }

    private static string CenterText(string text, int width)
    {
        int padding = (width - text.Length) / 2;
        return text.PadLeft(text.Length + padding).PadRight(width);
    }

    public override void DisplayBoard()
    {
        int maxRowNumberWidth = CalculateMaxRowIdentifierWidth();
        int maxColumnWidth = ConvertNumberToRowIdentifier(Size).Length + 2; // 2 is the left and right space
        string header = "".PadRight(maxRowNumberWidth) + " |";
        for (int i = 1; i <= Size; i++)
        {
            header += CenterText(ConvertToExcelColumn(i), maxColumnWidth) + "|";
        }

        string divider = new string('-', header.Length);

        Console.WriteLine(header);
        // Print row headers and the grid itself
        for (int i = 1; i <= Size; i++)
        {
            Console.WriteLine(divider);
            Console.Write(i.ToString().PadLeft(maxRowNumberWidth) + " |");
            for (int j = 1; j <= Size; j++)
            {
                string output = board[i, j] == NOT_PLACED_FLAG ? "." : board[i, j].ToString();
                Console.Write(CenterText(output, maxColumnWidth) + "|"); // Padding for equal space width
            }

            Console.WriteLine();
        }

        Console.WriteLine(divider);
    }

    // Convert number to Excel-style column (A-Z, AA-AZ, ...)
    private static string ConvertToExcelColumn(int number)
    {
        string result = string.Empty;
        while (number > 0)
        {
            number--; // Adjusting because Excel columns start at 1
            result = (char)('A' + (number % 26)) + result;
            number /= 26;
        }

        return result;
    }

    private string ConvertNumberToRowIdentifier(int number)
    {
        string result = "";
        while (number > 0)
        {
            number--; // Adjusting because the alphabet starts at 1, not 0
            result = (char)('A' + (number % 26)) + result;
            number /= 26;
        }

        return result;
    }

    private void UpdateCacheMaps(int row, int col, object value, string action = "insert")
    {
        int updateValue = (int)value;
        int filledCountUpdate = 1;
        
        if(action == "delete")
        {
            updateValue *= -1;
            filledCountUpdate = -1;
        }
        currentSumMap["row_" + row] += updateValue;
        currentFilledCountMap["row_" + row] += filledCountUpdate;

        currentSumMap["col_" + col] += updateValue;
        currentFilledCountMap["col_" + col] += filledCountUpdate;

        if (row == col)
        {
            currentSumMap["diagonal_tl_to_br"] += updateValue;
            currentFilledCountMap["diagonal_tl_to_br"] += filledCountUpdate;
        }

        if (row + col == Size + 1)
        {
            currentSumMap["diagonal_tr_to_bl"] += updateValue;
            currentFilledCountMap["diagonal_tr_to_bl"] += filledCountUpdate;
        }
    }

    protected override void PrePlace(int row, int col, object value)
    {
        if (GetCurrentPlayer() is ICardHoldingPlayer cardHolder)
        {
            // undo
            if (value == NOT_PLACED_FLAG)
            {
                cardHolder.UnmarkCardAsUsed(board[row, col]);
                UpdateCacheMaps(row, col, board[row, col], action: "delete");
            }
            else // make new move
            {
                cardHolder.MarkCardAsUsed(value);
                UpdateCacheMaps(row, col, value);
            }
        }
    }


    protected override void PostPlace(int row, int col, object value)
    {
        
    }

    public override bool CheckWin(int row, int col, object value = null)
    {
        int number = value == null ? 0 : Convert.ToInt32(value);
        int lineCount = value == null ? 0 : 1;
        
        if ((currentSumMap["row_" + row] + number == targetNumber && currentFilledCountMap["row_" + row] + lineCount == Size)
            || (currentSumMap["col_" + col] + number == targetNumber &&
                currentFilledCountMap["col_" + col] + lineCount == Size))
            return true;

        // point is on the top left to bottom right diagonal line
        if (row == col && currentSumMap["diagonal_tl_to_br"] + number == targetNumber &&
            currentFilledCountMap["diagonal_tl_to_br"] + lineCount == Size)
            return true;

        // point is on the top right to bottom left diagonal line
        if (row + col == Size + 1 && currentSumMap["diagonal_tr_to_bl"] + number == targetNumber &&
            currentFilledCountMap["diagonal_tr_to_bl"] + lineCount == Size)
            return true;

        return false;
    }

    public override void DisplayHelpMenu()
    {
        Console.WriteLine("\n=== HELP MENU ===");
        Console.WriteLine("TicTacToe Rules:");
        Console.WriteLine("1. Each player has cards with numbers on them.");
        Console.WriteLine("2. Players take turns placing a card on the board.");
        Console.WriteLine(
            "3. The goal is to create a row, column, or diagonal where the sum equals the target number.");
        Console.WriteLine("4. The first player to achieve this wins.\n");

        Console.WriteLine("Actions:");
        Console.WriteLine("1. Make a Move: you can choose to make next move.");
        Console.WriteLine("2. Save current game: you can save the current game state and resume later.");
        PauseProgramByReadingKeyPress();
    }

    protected override void DisplayMoreInformationForHumanPlayer()
    {
        Console.WriteLine("\nYour goal is {0}", targetNumber);
    }
}