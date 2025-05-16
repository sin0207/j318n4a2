namespace GameBoard;

public class Move
{
    public int Row { get; set; }
    public int Col { get; set; }
    public object Value { get; set; }
    public int PlayerIndex { get; set; }
}