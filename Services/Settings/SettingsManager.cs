using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Services.Settings
{
    public class SettingsConfig
    {
        public Dictionary<string, string> DefaultEmulators { get; set; } = new();
        public bool IsFirstRun { get; set; } = true;
        public int WindowWidth { get; set; } = 1100;
        public int WindowHeight { get; set; } = 650;
        public int WindowLeft { get; set; } = -1;
        public int WindowTop { get; set; } = -1;
        public bool IsMaximized { get; set; } = false;
    }

    public static class SettingsManager
    {
        public static SettingsConfig LoadSettings()
        {
            var service = ApplicationSettingsService.Instance;
            service.LoadSettings();
            return new SettingsConfig
            {
                DefaultEmulators = service.DefaultEmulators,
                IsFirstRun = service.IsFirstRun,
                WindowWidth = service.WindowWidth,
                WindowHeight = service.WindowHeight,
                WindowLeft = service.WindowLeft,
                WindowTop = service.WindowTop,
                IsMaximized = service.IsMaximized
            };
        }

        public static void SaveSettings(SettingsConfig settings)
        {
            var service = ApplicationSettingsService.Instance;
            service.DefaultEmulators = settings.DefaultEmulators;
            service.IsFirstRun = settings.IsFirstRun;
            service.WindowWidth = settings.WindowWidth;
            service.WindowHeight = settings.WindowHeight;
            service.WindowLeft = settings.WindowLeft;
            service.WindowTop = settings.WindowTop;
            service.IsMaximized = settings.IsMaximized;
            service.SaveSettings();
        }
    }
}
