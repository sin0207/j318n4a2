using GameBoard;

namespace TicTacToe;

public class TicTacToeCardHolderInteractionStrategy : ICardHolderInteraction
{
    public object[] InitializeCards(int totalCardNumber, bool isFirstPlayer)
    {
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

    public object[] MarkCardAsUsed(object[] remainingCards, object value)
    {
        return remainingCards.Where(val => !val.Equals(value)).ToArray();
    }

    public object[] UnmarkCardAsUsed(object[] remainingCards, object value)
    {
        return remainingCards.Append(value).ToArray();
    }
}