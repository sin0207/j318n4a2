using GameBoard;
namespace Notakto;
public class NotaktoComputerPlayer : ComputerPlayer
{
  public NotaktoComputerPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
  {
    RemainingHoldings = Array.Empty<object>();
  }

  public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
  {
    object symbol = GetValueForNextMove();
    var safeMoves = new List<(int row, int col)>();
    NotaktoBoard notaktoBoard = gameBoard as NotaktoBoard;

    // 1. find safe moves
    for (int row = 1; row <= gameBoard.RowSize; row++)
    {
      if (notaktoBoard.IsBoardDead(row)) continue;

      for (int col = 1; col <= gameBoard.ColSize; col++)
      {
        if (!gameBoard.IsAvailablePosition(row, col)) continue;

        // detecting the move by not actually make a move.
        notaktoBoard.SetTempMove(row, col, symbol);
        bool causesLine = notaktoBoard.CauseLineDetect(row, col, symbol);
        notaktoBoard.SetTempMove(row, col, null); 

        if (!causesLine)
          safeMoves.Add((row, col));
      }
    }

    // 2. randomly pick one safe move
    if (safeMoves.Count > 0)
    {
      var random = new Random();
      var move = safeMoves[random.Next(safeMoves.Count)];
      return (move.row, move.col, symbol);
    }

    // 3. force to make a move due to not having safe moves ahead. 
    for (int row = 1; row <= gameBoard.RowSize; row++)
    {
      if (notaktoBoard.IsBoardDead(row)) continue;

      for (int col = 1; col <= gameBoard.ColSize; col++)
      {
        if (gameBoard.IsAvailablePosition(row, col))
          return (row, col, symbol);
      }
    }

    throw new Exception("No valid move found.");
  }

  protected override object GetValueForNextMove() => 'X';
}
