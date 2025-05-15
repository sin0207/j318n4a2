using System.Text.Json;

namespace TicTacToe;

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
    protected int remainingFilledCount;
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
    
    public abstract void DisplayBoard();
    public abstract bool CheckWin(int row, int col, object value);
    public abstract void DisplayHelpMenu();

    public GameBoard()
    {
        Console.WriteLine("Welcome to {0}!", GAMEBOARD_NAME);
        int startOption = RequestUserToChooseStartOption();

        if (startOption == START_NEW_GAME_OPTION) // start a new game
        {
            InitializeNewGameBoard();
        }
        else
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
        remainingFilledCount = Size * Size;
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
        moveHistory = loadedGame.MoveHistory;
        movePointer = loadedGame.MovePointer;

        SetupGameBoard();
        InitializePlayers();
        LoadFromJaggedArray(loadedGame.Board);
        ResumePlayerHoldings(loadedGame.PlayerHoldings);
    }
    
    private void InitializePlayers()
    {
        int boardSize = Size * Size;
        players = new BasePlayer[PLAYER_COUNT];
        if (humanPlayFirst)
        {
            players[0] = new HumanPlayer(boardSize, 1);    
        }
        else
        {
            players[0] = new ComputerPlayer(boardSize, 1);
        }

        if (mode == PLAY_WITH_PLAYER_MODE)
        {
            // player 2 will be human player only when human vs human
            players[1] = new HumanPlayer(boardSize, 2);
        }
        else
        {
            players[1] = players[0].IsHumanPlayer() ? new ComputerPlayer(boardSize, 2) : new HumanPlayer(boardSize, 2);
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

    protected virtual void PrePlace(int row, int col, object number) { }
    protected virtual void PostPlace(int row, int col, object number) { }

    public void Place(int row, int col, object value)
    {
        PrePlace(row, col, value);
        
        board[row, col] = value;
        RefreshGameStatus(row, col, value);
        currentPlayerIndex = (currentPlayerIndex + 1) % PLAYER_COUNT;
        AppendMove(row, col, value);
        
        PostPlace(row, col, value);
    }

    public bool IsAvailablePosition(int row, int col)
    {
        if(row < 1 || row > Size || col < 1 || col > Size)
            return false;
        
        return board[row, col] == NOT_PLACED_FLAG;
    }

    private void RefreshGameStatus(int row, int col, object value)
    {
        remainingFilledCount--;
        bool isPlayerWin = CheckWin(row, col, value);
        if (isPlayerWin) winnerId = currentPlayerIndex;
        
        isGameOver = remainingFilledCount == 0 || isPlayerWin;
    }
    
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

    private void AppendMove(int row, int col, object value)
    {
        // should clear outdated history when make a new move
        if (movePointer < moveHistory.Count - 1)
        {
            moveHistory.RemoveRange(movePointer + 1, moveHistory.Count - movePointer - 1);
        }
        
        moveHistory.Add(new Move { Row = row, Col = col, Value = value });
        movePointer++;
    }

    public void Undo()
    {
        if (movePointer < 0)
        {
            Console.WriteLine("Nothing to undo.");
        }
        else
        {
            var move = moveHistory[movePointer];
            board[move.Row, move.Col] = NOT_PLACED_FLAG;
            movePointer--;
        }
    }

    public void Redo()
    {
        if (movePointer + 1 >= moveHistory.Count)
        {
            Console.WriteLine("Nothing to redo.");
        }
        else
        {
            movePointer++;
            var move = moveHistory[movePointer];
            Place(move.Row, move.Col, move.Value);
        }
    }
}