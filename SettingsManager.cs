using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class SettingsConfig
    {
        public Dictionary<string, string> DefaultEmulators { get; set; } = new();
        public bool IsFirstRun { get; set; } = true;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static SettingsConfig LoadSettings()
        {
            try
            {
                string pathToUse = SettingsPath;
                
                if (!File.Exists(pathToUse))
                {
                    string localPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                    if (File.Exists(localPath))
                    {
                        pathToUse = localPath;
                    }
                }

                if (!File.Exists(pathToUse))
                {
                    return new SettingsConfig();
                }

                string json = File.ReadAllText(pathToUse);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<SettingsConfig>(json, options) ?? new SettingsConfig();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return new SettingsConfig();
            }
        }

        public static void SaveSettings(SettingsConfig settings)
        {
            try
            {
                string pathToUse = SettingsPath;
                if (!File.Exists(pathToUse))
                {
                    string localPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                    if (File.Exists(localPath))
                    {
                        pathToUse = localPath;
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(settings, options);
                
                string? dir = Path.GetDirectoryName(pathToUse);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                File.WriteAllText(pathToUse, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save settings.\n\nError: {ex.Message}",
                    "Settings Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
