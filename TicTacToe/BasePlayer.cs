namespace TicTacToe;

public abstract class BasePlayer
{
    protected Dictionary<int, bool> usedCardMap = new Dictionary<int, bool>();
    
    public int PlayerNumber { get; protected set; }
    public int[] RemainingCards { get; set; }
    
    protected BasePlayer(int boardSize, int playerNumber)
    {
        PlayerNumber = playerNumber;
        RemainingCards = InitializeCards(boardSize);
    }
    
    protected int[] InitializeCards(int totalCardNumber)
    {
        bool isFirstPlayer = (PlayerNumber == 1);
        
        // The first player has one more card compared to the second player if totalCardNumber is odd
        int cardSize = (totalCardNumber / 2) + (isFirstPlayer && IsOdd(totalCardNumber) ? 1 : 0);
        int[] cards = new int[cardSize];

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

    public void MarkCardAsUsed(int number)
    {
        usedCardMap[number] = true;
        RemainingCards = RemainingCards.Where(val => val != number).ToArray();
    }
    
    public abstract bool IsHumanPlayer();
    
    public abstract (int, int, int) GetNextMove(TicTacToeBoard ticTacToeBoard);
}