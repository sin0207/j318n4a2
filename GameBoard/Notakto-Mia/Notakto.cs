using GameBoard;
using System.Text.Json;

namespace Notakto;


    public class NotaktoBoard : GameBoard.GameBoard
    {
        static NotaktoBoard() => GameBoardFactory.RegisterGame(() => new NotaktoBoard());

        public const int BoardCount = 3;
        private readonly object[][,] boards = new object[BoardCount][,];
        protected readonly bool[] dead = new bool[BoardCount];
        private int deadCount;

        protected override int PlayerCount => 2;
        public override string GameName => "Notakto";
        protected override string GameRecordFileName => "notakto_record.json";

        public NotaktoBoard() => Size = 3;

        protected override void SetupGameBoard()
        {
            base.SetupGameBoard();
            for (int b = 0; b < BoardCount; b++)
            {
                boards[b] = new object[Size + 1, Size + 1];
                for (int i = 1; i <= Size; i++)
                    for (int j = 1; j <= Size; j++)
                        boards[b][i, j] = NotPlacedFlag;
                dead[b] = false;
            }
            deadCount = 0;
        }

        public new void Place(int encodedRow, int col, object value)
        {
            int b = (encodedRow / 100) - 1;
            int r = encodedRow % 100;
            boards[b][r, col] = value;
            base.Place(r, col, value);
            // If completed a line, kill that board
            if (!dead[b] && IsKilled(b, r, col))
            {
                dead[b] = true; deadCount++;
                Console.WriteLine($"Board {b + 1}  is now dead.");
            }
            if (deadCount == BoardCount) base.HandleGameOver();
        }

        private bool IsKilled(int b, int r, int c)
            => Enumerable.Range(1, Size).All(j => boards[b][r, j] != NotPlacedFlag)
            || Enumerable.Range(1, Size).All(i => boards[b][i, c] != NotPlacedFlag)
            || (r == c && Enumerable.Range(1, Size).All(i => boards[b][i, i] != NotPlacedFlag))
            || (r + c == Size + 1 && Enumerable.Range(1, Size).All(i => boards[b][i, Size + 1 - i] != NotPlacedFlag));

        public override bool CheckWin(int row, int col, object? v = null)
            => deadCount == BoardCount;

    public override void DisplayHelpMenu()
    {
        Console.WriteLine("Notakto Rules:");
        Console.WriteLine("1. Place X on any board.");
        Console.WriteLine("2. Kill boards by completing lines.");
        Console.WriteLine("3. Last mover loses when all dead.");
    
            PauseProgramByReadingKeyPress();
        }

        protected override HumanPlayer    InitializeHumanPlayer(int bs, int pn)
            => new NotaktoHumanPlayer(bs, pn);

        protected override ComputerPlayer InitializeComputerPlayer(int bs, int pn)
            => new NotaktoComputerPlayer(bs, pn);

        // Expose for human‐player
        public bool IsBoardDead(int idx) => dead[idx];
    }

  