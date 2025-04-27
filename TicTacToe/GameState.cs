namespace TicTacToe;

public class GameState
{
    public int BoardSize { get; set; }
    public int[][] Board { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public Dictionary<int, int[]> PlayerCards { get; set; } // PlayerNumber -> RemainingCards
    public int Mode { get; set; }
    public bool HumanPlayFirst { get; set; }
    
    public GameState(int boardSize, int[][] board, int currentPlayerIndex, Dictionary<int, int[]> playerCards, int mode, bool humanPlayFirst)
    {
        BoardSize = boardSize;
        Board = board;
        CurrentPlayerIndex = currentPlayerIndex;
        PlayerCards = playerCards;
        Mode = mode;
        HumanPlayFirst = humanPlayFirst;
    }
}