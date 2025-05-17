using GameBoard;

namespace TicTacToe;

public class TicTacToeComputerPlayer : ComputerPlayer, ICardHoldingPlayer
{
    // for card holder interactions
    // due to the c# constraint, so we can't inherit multiple classes at the same time
    private readonly ICardHolderInteraction _interaction;
    
    public TicTacToeComputerPlayer(int boardSize, int playerNumber, ICardHolderInteraction interaction) : base(boardSize, playerNumber)
    {
        _interaction = interaction;
        RemainingHoldings = _interaction.InitializeCards(boardSize, playerNumber == 1);
    }

    public void MarkCardAsUsed(object value)
    {
        RemainingHoldings = _interaction.MarkCardAsUsed(RemainingHoldings, value);
    }

    public void UnmarkCardAsUsed(object value)
    {
        RemainingHoldings = _interaction.UnmarkCardAsUsed(RemainingHoldings, value);
    }
    
    protected override object GetValueForNextMove()
    {
        return RemainingHoldings[PickIndexRandomly(RemainingHoldings.Length)];
    }
}