using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SudokuSolverApp.ViewModels;
using SudokuSolverApp.Views;

namespace SudokuSolverApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
           
            builder.Services.AddSingleton<ManualInPage>();
            builder.Services.AddSingleton<ManualInViewModel>();

            builder.Services.AddTransient<ResultPage>();
            builder.Services.AddTransient<ResultPageViewModel>();

            builder.Services.AddSingleton<CameraPage>();
            builder.Services.AddSingleton<CameraViewModel>();

            return builder.Build();
        }
    }
}
