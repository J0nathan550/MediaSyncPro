using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace MediaSyncPro
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private void SetWindowLayout()
        {
            if (Window != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
#pragma warning disable CA1416

                    IWindowInsetsController? wicController = Window.InsetsController;


                    Window.SetDecorFitsSystemWindows(false);
                    Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                    if (wicController != null)
                    {
                        wicController.Hide(WindowInsets.Type.Ime());
                        wicController.Hide(WindowInsets.Type.NavigationBars());
                    }
#pragma warning restore CA1416
                }
                else
                {
#pragma warning disable CS0618

                    Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Fullscreen |
                                                                                 SystemUiFlags.HideNavigation |
                                                                                 SystemUiFlags.Immersive |
                                                                                 SystemUiFlags.ImmersiveSticky |
                                                                                 SystemUiFlags.LayoutHideNavigation |
                                                                                 SystemUiFlags.LayoutStable |
                                                                                 SystemUiFlags.LowProfile);
#pragma warning restore CS0618
                }
            }
        }
        protected override void OnCreate(Bundle? bSavedInstanceState)
        {
            base.OnCreate(bSavedInstanceState);

            SetWindowLayout();
        }
    }
}