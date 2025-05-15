namespace TicTacToe;

public abstract class BasePlayer
{
    public int PlayerNumber { get; }
    public object[] RemainingHoldings { get; set; }
    
    protected BasePlayer(int boardSize, int playerNumber)
    {
        PlayerNumber = playerNumber;
    }
    
    public abstract bool IsHumanPlayer();
    
    public abstract (int, int, object) GetNextMove(GameBoard gameboard);
}