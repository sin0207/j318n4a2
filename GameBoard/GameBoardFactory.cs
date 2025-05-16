using TicTacToe;

namespace GameBoard;

public class GameBoardFactory
{
    public static readonly Dictionary<string, Func<GameBoard>> GameBoardMap = new Dictionary<string, Func<GameBoard>>
    {
        { "TicTacToe", () => new TicTacToeBoard() }
    };
    
    public GameBoard Create(string gameBoard)
    {
        if (GameBoardMap.TryGetValue(gameBoard, out var constructor))
        {
            // create game board
            return constructor();
        }

        throw new ArgumentException($"Game board {gameBoard} does not exist.");
    }
}