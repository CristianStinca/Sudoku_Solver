using SudokuSolverApp.Views;

namespace SudokuSolverApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ResultPage), typeof(ResultPage));
            Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));

#if DEBUG
            DebugContent.IsVisible = true;
            Routing.RegisterRoute(nameof(DebuggingPage), typeof(DebuggingPage));
#endif
        }
    }
}
