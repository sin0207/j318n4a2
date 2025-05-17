using System.Text.Json;
using GameBoard;

namespace TicTacToe;

public class TicTacToeBoard : GameBoard.GameBoard
{
    protected override int PlayerCount => 2;
    protected override string GameRecordFileName => "record.json";
    public override string GameName => "TicTacToe";
    private int _targetNumber;

    // cache some value for better performance
    private Dictionary<string, int> _currentSumMap;
    private Dictionary<string, int> _currentFilledCountMap;

    static TicTacToeBoard()
    {
        GameBoardFactory.RegisterGame(() => new TicTacToeBoard());
    }

    protected override void SetupGameBoard()
    {
        base.SetupGameBoard();

        _targetNumber = CalculateTargetNumber();
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
        _currentSumMap = new Dictionary<string, int>()
        {
            { "diagonal_tl_to_br", 0 }, // top left to bottom right
            { "diagonal_tr_to_bl", 0 }, // top right to bottom left
        };
        _currentFilledCountMap = new Dictionary<string, int>()
        {
            { "diagonal_tl_to_br", 0 }, // top left to bottom right
            { "diagonal_tr_to_bl", 0 }, // top right to bottom left
        };

        for (int i = 1; i <= Size; i++)
        {
            _currentSumMap["row_" + i] = 0;
            _currentSumMap["col_" + i] = 0;
            _currentFilledCountMap["row_" + i] = 0;
            _currentFilledCountMap["col_" + i] = 0;
        }
    }

    private void UpdateCacheMaps(int row, int col, object value, string action = "insert")
    {
        int updateValue = (int)value;
        int filledCountUpdate = 1;
        
        // when undo, we have to add the value back to the cache map
        if(action == "delete")
        {
            updateValue *= -1;
            filledCountUpdate = -1;
        }
        _currentSumMap["row_" + row] += updateValue;
        _currentFilledCountMap["row_" + row] += filledCountUpdate;

        _currentSumMap["col_" + col] += updateValue;
        _currentFilledCountMap["col_" + col] += filledCountUpdate;

        if (row == col)
        {
            _currentSumMap["diagonal_tl_to_br"] += updateValue;
            _currentFilledCountMap["diagonal_tl_to_br"] += filledCountUpdate;
        }

        if (row + col == Size + 1)
        {
            _currentSumMap["diagonal_tr_to_bl"] += updateValue;
            _currentFilledCountMap["diagonal_tr_to_bl"] += filledCountUpdate;
        }
    }

    protected override void PrePlace(int row, int col, object value)
    {
        if (GetCurrentPlayer() is ICardHoldingPlayer cardHolder)
        {
            // undo
            if (value == NotPlacedFlag)
            {
                cardHolder.UnmarkCardAsUsed(Board[row, col]);
                UpdateCacheMaps(row, col, Board[row, col], action: "delete");
            }
            else // make new move
            {
                cardHolder.MarkCardAsUsed(value);
                UpdateCacheMaps(row, col, value);
            }
        }
    }

    public override bool CheckWin(int row, int col, object value = null)
    {
        int number = value == null ? 0 : Convert.ToInt32(value);
        int lineCount = value == null ? 0 : 1;
        
        // when the row or col reaches the target number and all positions are filled
        if ((_currentSumMap["row_" + row] + number == _targetNumber && _currentFilledCountMap["row_" + row] + lineCount == Size)
            || (_currentSumMap["col_" + col] + number == _targetNumber &&
                _currentFilledCountMap["col_" + col] + lineCount == Size))
            return true;

        // point is on the top left to bottom right diagonal line
        if (row == col && _currentSumMap["diagonal_tl_to_br"] + number == _targetNumber &&
            _currentFilledCountMap["diagonal_tl_to_br"] + lineCount == Size)
            return true;

        // point is on the top right to bottom left diagonal line
        if (row + col == Size + 1 && _currentSumMap["diagonal_tr_to_bl"] + number == _targetNumber &&
            _currentFilledCountMap["diagonal_tr_to_bl"] + lineCount == Size)
            return true;

        return false;
    }

    public override void DisplayHelpMenu()
    {
        Console.WriteLine("\n=== HELP MENU ===");
        Console.WriteLine("{0} Rules:", GameName);
        Console.WriteLine("1. Each player has cards with numbers on them.");
        Console.WriteLine("2. Players take turns placing a card on the board.");
        Console.WriteLine(
            "3. The goal is to create a row, column, or diagonal where the sum equals the target number.");
        Console.WriteLine("4. The first player to achieve this wins.\n");

        Console.WriteLine("Actions:");
        Console.WriteLine("1. Make a Move: you can choose to make next move.");
        Console.WriteLine("2. Undo move: you can undo your previous move from current game board.");
        Console.WriteLine("3. Redo move: you can redo your previous undo move from current game board.");
        Console.WriteLine("4. Save current game: you can save the current game state and resume later.");
        PauseProgramByReadingKeyPress();
    }

    protected override void DisplayMoreInformationForHumanPlayer()
    {
        Console.WriteLine("\nYour goal is {0}", _targetNumber);
    }

    protected override TicTacTocHumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
    {
        return new TicTacTocHumanPlayer(boardSize, playerNumber, new TicTacToeCardHolderInteractionStrategy());
    }
    
    protected override TicTacToeComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
    {
        return new TicTacToeComputerPlayer(boardSize, playerNumber, new TicTacToeCardHolderInteractionStrategy());
    }
}