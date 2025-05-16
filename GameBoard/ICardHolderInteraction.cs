namespace GameBoard;

public interface ICardHolderInteraction
{
    object[] InitializeCards(int boardSize, bool isFirstPlayer);
    object[] MarkCardAsUsed(object[] remainingCards, object value);
    object[] UnmarkCardAsUsed(object[] remainingCards, object value);
}
