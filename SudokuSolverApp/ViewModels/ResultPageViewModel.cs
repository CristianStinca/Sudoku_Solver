using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverApp.ViewModels
{
    [QueryProperty("Matrix", "Text")]
    public partial class ResultPageViewModel : ObservableObject
    {
        [ObservableProperty]
        int[,] matrix;
    }
}
