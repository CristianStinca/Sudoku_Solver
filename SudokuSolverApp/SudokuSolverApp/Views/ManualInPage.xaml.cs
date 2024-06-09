using Microsoft.Maui.Controls.StyleSheets;
using SudokuLogicLibr.SudokuLogic;
using SudokuSolverApp.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace SudokuSolverApp.Views;

public partial class ManualInPage : ContentPage
{
    private Button[,] _matrix = new Button[9, 9];
    private readonly ManualInViewModel _vm;

    public ManualInPage(ManualInViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        this._vm = vm;

        for (int i = 0; i < _matrix.GetLength(0); i++)
        {
            for (int j = 0; j < _matrix.GetLength(1); j++)
            {
                Button button = new Button();
                if (Resources.TryGetValue("Secondary", out object primaryColor))
                    button.BackgroundColor = (Color)primaryColor;

                button.Padding = 10;

                //if (Application.Current.RequestedTheme == AppTheme.Dark)

                _matrix[i, j] = button;

                int ic = (i + ((i) / 3));
                int jc = (j + ((j) / 3));

                int ip = i;
                int jp = j;

                button.Clicked += (o, e) => OnBoardClicked(o, e, ip, jp);

                MatrixGrid.Add(button, ic, jc);
            }
        }

        for (int i = 1; i < 10; i++)
        {
            Button button = new Button();
            button.BackgroundColor = Colors.DimGray;
            button.Text = i.ToString();
            button.Padding = 0;
            byte n = (byte)i;
            button.Clicked += (o, e) => OnNumberClicked(o, e, n);

            NumbersGrid.Add(button, i-1);
        }

        _vm.MatrixChanged += OnMatrixChanged;
    }

    private void OnBoardClicked(object sender, EventArgs e, int i, int j)
    {
        _vm.BoardClicked(i, j);
    }

    private void OnNumberClicked(object sender, EventArgs e, byte n)
    {
        _vm.number = n;
    }

    private void OnMatrixChanged(object sender, EventArgs e, int i, int j)
    {
        if ((e as PropertyChangedEventArgs).PropertyName != "matrix") return;

        _matrix[i, j].Text = _vm.Matrix[i, j].ToString();
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        _vm.ClearMatrix();
    }

}