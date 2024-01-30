using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SpotifyExplode;
using SpotifyExplode.Playlists;
using SpotifyExplode.Tracks;

namespace MediaSyncPro.Pages;

public partial class DownloadPage : ContentPage
{
	public DownloadPage()
	{
		InitializeComponent();
	}

    private async void StartDownloadingButton_Clicked(object sender, EventArgs e)
    {
        // Checking if we have at least something in textBox.
        if (string.IsNullOrEmpty(linkTextBox.Text))
        {
            string text = "The link you typed is empty!";
            ToastDuration duration = ToastDuration.Long;
            double fontSize = 14;

            var toast = Toast.Make(text, duration, fontSize);

            await toast.Show(new CancellationToken());
            return;
        }
        downloadInformationStackLayout.Children.Clear();
        await Task.Run(async () =>
        {
            SpotifyClient client = new();
            List<Track> tracks = [];
            try // trying to parse link and get playlist
            {
                tracks = (await client.Playlists.GetAsync((PlaylistId)linkTextBox.Text, new CancellationToken())).Tracks;
            }
            catch // no playlist? trying to get single track. 
            {
                try
                {
                    var track = await client.Tracks.GetAsync((TrackId)linkTextBox.Text, new CancellationToken());
                    tracks.Add(track);
                }
                catch // no track? error for now.
                {
                    Dispatcher.Dispatch(async () =>
                    {
                        string text = "The link you typed is wrong!";
                        ToastDuration duration = ToastDuration.Long;
                        double fontSize = 14;

                        var toast = Toast.Make(text, duration, fontSize);

                        await toast.Show(new CancellationToken());
                    });
                    return;
                }
            }
            uint indexCount = 1;
            foreach (var track in tracks)
            {
                try
                {
                    await Task.Delay(200);
                    Dispatcher.Dispatch(() =>
                    {
                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading: {track.Title}" });
                        indexCount++;
                    });
                }
                catch
                {
                    Dispatcher.Dispatch(() =>
                    {
                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {track.Title}" });
                        indexCount++;
                    });
                }
            }
        });
    }
}