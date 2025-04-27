using System.Text.Json;

namespace TicTacToe;

public class TicTacToeBoard
{
    public const int PLAYER_COUNT = 2;
    
    // constants for player mode
    const int PLAY_WITH_PLAYER_MODE = 1;
    const int PLAY_WITH_COMPUTER_MODE = 2;
    
    // constants for gaming option
    const int START_NEW_GAME_OPTION = 1;
    const int RESUME_PREVIOUS_GAME_OPTION = 2;
    
    const string GAME_RECORD_FILE_NAME = "record.json";
    
    private const int NOT_PLACED_FLAG = 0;
    
    private int targetNumber;
    
    // for cache
    private Dictionary<string, int> currentSumMap;
    private Dictionary<string, int> currentFilledCountMap;
    private int remainingFilledCount;
    
    public int Size { get; set; }
    private bool isGameOver;
    public bool IsGameOver { get => isGameOver; }
    private int[,] board;
    private int mode;
    private int currentPlayerIndex;
    private bool humanPlayFirst;
    private BasePlayer[] players;
    
    public TicTacToeBoard()
    {
        Console.WriteLine("Welcome to Tic-Tac-Toe!");
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

    private void InitializeNewGameBoard()
    {
        RequestUserToInputBoardSize();
        SetupGameBoard();
        RequestUserToChooseMode();
        RequestUserToChoosePlayerOrder();
        InitializePlayers();
    }

    private void SetupGameBoard()
    {
        targetNumber = CalculateTargetNumber();
        remainingFilledCount = Size * Size;
        board = new int[Size + 1, Size + 1];
        isGameOver = false;
        currentPlayerIndex = 0;
        InitializeCacheMap();
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

    private void ResumePreviousGameBoard()
    {
        string json = File.ReadAllText(GAME_RECORD_FILE_NAME);
        GameState loadedGame = JsonSerializer.Deserialize<GameState>(json);
    
        // assign values to restore the tic tac toe board and players
        Size = loadedGame.BoardSize;
        mode = loadedGame.Mode;
        currentPlayerIndex = loadedGame.CurrentPlayerIndex;
        humanPlayFirst = loadedGame.HumanPlayFirst;

        SetupGameBoard();
        LoadFromJaggedArray(loadedGame.Board);
        InitializePlayers();
        ResumePlayerCards(loadedGame.PlayerCards);
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
                string output = board[i, j] == 0 ? "." : board[i, j].ToString();
                Console.Write(CenterText(output, maxColumnWidth) + "|");  // Padding for equal space width
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
            number--;  // Adjusting because Excel columns start at 1
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

    private void UpdateCacheMaps(int row, int col, int number)
    {
        currentSumMap["row_" + row] += number;
        currentFilledCountMap["row_" + row] ++;
        
        currentSumMap["col_" + col] += number;
        currentFilledCountMap["col_" + col] ++;

        if (row == col)
        {
            currentSumMap["diagonal_tl_to_br"] += number;
            currentFilledCountMap["diagonal_tl_to_br"]++;
        }

        if (row + col == Size + 1)
        {
            currentSumMap["diagonal_tr_to_bl"] += number;
            currentFilledCountMap["diagonal_tr_to_bl"]++;
        }
    }

    public void Place(int row, int col, int number)
    {
        board[row, col] = number;
        remainingFilledCount--;
        RefreshGameStatus(row, col, number);
        UpdateCacheMaps(row, col, number);
        currentPlayerIndex = (currentPlayerIndex + 1) % PLAYER_COUNT;
    }

    public bool IsAvailablePosition(int row, int col)
    {
        if(row < 1 || row > Size || col < 1 || col > Size)
            return false;
        
        return board[row, col] == NOT_PLACED_FLAG;
    }

    public bool CheckWin(int row, int col, int number)
    {
        if ((currentSumMap["row_" + row] + number == targetNumber && currentFilledCountMap["row_" + row] + 1 == Size) 
            || (currentSumMap["col_" + col] + number == targetNumber && currentFilledCountMap["col_" + col] + 1 == Size))
            return true;
        
        // point is on the top left to bottom right diagonal line
        if (row == col && currentSumMap["diagonal_tl_to_br"] + number == targetNumber &&
            currentFilledCountMap["diagonal_tl_to_br"] + 1 == Size)
            return true;
        
        // point is on the top right to bottom left diagonal line
        if (row + col == Size + 1 && currentSumMap["diagonal_tr_to_bl"] + number == targetNumber &&
            currentFilledCountMap["diagonal_tr_to_bl"] + 1 == Size)
            return true;

        return false;
    }

    private void RefreshGameStatus(int row, int col, int number)
    {
        isGameOver = remainingFilledCount == 0 || CheckWin(row, col, number);
    }
    
    private int[][] ConvertToJaggedArray(int[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        int[][] jaggedArray = new int[rows][];

        for (int i = 0; i < rows; i++)
        {
            jaggedArray[i] = new int[cols];
            for (int j = 0; j < cols; j++)
            {
                jaggedArray[i][j] = board[i, j];
            }
        }

        return jaggedArray;
    }
    
    public void LoadFromJaggedArray(int[][] loadedBoard)
    {
        for (int i = 0; i < loadedBoard.Length; i++)
        {
            for (int j = 0; j < loadedBoard[0].Length; j++)
            {
                if(loadedBoard[i][j] != 0)
                    Place(i, j, loadedBoard[i][j]);
            }
        }
    }
    
    private void ResumePlayerCards(Dictionary<int, int[]> playerCards)
    {
        foreach (var player in players)
        {
            player.RemainingCards = playerCards[player.PlayerNumber];
        }
    }

    public BasePlayer GetCurrentPlayer()
    {
        return players[currentPlayerIndex];
    }
    
    public void DisplayHelpMenu()
    {
        Console.WriteLine("\n=== HELP MENU ===");
        Console.WriteLine("TicTacToe Rules:");
        Console.WriteLine("1. Each player has cards with numbers on them.");
        Console.WriteLine("2. Players take turns placing a card on the board.");
        Console.WriteLine("3. The goal is to create a row, column, or diagonal where the sum equals the target number.");
        Console.WriteLine("4. The first player to achieve this wins.\n");
    
        Console.WriteLine("Actions:");
        Console.WriteLine("1. Make a Move: you can choose to make next move.");
        Console.WriteLine("2. Save current game: you can save the current game state and resume later.");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadLine();
    }

    public void SaveGame()
    {
        Dictionary<int, int[]> playerCards = new Dictionary<int, int[]>();
        foreach (BasePlayer p in players)
        {
            playerCards[p.PlayerNumber] = p.RemainingCards;
        }
        GameState gameState = new GameState(Size, ConvertToJaggedArray(board), currentPlayerIndex, playerCards, mode, humanPlayFirst);
                
        string json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GAME_RECORD_FILE_NAME, json);
        Console.WriteLine("Game saved successfully!");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadLine();
    }
    
    public void HandleGameOver(bool isPlayerWon)
    {
        ShowResult(isPlayerWon);
        Console.WriteLine("The final board is:");
        DisplayBoard();
    }

    private void ShowResult(bool isPlayerWon)
    {
        if (isPlayerWon)
        {
            BasePlayer winner = GetCurrentPlayer();
            if (winner.IsHumanPlayer())
            {
                Console.WriteLine("Congratulations! You win!");
            }
            else
            {
                Console.WriteLine("Sorry, you lose!");
            }
        }
        else
        {
            Console.WriteLine("Game over! Draw!");
        }
    }
    
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
            Console.WriteLine("\nYour goal is {0}", targetNumber);
        }
        else
        {
            Console.WriteLine("Computer's turn: ");
        }
    
        Console.WriteLine("The current game board is:");
        DisplayBoard();
    }
}