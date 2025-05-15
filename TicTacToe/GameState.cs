namespace TicTacToe;

public class GameState
{
    public int BoardSize { get; set; }
    public object[][] Board { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public Dictionary<int, object[]> PlayerHoldings { get; set; } // PlayerNumber -> RemainingCards
    public int Mode { get; set; }
    public bool HumanPlayFirst { get; set; }
    public List<Move> MoveHistory { get; set; }
    public int MovePointer { get; set; }
    
    public GameState(int boardSize, object[][] board, int currentPlayerIndex, Dictionary<int, object[]> playerHoldings, int mode, bool humanPlayFirst, List<Move> moveHistory, int movePointer)
    {
        BoardSize = boardSize;
        Board = board;
        CurrentPlayerIndex = currentPlayerIndex;
        PlayerHoldings = playerHoldings;
        Mode = mode;
        HumanPlayFirst = humanPlayFirst;
        MoveHistory = moveHistory;
        MovePointer = movePointer;
    }
}