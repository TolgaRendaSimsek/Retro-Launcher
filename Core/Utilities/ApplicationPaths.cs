using System;
using System.IO;

namespace RetroLauncher.Core.Utilities
{
    public static class ApplicationPaths
    {
        public static string BaseDataDir { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RetroLauncher"
        );

        public static string EmulatorsDir => Path.Combine(BaseDataDir, "Emulators");
        public static string BiosDir => Path.Combine(BaseDataDir, "BIOS");
        public static string GamesDir => Path.Combine(BaseDataDir, "Games");
        public static string SavesDir => Path.Combine(BaseDataDir, "Saves");
        public static string ConfigDir => Path.Combine(BaseDataDir, "Config");
        public static string LogsDir => Path.Combine(BaseDataDir, "Logs");
        public static string TempDir => Path.Combine(BaseDataDir, "Temp");
        public static string DownloadsDir => Path.Combine(BaseDataDir, "Downloads");
        public static string CacheDir => Path.Combine(BaseDataDir, "Cache");

        // Central Config File Paths
        public static string EmulatorsJson => Path.Combine(ConfigDir, "emulators.json");
        public static string GamesJson => Path.Combine(ConfigDir, "games.json");
        public static string BiosJson => Path.Combine(ConfigDir, "bios.json");
        public static string SettingsJson => Path.Combine(ConfigDir, "app_settings.json");
        public static string GlobalControllerConfigJson => Path.Combine(ConfigDir, "global_controller_config.json");
        public static string ThemeSettingsJson => Path.Combine(ConfigDir, "theme_settings.json");
        public static string LanguageSettingsJson => Path.Combine(ConfigDir, "language_settings.json");

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(BaseDataDir);
            Directory.CreateDirectory(EmulatorsDir);
            Directory.CreateDirectory(BiosDir);
            Directory.CreateDirectory(GamesDir);
            Directory.CreateDirectory(SavesDir);
            Directory.CreateDirectory(ConfigDir);
            Directory.CreateDirectory(LogsDir);
            Directory.CreateDirectory(TempDir);
            Directory.CreateDirectory(DownloadsDir);
            Directory.CreateDirectory(CacheDir);
        }

        public static string ResolveWritablePath(string relativeOrFullPath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrFullPath))
            {
                return BaseDataDir;
            }

            if (Path.IsPathRooted(relativeOrFullPath))
            {
                return Path.GetFullPath(relativeOrFullPath);
            }

            string normalized = relativeOrFullPath.Replace('\\', '/').TrimStart('/');
            return Path.GetFullPath(Path.Combine(BaseDataDir, normalized));
        }
    }
}
