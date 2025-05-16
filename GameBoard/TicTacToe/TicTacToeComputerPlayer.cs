using GameBoard;

namespace TicTacToe;

public class TicTacToeComputerPlayer : ComputerPlayer, ICardHoldingPlayer
{
    public TicTacToeComputerPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
    {
        RemainingHoldings = InitializeCards(boardSize);
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
        RemainingHoldings = RemainingHoldings.Where(val => !val.Equals(value)).ToArray();
    }

    public void UnmarkCardAsUsed(object value)
    {
        RemainingHoldings = RemainingHoldings.Append(value).ToArray();
    }
}