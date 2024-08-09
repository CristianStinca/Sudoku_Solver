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
            Routing.RegisterRoute(nameof(DebuggingPage), typeof(DebuggingPage));

#if DEBUG
            DebugContent.IsVisible = true;
#endif
        }
    }
}
