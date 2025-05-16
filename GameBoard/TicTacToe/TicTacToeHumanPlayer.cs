using GameBoard;

namespace TicTacToe;

public class TicTacTocHumanPlayer : HumanPlayer, ICardHoldingPlayer
{
    // for card holder interactions
    // due to the c# constraint, so we can't inherit multiple classes at the same time
    private readonly ICardHolderInteraction _interaction;
    
    public TicTacTocHumanPlayer(int boardSize, int playerNumber, ICardHolderInteraction interaction) : base(boardSize, playerNumber)
    {
        _interaction = interaction;
        RemainingHoldings = _interaction.InitializeCards(boardSize, playerNumber == 1);
    }

    public override (int, int, object) GetNextMove(GameBoard.GameBoard gameBoard)
    {
        object chosenCard = GetValueForNextMove();
        (int row, int col) = RequestUserToChoosePositions(gameBoard);

        return (row, col, chosenCard);
    }
    
    protected override object GetValueForNextMove()
    {
        string chosenCard;
        while (true)
        {
            Console.Write("Please choose one of the following cards({0}): ", String.Join(", ", RemainingHoldings));
            chosenCard = Console.ReadLine();
            if (RemainingHoldings.Any(c => c.ToString() == chosenCard))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }

        return int.Parse(chosenCard);
    }

    public void MarkCardAsUsed(object value)
    {
        RemainingHoldings = _interaction.MarkCardAsUsed(RemainingHoldings, value);
    }

    public void UnmarkCardAsUsed(object value)
    {
        RemainingHoldings = _interaction.UnmarkCardAsUsed(RemainingHoldings, value);
    }
}