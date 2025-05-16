namespace GameBoard;

public abstract class ComputerPlayer : HumanPlayer
{
    public ComputerPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
    {
        
    }
    
    public override bool IsHumanPlayer()
    {
        return false;
    }
    
    public override (int, int, object) GetNextMove(GameBoard gameBoard)
    {
        List<(int, int)> availablePositions = new List<(int, int)>();
        
        // if there's position where is able to win now, then return it.
        for (int i = 1; i <= gameBoard.Size; i++)
        {
            for (int j = 1; j <= gameBoard.Size; j++)
            {
                if (gameBoard.IsAvailablePosition(i, j))
                {
                    object? winningCard = FindWinningValue(i, j, gameBoard);
                    if (winningCard == null)
                    {
                        availablePositions.Add((i, j));
                    }
                    else
                    {
                        // return the position and value can let computer player to win
                        return (i, j, winningCard);
                    }
                }
            }
        }

        // pick a position and value randomly
        (int nextPositionRow, int nextPositionColumn) = availablePositions[PickIndexRandomly(availablePositions.Count)];

        return (nextPositionRow, nextPositionColumn, GetValueForNextMove());
    }
    
    protected int PickIndexRandomly(int length)
    {
        Random random = new Random();
        
        return random.Next(0, length);
    }

    private object? FindWinningValue(int row, int col, GameBoard gameBoard)
    {
        foreach(var value in RemainingHoldings)
        {
            // if there's any position can let computer player win, then select it.
            if (gameBoard.CheckWin(row, col, value))
            {
                return value;
            }
        }

        return null;
    }
}