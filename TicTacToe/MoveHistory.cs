namespace TicTacToe;

public class MoveHistory
{
    private readonly List<string> moves;
    private int pointer;
    
    public MoveHistory(List<string> loadedMoves)
    {
        moves = new List<string>(loadedMoves);
        pointer = moves.Count - 1;
    }
}