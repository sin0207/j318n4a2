namespace TicTacToe;

public class GameState
{
    public int BoardSize { get; set; }
    public int[][] Board { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public Dictionary<int, int[]> PlayerHoldings { get; set; } // PlayerNumber -> RemainingCards
    public int Mode { get; set; }
    public bool HumanPlayFirst { get; set; }
    
    public GameState(int boardSize, int[][] board, int currentPlayerIndex, Dictionary<int, int[]> playerHoldings, int mode, bool humanPlayFirst)
    {
        BoardSize = boardSize;
        Board = board;
        CurrentPlayerIndex = currentPlayerIndex;
        PlayerHoldings = playerHoldings;
        Mode = mode;
        HumanPlayFirst = humanPlayFirst;
    }
}