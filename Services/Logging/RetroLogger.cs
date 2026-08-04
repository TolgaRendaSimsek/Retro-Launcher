using System;
using System.IO;

namespace RetroLauncher.Services.Logging
{
    public static class RetroLogger
    {
        private static readonly string LogPath = Path.Combine(ApplicationPaths.LogsDir, "package_manager.log");
        private static readonly object LogLock = new object();

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                lock (LogLock)
                {
                    using (var fs = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fs))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                    }
                }
            }
            catch { }
            System.Diagnostics.Debug.WriteLine($"[{level}] {message}");
        }
    }
}
