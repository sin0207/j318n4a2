using System.Text.Json;

namespace GameBoard;

public abstract class GameBoard
{
    // constants for player mode
    protected const int PLAY_WITH_PLAYER_MODE = 1;
    protected const int PLAY_WITH_COMPUTER_MODE = 2;
    
    // constants for gaming option
    protected const int START_NEW_GAME_OPTION = 1;
    protected const int RESUME_PREVIOUS_GAME_OPTION = 2;
    
    // Flags
    protected const object NOT_PLACED_FLAG = null;
    private const int NO_WINNER_FLAG = -1;
    private int winnerId = NO_WINNER_FLAG;
    
    // for finished checking
    protected int remainingPositionCount;
    protected bool isGameOver;
    public bool IsGameOver { get => isGameOver; }
    
    // game board settings
    protected virtual int PLAYER_COUNT { get; }
    public int Size { get; set; }
    protected object[,] board;
    protected int mode;
    protected int currentPlayerIndex;
    protected bool humanPlayFirst;
    protected BasePlayer[] players;
    protected virtual string GAME_RECORD_FILE_NAME { get; }
    protected virtual string GAMEBOARD_NAME { get; }
    
    private List<Move> moveHistory = new List<Move>();
    private int movePointer = -1;
    
    public abstract bool CheckWin(int row, int col, object value = null);
    public abstract void DisplayHelpMenu();
    protected abstract HumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber);
    protected abstract ComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber);

    public GameBoard()
    {
        Console.WriteLine("Welcome to {0}!", GAMEBOARD_NAME);
        int startOption = RequestUserToChooseStartOption();

        if (startOption == START_NEW_GAME_OPTION) // start a new game
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
            Console.WriteLine("{0}. Start new game", START_NEW_GAME_OPTION);
            Console.WriteLine("{0}. Resume previous game", RESUME_PREVIOUS_GAME_OPTION);
            Console.Write("Please choose one of the start options: ");

            int.TryParse(Console.ReadLine(), out startOption);
            if (startOption == START_NEW_GAME_OPTION)
            {
                break;
            }
            else if(startOption == RESUME_PREVIOUS_GAME_OPTION)
            {
                if (File.Exists(GAME_RECORD_FILE_NAME))
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
        remainingPositionCount = Size * Size;
        board = new object[Size + 1, Size + 1];
        isGameOver = false;
        currentPlayerIndex = 0;
    }
    
    protected void ResumePreviousGameBoard()
    {
        string json = File.ReadAllText(GAME_RECORD_FILE_NAME);
        GameState loadedGame = JsonSerializer.Deserialize<GameState>(json);
    
        // assign values to restore the tic tac toe board and players
        Size = loadedGame.BoardSize;
        mode = loadedGame.Mode;
        currentPlayerIndex = loadedGame.CurrentPlayerIndex;
        humanPlayFirst = loadedGame.HumanPlayFirst;

        SetupGameBoard();
        InitializePlayers();
        LoadFromJaggedArray(loadedGame.Board);
        ResumePlayerHoldings(loadedGame.PlayerHoldings);
        ResumeMoveHistory(loadedGame);
    }
    
    private void InitializePlayers()
    {
        int boardSize = Size * Size;
        players = new BasePlayer[PLAYER_COUNT];
        if (humanPlayFirst)
        {
            players[0] = InitializeHumanPlayer(boardSize, 1);    
        }
        else
        {
            players[0] = InitializeComputerPlayer(boardSize, 1);
        }

        if (mode == PLAY_WITH_PLAYER_MODE)
        {
            // player 2 will be human player only when human vs human
            players[1] = InitializeHumanPlayer(boardSize, 2);
        }
        else
        {
            // player 2 could be computer or human player based on the mode and player 1's type
            players[1] = players[0].IsHumanPlayer() ? InitializeComputerPlayer(boardSize, 2) : InitializeHumanPlayer(boardSize, 2);
        }
    }
    
    private void RequestUserToChooseMode()
    {
        int mode;
        while (true)
        {
            Console.WriteLine("Gaming modes: ");
            Console.WriteLine("{0}. Human vs Human", PLAY_WITH_PLAYER_MODE);
            Console.WriteLine("{0}. Human vs Computer", PLAY_WITH_COMPUTER_MODE);
            Console.Write("Please choose one of the gaming modes: ");
            if (int.TryParse(Console.ReadLine(), out mode) && (mode == PLAY_WITH_PLAYER_MODE || mode == PLAY_WITH_COMPUTER_MODE))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }

        this.mode = mode;
    }
    
    private void RequestUserToChoosePlayerOrder()
    {
        if (mode == PLAY_WITH_PLAYER_MODE)
        {
            humanPlayFirst = true;
        }
        else
        {
            string? answer;
            while (true)
            {
                Console.Write("Would you like to play first(Y/N)? ");
                answer = Console.ReadLine();
                if (answer == "Y" || answer == "y")
                {
                    humanPlayFirst = true;
                    break;
                }
                else if (answer == "N" || answer == "n")
                {
                    humanPlayFirst = false;
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
    // hook for actions should be done after placing a new value
    protected virtual void PostPlace(int row, int col, object number) { }

    public void Place(int row, int col, object value)
    {
        PrePlace(row, col, value);
        
        board[row, col] = value;
        RefreshGameStatus(row, col, value);
        // move to next player
        currentPlayerIndex = (currentPlayerIndex + 1) % PLAYER_COUNT;
        
        PostPlace(row, col, board[row, col]);
    }

    public bool IsAvailablePosition(int row, int col)
    {
        if(row < 1 || row > Size || col < 1 || col > Size)
            return false;
        
        return board[row, col] == NOT_PLACED_FLAG;
    }

    private void RefreshGameStatus(int row, int col, object value)
    {
        if (value == null) // for undo
        {
            remainingPositionCount++;
        }
        else // for normal placement or redo
        {
            remainingPositionCount--;   
        }
        
        bool isPlayerWin = CheckWin(row, col);
        if (isPlayerWin) winnerId = currentPlayerIndex;
        
        isGameOver = remainingPositionCount == 0 || isPlayerWin;
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
    public void LoadFromJaggedArray(object[][] loadedBoard)
    {
        for (int i = 0; i < loadedBoard.Length; i++)
        {
            for (int j = 0; j < loadedBoard[0].Length; j++)
            {
                if(loadedBoard[i][j] != NOT_PLACED_FLAG)
                    Place(i, j, ConvertFromJsonElement(loadedBoard[i][j]));
            }
        }
    }
    
    private void ResumePlayerHoldings(Dictionary<int, object[]> playerHolding)
    {
        foreach (var player in players)
        {
            player.RemainingHoldings = playerHolding[player.PlayerNumber].Select(ConvertFromJsonElement).ToArray();
        }
    }

    private void ResumeMoveHistory(GameState loadedGame)
    {
        List<Move> loadedHistory = loadedGame.MoveHistory;
        loadedHistory.ForEach(m => m.Value = ConvertFromJsonElement(m.Value));

        moveHistory = loadedHistory;
        movePointer = loadedGame.MovePointer;
    }

    public BasePlayer GetCurrentPlayer()
    {
        return players[currentPlayerIndex];
    }

    public void SaveGame()
    {
        Dictionary<int, object[]> playerCards = new Dictionary<int, object[]>();
        foreach (BasePlayer p in players)
        {
            playerCards[p.PlayerNumber] = p.RemainingHoldings;
        }
        GameState gameState = new GameState(Size, ConvertToJaggedArray(board), currentPlayerIndex, playerCards, mode, humanPlayFirst, moveHistory, movePointer);
                
        string json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GAME_RECORD_FILE_NAME, json);
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
        if (winnerId == NO_WINNER_FLAG)
        {
            Console.WriteLine("Game over! Draw!");
        }
        else
        {
            BasePlayer winner = players[winnerId];
            if (mode == PLAY_WITH_COMPUTER_MODE && winner.IsHumanPlayer())
            {
                Console.WriteLine("Congratulations! You win!");
            }
            else if(mode == PLAY_WITH_PLAYER_MODE)
            {
                Console.WriteLine("Congratulations! Player {0} win!", winnerId + 1);
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
            if (mode == PLAY_WITH_PLAYER_MODE)
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
        if (movePointer < moveHistory.Count - 1)
        {
            moveHistory.RemoveRange(movePointer + 1, moveHistory.Count - movePointer - 1);
        }
        
        moveHistory.Add(new Move { Row = row, Col = col, Value = value, PlayerIndex = currentPlayerIndex});
        movePointer++;
    }

    public void Undo()
    {
        if (movePointer < 1)
        {
            Console.WriteLine("Nothing to undo.");
            PauseProgramByReadingKeyPress();
        }
        else
        {
            int tmp = currentPlayerIndex;
            for(int i = 0; i < 2; i++)
            {
                var move = moveHistory[movePointer];
                currentPlayerIndex = move.PlayerIndex;
                Place(move.Row, move.Col, NOT_PLACED_FLAG);
                movePointer--;
            }

            currentPlayerIndex = tmp;
        }
    }

    public void Redo()
    {
        if (movePointer + 2 >= moveHistory.Count)
        {
            Console.WriteLine("Nothing to redo.");
            PauseProgramByReadingKeyPress();
        }
        else
        {
            for(int i = 0; i < 2; i++)
            {
                movePointer++;
                var move = moveHistory[movePointer];
                Place(move.Row, move.Col, move.Value);
            }
        }
    }

    protected void PauseProgramByReadingKeyPress()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadLine();
    }
    
    public void DisplayBoard()
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
    
    private static string CenterText(string text, int width)
    {
        int padding = (width - text.Length) / 2;
        return text.PadLeft(text.Length + padding).PadRight(width);
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
}