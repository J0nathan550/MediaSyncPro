using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SpotifyExplode;
using SpotifyExplode.Playlists;
using SpotifyExplode.Tracks;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MediaSyncPro.Pages;

public partial class DownloadPage : ContentPage
{
	public DownloadPage()
	{
		InitializeComponent();
    }

    private CancellationTokenSource? _cancellationTokenSource;

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

        _cancellationTokenSource = new CancellationTokenSource();

        downloadInformationStackLayout.Children.Clear();
        await Task.Run(async () =>
        {
            SpotifyClient spotifyClient = new();
            YoutubeClient youtubeClient = new();
            List<Track> tracks = [];
            uint indexCount = 1;
            try // trying to parse link and get playlist
            {
                tracks = (await spotifyClient.Playlists.GetAsync((SpotifyExplode.Playlists.PlaylistId)linkTextBox.Text, _cancellationTokenSource.Token)).Tracks;
            }
            catch // no playlist? trying to get single track. 
            {
                try
                {
                    var track = await spotifyClient.Tracks.GetAsync((TrackId)linkTextBox.Text, _cancellationTokenSource.Token);
                    tracks.Add(track);
                }
                catch
                {
                    try // not spotify? trying to get youtube playlist 
                    {
                        var videos = await youtubeClient.Playlists.GetVideosAsync((YoutubeExplode.Playlists.PlaylistId)linkTextBox.Text, _cancellationTokenSource.Token);
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        foreach (var video in videos)
                        {
                            try
                            {
                                Dispatcher.Dispatch(async () =>
                                {
                                    downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading: {video.Title}" });
                                    await Task.Run(async () =>
                                    {
                                        try
                                        {
                                            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + video.Id);
                                            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                                            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + video.Author + "-" + video.Title + $".{streamInfo.Container}", null, _cancellationTokenSource.Token);
                                            Dispatcher.Dispatch(() =>
                                            {
                                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Download completed: {video.Title}" });
                                            });
                                        }
                                        catch (Exception e)
                                        {
                                            Trace.WriteLine(e.Message);
                                            Dispatcher.Dispatch(() =>
                                            {
                                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {video.Title}" });
                                            });
                                        }
                                    });
                                    indexCount++;
                                });
                            }
                            catch
                            {
                                Dispatcher.Dispatch(() =>
                                {
                                    downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {video.Title}" });
                                    indexCount++;
                                });
                            }
                        }
                    }
                    catch
                    {
                        try // no youtube playlist? maybe a video?
                        {
                            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var video = await youtubeClient.Videos.GetAsync((VideoId)linkTextBox.Text, _cancellationTokenSource.Token);
                            try
                            {
                                Dispatcher.Dispatch(async () =>
                                {
                                    downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading: {video.Title}" });
                                    await Task.Run(async () =>
                                    {
                                        try
                                        {
                                            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + video.Id);
                                            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                                            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + video.Author + "-" + video.Title + $".{streamInfo.Container}", null, _cancellationTokenSource.Token);
                                            Dispatcher.Dispatch(() =>
                                            {
                                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Download completed: {video.Title}" });
                                            });
                                        }
                                        catch (Exception e)
                                        {
                                            Trace.WriteLine(e.Message);
                                            Dispatcher.Dispatch(() =>
                                            {
                                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {video.Title}" });
                                            });
                                        }
                                    });
                                    indexCount++;
                                });
                            }
                            catch
                            {
                                Dispatcher.Dispatch(() =>
                                {
                                    downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {video.Title}" });
                                    indexCount++;
                                });
                            }
                            return;
                        }
                        catch // nothing? error.
                        {
                            Dispatcher.Dispatch(async () =>
                            {
                                string text = "The link you typed is wrong!";
                                ToastDuration duration = ToastDuration.Long;
                                double fontSize = 14;

                                var toast = Toast.Make(text, duration, fontSize);

                                await toast.Show(new CancellationToken());
                            });
                        }
                    }
                    return;
                }
            }
            foreach (var track in tracks)
            {
                try
                {
                    Dispatcher.Dispatch(async () =>
                    {
                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading: {track.Title}" });
                        await Task.Run(async () =>
                        {
                            string? youTubeID = await spotifyClient.Tracks.GetYoutubeIdAsync(track.Id, _cancellationTokenSource.Token);
                            if (youTubeID != null)
                            {
                                try
                                {
                                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                    var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + youTubeID, _cancellationTokenSource.Token);
                                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                                    await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + track.Artists[0].Name + "-" + track.Title + $".{streamInfo.Container}", null, _cancellationTokenSource.Token);
                                    Dispatcher.Dispatch(() =>
                                    {
                                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Download completed: {track.Title}" });
                                    });
                                }
                                catch (Exception e)
                                {
                                    Trace.WriteLine(e.Message);
                                    Dispatcher.Dispatch(() =>
                                    {
                                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {track.Title}" });
                                    });
                                }
                            }
                            else
                            {
                                Dispatcher.Dispatch(() =>
                                {
                                    downloadInformationStackLayout.Children.Add(new Label() { Text = $"{indexCount}. Downloading failed: {track.Title}" });
                                });
                            }
                        });
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

    private void CancelDownloadingButton_Clicked(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }
}