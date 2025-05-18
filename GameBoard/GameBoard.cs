using System.Text.Json;

namespace GameBoard;

public abstract class GameBoard
{
    // constants for player mode
    private const int PlayWithPlayerMode = 1;
    private const int PlayWithComputerMode = 2;
    private const int AutoMode = 3;
    
    // constants for gaming option
    private const int StartNewGameOption = 1;
    private const int ResumePreviousGameOption = 2;
    
    // Flags
    protected const object NotPlacedFlag = null;
    private const int NoWinnerFlag = -1;
    private int _winnerId = NoWinnerFlag;
    
    // for finished checking
    private int _remainingPositionCount;
    private bool _isGameOver;
    public bool IsGameOver { get => _isGameOver; }
    
    // game board settings
    protected virtual int PlayerCount { get; }
    public int Size { get; protected set; }
    protected object[,] Board;
    private int _mode;
    private int _currentPlayerIndex;
    private bool _humanPlayFirst;
    private BasePlayer[] _players;
    protected virtual string GameRecordFileName { get; }
    public virtual string GameName => "Generic Game Board";

    // for move history
    private List<Move> _moveHistory = new ();
    private int _movePointer = -1;
    
    public abstract bool CheckWin(int row, int col, object? value = null);
    public abstract void DisplayHelpMenu();
    protected abstract HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber);
    protected abstract ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber);
    
    protected GameBoard() { }

    public void Start()
    {
        Console.WriteLine("Welcome to {0}!", GameName);
        int startOption = RequestUserToChooseStartOption();

        if (startOption == StartNewGameOption) // start a new game
        {
            InitializeNewGameBoard();
        }
        else // resume previous game
        {
            ResumePreviousGameBoard();
        }        
    }
    
    private int RequestUserToChooseStartOption()
    {
        int startOption;
        
        while (true)
        {
            Console.WriteLine("Start options: ");
            Console.WriteLine("{0}. Start new game", StartNewGameOption);
            Console.WriteLine("{0}. Resume previous game", ResumePreviousGameOption);
            Console.Write("Please choose one of the start options: ");

            int.TryParse(Console.ReadLine(), out startOption);
            if (startOption == StartNewGameOption)
            {
                break;
            }
            else if(startOption == ResumePreviousGameOption)
            {
                if (File.Exists(GameRecordFileName))
                {
                    break;
                }
                else 
                {
                    // try to resume a previous game, but the file does not exist, then request user to start a new game
                    Console.WriteLine("Previous game record file does not exist! Please start a new game!\n");   
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please try again.\n");
            }
        }

        return startOption;
    }

    protected virtual void InitializeNewGameBoard()
    {
        SetupGameBoard();
        RequestUserToChooseMode();
        RequestUserToChoosePlayerOrder();
        InitializePlayers();
    }

    protected virtual void SetupGameBoard()
    {
        _remainingPositionCount = Size * Size;
        Board = new object[Size + 1, Size + 1];
        _isGameOver = false;
        _currentPlayerIndex = 0;
    }
    
    private void ResumePreviousGameBoard()
    {
        string json = File.ReadAllText(GameRecordFileName);
        GameState loadedGame = JsonSerializer.Deserialize<GameState>(json);
    
        // assign values to restore the tic-tac-toe board and players
        Size = loadedGame.BoardSize;
        _mode = loadedGame.Mode;
        _currentPlayerIndex = loadedGame.CurrentPlayerIndex;
        _humanPlayFirst = loadedGame.HumanPlayFirst;

        SetupGameBoard();
        InitializePlayers();
        LoadFromJaggedArray(loadedGame.Board);
        ResumePlayerHoldings(loadedGame.PlayerHoldings);
        ResumeMoveHistory(loadedGame);
    }
    
    private void InitializePlayers()
    {
        int boardSize = Size * Size;
        _players = new BasePlayer[PlayerCount];
        if (_humanPlayFirst)
        {
            _players[0] = InitializeHumanPlayer(boardSize, 1);    
        }
        else
        {
            _players[0] = InitializeComputerPlayer(boardSize, 1);
        }

        if (_mode == PlayWithPlayerMode)
        {
            // player 2 will be human player only when human vs human
            _players[1] = InitializeHumanPlayer(boardSize, 2);
        }
        else if (_mode == AutoMode)
        {
            _players[1] = InitializeComputerPlayer(boardSize, 2);
        }
        else
        {
            // player 2 could be computer or human player based on the mode and player 1's type
            _players[1] = _players[0].IsHumanPlayer() ? InitializeComputerPlayer(boardSize, 2) : InitializeHumanPlayer(boardSize, 2);
        }
    }
    
    private void RequestUserToChooseMode()
    {
        int mode;
        while (true)
        {
            Console.WriteLine("Gaming modes: ");
            Console.WriteLine("{0}. Human vs Human", PlayWithPlayerMode);
            Console.WriteLine("{0}. Human vs Computer", PlayWithComputerMode);
            Console.WriteLine("{0}. Computer vs Computer", AutoMode);
            Console.Write("Please choose one of the gaming modes: ");
            if (int.TryParse(Console.ReadLine(), out mode) && (mode == PlayWithPlayerMode || mode == PlayWithComputerMode || mode == AutoMode))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }

        _mode = mode;
    }
    
    private void RequestUserToChoosePlayerOrder()
    {
        if (_mode == PlayWithPlayerMode)
        {
            _humanPlayFirst = true;
        }
        else if(_mode == PlayWithComputerMode)
        {
            while (true)
            {
                Console.Write("Would you like to play first(Y/N)? ");
                string? answer = Console.ReadLine();
                if (answer.ToUpper() == "Y")
                {
                    _humanPlayFirst = true;
                    break;
                }
                else if (answer.ToUpper() == "N")
                {
                    _humanPlayFirst = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Try again.");
                }
            };
        }
    }

    // hook for actions should be done before placing a new value
    protected virtual void PrePlace(int row, int col, object number) { }

    public void Place(int row, int col, object value)
    {
        PrePlace(row, col, value);
        
        Board[row, col] = value;
        RefreshGameStatus(row, col, value);
        // move to next player
        _currentPlayerIndex = (_currentPlayerIndex + 1) % PlayerCount;
    }

    public bool IsAvailablePosition(int row, int col)
    {
        if(row < 1 || row > Size || col < 1 || col > Size)
            return false;
        
        return Board[row, col] == NotPlacedFlag;
    }

    private void RefreshGameStatus(int row, int col, object? value)
    {
        if (value == null) // for undo
        {
            _remainingPositionCount++;
        }
        else // for normal placement or redo
        {
            _remainingPositionCount--;   
        }
        
        bool isPlayerWin = CheckWin(row, col);
        if (isPlayerWin) _winnerId = _currentPlayerIndex;
        
        _isGameOver = _remainingPositionCount == 0 || isPlayerWin;
    }
    
    // covert board to jagged array for saving game
    private object[][] ConvertToJaggedArray(object[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        object[][] jaggedArray = new object[rows][];

        for (int i = 0; i < rows; i++)
        {
            jaggedArray[i] = new object[cols];
            for (int j = 0; j < cols; j++)
            {
                jaggedArray[i][j] = board[i, j];
            }
        }

        return jaggedArray;
    }
    
    // convert loaded object element to the type it should be
    private object ConvertFromJsonElement(object value)
    {
        if (value is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Number:
                    return jsonElement.GetInt32();
                case JsonValueKind.String:
                    return jsonElement.GetString();
                default:
                    throw new InvalidCastException($"Cannot convert '{value}' to a number");
            }
        }

        return value;
    }
    
    // covert loaded jagged array for board
    private void LoadFromJaggedArray(object[][] loadedBoard)
    {
        for (int i = 0; i < loadedBoard.Length; i++)
        {
            for (int j = 0; j < loadedBoard[0].Length; j++)
            {
                if(loadedBoard[i][j] != NotPlacedFlag)
                    Place(i, j, ConvertFromJsonElement(loadedBoard[i][j]));
            }
        }
    }
    
    private void ResumePlayerHoldings(Dictionary<int, object[]> playerHolding)
    {
        foreach (BasePlayer player in _players)
        {
            player.RemainingHoldings = playerHolding[player.PlayerNumber].Select(ConvertFromJsonElement).ToArray();
        }
    }

    private void ResumeMoveHistory(GameState loadedGame)
    {
        List<Move> loadedHistory = loadedGame.MoveHistory;
        loadedHistory.ForEach(m => m.Value = ConvertFromJsonElement(m.Value));

        _moveHistory = loadedHistory;
        _movePointer = loadedGame.MovePointer;
    }

    public BasePlayer GetCurrentPlayer()
    {
        return _players[_currentPlayerIndex];
    }

    public void SaveGame()
    {
        Dictionary<int, object[]> playerHoldings = new Dictionary<int, object[]>();
        foreach (BasePlayer p in _players)
        {
            playerHoldings[p.PlayerNumber] = p.RemainingHoldings;
        }
        GameState gameState = new GameState(Size, ConvertToJaggedArray(Board), _currentPlayerIndex, playerHoldings, _mode, _humanPlayFirst, _moveHistory, _movePointer);
                
        string json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GameRecordFileName, json);
        Console.WriteLine("Game saved successfully!");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadLine();
    }
    
    public void HandleGameOver()
    {
        ShowResult();
        Console.WriteLine("The final board is:");
        DisplayBoard();
    }

    private void ShowResult()
    {
        if (_winnerId == NoWinnerFlag)
        {
            Console.WriteLine("Game over! Draw!");
        }
        else
        {
            BasePlayer winner = _players[_winnerId];
            if (_mode == PlayWithComputerMode && winner.IsHumanPlayer())
            {
                Console.WriteLine("Congratulations! You win!");
            }
            else if(_mode == PlayWithPlayerMode)
            {
                Console.WriteLine("Congratulations! Player {0} win!", _winnerId + 1);
            }
            else
            {
                Console.WriteLine("Sorry, you lose!");
            }
        }
    }

    // hook for displaying more information while displaying the board to user
    protected virtual void DisplayMoreInformationForHumanPlayer() { }

    public void DisplayCurrentInformation()
    {
        BasePlayer player = GetCurrentPlayer();
        if (player.IsHumanPlayer())
        {
            if (_mode == PlayWithPlayerMode)
            {
                Console.WriteLine("Player {0}'s turn: ", player.PlayerNumber);   
            }
            else
            {
                Console.WriteLine("Player 1's turn: ");
            }

            DisplayMoreInformationForHumanPlayer();
        }
        else
        {
            Console.WriteLine("Computer's turn: ");
        }
    
        Console.WriteLine("The current game board is:");
        DisplayBoard();
    }

    public void AppendMove(int row, int col, object value)
    {
        // should clear outdated history when make a new move
        if (_movePointer < _moveHistory.Count - 1)
        {
            _moveHistory.RemoveRange(_movePointer + 1, _moveHistory.Count - _movePointer - 1);
        }
        
        _moveHistory.Add(new Move { Row = row, Col = col, Value = value, PlayerIndex = _currentPlayerIndex});
        _movePointer++;
    }

    public void Undo()
    {
        // shouldn't undo until all players already made at least one move
        if (_movePointer < PlayerCount - 1)
        {
            Console.WriteLine("Nothing to undo.");
            PauseProgramByReadingKeyPress();
        }
        else
        {
            // save current player
            int tmp = _currentPlayerIndex;
            
            // undo all the moves in the previous round(all players)
            for(int i = 0; i < PlayerCount; i++)
            {
                var move = _moveHistory[_movePointer];
                _currentPlayerIndex = move.PlayerIndex;
                Place(move.Row, move.Col, NotPlacedFlag);
                _movePointer--;
            }

            // restore current user 
            _currentPlayerIndex = tmp;
        }
    }

    public void Redo()
    {
        // shouldn't redo when the pointer is already pointing the last position
        if (_movePointer + PlayerCount >= _moveHistory.Count)
        {
            Console.WriteLine("Nothing to redo.");
            PauseProgramByReadingKeyPress();
        }
        else
        {
            // redo all the moves in the next round from state of current game board
            for(int i = 0; i < PlayerCount; i++)
            {
                _movePointer++;
                var move = _moveHistory[_movePointer];
                Place(move.Row, move.Col, move.Value);
            }
        }
    }

    protected void PauseProgramByReadingKeyPress()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadLine();
    }
    
    private void DisplayBoard()
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
                string output = Board[i, j] == NotPlacedFlag ? "." : Board[i, j].ToString();
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
    
    private static string CenterText(string text, int width)
    {
        int padding = (width - text.Length) / 2;
        return text.PadLeft(text.Length + padding).PadRight(width);
    }
    
    private int CalculateMaxRowIdentifierWidth()
    {
        int currentWidth = 1;
        int size = Size;
        while (size > 1)
        {
            currentWidth++;
            size /= 10;
        }

        return currentWidth;
    }
}