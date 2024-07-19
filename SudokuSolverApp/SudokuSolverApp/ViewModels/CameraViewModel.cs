using CommunityToolkit.Mvvm.ComponentModel;

namespace SudokuSolverApp.ViewModels
{
    public partial class CameraViewModel : ObservableObject
    {
        public CancellationToken Token => CancellationToken.None;
        public CameraViewModel() { }

        public async void SaveImage(Stream img)
        {
            string imagePath = Path.Combine(FileSystem.Current.CacheDirectory, "camera-view-cache.jpg");
            using var localFileStream = File.Create(imagePath);

            img.CopyTo(localFileStream);

            await Shell.Current.GoToAsync("..",
                new Dictionary<string, object>
                {
                    ["ImagePath"] = imagePath
                });
        }
    }
}
