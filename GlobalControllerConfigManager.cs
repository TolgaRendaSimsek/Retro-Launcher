using System;
using System.IO;
using System.Text.Json;

namespace RetroLauncher
{
    public class GlobalControllerConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "controller_global_settings.json");
        private static readonly object FileLock = new object();
        private static GlobalControllerConfigManager? _instance;
        public static GlobalControllerConfigManager Instance => _instance ??= new GlobalControllerConfigManager();

        public GlobalControllerConfig Config { get; private set; } = new();

        public GlobalControllerConfigManager()
        {
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ConfigPath))
                    {
                        string json = File.ReadAllText(ConfigPath);
                        Config = JsonSerializer.Deserialize<GlobalControllerConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new GlobalControllerConfig();
                    }
                    else
                    {
                        Config = new GlobalControllerConfig();
                        Save();
                    }
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Error loading global controller settings: {ex.Message}. Seeding default settings.", "WARNING");
                    Config = new GlobalControllerConfig();
                }
            }
        }

        public void Save()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, json);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to save global controller config: {ex.Message}", "ERROR");
                }
            }
        }
    }
}
