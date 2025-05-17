using GameBoard;

namespace Gomoku;

public class GomokuComputerPlayer : ComputerPlayer
{
    public GomokuComputerPlayer(int boardSize, int playerNumber, char playerSymbol) : base(boardSize, playerNumber)
    {
        RemainingHoldings = [playerSymbol];
    }
    
    protected override object GetValueForNextMove()
    {
        return RemainingHoldings[0];
    }
}