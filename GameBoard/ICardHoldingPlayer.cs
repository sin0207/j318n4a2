namespace GameBoard;

public interface ICardHoldingPlayer
{
    object[] InitializeCards(int totalCardNumber);
    void MarkCardAsUsed(object value);
    void UnmarkCardAsUsed(object value);
}