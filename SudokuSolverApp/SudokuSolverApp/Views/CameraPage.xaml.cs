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

    public CameraPage(CameraViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        this._vm = vm;
    }

    public async void Camera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    {

    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        _bitmap = null;
    }
}