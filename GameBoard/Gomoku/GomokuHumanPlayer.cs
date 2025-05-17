using GameBoard;

namespace Gomoku;

public class GomokuHumanPlayer : HumanPlayer
{
    public GomokuHumanPlayer(int boardSize, int playerNumber, char playerSymbol) : base(boardSize, playerNumber)
    {
        RemainingHoldings = [playerSymbol];
    }
    
    protected override object GetValueForNextMove()
    {
        return RemainingHoldings[0];
    }
}