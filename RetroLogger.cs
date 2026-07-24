using System;
using System.IO;

namespace RetroLauncher
{
    public static class RetroLogger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "package_manager.log");
        private static readonly object LogLock = new object();

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                lock (LogLock)
                {
                    File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
                }
            }
            catch { }
            System.Diagnostics.Debug.WriteLine($"[{level}] {message}");
        }
    }
}
