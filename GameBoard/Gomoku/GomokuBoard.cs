using System.Text.Json;
using GameBoard;

namespace Gomoku;

public class GomokuBoard : GameBoard.GameBoard
{
    protected override int PlayerCount => 2;
    protected override string GameRecordFileName => "gomoku-record.json";
    protected override string GameBoardName => "Gomoku";
    private const char FirstPlayerSymbol = 'o';
    private const char SecondPlayerSymbol = 'x';

    protected override void SetupGameBoard()
    {
        Size = 15;

        base.SetupGameBoard();
    }
    public override bool CheckWin(int row, int col, object value = null)
    {
        return false;
    }

    public override void DisplayHelpMenu()
    {
        Console.WriteLine("\n=== HELP MENU ===");
        Console.WriteLine("Gomoku Command");
        
        PauseProgramByReadingKeyPress();
    }

    private char PickSymbol(int playerNumber)
    {
        return playerNumber == 1 ? FirstPlayerSymbol : SecondPlayerSymbol;
    }
    protected override GomokuHumanPlayer InitializeHumanPlayer(int boardSize, int playerNumber)
    {
        return new GomokuHumanPlayer(boardSize, playerNumber, PickSymbol(playerNumber));
    }
    
    protected override GomokuComputerPlayer InitializeComputerPlayer(int boardSize, int playerNumber)
    {
        return new GomokuComputerPlayer(boardSize, playerNumber, PickSymbol(playerNumber));
    }
}