using SkiaSharp.Views.Maui;
using SudokuSolverApp.ViewModels;
using System.Diagnostics;
using SkiaSharp;
using TesseractOcrMaui;

namespace SudokuSolverApp.Views;

public partial class CameraPage : ContentPage
{
    CameraViewModel _vm;

    public CameraPage(CameraViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        this._vm = vm;
    }

    public void Camera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    {
        _vm.SaveImage(e.Media);
    }
}