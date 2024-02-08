using Microsoft.UI.Xaml;

namespace MediaSyncPro.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            InitializeComponent();
            RequestedTheme = ApplicationTheme.Dark;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}