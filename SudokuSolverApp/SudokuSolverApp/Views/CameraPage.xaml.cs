using SudokuSolverApp.ViewModels;
using System.Diagnostics;

namespace SudokuSolverApp.Views;

public partial class CameraPage : ContentPage
{
    CameraViewModel _vm;

    public CameraPage(CameraViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        this._vm = vm;

        Camera.MediaCaptured += Camera_MediaCaptured;
    }

    public void Camera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    {
        //try
        //{
            _vm.SaveImage(e.Media);
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine($"_________Catched: {ex}");
        //}

        Dispatcher.Dispatch(() =>
        {
            // workaround for https://github.com/dotnet/maui/issues/13858
#if ANDROID
            image.Source = ImageSource.FromStream(() => File.OpenRead(_vm.imagePath));
#else
            image.Source = ImageSource.FromFile(_vm.imagePath);
#endif
        });
    }
}