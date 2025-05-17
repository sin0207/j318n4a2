namespace GameBoard;

public class MoveHistory
{
    private readonly List<string> moves;
    
    public MoveHistory(List<string> loadedMoves)
    {
        moves = new List<string>(loadedMoves);
    }
}