using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.OCR;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SudokuSolverApp.ViewModels;
using SudokuSolverApp.Views;
using TesseractOcrMaui;

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
                .UseSkiaSharp()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTesseractOcr(
            files =>
            {
                files.AddFile("eng.traineddata");
            });
#if DEBUG
            builder.Logging.AddDebug();
#endif
           
            builder.Services.AddSingleton<ManualInPage>();
            builder.Services.AddSingleton<ManualInViewModel>();

            builder.Services.AddTransient<ResultPage>();
            builder.Services.AddTransient<ResultPageViewModel>();

            builder.Services.AddTransient<CameraPage>();
            builder.Services.AddTransient<CameraViewModel>();

#if DEBUG
            builder.Services.AddSingleton<DebuggingPage>();
#endif

            return builder.Build();
        }
    }
}
