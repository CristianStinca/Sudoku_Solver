using Microsoft.Maui.Layouts;
using SudokuSolverApp.ViewModels;

namespace SudokuSolverApp.Views;

public partial class ResultPage : ContentPage
{
    private Button[,] _matrix = new Button[9, 9];
    ResultPageViewModel _vm;
    public ResultPage(ResultPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        this._vm = vm;

        vm.PropertyChanged += OnMatrixChanged;
    }

    public void OnMatrixChanged(object sender, EventArgs e)
    {
        for (int i = 0; i < _vm.Matrix.GetLength(0); i++)
        {
            for (int j = 0; j < _vm.Matrix.GetLength(1); j++)
            {
                Button button = new Button() 
                { 
                    Style = (Style)Application.Current.Resources["BoardButton"],
                    Text = _vm.Matrix[i, j].ToString()
                };

                int ic = (i + ((i) / 3));
                int jc = (j + ((j) / 3));

                int ip = i;
                int jp = j;

                MatrixGrid.Add(button, ic, jc);
                _matrix[i, j] = button;
            }
        }

        foreach (( int i, int j ) in _vm.given_fields)
        {
            //if (App.Current.Resources.TryGetValue("Yellow300Accent", out object primaryColor))
            //    _matrix[i, j].BackgroundColor = (Color)primaryColor;
            //else
            //    _matrix[i, j].BackgroundColor = Colors.Coral;

            _matrix[i, j].Style = (Style)Application.Current.Resources["BoardButtonSaved"];
        }
    }

    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        base.LayoutChildren(x, y, width, height);

        MatrixGrid.HeightRequest = MatrixGrid.Width;
    }
}