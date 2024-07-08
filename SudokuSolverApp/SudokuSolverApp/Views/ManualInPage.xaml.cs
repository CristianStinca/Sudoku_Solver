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
    private Button[] _numbers_buttons = new Button[9];
    private bool _is_loading = false;
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
                Button button = new Button() { Style = (Style)Application.Current.Resources["BoardButton"] };

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
            Button button = new Button() { Style = (Style)Application.Current.Resources["NumberButton"] };

            button.Text = i.ToString();
            byte n = (byte)i;
            button.Clicked += (o, e) => OnNumberClicked(o, e, n);

            _numbers_buttons[i-1] = button;
            NumbersGrid.Add(button, i-1);
        }

        _vm.MatrixChanged += OnMatrixChanged;
        _vm.PropertyChanged += OnMatrixClear;
    }

    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        base.LayoutChildren(x, y, width, height);

        MatrixGrid.HeightRequest = MatrixGrid.Width;
        NumbersGrid.HeightRequest = _numbers_buttons.First().Width;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        StopLoading();
    }

    private void OnBoardClicked(object sender, EventArgs e, int i, int j)
    {
        _vm.BoardClicked(i, j);
    }

    private void OnNumberClicked(object sender, EventArgs e, byte n)
    {
        if (_vm.number != null)
            _numbers_buttons[(int)_vm.number - 1].Style = (Style)Application.Current.Resources["NumberButton"];
        _vm.number = n;
        _numbers_buttons[n - 1].Style = (Style)Application.Current.Resources["NumberButtonSelected"];
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

    private void EraseBoardBttn_Clicked(object sender, EventArgs e)
    {
        _vm.ClearMatrix();
    }

    private void Solve_Clicked(object sender, EventArgs e)
    {
        StartLoading();
    }

    private void StartLoading()
    {
        LoadingIndicator.IsRunning = true;
    }

    private void StopLoading()
    {
        LoadingIndicator.IsRunning = false;
    }
}