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
    public const int winningCount = 5;
    public const int minEdge = 0;
    public const int size = 15;

    static GomokuBoard()
    {
        GameBoardFactory.RegisterGame(() => new GomokuBoard());
    }
    
    protected override void SetupGameBoard()
    {
        RowSize = 15;
        ColSize = 15;

        base.SetupGameBoard();
    }

    public override bool CheckWin(int row, int col, object value = null)
    {
        if (value == null)
        {
            value = Board[row, col];
            if(value == NotPlacedFlag) { return false; }
        }

        return CheckLine(row, col, 0, 1, value) || //Horizontal line
               CheckLine(row, col, 1, 0, value) || //Vertical line
               CheckLine(row, col, 1, 1, value) || //Diagonal line(\)
               CheckLine(row, col, 1, -1, value);  //Diagonal line(/)
    }

    public bool CheckLine(int row, int col, int rowDiff, int colDiff, object value)
    {
            int count = 1;
            int positive = CheckDirection(row, col, rowDiff, colDiff, value);   //Positive direction
            int negative = CheckDirection(row, col, -rowDiff, -colDiff, value); //Negative direction

            count = count + positive + negative;

            return count >= 5; 
    }

    public int CheckDirection(int row, int col, int rowDiff, int colDiff, object value)
    {
            int count = 0;

            for (int distance = 1; distance < winningCount; distance++)
            {
                int currentRow = row + (distance * rowDiff);
                int currentCol = col + (distance * colDiff);

                if(currentRow<minEdge || currentRow >= size || currentCol < minEdge || currentCol >= size || 
                    Board[currentRow, currentCol] == null || !Board[currentRow, currentCol].Equals(value))
                {
                    break;
                }
                
                count++;
            }

            return count;
    }

    public override void DisplayHelpMenu()
    {
        Console.WriteLine("\n=== HELP MENU ===");
        Console.WriteLine("{0} Rules:", GameName);
        Console.WriteLine("1. Each player has stone.");
        Console.WriteLine("2. Players take turns placing a stone on the board.");
        Console.WriteLine(
            "3. The goal is to get an unbroken line of five stones in a row, column or diagonal.");
        Console.WriteLine("4. The first player to achieve this wins.\n");
        
        Console.WriteLine("Actions:");
        Console.WriteLine("1. Make a Move: you can choose to make next move.");
        Console.WriteLine("2. Undo move: you can undo your previous move from current game board.");
        Console.WriteLine("3. Redo move: you can redo your previous undo move from current game board.");
        Console.WriteLine("4. Save current game: you can save the current game state and resume later.");
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