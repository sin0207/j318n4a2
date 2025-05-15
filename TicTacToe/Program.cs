// See https://aka.ms/new-console-template for more information

using TicTacToe;

// constants for operation options
const int MAKE_MOVE_OPERATION_OPTION = 1;
const int SAVE_GAME_OPERATION_OPTION = 2;
const int VIEW_MANU_OPERATION_OPTION = 3;

TicTacToeBoard ticTacToeBoard = new TicTacToeBoard();
BasePlayer player;

while (!ticTacToeBoard.IsGameOver)
{
    player = ticTacToeBoard.GetCurrentPlayer();
    ticTacToeBoard.DisplayCurrentInformation();

    if (player.IsHumanPlayer())
    {
        int option = RequestUserToChooseNextOperation();
        switch (option)
        {
            case MAKE_MOVE_OPERATION_OPTION:
                MakeMove(player, ticTacToeBoard);
                break;
            case SAVE_GAME_OPERATION_OPTION:
                ticTacToeBoard.SaveGame();
                MakeMove(player, ticTacToeBoard);
                break;
            case VIEW_MANU_OPERATION_OPTION:
                ticTacToeBoard.DisplayHelpMenu();
                break;
        }
    }
    else
    {
        MakeMove(player, ticTacToeBoard);
    }
    Console.WriteLine();
}

ticTacToeBoard.HandleGameOver();

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
        }
    }
    
    return operationOption;
}

void MakeMove(BasePlayer player, TicTacToeBoard ticTacToeBoard)
{
    (int row, int col, int chosenCard) = player.GetNextMove(ticTacToeBoard);
    ticTacToeBoard.Place(row, col, chosenCard);
}