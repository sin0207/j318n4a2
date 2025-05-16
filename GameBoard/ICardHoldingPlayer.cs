namespace GameBoard;

public interface ICardHoldingPlayer
{
    void MarkCardAsUsed(object value);
    void UnmarkCardAsUsed(object value);
}