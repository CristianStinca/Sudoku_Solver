using Microsoft.Maui.ApplicationModel.DataTransfer;
using SudokuLogicLibr.SudokuLogic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SudokuSolverApp.Views;

public partial class ManualInPage : ContentPage
{
    //Entry[,] matrix = new Entry[9, 9];
    ObservableCollection<ObservableCollection<Entry>> matrix = new();
    public ManualInPage()
    {
        InitializeComponent();

        for (int i = 0; i < 9; i++)
        {
            matrix.Add(new ObservableCollection<Entry>());
            for (int j = 0; j < 9; j++)
            {
                matrix.ElementAt(i).Add(new Entry
                {
                    Keyboard = Keyboard.Numeric,
                    Text = "",
                    MaxLength = 1,
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.Black,
                    CursorPosition = 0,
                    SelectionLength = 5,
                    HorizontalTextAlignment = TextAlignment.Center,
                }); ;
                matrix.ElementAt(i).ElementAt(j).SetBinding(Entry.TextProperty, new Binding($"MatrixData[{i}][{j}]"));

                int ic = (i + ((i) / 3));
                int jc = (j + ((j) / 3));

                MatrixGrid.Add(matrix.ElementAt(i).ElementAt(j), ic, jc);
            }
        }
    }

    private int[,] ArrayEntryToInts(ObservableCollection<ObservableCollection<Entry>> input)
    {
        //assumnig every list is of the same size
        int h = input.Count();
        int w = input.ElementAt(0).Count();
        int[,] outp = new int[9, 9];
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                try
                {
                    outp[i, j] = int.Parse(input.ElementAt(j).ElementAt(i).Text);
                }
                catch
                {
                    outp[i, j] = 0;
                }
            }
        }

        return outp;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        int[,] mat = ArrayEntryToInts(matrix);
        SudokuBoard sdkMat = new SudokuBoard(mat);

        try
        {
            sdkMat.Solve();
            await DisplayAlert("Sudoku", sdkMat.ToString(), "ok");
            /*await Shell.Current.GoToAsync($"{nameof(ResultPage)}?",
                new Dictionary<string, object>
                {
                    { "Obj1", mat},
                }) ;*/
        }
        catch (SudokuCannotBeSolvedException)
        {
            await DisplayAlert("Error", "Sudoku cannot be solved", "OK");
        }
    }

    private string PrintBoard(int[,] source)
    {
        string str = "┌───────┬───────┬───────┐\n";
        for (int i = 0; i < 9; i++)
        {
            str += "│  ";
            for (int j = 0; j < 9; j++)
            {
                if (source[i, j] == -1)
                    str += " u ";
                else
                    str += " " + source[i, j] + " ";
                if ((j + 1) % 3 == 0 && j != 8)
                    str += "   │   ";
                else
                    str += " ";
            }
            str += " │\n";
            if (i == 8)
                str += "└───────┴───────┴───────┘\n";
            else if ((i + 1) % 3 == 0)
                str += "├───────┼───────┼───────┤\n";
        }

        return str;
    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;

        entry.CursorPosition = 0;
        entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        int h = matrix.Count();
        int w = matrix.ElementAt(0).Count();

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                matrix.ElementAt(i).ElementAt(j).Text = "";
            }
        }
    }
}