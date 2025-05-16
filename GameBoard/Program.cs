// See https://aka.ms/new-console-template for more information

using GameBoard;

// constants for operation options
const string MAKE_MOVE_OPERATION_OPTION = "make-move";
const string UNDO_MOVE_OPERATION_OPTION = "undo";
const string REDO_MOVE_OPERATION_OPTION = "redo";
const string SAVE_GAME_OPERATION_OPTION = "save-game";
const string VIEW_MANU_OPERATION_OPTION = "view-help-manu";
Dictionary<string, string> AllowedOperationMap = new Dictionary<string, string>
{
    { MAKE_MOVE_OPERATION_OPTION, "Make a move" },
    { UNDO_MOVE_OPERATION_OPTION, "Undo move" },
    { REDO_MOVE_OPERATION_OPTION, "Redo move" },
    { SAVE_GAME_OPERATION_OPTION, "Save current game" },
    { VIEW_MANU_OPERATION_OPTION, "View help menu" },
};

GameBoardFactory gameBoardFactory = new GameBoardFactory();
string userChosenGame = RequestUserToChooseGameBoard();

Console.WriteLine(); // divider

GameBoard.GameBoard gameBoard = gameBoardFactory.Create(userChosenGame);
BasePlayer player;

while (!gameBoard.IsGameOver)
{
    player = gameBoard.GetCurrentPlayer();
    gameBoard.DisplayCurrentInformation();

    if (player.IsHumanPlayer())
    {
        string option = RequestUserToChooseNextOperation();
        switch (option)
        {
            case MAKE_MOVE_OPERATION_OPTION:
                MakeMove(player, gameBoard);
                break;
            case UNDO_MOVE_OPERATION_OPTION:
                gameBoard.Undo();
                break;
            case REDO_MOVE_OPERATION_OPTION:
                gameBoard.Redo();
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

Dictionary<int, T> ToIndexedDictionary<T>(IEnumerable<T> source, int startIndex = 1)
{
    return source
        .Select((item, index) => new { Index = index + startIndex, Value = item })
        .ToDictionary(x => x.Index, x => x.Value);
}

int RequestUserToChooseFromList(Dictionary<int, string> indexToKeyMap, string subject, string question)
{
    while (true)
    {
        Console.WriteLine(subject);
        foreach (var item in indexToKeyMap)
        {
            Console.WriteLine($"{item.Key}. {item.Value}");
        }
        
        Console.Write(question);
        if (int.TryParse(Console.ReadLine(), out int choice) && indexToKeyMap.ContainsKey(choice))
        {
            // return selected valid option
            return choice;
        }
        else
        {
            Console.WriteLine("Invalid selection.");
            Console.WriteLine();
        }
    }
}

string RequestUserToChooseGameBoard()
{
    var indexToKeyMap = ToIndexedDictionary(GameBoardFactory.GameBoardMap.Keys);
    int selectedIndex = RequestUserToChooseFromList(indexToKeyMap, "Please choose a game board to play: ", "Please enter the number of the game you want to play: ");
    
    return indexToKeyMap[selectedIndex];
}

string RequestUserToChooseNextOperation()
{
    // int operationOption;
    var indexToKeyMap = ToIndexedDictionary(AllowedOperationMap.Values);
    int selectedIndex = RequestUserToChooseFromList(indexToKeyMap, "Operations: ", "Please choose one of the operations: ");
    
    string selectedLabel =  indexToKeyMap[selectedIndex];
    
    return AllowedOperationMap
        .FirstOrDefault(pair => pair.Value == selectedLabel).Key;
}

void MakeMove(BasePlayer player, GameBoard.GameBoard gameBoard)
{
    (int row, int col, object chosenCard) = player.GetNextMove(gameBoard);
    gameBoard.AppendMove(row, col, chosenCard);
    gameBoard.Place(row, col, chosenCard);
}