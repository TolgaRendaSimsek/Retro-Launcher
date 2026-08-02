using System;
using System.Diagnostics;
using System.Reflection;

namespace RetroLauncher.Core.Utilities
{
    public interface IApplicationVersionProvider
    {
        Version InstalledVersion { get; }
        string SemanticVersionString { get; }
        string AssemblyVersionString { get; }
        string InformationalVersionString { get; }
        string ExecutablePath { get; }
    }

    public class ApplicationVersionProvider : IApplicationVersionProvider
    {
        private static ApplicationVersionProvider? _instance;
        public static ApplicationVersionProvider Instance => _instance ??= new ApplicationVersionProvider();

        public Version InstalledVersion { get; }
        public string SemanticVersionString { get; }
        public string AssemblyVersionString { get; }
        public string InformationalVersionString { get; }
        public string ExecutablePath { get; }

        public ApplicationVersionProvider()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // 1. Executable Path
            ExecutablePath = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? AppContext.BaseDirectory;

            // 2. Assembly Version
            Version asmVersion = assembly.GetName().Version ?? new Version(1, 0, 0, 0);
            AssemblyVersionString = asmVersion.ToString();

            // 3. Informational Version
            var infoAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            InformationalVersionString = infoAttr?.InformationalVersion ?? asmVersion.ToString();

            // 4. File Version
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);
            string fileVerStr = fileVersionInfo.ProductVersion ?? fileVersionInfo.FileVersion ?? asmVersion.ToString();

            // 5. Parse Semantic Version
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
