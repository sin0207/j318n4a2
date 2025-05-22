using GameBoard;

namespace Notakto
{
  public class NotakoHumanPlayer : HumanPlayer
  {
    public NotakoHumanPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
    { 
      // setting this for saving purpose since the game does not utilize this variable
      RemainingHoldings = Array.Empty<object>();
    }
    protected override object GetValueForNextMove() => 'X';
  }
}
