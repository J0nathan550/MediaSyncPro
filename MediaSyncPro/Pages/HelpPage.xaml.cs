using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MediaSyncPro.Pages;

public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
        text.Text = "\r\nWelcome to MediaSync Pro, where you can swiftly install audio and video content from YouTube and Spotify. Our application supports playlist links, allowing you to effortlessly download packs of your favorite videos and music.\r\n\r\nWe hope you enjoy using our program! Your support means the world to us. By clicking the button below, you can help us develop more exciting software and continue our journey.\r\n\r\nMediaSync Pro is ad-free and solely supported by generous contributions from users like you. If you appreciate our work, consider donating a Ko-Fi.\r\n\r\nThank you for your love and support!\r\n\r\nSincerely,\r\nJ0nathan550";
    }

    private async void SupportButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            Uri uri = new("https://ko-fi.com/j0nathan550");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            Dispatcher.Dispatch(async () =>
            {
                string text = "Looks like you don't have a browser to open Ko-Fi page! :D";
                ToastDuration duration = ToastDuration.Long;
                double fontSize = 14;

                var toast = Toast.Make(text, duration, fontSize);

                await toast.Show(new CancellationToken());
            });
        }
    }
}