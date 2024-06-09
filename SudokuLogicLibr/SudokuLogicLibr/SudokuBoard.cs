using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SudokuLogicLibr.SudokuLogic;

namespace SudokuLogicLibr.SudokuLogic
{
    public class SudokuCannotBeSolvedException : Exception { }
    public class SudokuBoard
    {
        #region UNDERLINE_DEF

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        #endregion

        private Square[,] board;
        public SudokuBoard(int[,] matrix)
        {
            board = new Square[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] == 0)
                        board[i, j] = new Square();
                    else
                        board[i, j] = new Square(matrix[i, j]);
                }
            }
        }

        public int[,] Solve()
        {
            if (!isSolveable())
                throw new SudokuCannotBeSolvedException();

            //Console.WriteLine(this.ToString());
            /*for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (!board[i, j].IsGuessed)
                        board[i, j].MakeGuess(CollectGuessed(i, j));*/

            bool step1 = true;
            while (step1)
            {
                step1 = false;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (!board[i, j].IsGuessed)
                        {
                            board[i, j].MakeGuess(CollectGuessed(i, j));

                            if (board[i, j].CanBeGuessed)
                            {
                                board[i, j].ExecuteGuess();
                                step1 = true;
                            }
                        }
                        //collect all the board squares with 1 possible guess to resolve and maybe arrange the other ones for easyness
                    }
                }
                //Console.WriteLine(this.ToString());
            }

            //at this stage there should not be any cell with just one guess

            //iterate via vertical + horizontal + square and check for each structure if there are unique guesses

            //while (!this.IsSolved())
            for (int z = 0; z < 100; z++)
            {
                //implement UpdateGuess() to square_guesses - collected_guesses (instead of a whole set)
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        if (!board[i, j].IsGuessed)
                            board[i, j].MakeGuess(CollectGuessed(i, j));

                //each row
                for (int i = 0; i < 9; i++)
                {
                    int[] possible_arr = new int[9];
                    for (int j = 0; j < 9; j++)
                        possible_arr[j] = -1;

                    for (int j = 0; j < 9; j++)
                    {
                        if (!board[i, j].IsGuessed)
                        {
                            int[] actual_guesses = board[i, j].Guess!.ToArray();

                            foreach (int gueses in actual_guesses)
                            {
                                int g = gueses - 1;
                                if (possible_arr[g] == -1)
                                    possible_arr[g] = j;
                                else
                                    possible_arr[g] = -2;
                            }
                        }
                    }

                    for (int j = 0; j < 9; j++)
                    {
                        if (possible_arr[j] >= 0)
                        {
                            board[i, possible_arr[j]].ExecuteGuess(j + 1);
                        }
                    }
                }

                //refresh
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        if (!board[i, j].IsGuessed)
                            board[i, j].MakeGuess(CollectGuessed(i, j));

                //each column
                for (int i = 0; i < 9; i++)
                {
                    int[] possible_arr = new int[9];
                    for (int j = 0; j < 9; j++)
                        possible_arr[j] = -1;

                    for (int j = 0; j < 9; j++)
                    {
                        if (!board[j, i].IsGuessed)
                        {
                            int[] actual_guesses = board[j, i].Guess!.ToArray();
                            foreach (int gueses in actual_guesses)
                            {
                                int g = gueses - 1;
                                if (possible_arr[g] == -1)
                                    possible_arr[g] = j;
                                else
                                    possible_arr[g] = -2;
                            }
                        }
                    }

                    for (int j = 0; j < 9; j++)
                    {
                        if (possible_arr[j] >= 0)
                        {
                            board[possible_arr[j], i].ExecuteGuess(j + 1);
                        }
                    }
                }

                //refresh
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        if (!board[i, j].IsGuessed)
                            board[i, j].MakeGuess(CollectGuessed(i, j));

                //each square
                for (int i = 0; i < 9; i += 3)
                {
                    for (int j = 0; j < 9; j += 3)
                    {
                        (int, int)[] possible_arr = new (int, int)[9];
                        for (int l = 0; l < 9; l++)
                            possible_arr[l].Item1 = -1;

                        for (int k = i; k < i + 3; k++)
                        {
                            for (int l = j; l < j + 3; l++)
                            {
                                if (!board[k, l].IsGuessed)
                                {
                                    int[] actual_guesses = board[k, l].Guess!.ToArray();
                                    foreach (int gueses in actual_guesses)
                                    {
                                        int g = gueses - 1;
                                        if (possible_arr[g].Item1 == -1)
                                        {
                                            possible_arr[g].Item1 = k;
                                            possible_arr[g].Item2 = l;
                                        }

                                        else
                                            possible_arr[g].Item1 = -2;
                                    }
                                }
                            }
                        }

                        for (int k = 0; k < 9; k++)
                        {
                            if (possible_arr[k].Item1 >= 0)
                            {
                                board[possible_arr[k].Item1, possible_arr[k].Item2].ExecuteGuess(k + 1);
                            }
                        }
                    }
                }

            }

            //Square[,] tempBoard = CloneBoard(board);
            //board = MakeRecursiveGuess(board);
            bool canBeSolved = MakeRecursiveGuess(board);

            if (!canBeSolved)
                throw new SudokuCannotBeSolvedException();

            //Console.WriteLine(this);
            TransformTempToNum(ref board);
            //Console.WriteLine(PrintBoard(board));

            //to delete
            /*int[] checker = new int[9];
            for (int i = 0; i < 9; i++)
                checker[i] = 0;*/

            return ToIntArr();
        }

        private Square[,] CloneBoard(Square[,] source)
        {
            Square[,] tempBoard = new Square[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    tempBoard[i, j] = new Square(source[i, j]);
                }
            }

            return tempBoard;
        }

        /*private Square[,] MakeRecursiveGuess(Square[,] recursiveBoard, int row = 0, int col = 0)
        {
            if (row == 9)
                return recursiveBoard;

            if (recursiveBoard == null)
                return null;

            if (!recursiveBoard[row, col].IsGuessed && recursiveBoard[row, col].Guess!.Count == 0)
            {
                Console.WriteLine("\n\n\n\nInvalid guess.");
                return null;
            }

            if (recursiveBoard[row, col].IsGuessed)
            {
                if (col == 8)
                    return MakeRecursiveGuess(recursiveBoard, row + 1, 0);
                else
                    return MakeRecursiveGuess(recursiveBoard, row, col + 1);
            }

            Console.WriteLine(PrintBoard(recursiveBoard));

            int[] possibleVals;
            bool isFirstIteration = (row == 0 && col == 0);

            possibleVals = board[row, col].Guess!.ToArray();
            int prevVal = 0;
            do
            {
                Square[,] tempBoard;// = (Square[,])recursiveBoard!.Clone();
                foreach (int val in possibleVals)
                {
                    recursiveBoard![row, col].TempNumber = val;
                    RemoveGuesses(ref recursiveBoard, row, col, val);
                    if (col == 8)
                        tempBoard = MakeRecursiveGuess(recursiveBoard, row + 1, 0);
                    else
                        tempBoard = MakeRecursiveGuess(recursiveBoard, row, col + 1);

                    if (tempBoard != null)
                    {
                        return tempBoard;
                    }

                    prevVal = val;
                }
            }
            while (!isFirstIteration);
            return null;
        }*/

        private bool MakeRecursiveGuess(Square[,] recursiveBoard, int row = 0, int col = 0)
        {
            if (row == 9)
                return true;

            if (recursiveBoard[row, col].IsGuessed)
            {
                if (col == 8)
                    return MakeRecursiveGuess(recursiveBoard, row + 1, 0);
                else
                    return MakeRecursiveGuess(recursiveBoard, row, col + 1);
            }

            if (recursiveBoard[row, col].Guess!.Count == 0)
            {
                Console.WriteLine("\n\n\n\nInvalid guess.");
                return false;
            }

            int[] possibleVals = board[row, col].Guess!.ToArray();

            foreach (int val in possibleVals)
            {
                //RemoveGuesses(ref recursiveBoard, row, col, val);
                //Console.WriteLine(recursiveBoard[2, 7].PrintGuesses());
                //Console.WriteLine(PrintBoard(recursiveBoard));

                if (isSafe(board, row, col, val))
                {
                    recursiveBoard[row, col].TempNumber = val;

                    bool isSolveable;
                    if (col == 8)
                        isSolveable = MakeRecursiveGuess(recursiveBoard, row + 1, 0);
                    else
                        isSolveable = MakeRecursiveGuess(recursiveBoard, row, col + 1);

                    if (isSolveable)
                        return true;

                    recursiveBoard[row, col].TempNumber = 0;
                }

                //RestoreGuesses(ref recursiveBoard, row, col, val);
                //Console.WriteLine(recursiveBoard[2, 7].PrintGuesses());
                //return false;
            }

            /*recursiveBoard[0, 7].TempNumber = 3;
            Console.WriteLine(recursiveBoard[7, 7].PrintGuesses());
            RemoveGuesses(ref recursiveBoard, 0, 7, 3);
            Console.WriteLine(PrintBoard(recursiveBoard));
            Console.WriteLine(recursiveBoard[7, 7].PrintGuesses());
            recursiveBoard[0, 7].TempNumber = 0;
            RestoreGuesses(ref recursiveBoard, 0, 7, 3);
            Console.WriteLine(recursiveBoard[7, 7].PrintGuesses());*/

            return false;
        }

        private void TransformTempToNum(ref Square[,] source)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (source[i, j].IsGuessed)
                        continue;
                    if (source[i, j].TempNumber == 0)
                        throw new ArgumentException(); // change to normal exception
                    source[i, j].ExecuteGuess(source[i, j].TempNumber);
                }
            }
        }

        private void RemoveGuesses(ref Square[,] tempBoard, int v, int h, int removeVal)
        {
            Square val;

            for (int i = v + 1; i < 9; i++)
            {
                val = tempBoard[i, h];
                if (!val.IsGuessed)
                {
                    val.Guess!.Remove(removeVal);
                }
            }

            for (int j = h + 1; j < 9; j++)
            {
                val = tempBoard[v, j];
                if (!val.IsGuessed)
                {
                    val.Guess!.Remove(removeVal);
                }
            }

            int t = (3 * (v % 3)) + (h % 3);
            //Console.WriteLine("v, h = " + (v % 3) + ", " + (h % 3));
            //Console.WriteLine("t = " + t);
            for (int i = v; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if ((3 * i) + j < t)
                    {
                        //Console.WriteLine("Coords" + i + ", " + j);
                        continue;
                    }
                    val = tempBoard[i + ((v / 3) * 3), j + ((h / 3) * 3)];
                    if (!val.IsGuessed)
                    {
                        val.Guess!.Remove(removeVal);
                    }
                }
            }
        }

        private void RestoreGuesses(ref Square[,] tempBoard, int v, int h, int restoreVal)
        {
            Square val;

            for (int i = v + 1; i < 9; i++)
            {
                val = tempBoard[i, h];
                if (!val.IsGuessed)
                {
                    val.Guess!.Add(restoreVal);
                }
            }

            for (int j = h + 1; j < 9; j++)
            {
                val = tempBoard[v, j];
                if (!val.IsGuessed)
                {
                    val.Guess!.Add(restoreVal);
                }
            }

            int t = (3 * v) + h;
            for (int i = v; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if ((3 * i) + j < t)
                        continue;
                    val = tempBoard[i + ((v / 3) * 3), j + ((h / 3) * 3)];
                    if (!val.IsGuessed)
                    {
                        val.Guess!.Add(restoreVal);
                    }
                }
            }
        }

        //collects guessed values from any of the directions
        private HashSet<int> CollectGuessed(int v, int h, bool onlyV = false, bool onlyH = false, bool onlyS = false)
        {
            //iterate matrix int vertical + horizontal + the square is in and return list of unique numbers
            HashSet<int> set = new HashSet<int>();
            Square val;

            bool isVanilla = !(onlyV || onlyH || onlyS);

            //collect the vertical values
            if (isVanilla || onlyV)
            {
                for (int i = 0; i < 9; i++)
                {
                    val = board[i, h];
                    if (val.IsGuessed)
                        set.Add(val.Number);
                }
            }

            //collect the horizontal values
            if (isVanilla || onlyH)
            {
                for (int j = 0; j < 9; j++)
                {
                    val = board[v, j];
                    if (val.IsGuessed)
                        set.Add(val.Number);
                }
            }

            //colect the square values
            //Console.WriteLine(v + " " + h);
            if (isVanilla || onlyS)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        val = board[i + ((v / 3) * 3), j + ((h / 3) * 3)];
                        //Console.Write(val.Number.ToString() + " ");
                        if (val.IsGuessed)
                            set.Add(val.Number);
                    }
                    //Console.WriteLine("");
                }
                //Console.WriteLine("");
            }

            return set;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (!board[i, j].IsGuessed)
                        return false;

            return true;
        }

        private bool isSolveable()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (this.board[i, j].IsGuessed)
                    {
                        if (!isSafe(this.board, i, j, this.board[i, j].Number))
                            return false;
                    }
                }
            }
            return true;
        }

        #region PRINTS
        public override string ToString()
        {
            string str = "┌───────┬───────┬───────┐\n";
            for (int i = 0; i < 9; i++)
            {
                str += "│ ";
                for (int j = 0; j < 9; j++)
                {
                    if (board[i, j].IsGuessed)
                        str += board[i, j].Number.ToString();
                    else if (board[i, j].TempNumber != 0)
                        str += "G";
                    else
                        str += "U";
                    if ((j + 1) % 3 == 0 && j != 8)
                        str += " │ ";
                    else
                        str += " ";
                }
                str += "│\n";
                if (i == 8)
                    str += "└───────┴───────┴───────┘\n";
                else if ((i + 1) % 3 == 0)
                    str += "├───────┼───────┼───────┤\n";
            }

            return str;
        }

        private string PrintBoard(Square[,] source)
        {
            string str = "┌───────┬───────┬───────┐\n";
            for (int i = 0; i < 9; i++)
            {
                str += "│ ";
                for (int j = 0; j < 9; j++)
                {
                    if (source[i, j].IsGuessed)
                        str += board[i, j].Number.ToString();
                    else if (source[i, j].TempNumber != 0)
                        str += $"{WriteUnderline(source[i, j].TempNumber.ToString())}";
                    else
                        str += "U";
                    if ((j + 1) % 3 == 0 && j != 8)
                        str += " │ ";
                    else
                        str += " ";
                }
                str += "│\n";
                if (i == 8)
                    str += "└───────┴───────┴───────┘\n";
                else if ((i + 1) % 3 == 0)
                    str += "├───────┼───────┼───────┤\n";
            }

            return str;
        }

        private static string WriteUnderline(string s)
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);
            return ($"\x1B[4m{s}\x1B[24m");
        }

        #endregion

        private bool isSafe(Square[,] grid, int row, int col, int num)
        {
            for (int x = 0; x <= 8; x++)
            {
                if (x == col)
                    continue;
                if (grid[row, x].Number == num || grid[row, x].TempNumber == num)
                    return false;
            }


            for (int x = 0; x <= 8; x++)
            {
                if (x == row)
                    continue;
                if (grid[x, col].Number == num || grid[x, col].TempNumber == num)
                    return false;
            }

            int startRow = row - row % 3, startCol
              = col - col % 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (i + startRow == row && j + startCol == col) continue;
                    if (grid[i + startRow, j + startCol].Number == num || grid[i + startRow, j + startCol].TempNumber == num) return false;
                }

            return true;
        }

        private int[,] ToIntArr()
        {
            int[,] arr = new int[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    arr[i, j] = board[i, j].Number;
                }
            }

            return arr;
        }
    }
}

