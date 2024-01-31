using MediaSyncPro.Classes;

namespace MediaSyncPro
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Dark;
            SettingsClass.LoadSettings();
            MainPage = new AppShell();
        }
    }
}
