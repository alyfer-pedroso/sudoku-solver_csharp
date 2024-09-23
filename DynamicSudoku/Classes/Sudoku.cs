using System;
using System.Collections.Generic;

namespace DynamicSudoku.Classes
{
    internal class Sudoku
    {
        public int Size { get; private set; }
        public int Base { get; private set; }
        public int[,] Board { get => Board; private set => value = Board; } // two-dimensional multidimensional array
        public int[,] Solution { get => Solution; private set => value = Solution; } // two-dimensional multidimensional array
        public bool MemorizedSolution { get; set; }

        public Random random = new Random();

        public Sudoku(int size = 9)
        {
            Size = size;
            Base = (int)Math.Sqrt(size);
            if (Base * Base != Size)
            {
                throw new ArgumentException("O tamanho da grade deve ser um quadrado perfeito.");
            }
            Board = new int[Size, Size];
            Solution = new int[Size, Size];
            MemorizedSolution = false;
        }

        public void Generate()
        {
            FillBoard();
            CopyBoard(Solution, Board);
            RemoveNumbers();
        }

        private void FillBoard()
        {
            // initialize numbers
            List<int> nums = new List<int>();
            for (int i = 1; i <= Size; i++) nums.Add(i);
            Shuffle(nums);

            // fill the diagonal subgrids
            for (int i = 0;i < Size; i += Base) FillSubgrid(i, i, new List<int>(nums));

            // solve the board to get the solution
            if (!Solve(Board)) throw new Exception("Falha ao gerar Sudoku válido");
        }

        private void FillSubgrid(int row, int col, List<int> nums)
        {
            int idx = 0;
            for (int r = 0; r < Base; r++)
            {
                for (int c = 0; c < Base; r++)
                {
                    Board[row + r, col + c] = nums[idx++];
                }
            }
        }

        private void Shuffle(List<int> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = list[j];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private bool Solve(int[,] board)
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        for (int num = 1; num <= Size; num++)
                        {
                            if (IsSafe(board, row, col, num))
                            {
                                board[row, col] = num;
                                if (Solve(board))
                                    return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsSafe(int[,] board, int row, int col, int num)
        {
             // check row and column
             for (int i = 0; i < Size; i++)
             {
                if (board[row, i] == num || board[i, col] == num) return false;
             }

            // check subgrid
            int startRow = row - row % Base;
            int startCol = col - col % Base;
             for (int r = 0; r < Base; r++)
             {
                for (int c = 0; c < Base; c++)
                {
                    if (board[startRow + r, startCol + c] == num) return false;
                }
             }

            return true;
        }

        private void CopyBoard(int[,] destination, int[,] source)
        {
            for (int row = 0; row < Size; row++)
                for (int col = 0; col < Size; col++)
                    destination[row, col] = source[row, col];
        }

        private void RemoveNumbers()
        {
            int squares = Size * Size;
            int empties = squares * 3 / 4;

            while(empties > 0)
            {
                int row = random.Next(Size);
                int col = random.Next(Size);
                if (Board[row, col] != 0)
                {
                    Board[row, col] = 0;
                    empties--;
                }
            }
        }

        public bool Validate()
        {
            // check rows and columns
            for (int row = 0; row < Size; row++)
            {
                HashSet<int> rowSet = new HashSet<int>();
                HashSet<int> colSet = new HashSet<int>();
                for(int col = 0; col < Size; col++)
                {
                    if (Board[row, col] != 0)
                    {
                        if (rowSet.Contains(Board[row, col]))
                            return false;
                        rowSet.Add(Board[row, col]);
                    }

                    if (Board[col, row] != 0)
                    {
                        if (colSet.Contains(Board[col, row]))
                            return false;
                        colSet.Add(Board[col, row]);
                    }
                }
            }

            // check subgrids
            for (int row = 0; row < Size; row += Base)
            {
                for (int col = 0; col < Size; col += Base)
                {
                    HashSet<int> set = new HashSet<int>();
                    for (int r = 0; r < Base; r++)
                    {
                        for (int c = 0; c < Base; c++)
                        {
                            int num = Board[row + r, col + c];
                            if (num != 0)
                            {
                                if (set.Contains(num))
                                    return false;
                                set.Add(num);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool SolveCurrent()
        {
            int[,] tempBoard = new int[Size, Size];
            CopyBoard(tempBoard, MemorizedSolution ? Solution : Board);
            if (Solve(tempBoard))
            {
                if (MemorizedSolution)
                    CopyBoard(Board, Solution);
                else
                    CopyBoard(Board, tempBoard);
                return true;
            }
            return false;
        }

        public void Display()
        {
            string horizontalLine = new string('-', Size * 4 + 1);
            for (int row = 0; row < Size; row++)
            {
                if (row % Base == 0)
                    Console.WriteLine(horizontalLine);

                for (int col = 0; col < Size; col++)
                {
                    if (col % Base == 0)
                        Console.Write("| ");

                    if (Board[row, col] == 0)
                        Console.Write(". ");
                    else
                        Console.Write($"{Board[row, col]} ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine(horizontalLine);
        }

        public void SetCell(int row, int col, int value)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                throw new ArgumentException("Posição fora do tabuleiro.");

            if (Solution[row, col] != 0)
                throw new InvalidOperationException("Não é possível alterar uma célula pré-preenchida.");

            if (value < 0 || value > Size)
                throw new ArgumentException($"O valor deve estar entre 1 e {Size}.");

            Board[row, col] = value;
        }
    }
}
