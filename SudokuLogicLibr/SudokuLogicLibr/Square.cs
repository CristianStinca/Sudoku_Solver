using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuLogicLibr.SudokuLogic
{
    internal class Square
    {
        public HashSet<int>? Guess { get; set; } = null;
        public int Number { get; private set; } = 0;
        public int TempNumber { get; set; } = 0;
        public bool IsGuessed { get; private set; } = false;
        public bool CanBeGuessed { get; private set; } = false;

        public Square()
        {
            IsGuessed = false;
            Number = 0;
            Guess = new HashSet<int>();
            for (int i = 1; i <= 9; i++)
                Guess.Add(i);
        }

        public Square(int nr)
        {
            IsGuessed = true;
            Guess = null;
            Number = nr;
        }

        public Square(Square source)
        {
            Guess = source.Guess;
            Number = source.Number;
            TempNumber = source.TempNumber;
            IsGuessed = source.IsGuessed;
            CanBeGuessed = source.CanBeGuessed;
        }

        public void MakeGuess(HashSet<int> set)
        {
            Guess!.ExceptWith(set);
            if (Guess.Count == 1)
                CanBeGuessed = true;
        }

        public void ExecuteGuess()
        {
            if (IsGuessed && !CanBeGuessed) return;
            try
            {
                int[] x = new int[1];
                Guess!.CopyTo(x);
                Number = x[0];
                Guess = null;
                IsGuessed = true;
                TempNumber = 0;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Error with transforming guesses to actual number!");
            }
        }

        public void ExecuteGuess(int value)
        {
            Number = value;
            Guess = null;
            IsGuessed = true;
            TempNumber = 0;
        }

        public string PrintGuesses()
        {
            string str = "";
            if (Guess != null)
            {
                int[] vals = Guess!.ToArray();
                foreach (int x in vals)
                    str += x + ", ";
            }
            else
                throw new Exception();
            return str;
        }
    }
}

