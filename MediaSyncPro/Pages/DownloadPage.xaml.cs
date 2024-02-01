using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SpotifyExplode;
using SpotifyExplode.Tracks;
using System.Diagnostics;
using System.Net.NetworkInformation;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MediaSyncPro.Pages;

public partial class DownloadPage : ContentPage
{
    public DownloadPage()
    {
        InitializeComponent();
    }

    private CancellationTokenSource? cancelDownloadToken;
    private List<Task> downloadTasks = [];
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

        downloadTasks = [];
        cancelDownloadToken = new CancellationTokenSource();

        Dispatcher.Dispatch(() =>
        {
            startDownloadBtn.IsEnabled = false;
            cancelDownloadBtn.IsEnabled = true;
        });

        downloadInformationStackLayout.Children.Clear();
        await Task.Run(async () =>
        {
            SpotifyClient spotifyClient = new();
            YoutubeClient youtubeClient = new();
            bool completed = false;
            _ = Task.Run(async () =>
            {
                while (!completed)
                {
                    try
                    {
                        using Ping ping = new();
                        PingReply reply = await ping.SendPingAsync("google.com");
                        if (reply == null || reply.Status != IPStatus.Success)
                        {
                            CancelDownload();
                        }
                        await Task.Delay(15000);
                    }
                    catch
                    {
                        CancelDownload();
                    }
                }
            }); // ping internet to check for errors
            List<Track>? tracks = await GetSpotifyTracksInList(spotifyClient, cancelDownloadToken.Token);
            if (tracks == null)
            {
                var track = await GetSpotifyTrack(spotifyClient, cancelDownloadToken.Token);
                if (track != null)
                {
                    tracks = [track];
                }
                else // youtube playlist
                {
                    var videos = await GetYouTubePlaylist(youtubeClient, cancelDownloadToken.Token);
                    if (videos == null)
                    {
                        var video = await GetYouTubeVideo(youtubeClient, cancelDownloadToken.Token);
                        if (video == null)
                        {
                            CancelDownload();
                            Dispatcher.Dispatch(async () =>
                            {
                                string text = "The link you typed is wrong, or you have problems with internet connection!";
                                ToastDuration duration = ToastDuration.Long;
                                double fontSize = 14;

                                startDownloadBtn.IsEnabled = true;
                                cancelDownloadBtn.IsEnabled = false;

                                var toast = Toast.Make(text, duration, fontSize);

                                await toast.Show(new CancellationToken());
                            });
                        }
                        else
                        {
                            await DownloadYouTubeVideo(youtubeClient, video, cancelDownloadToken.Token);
                        }
                    }
                    else
                    {
                        await DownloadYouTubeVideos(youtubeClient, videos, cancelDownloadToken.Token);
                    }
                }
            }
            if (tracks != null)
            {
                await DownloadTracks(tracks, spotifyClient, youtubeClient, cancelDownloadToken.Token);
            }
            else
            {
                CancelDownload();
            }
            completed = true;
        });
    }

    private async Task<List<Track>?> GetSpotifyTracksInList(SpotifyClient spotifyClient, CancellationToken token)
    {
        try
        {
            return (await spotifyClient.Playlists.GetAsync((SpotifyExplode.Playlists.PlaylistId)linkTextBox.Text, token)).Tracks;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Track?> GetSpotifyTrack(SpotifyClient spotifyClient, CancellationToken token)
    {
        try
        {
            return await spotifyClient.Tracks.GetAsync((TrackId)linkTextBox.Text, token);
        }
        catch
        {
            return null;
        }
    }

    private async Task DownloadTracks(List<Track> tracks, SpotifyClient spotifyClient, YoutubeClient youtubeClient, CancellationToken token)
    {
        try
        {
            foreach (var track in tracks)
            {
                Task task = Task.Run(async () =>
                {
                    string? youTubeID = await spotifyClient.Tracks.GetYoutubeIdAsync(track.Id, token);
                    if (youTubeID != null)
                    {
                        try
                        {
                            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + youTubeID, token);
                            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + track.Artists[0].Name + "-" + track.Title + $".{streamInfo.Container}", null, token);
                            Dispatcher.Dispatch(() =>
                            {
                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"Download completed: {track.Title}" });
                            });
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.Message);
                            Dispatcher.Dispatch(() =>
                            {
                                downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading failed: {track.Title}" });
                            });
                        }
                    }
                    else
                    {
                        Dispatcher.Dispatch(() =>
                        {
                            downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading failed: {track.Title}" });
                        });
                    }
                }, token);
                downloadTasks.Add(task);
            }
            await Task.WhenAll(downloadTasks);
            downloadTasks = [];
            Dispatcher.Dispatch(() =>
            {
                startDownloadBtn.IsEnabled = true;
                cancelDownloadBtn.IsEnabled = false;
            });
        }
        catch
        {
            CancelDownload();
        }
    }

    private async Task<List<YoutubeExplode.Playlists.PlaylistVideo>?> GetYouTubePlaylist(YoutubeClient youtubeClient, CancellationToken token)
    {
        try
        {
            return (List<YoutubeExplode.Playlists.PlaylistVideo>?)await youtubeClient.Playlists.GetVideosAsync((YoutubeExplode.Playlists.PlaylistId)linkTextBox.Text, token);
        }
        catch
        {
            return null;
        }
    }

    private async Task DownloadYouTubeVideos(YoutubeClient youtubeClient, List<YoutubeExplode.Playlists.PlaylistVideo> videos, CancellationToken token)
    {
        try
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            foreach (var video in videos)
            {
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + video.Id, token);
                        var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                        await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + video.Author + "-" + video.Title + $".{streamInfo.Container}", null, token);
                        Dispatcher.Dispatch(() =>
                        {
                            downloadInformationStackLayout.Children.Add(new Label() { Text = $"Download completed: {video.Title}" });
                        });
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                        Dispatcher.Dispatch(() =>
                        {
                            downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading failed: {video.Title}" });
                        });
                    }
                }, token);
                downloadTasks.Add(task);
            }
            await Task.WhenAll(downloadTasks);
            downloadTasks = [];
            Dispatcher.Dispatch(() =>
            {
                startDownloadBtn.IsEnabled = true;
                cancelDownloadBtn.IsEnabled = false;
            });
        }
        catch
        {
            CancelDownload();
        }
    }

    private async Task<Video?> GetYouTubeVideo(YoutubeClient client, CancellationToken token)
    {
        try
        {
            return await client.Videos.GetAsync((VideoId)linkTextBox.Text, token);
        }
        catch
        {
            return null;
        }
    }

    private async Task DownloadYouTubeVideo(YoutubeClient youtubeClient, Video video, CancellationToken token)
    {
        try // no youtube playlist? maybe a video?
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Task task = Task.Run(async () =>
            {
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/MediaSync Pro";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + video.Id);
                    var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                    await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path + "/" + video.Author + "-" + video.Title + $".{streamInfo.Container}", null, token);
                    Dispatcher.Dispatch(() =>
                    {
                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"Download completed: {video.Title}" });
                    });
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    Dispatcher.Dispatch(() =>
                    {
                        downloadInformationStackLayout.Children.Add(new Label() { Text = $"Downloading failed: {video.Title}" });
                    });
                }
            }, token);
            downloadTasks.Add(task);
            await Task.WhenAll(downloadTasks);
            downloadTasks = [];
            Dispatcher.Dispatch(() =>
            {
                startDownloadBtn.IsEnabled = true;
                cancelDownloadBtn.IsEnabled = false;
            });
        }
        catch // nothing? error.
        {
            CancelDownload();
        }
    }

    private void CancelDownload()
    {
        cancelDownloadToken?.Cancel();
        downloadTasks = [];
        Dispatcher.Dispatch(() =>
        {
            startDownloadBtn.IsEnabled = true;
            cancelDownloadBtn.IsEnabled = false;
        });
    }

    private void CancelDownloadingButton_Clicked(object sender, EventArgs e)
    {
        CancelDownload();
    }
}