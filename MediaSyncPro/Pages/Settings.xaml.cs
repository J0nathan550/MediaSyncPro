using CommunityToolkit.Maui.Storage;
using MediaSyncPro.Classes;

namespace MediaSyncPro.Pages;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();
        SettingsClass.LoadSettings();
        if (SettingsClass.settingsClass != null) settingsPath.Text = SettingsClass.settingsClass.SavePath;
    }

    private async void SettingsButtonPath_ClickedAsync(object sender, EventArgs e)
    {
        if (SettingsClass.settingsClass == null) return;
        if (!Path.Exists(SettingsClass.settingsClass.SavePath))
        {
            SettingsClass.LoadSettings();
            settingsPath.Text = SettingsClass.settingsClass.SavePath;
        }
        var result = await FolderPicker.PickAsync(SettingsClass.settingsClass.SavePath, new CancellationToken());
        if (result.IsSuccessful)
        {
            SettingsClass.settingsClass.SavePath = result.Folder.Path;
        }
        settingsPath.Text = SettingsClass.settingsClass.SavePath;
        SettingsClass.SaveSettings();
    }
}