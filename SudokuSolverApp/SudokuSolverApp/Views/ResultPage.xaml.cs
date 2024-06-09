using Microsoft.Maui.Layouts;
using SudokuSolverApp.ViewModels;

namespace SudokuSolverApp.Views;

public partial class ResultPage : ContentPage
{
    private Frame[,] _matrix = new Frame[9, 9];
    ResultPageViewModel _vm;
    public ResultPage(ResultPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        this._vm = vm;

        //BoxView boxView = new BoxView();
        //boxView.BackgroundColor = Colors.Beige;

        //      MatrixGrid.Add(boxView, 0, 0);

        vm.PropertyChanged += OnMatrixChanged;
    }

    public void OnMatrixChanged(object sender, EventArgs e)
    {
        for (int i = 0; i < _vm.Matrix.GetLength(0); i++)
        {
            for (int j = 0; j < _vm.Matrix.GetLength(1); j++)
            {
                //Button button = new Button();
                //if (Resources.TryGetValue("Secondary", out object primaryColor))
                //    button.BackgroundColor = (Color)primaryColor;

                Frame frame = new Frame();
                if (App.Current.Resources.TryGetValue("White", out object primaryColor))
                    frame.BackgroundColor = (Color)primaryColor;

                Label label = new Label();

                frame.Padding = 0;
                frame.HeightRequest = frame.Width;
                frame.CornerRadius = 0;
                frame.Content = label;
                //frame.ArrangeContent();
                //frame.HeightRequest = frame.WidthRequest;
                //frame.HeightRequest = frame.Measure(frame.MaximumHeightRequest, frame.MaximumWidthRequest).Request.Width;

                label.Text = _vm.Matrix[i, j].ToString();
                label.HorizontalTextAlignment = TextAlignment.Center;
                label.VerticalTextAlignment = TextAlignment.Center;
                label.FontSize = 20;
                label.TextColor = Colors.Black;

                //if (Application.Current.RequestedTheme == AppTheme.Dark)

                //_vm.Matrix[i, j] = button;

                int ic = (i + ((i) / 3));
                int jc = (j + ((j) / 3));

                int ip = i;
                int jp = j;

                //button.Clicked += (o, e) => OnBoardClicked(o, e, ip, jp);

                MatrixGrid.Add(frame, ic, jc);
                _matrix[i, j] = frame;
            }
        }

        foreach (( int i, int j ) in _vm.given_fields)
        {
            if (App.Current.Resources.TryGetValue("Yellow300Accent", out object primaryColor))
                _matrix[i, j].BackgroundColor = (Color)primaryColor;
            else
                _matrix[i, j].BackgroundColor = Colors.Coral;
        }
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
}