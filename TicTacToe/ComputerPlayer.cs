namespace TicTacToe;

public class ComputerPlayer : HumanPlayer
{
    public ComputerPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
    {
        
    }
    
    public override bool IsHumanPlayer()
    {
        return false;
    }
    
    public override (int, int, int) GetNextMove(TicTacToeBoard ticTacToeBoard)
    {
        List<(int, int)> availablePositions = new List<(int, int)>();
        
        // if there's position where is able to win now, then return it.
        for (int i = 1; i <= ticTacToeBoard.Size; i++)
        {
            for (int j = 1; j <= ticTacToeBoard.Size; j++)
            {
                if (ticTacToeBoard.IsAvailablePosition(i, j))
                {
                    int? winningCard = FindWinningCard(i, j, ticTacToeBoard);
                    if (winningCard.HasValue)
                    {
                        return (i, j, winningCard.Value);
                    }
                    else
                    {
                        availablePositions.Add((i, j));
                    }
                }
            }
        }

        // pick a position and value randomly
        (int nextPositionRow, int nextPositionColumn) = availablePositions[PickIndexRandomly(availablePositions.Count)];
        return (nextPositionRow, nextPositionColumn, RemainingCards[PickIndexRandomly(RemainingCards.Length)]);
    }

    private int PickIndexRandomly(int length)
    {
        Random random = new Random();
        
        return random.Next(0, length);
    }

    private int? FindWinningCard(int row, int col, TicTacToeBoard ticTacToeBoard)
    {
        foreach(int number in RemainingCards)
        {
            // if there's any position can let computer player win, then select it.
            if (ticTacToeBoard.CheckWin(row, col, number))
            {
                return number;
            }
        }

        return null;
    }
}