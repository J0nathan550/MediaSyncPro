using System.Text.Json;

namespace MediaSyncPro.Classes
{
    public class SettingsClass
    {
        public static SettingsClass? settingsClass = new();
        public string SavePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\MediaSync Pro";
        public static void LoadSettings()
        {
            settingsClass = new SettingsClass();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "save.json"))
            {
                string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "save.json");
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
            string json = JsonSerializer.Serialize(settingsClass);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "save.json", json);
        }
    }
}