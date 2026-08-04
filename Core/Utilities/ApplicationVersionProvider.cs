using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace RetroLauncher.Core.Utilities
{
    public interface IApplicationVersionProvider
    {
        Version InstalledVersion { get; }
        string SemanticVersionString { get; }
        string AssemblyVersionString { get; }
        string FileVersionString { get; }
        string InformationalVersionString { get; }
        string ExecutablePath { get; }
        string ProcessPath { get; }
        string BaseDirectory { get; }
        string BuildConfiguration { get; }
        DateTime ExecutableLastWriteTime { get; }
        string ExecutableTimestampString { get; }
    }

    public class ApplicationVersionProvider : IApplicationVersionProvider
    {
        private static ApplicationVersionProvider? _instance;
        public static ApplicationVersionProvider Instance => _instance ??= new ApplicationVersionProvider();

        public Version InstalledVersion { get; }
        public string SemanticVersionString { get; }
        public string AssemblyVersionString { get; }
        public string FileVersionString { get; }
        public string InformationalVersionString { get; }
        public string ExecutablePath { get; }
        public string ProcessPath { get; }
        public string BaseDirectory { get; }
        public string BuildConfiguration { get; }
        public DateTime ExecutableLastWriteTime { get; }
        public string ExecutableTimestampString { get; }

        public ApplicationVersionProvider()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // 1. Executable Path & Base Directory
            ProcessPath = Environment.ProcessPath ?? AppContext.BaseDirectory;
            ExecutablePath = Process.GetCurrentProcess().MainModule?.FileName ?? ProcessPath;
            BaseDirectory = AppContext.BaseDirectory;

            // 2. Executable Timestamp
            if (File.Exists(ExecutablePath))
            {
                ExecutableLastWriteTime = File.GetLastWriteTime(ExecutablePath);
            }
            else
            {
                ExecutableLastWriteTime = DateTime.Now;
            }
            ExecutableTimestampString = ExecutableLastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

            // 3. Build Configuration
#if DEBUG
            BuildConfiguration = "Debug";
#else
            BuildConfiguration = "Release";
#endif

            // 4. Assembly Version
            Version asmVersion = assembly.GetName().Version ?? new Version(1, 0, 0, 0);
            AssemblyVersionString = asmVersion.ToString();

            // 5. Informational Version
            var infoAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            InformationalVersionString = infoAttr?.InformationalVersion ?? asmVersion.ToString();

            // 6. File Version
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);
            FileVersionString = fileVersionInfo.FileVersion ?? fileVersionInfo.ProductVersion ?? asmVersion.ToString();
            string fileVerStr = FileVersionString;

            // 7. Parse Semantic Version
            if (Version.TryParse(CleanVersionString(InformationalVersionString), out Version? parsedInfoVer))
            {
                InstalledVersion = parsedInfoVer;
            }
            else if (Version.TryParse(CleanVersionString(fileVerStr), out Version? parsedFileVer))
            {
                InstalledVersion = parsedFileVer;
            }
            else
            {
                InstalledVersion = new Version(asmVersion.Major, Math.Max(0, asmVersion.Minor), Math.Max(0, asmVersion.Build));
            }

            SemanticVersionString = InstalledVersion.ToString();
        }

        private static string CleanVersionString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "1.0.0";
            string clean = input.Trim();
            if (clean.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(1);
            }
            int plusIndex = clean.IndexOf('+');
            if (plusIndex >= 0)
            {
                clean = clean.Substring(0, plusIndex);
            }
            int dashIndex = clean.IndexOf('-');
            if (dashIndex >= 0)
            {
                clean = clean.Substring(0, dashIndex);
            }
            return clean;
        }
    }
}
