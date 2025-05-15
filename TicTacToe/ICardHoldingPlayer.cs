namespace TicTacToe;

public interface ICardHoldingPlayer
{
    void MarkCardAsUsed(object value);
    void UnmarkCardAsUsed(object value);
}