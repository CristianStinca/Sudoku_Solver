using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SudokuLogicLibr.SudokuLogic;
using SudokuSolverApp.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SudokuSolverApp.ViewModels
{
    public partial class ManualInViewModel : ObservableObject
    {
        public event MatrixChangedHandler MatrixChanged;
        public delegate void MatrixChangedHandler(object obj, EventArgs e, int i, int j);

        public ManualInViewModel()
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = 0;
                }
            }
        }

        [ObservableProperty]
        int[,] matrix = new int[9, 9];

        [ObservableProperty]
        string text;

        public byte? number;

        public void BoardClicked(int i, int j)
        {
            if (number is null) return;
            if (number < 1 || number > 9) return;

            Matrix[i, j] = (byte)number;
            //this.OnPropertyChanged(new PropertyChangedEventArgs("matrix"));
            this.MatrixChanged?.Invoke(this, new PropertyChangedEventArgs("matrix"), i, j);
        }

        [RelayCommand]
        async Task Solve()
        {
            SudokuBoard sdkMat = new SudokuBoard(Matrix);

            try
            {
                int[,] arr = sdkMat.Solve();
                Text = sdkMat.ToString();

                await Shell.Current.GoToAsync(nameof(ResultPage),
                    new Dictionary<string, object>
                        {
                            {"Solved", arr},
                            {"Raw", Matrix}
                        });
            }
            catch (SudokuCannotBeSolvedException)
            {
                Text = "Sudoku cannot be solved";
            }
        }

        [RelayCommand]
        void TextTask()
        {
            Console.WriteLine(Text);
        }

        public void ClearMatrix()
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Matrix[i, j] = 0;
                }
            }

            this.OnPropertyChanged("matrix");
        }
    }
}