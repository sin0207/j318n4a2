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
    
    public object[] InitializeCards(int totalCardNumber)
    {
        bool isFirstPlayer = (PlayerNumber == 1);
        
        // The first player has one more card compared to the second player if totalCardNumber is odd
        int cardSize = (totalCardNumber / 2) + (isFirstPlayer && IsOdd(totalCardNumber) ? 1 : 0);
        object[] cards = new object[cardSize];

        int index = 0;
        for (int i = 1; i <= totalCardNumber; i++)
        {
            if (IsOdd(i) == isFirstPlayer)
            {
                cards[index] = i;
                index++;
            }
        }
        
        return cards;
    }

    protected bool IsOdd(int number)
    {
        return number % 2 != 0;
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