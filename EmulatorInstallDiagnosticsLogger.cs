using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RetroLauncher
{
    public class InstallationDiagnosticSession
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC");
        public string OperationId { get; set; } = "";
        public string PackageId { get; set; } = "";
        public string CurrentStage { get; set; } = "N/A";
        public List<string> GitHubApiEndpoints { get; set; } = new List<string>();
        public string HttpStatusCode { get; set; } = "N/A";
        public string SelectedReleaseTag { get; set; } = "N/A";
        public List<string> CandidateAssetNames { get; set; } = new List<string>();
        public string SelectedAssetAndScore { get; set; } = "N/A";
        public long ExpectedSize { get; set; } = -1;
        public long DownloadedSize { get; set; } = -1;
        public string TemporaryFilePath { get; set; } = "N/A";
        public string ArchiveType { get; set; } = "N/A";
        public string ExtractionDestination { get; set; } = "N/A";
        public List<string> DiscoveredExecutableCandidates { get; set; } = new List<string>();
        public string FinalExecutablePath { get; set; } = "N/A";
        public string CleanupResult { get; set; } = "N/A";
        public string ExceptionType { get; set; } = "None";
        public string ExceptionStackTrace { get; set; } = "None";
        public List<string> Logs { get; set; } = new List<string>();
    }

    public static class EmulatorInstallDiagnosticsLogger
    {
        private static readonly string BaseLogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "package-installation");
        private static readonly object FileLock = new object();
        private static readonly ConcurrentDictionary<string, InstallationDiagnosticSession> Sessions = new ConcurrentDictionary<string, InstallationDiagnosticSession>();

        public static string GetLogFilePath(string operationId)
        {
            return Path.Combine(BaseLogDirectory, $"{operationId}.log");
        }

        public static void StartSession(string operationId, string packageId)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                var session = new InstallationDiagnosticSession
                {
                    OperationId = operationId,
                    PackageId = packageId
                };
                Sessions[operationId] = session;
                LogToSession(operationId, $"Started installation operation for package '{packageId}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting session: {ex.Message}");
            }
        }

        public static void LogToSession(string operationId, string message)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging to session: {ex.Message}");
            }
        }

        public static void SetStage(string operationId, string stage)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.CurrentStage = stage;
                    }
                }
            }
            catch { }
        }

        public static void AddGitHubApiEndpoint(string operationId, string url)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    string cleanUrl = RemoveSecrets(url);
                    lock (session)
                    {
                        if (!session.GitHubApiEndpoints.Contains(cleanUrl))
                        {
                            session.GitHubApiEndpoints.Add(cleanUrl);
                        }
                    }
                }
            }
            catch { }
        }

        private static string RemoveSecrets(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            try
            {
                string clean = Regex.Replace(url, @"token=[a-zA-Z0-9_\-]+", "token=REDACTED");
                clean = Regex.Replace(clean, @"access_token=[a-zA-Z0-9_\-]+", "access_token=REDACTED");
                return clean;
            }
            catch
            {
                return url;
            }
        }

        public static void SetHttpStatusCode(string operationId, int statusCode)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.HttpStatusCode = statusCode.ToString();
                    }
                }
            }
            catch { }
        }

        public static void SetReleaseTag(string operationId, string tag)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.SelectedReleaseTag = tag;
                    }
                }
            }
            catch { }
        }

        public static void SetCandidateAssetNames(string operationId, IEnumerable<string> names)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.CandidateAssetNames = names.ToList();
                    }
                }
            }
            catch { }
        }

        public static void SetSelectedAssetAndScore(string operationId, string assetName, int score)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.SelectedAssetAndScore = $"{assetName} (Score: {score})";
                    }
                }
            }
            catch { }
        }

        public static void SetExpectedSize(string operationId, long size)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.ExpectedSize = size;
                    }
                }
            }
            catch { }
        }

        public static void SetDownloadedSize(string operationId, long size)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.DownloadedSize = size;
                    }
                }
            }
            catch { }
        }

        public static void SetTemporaryFilePath(string operationId, string path)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.TemporaryFilePath = path;
                    }
                }
            }
            catch { }
        }

        public static void SetArchiveType(string operationId, string type)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.ArchiveType = type;
                    }
                }
            }
            catch { }
        }

        public static void SetExtractionDestination(string operationId, string path)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.ExtractionDestination = path;
                    }
                }
            }
            catch { }
        }

        public static void SetDiscoveredExecutables(string operationId, IEnumerable<string> candidates)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.DiscoveredExecutableCandidates = candidates.ToList();
                    }
                }
            }
            catch { }
        }

        public static void SetFinalExecutablePath(string operationId, string path)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.FinalExecutablePath = path;
                    }
                }
            }
            catch { }
        }

        public static void SetCleanupResult(string operationId, string result)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.CleanupResult = result;
                    }
                }
            }
            catch { }
        }

        public static void SetException(string operationId, Exception ex)
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryGetValue(operationId, out var session))
                {
                    lock (session)
                    {
                        session.ExceptionType = ex.GetType().FullName ?? ex.GetType().Name;
                        session.ExceptionStackTrace = ex.ToString();
                    }
                }
            }
            catch { }
        }

        public static void CompleteSession(string operationId, bool success, string message = "")
        {
            if (string.IsNullOrEmpty(operationId)) return;
            try
            {
                if (Sessions.TryRemove(operationId, out var session))
                {
                    if (!Directory.Exists(BaseLogDirectory))
                    {
                        Directory.CreateDirectory(BaseLogDirectory);
                    }

                    string logFilePath = GetLogFilePath(operationId);

                    var sb = new StringBuilder();
                    sb.AppendLine("================================================================================");
                    sb.AppendLine($"EMULATOR INSTALLATION DIAGNOSTIC LOG - {session.PackageId.ToUpper()}");
                    sb.AppendLine("================================================================================");
                    sb.AppendLine($"Timestamp:               {session.Timestamp}");
                    sb.AppendLine($"Operation (Correlation): {session.OperationId}");
                    sb.AppendLine($"Package ID:              {session.PackageId}");
                    sb.AppendLine($"Status:                  {(success ? "SUCCESS" : "FAILED")}");
                    if (!success && !string.IsNullOrEmpty(message))
                    {
                        sb.AppendLine($"Error Message:           {message}");
                    }
                    sb.AppendLine($"Current Stage:           {session.CurrentStage}");
                    sb.AppendLine($"Selected Release Tag:    {session.SelectedReleaseTag}");
                    sb.AppendLine($"Selected Asset & Score:  {session.SelectedAssetAndScore}");
                    sb.AppendLine($"Expected Size:           {FormatBytes(session.ExpectedSize)}");
                    sb.AppendLine($"Downloaded Size:         {FormatBytes(session.DownloadedSize)}");
                    sb.AppendLine($"Temporary File Path:     {session.TemporaryFilePath}");
                    sb.AppendLine($"Archive Type:            {session.ArchiveType}");
                    sb.AppendLine($"Extraction Destination:  {session.ExtractionDestination}");
                    sb.AppendLine($"Final Executable Path:   {session.FinalExecutablePath}");
                    sb.AppendLine($"Cleanup Result:          {session.CleanupResult}");
                    sb.AppendLine($"HTTP Status Code:        {session.HttpStatusCode}");

                    sb.AppendLine();
                    sb.AppendLine("--- GitHub API Endpoints Visited ---");
                    if (session.GitHubApiEndpoints.Any())
                    {
                        foreach (var url in session.GitHubApiEndpoints)
                        {
                            sb.AppendLine($"* {url}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("None");
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Candidate Asset Names ---");
                    if (session.CandidateAssetNames.Any())
                    {
                        foreach (var asset in session.CandidateAssetNames)
                        {
                            sb.AppendLine($"* {asset}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("None");
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Discovered Executables inside Archive ---");
                    if (session.DiscoveredExecutableCandidates.Any())
                    {
                        foreach (var exe in session.DiscoveredExecutableCandidates)
                        {
                            sb.AppendLine($"* {exe}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("None");
                    }

                    if (session.ExceptionType != "None")
                    {
                        sb.AppendLine();
                        sb.AppendLine("--- Exception details ---");
                        sb.AppendLine($"Type: {session.ExceptionType}");
                        sb.AppendLine(session.ExceptionStackTrace);
                    }

                    sb.AppendLine();
                    sb.AppendLine("--- Step-by-Step Execution Log ---");
                    lock (session)
                    {
                        foreach (var logLine in session.Logs)
                        {
                            sb.AppendLine(logLine);
                        }
                    }

                    sb.AppendLine("================================================================================");

                    lock (FileLock)
                    {
                        File.WriteAllText(logFilePath, sb.ToString());
                    }

                    RotateLogs();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to complete session or write log: {ex.Message}");
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "Unknown";
            double mb = (double)bytes / (1024 * 1024);
            return $"{mb:F2} MB ({bytes} bytes)";
        }

        private static void RotateLogs()
        {
            try
            {
                if (!Directory.Exists(BaseLogDirectory)) return;

                var dirInfo = new DirectoryInfo(BaseLogDirectory);
                var files = dirInfo.GetFiles("*.log")
                                   .OrderByDescending(f => f.LastWriteTime)
                                   .ToList();

                int maxLogs = 50;
                if (files.Count > maxLogs)
                {
                    for (int i = maxLogs; i < files.Count; i++)
                    {
                        try
                        {
                            files[i].Delete();
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}
