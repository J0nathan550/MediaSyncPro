using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediaSyncPro.Classes
{
    public class SettingsClass
    {
        public static SettingsClass? settingsClass = new();
        public string SavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\MediaSync Pro";
        [JsonIgnore] public string SaveSettingsPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\J0nathan550" + @"\MediaSync Pro";
        public static void LoadSettings()
        {
            settingsClass = new SettingsClass();
            if (!Directory.Exists(settingsClass.SaveSettingsPath))
            {
                Directory.CreateDirectory(settingsClass.SaveSettingsPath);
            }
            if (File.Exists(Path.Combine(settingsClass.SaveSettingsPath, "save.json")))
            {
                string json = File.ReadAllText(Path.Combine(settingsClass.SaveSettingsPath, "save.json"));
                settingsClass = JsonSerializer.Deserialize<SettingsClass>(json);
            }
            else
            {
                settingsClass = new SettingsClass();
                SaveSettings();
                return;
            }
            if (settingsClass != null)
            {
                if (!Path.Exists(settingsClass.SavePath))
                {
                    settingsClass = new SettingsClass();
                    SaveSettings();
                }
            }
            else
            {
                settingsClass = new SettingsClass();
                SaveSettings();
            }
        }
        public static void SaveSettings()
        {
            settingsClass ??= new SettingsClass();
            string json = JsonSerializer.Serialize(settingsClass);
            File.WriteAllText(Path.Combine(settingsClass.SaveSettingsPath, "save.json"), json);
        }
    }
}