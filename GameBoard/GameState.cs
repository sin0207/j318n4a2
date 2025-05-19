namespace GameBoard;

public class GameState
{
    public int RowSize { get; set; }
    public int ColSize { get; set; }
    public object[][] Board { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public Dictionary<int, object[]> PlayerHoldings { get; set; } // PlayerNumber -> RemainingCards
    public int Mode { get; set; }
    public bool HumanPlayFirst { get; set; }
    public List<Move> MoveHistory { get; set; }
    public int MovePointer { get; set; }
    
    public GameState(int rowSize, int colSize, object[][] board, int currentPlayerIndex, Dictionary<int, object[]> playerHoldings, int mode, bool humanPlayFirst, List<Move> moveHistory, int movePointer)
    {
        RowSize = rowSize;
        ColSize = colSize;
        Board = board;
        CurrentPlayerIndex = currentPlayerIndex;
        PlayerHoldings = playerHoldings;
        Mode = mode;
        HumanPlayFirst = humanPlayFirst;
        MoveHistory = moveHistory;
        MovePointer = movePointer;
    }
}