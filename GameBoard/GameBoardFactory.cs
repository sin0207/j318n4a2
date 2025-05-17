namespace GameBoard;

public class GameBoardFactory
{
    private static readonly Dictionary<string, Func<GameBoard>> _registerdBoards = new();
    
    public GameBoard Create(string gameBoard)
    {
        if (_registerdBoards.TryGetValue(gameBoard, out var constructor))
        {
            // create game board
            return constructor();
        }

        throw new ArgumentException($"Game board {gameBoard} does not exist.");
    }

    public static void RegisterGame(Func<GameBoard> creator)
    {
        var tmpInstance = creator();
        _registerdBoards[tmpInstance.GameName] = creator;
    }
    
    public static IEnumerable<string> ListRegisteredGames()
    {
        return _registerdBoards.Keys;
    }
}