using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverApp.ViewModels
{
    [QueryProperty("Matrix", "Solved")]
    [QueryProperty("RawMatrix", "Raw")]
    public partial class ResultPageViewModel : ObservableObject
    {
        [ObservableProperty]
        int[,] matrix;

        [ObservableProperty]
        int[,] rawMatrix;
        public List<ValueTuple<int, int>> given_fields = new();

        partial void OnRawMatrixChanged(int[,] value)
        {
            for (int i = 0; i < RawMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < RawMatrix.GetLength(1); j++)
                {
                    if (RawMatrix[i, j] == 0) continue;

                    given_fields.Add(new ValueTuple<int, int>(i, j));
                }
            }
        }
    }
}
