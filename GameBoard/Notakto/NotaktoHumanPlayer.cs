using GameBoard;

namespace Notakto
{
  public class NotakoHumanPlayer : HumanPlayer
  {
    private const char Symbol = 'X';
    public NotakoHumanPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber) { }
    protected override object GetValueForNextMove() => Symbol;
  }
}
