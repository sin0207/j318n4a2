using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using GameBoard;

namespace Gomoku;

public class GomokuBoard : GameBoard.GameBoard
{
    protected override int PlayerCount => 2;
    protected override string GameRecordFileName => "gomoku-record.json";
    public override string GameName => "Gomoku";
    private const char FirstPlayerSymbol = 'o';
    private const char SecondPlayerSymbol = 'x';

    static GomokuBoard()
    {
        GameBoardFactory.RegisterGame(() => new GomokuBoard());
    }
    
    protected override void SetupGameBoard()
    {
        Size = 15;

        base.SetupGameBoard();
    }

    public override bool CheckWin(int row, int col, object value = null)
    {
        const int minEdge = 0;
        const int winningCount = 5;
        if (value == null)
        {
            value = Board[row, col];
            if (value == null) { return false; }
        }
        //Horizontal line
        int count = 1;
        for (int distance = 1; distance < winningCount; distance++)
        {
            if (col + distance < Size && Board[row, col + distance] != null && Board[row, col + distance].Equals(value)) { count++; }
            else { break; }
        }
        for (int distance =1; distance <winningCount; distance++)
        {
            if (col - distance >= minEdge && Board[row, col - distance] != null && Board[row, col - distance].Equals(value)) { count++; }
            else { break; }
        }
        if (count >= winningCount) { return true; }
        //Vertical line
        count = 1;
        for (int distance = 1; distance < winningCount; distance++)
        {
            if (row + distance < Size && Board[row + distance, col] != null && Board[row + distance, col].Equals(value)) { count++; }
            else { break; }
        }
        for (int distance = 1; distance < winningCount; distance++)
        {
            if (row - distance >= minEdge && Board[row - distance, col] != null && Board[row - distance, col].Equals(value)) { count++; }
            else { break; }
        }
        if (count >= winningCount) { return true; }
        //Diagonally(\)
        count = 1;
        for (int distance = 1; distance < winningCount; distance++)
        {
            if (row + distance <Size && col + distance < Size && Board[row + distance, col + distance] != null && Board[row +distance, col + distance].Equals(value)) { count++; }
            else { break; }
        }
        for(int distance = 1; distance < winningCount; distance++)
        {
            if (row - distance >= minEdge && col - distance >= minEdge && Board[row - distance, col - distance] != null && Board[row - distance, col - distance].Equals(value)) { count++; }
            else { break; }
        }
        if (count >= winningCount) { return true; }
        //Diagonally(/)
        count = 1;
        for(int distance = 1; distance < winningCount; distance++)
        {
            if (row + distance < Size && col - distance >= minEdge && Board[row + distance, col - distance] != null && Board[row + distance, col - distance].Equals(value)) { count++; }
            else { break; }
        }
        for (int distance = 1; distance < winningCount; distance++)
        {
            if (row - distance >= minEdge && col + distance < Size && Board[row - distance, col + distance] != null && Board[row - distance, col + distance].Equals(value)) { count++; }
            else { break; }
        }
        if (count >= winningCount) { return true; }
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