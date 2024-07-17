using SkiaSharp.Views.Maui;
using SudokuSolverApp.ViewModels;
using System.Diagnostics;
using SkiaSharp;
using TesseractOcrMaui;

namespace SudokuSolverApp.Views;

public partial class CameraPage : ContentPage
{
    CameraViewModel _vm;
    SKBitmap _bitmap;

    public CameraPage(CameraViewModel vm, ITesseract tesseract)
	{
		InitializeComponent();
        BindingContext = vm;
        this._vm = vm;
        _vm.tes = tesseract;
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

    //public async void Camera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    //{
    //    //try
    //    //{
    //    SKBitmap img = await _vm.SaveImage(e.Media);

    //    this._bitmap = img;

    //    mycanvas.InvalidateSurface();
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    Debug.WriteLine($"_________Catched: {ex}");
    //    //}

    //    //        Dispatcher.Dispatch(() =>
    //    //        {
    //    //            // workaround for https://github.com/dotnet/maui/issues/13858
    //    //            //#if ANDROID
    //    //            image.Source = ImageSource.FromStream(() => File.OpenRead(_vm.outPath));

    //    //            //image.Source = ImageSource.FromStream(() => img);
    //    ////#else
    //    //            //image.Source = ImageSource.FromFile(_vm.cropPath);
    //    ////#endif
    //    //        });
    //}

    private void Button_Clicked(object sender, EventArgs e)
    {
        _bitmap = null;

        mycanvas.InvalidateSurface();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        this._bitmap = await _vm.GetFromMedia();

        mycanvas.InvalidateSurface();
    }

    private void Button_Clicked_2(object sender, EventArgs e)
    {
        this._bitmap = _vm.originalImg;

        mycanvas.InvalidateSurface();
    }

    private void Button_Clicked_3(object sender, EventArgs e)
    {
        //this._bitmap = _vm.bwImg;
        this._bitmap = _vm.num_arr[0, 0];

        mycanvas.InvalidateSurface();
    }

    private void Button_Clicked_4(object sender, EventArgs e)
    {
        this._bitmap = _vm.contouredImg;

        mycanvas.InvalidateSurface();
    }

    private void Button_Clicked_5(object sender, EventArgs e)
    {
        this._bitmap = _vm.deskewedImg;

        mycanvas.InvalidateSurface();
    }
}