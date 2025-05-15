// See https://aka.ms/new-console-template for more information

using TicTacToe;

// constants for operation options
const int MAKE_MOVE_OPERATION_OPTION = 1;
const int SAVE_GAME_OPERATION_OPTION = 2;
const int VIEW_MANU_OPERATION_OPTION = 3;

GameBoardFactory gameBoardFactory = new GameBoardFactory();
string userChosenGame = RequestUserToChooseGameBoard();
GameBoard gameBoard = gameBoardFactory.Create(userChosenGame);
BasePlayer player;

while (!gameBoard.IsGameOver)
{
    player = gameBoard.GetCurrentPlayer();
    gameBoard.DisplayCurrentInformation();

    if (player.IsHumanPlayer())
    {
        int option = RequestUserToChooseNextOperation();
        switch (option)
        {
            case MAKE_MOVE_OPERATION_OPTION:
                MakeMove(player, gameBoard);
                break;
            case SAVE_GAME_OPERATION_OPTION:
                gameBoard.SaveGame();
                MakeMove(player, gameBoard);
                break;
            case VIEW_MANU_OPERATION_OPTION:
                gameBoard.DisplayHelpMenu();
                break;
        }
    }
    else
    {
        MakeMove(player, gameBoard);
    }
    Console.WriteLine();
}

gameBoard.HandleGameOver();

string RequestUserToChooseGameBoard()
{
    var indexToKeyMap = GameBoardFactory.GameBoardMap.Keys
        .Select((key, index) => new { Index = index + 1, Key = key })
        .ToDictionary(x => x.Index, x => x.Key);

    while (true)
    {
        Console.WriteLine("Please choose a game board to play: ");
        foreach (var item in indexToKeyMap)
        {
            Console.WriteLine($"{item.Key}. {item.Value}");
        }
        
        Console.Write("Please enter the number of the game you want to play: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && indexToKeyMap.TryGetValue(choice, out string selectedKey))
        {
            // the key for creating game board
            return selectedKey;
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }
}

int RequestUserToChooseNextOperation()
{
    int operationOption;
    while (true)
    {
        Console.WriteLine("Operations: ");
        Console.WriteLine("{0}. Make a Move", MAKE_MOVE_OPERATION_OPTION);
        Console.WriteLine("{0}. Save current game", SAVE_GAME_OPERATION_OPTION);
        Console.WriteLine("{0}. View help menu", VIEW_MANU_OPERATION_OPTION);
        Console.Write("Please choose one of the operations: ");
        if (int.TryParse(Console.ReadLine(), out operationOption) && (operationOption == MAKE_MOVE_OPERATION_OPTION || operationOption == SAVE_GAME_OPERATION_OPTION || operationOption == VIEW_MANU_OPERATION_OPTION ))
        {
            break;
        }
        else
        {
            Console.WriteLine("Invalid input. Try again.");
            Console.WriteLine("");
        }
    }
    
    return operationOption;
}

void MakeMove(BasePlayer player, GameBoard gameBoard)
{
    (int row, int col, object chosenCard) = player.GetNextMove(gameBoard);
    gameBoard.Place(row, col, chosenCard);
}