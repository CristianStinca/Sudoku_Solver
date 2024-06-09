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
                if (App.Current.Resources.TryGetValue("White", out object colorvalue1))
                    button.BackgroundColor = (Color)colorvalue1;
                
                if (App.Current.Resources.TryGetValue("Black", out object colorvalue2))
                    button.TextColor = (Color)colorvalue2;
                
                if (App.Current.Resources.TryGetValue("Gray100", out object colorvalue3))
                    button.BorderColor = (Color)colorvalue3;

                button.Padding = 0;
                button.CornerRadius = 0;
                button.BorderWidth = 1;
                button.FontSize = 20;

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
        _vm.PropertyChanged += OnMatrixClear;
    }

    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        base.LayoutChildren(x, y, width, height);

        var w = _matrix[0, 0].Width;
        for (int i = 0; i < _matrix.GetLength(0); i++)
        {
            for (int j = 0; j < _matrix.GetLength(1); j++)
            {
                _matrix[i, j].HeightRequest = w;
            }
        }
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

    private void OnMatrixClear(object sender, EventArgs e)
    {
        if ((e as PropertyChangedEventArgs).PropertyName != "matrix") return;

        for (int i = 0; i < _matrix.GetLength(0); i++)
        {
            for (int j = 0; j < _matrix.GetLength(1); j++)
            {
                _matrix[i, j].Text = String.Empty;
            }
        }
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        _vm.ClearMatrix();
    }

}