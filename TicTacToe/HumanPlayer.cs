using System.Text.RegularExpressions;

namespace TicTacToe;

public class HumanPlayer : BasePlayer
{
    public HumanPlayer(int boardSize, int playerNumber) : base(boardSize, playerNumber)
    {
    }

    public override bool IsHumanPlayer()
    {
        return true;
    }

    public override (int, int, int) GetNextMove(TicTacToeBoard ticTacToeBoard)
    {
        int chosenCard = RequestUserToChooseACard();
        (int row, int col) = RequestUserToChoosePositions(ticTacToeBoard);

        return (row, col, chosenCard);
    }
    
    private int RequestUserToChooseACard()
    {
        int chosenCard;
        while (true)
        {
            Console.Write("Please choose one of the following cards({0}): ", String.Join(", ", RemainingCards));
            if (int.TryParse(Console.ReadLine(), out chosenCard) && RemainingCards.Contains(chosenCard))
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }

        return chosenCard;
    }

    private (int, int) RequestUserToChoosePositions(TicTacToeBoard ticTacToeBoard)
    {
        int row, col;
        while (true)
        {
            Console.Write("Please choose which row do you like to place in format(e.g. A1): ");
            try
            {
                Match match = Regex.Match(Console.ReadLine(), @"^([A-Za-z]+)(\d+)$");
                if (!match.Success)
                    throw new ArgumentException("Invalid cell format");

                string colLetters = match.Groups[1].Value.ToUpper();
                row = int.Parse(match.Groups[2].Value);

                col = 0;
                foreach (char c in colLetters)
                {
                    col = col * 26 + (c - 'A' + 1);
                }
                
                if (ticTacToeBoard.IsAvailablePosition(row, col))
                    break;
                
                Console.WriteLine("The chosen position is not available, please try again.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }
    
        return (row, col);
    }
}