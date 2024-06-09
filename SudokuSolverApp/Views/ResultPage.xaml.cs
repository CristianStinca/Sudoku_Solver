using Microsoft.Maui.Layouts;
using SudokuSolverApp.ViewModels;

namespace SudokuSolverApp.Views;

public partial class ResultPage : ContentPage
{
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
                if (Resources.TryGetValue("Secondary", out object primaryColor))
                    frame.BackgroundColor = (Color)primaryColor;

                Label label = new Label();

                frame.CornerRadius = 8;
                frame.Padding = 10;
                frame.Content = label;
                //frame.ArrangeContent();
                //frame.HeightRequest = frame.WidthRequest;
                //frame.HeightRequest = frame.Measure(frame.MaximumHeightRequest, frame.MaximumWidthRequest).Request.Width;

                label.Text = _vm.Matrix[i, j].ToString();
                label.HorizontalTextAlignment = TextAlignment.Center;

                //if (Application.Current.RequestedTheme == AppTheme.Dark)

                //_vm.Matrix[i, j] = button;

                int ic = (i + ((i) / 3));
                int jc = (j + ((j) / 3));

                int ip = i;
                int jp = j;

                //button.Clicked += (o, e) => OnBoardClicked(o, e, ip, jp);

                MatrixGrid.Add(frame, ic, jc);
            }
        }
    }
}