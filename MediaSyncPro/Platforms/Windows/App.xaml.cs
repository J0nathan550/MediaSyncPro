using Microsoft.UI.Windowing;
using Microsoft.UI;
using Colors = Microsoft.UI.Colors;

namespace MediaSyncPro.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
           InitializeComponent();
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

                //Make window button text white
                appWindow.TitleBar.ButtonBackgroundColor = Colors.Black;
                appWindow.TitleBar.ButtonHoverBackgroundColor = Colors.Gray;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Black;
                appWindow.TitleBar.ButtonHoverForegroundColor = Colors.White;
                appWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
                appWindow.TitleBar.ButtonForegroundColor = Colors.White;
            });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}