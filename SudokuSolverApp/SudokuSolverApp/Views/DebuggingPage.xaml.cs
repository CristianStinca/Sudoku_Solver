using SkiaSharp;
using SkiaSharp.Views.Maui;
using SudokuSolverApp.Models;
using System.Diagnostics;

namespace SudokuSolverApp.Views;

public partial class DebuggingPage : ContentPage
{
#if DEBUG
    SKBitmap _bitmap;

    public DebuggingPage()
	{
		InitializeComponent();

        picker_i.SelectedIndex = 0;
        picker_j.SelectedIndex = 0;

        picker_i.SelectedIndexChanged += Button_Clicked;
        picker_j.SelectedIndexChanged += Button_Clicked;
    }

    private void Mycanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        if (_bitmap != null)
        {
            try
            {
                //canvas.DrawColor(new SKColor(255, 255, 255));
                double coef = (double)_bitmap.Height / (double)_bitmap.Width;
                double w = args.Info.Width;
                double h = w * coef;

                _bitmap = _bitmap.Resize(new SKSizeI((int)w, (int)h), new SKFilterQuality());
                canvas.DrawBitmap(_bitmap, new SKPoint(0, 0));

                //for (int i = 0; i < 4; i++)
                //{
                //    var p1 = main_contour[i];
                //    var p2 = main_contour[(i + 1) % 4];
                //    canvas.DrawLine(new SKPoint((int)(p1.X / coef), (int)(p1.Y / coef)), new SKPoint((int)(p2.X / coef), (int)(p2.Y / coef)), new SKPaint() { Color = new SKColor(0, 0, 255), StrokeWidth = 5}); 
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        this._bitmap = ImageToSudokuService.originalImg;

        mycanvas.InvalidateSurface();
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        this._bitmap = ImageToSudokuService.bwImg;

        mycanvas.InvalidateSurface();
    }

    private void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
        this._bitmap = ImageToSudokuService.conturedImg;

        mycanvas.InvalidateSurface();
    }

    private void ToolbarItem_Clicked_3(object sender, EventArgs e)
    {
        this._bitmap = ImageToSudokuService.skewedImg;

        mycanvas.InvalidateSurface();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Byte i = (Byte)picker_i.SelectedItem;
        Byte j = (Byte)picker_j.SelectedItem;

        this._bitmap = ImageToSudokuService.separatedCells[i, j];

        mycanvas.InvalidateSurface();
    }
#endif
}