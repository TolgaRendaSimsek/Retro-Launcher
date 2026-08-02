using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RetroLauncher.Core.Utilities;

namespace RetroLauncher.Services.Updates
{
    public class ApplicationUpdateService
    {
        private static ApplicationUpdateService? _instance;
        public static ApplicationUpdateService Instance => _instance ??= new ApplicationUpdateService();

        private readonly IApplicationVersionProvider _versionProvider;
        private readonly IGitHubApplicationReleaseClient _githubClient;
        private readonly IApplicationUpdateDownloader _downloader;
        private readonly IApplicationUpdateInstaller _installer;

        public ApplicationUpdateStatus Status { get; private set; } = ApplicationUpdateStatus.NotChecked;
        public ApplicationUpdateCheckResult? LastCheckResult { get; private set; }
        public string? DownloadedPackagePath { get; private set; }
        public string? StagedPackagePath { get; private set; }

        public bool AllowPrerelease { get; set; } = false;
        public bool EnableDevBuildChecks { get; set; } = false;

        private static readonly string LogDir = Path.Combine(ApplicationPaths.LogsDir, "ApplicationUpdates");
        private static readonly string LogFilePath = Path.Combine(LogDir, "app_updates.log");
        private static readonly object LogLock = new object();

        public ApplicationUpdateService(
            IApplicationVersionProvider? versionProvider = null,
            IGitHubApplicationReleaseClient? githubClient = null,
            IApplicationUpdateDownloader? downloader = null,
            IApplicationUpdateInstaller? installer = null)
        {
            _versionProvider = versionProvider ?? ApplicationVersionProvider.Instance;
            _githubClient = githubClient ?? new GitHubApplicationReleaseClient();
            _downloader = downloader ?? new ApplicationUpdateDownloader();
            _installer = installer ?? new ApplicationUpdateInstaller();
        }

        public async Task<ApplicationUpdateCheckResult> CheckForUpdatesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            if (Status == ApplicationUpdateStatus.Checking)
            {
                return LastCheckResult ?? ApplicationUpdateCheckResult.Fail("BUSY", "An update check is already in progress.");
            }

            // Check dev build restriction
#if DEBUG
            if (!EnableDevBuildChecks && !forceRefresh)
            {
                Log("Update check skipped in Debug/Dev mode (EnableDevBuildChecks = false).");
                Status = ApplicationUpdateStatus.NotChecked;
                return ApplicationUpdateCheckResult.Fail("DEV_BUILD", "Update checks disabled in development build.", _versionProvider.InstalledVersion);
            }
#endif

            Status = ApplicationUpdateStatus.Checking;
            Log($"Starting update check... Installed Version: {_versionProvider.SemanticVersionString}");

            var result = await _githubClient.CheckForLatestReleaseAsync(_versionProvider, AllowPrerelease, cancellationToken);
            LastCheckResult = result;

            if (!result.CheckSucceeded)
            {
                Status = ApplicationUpdateStatus.CheckFailed;
                Log($"Update check failed. Code: {result.ErrorCode}, Message: {result.ErrorMessage}");
            }
            else if (result.UpdateAvailable)
            {
                Status = ApplicationUpdateStatus.UpdateAvailable;
                Log($"Update available! Remote Tag: {result.ReleaseTag}, Asset: {result.AssetName}, Size: {result.AssetSize} bytes");
            }
            else
            {
                Status = ApplicationUpdateStatus.UpToDate;
                Log($"Application is up to date ({result.CurrentVersion}).");
            }

            return result;
        }

        public async Task<string> DownloadUpdateAsync(IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            if (LastCheckResult == null || !LastCheckResult.UpdateAvailable)
            {
                throw new InvalidOperationException("No confirmed update available to download.");
            }

            Status = ApplicationUpdateStatus.Downloading;
            Log($"Downloading update asset: {LastCheckResult.AssetName} from {LastCheckResult.DownloadUri}");

            try
            {
                string downloadedPath = await _downloader.DownloadUpdatePackageAsync(LastCheckResult, progress, cancellationToken);
                DownloadedPackagePath = downloadedPath;
                Status = ApplicationUpdateStatus.ReadyToInstall;
                Log($"Download completed successfully: {downloadedPath}");
                return downloadedPath;
            }
            catch (Exception ex)
            {
                Status = ApplicationUpdateStatus.Failed;
                Log($"Download failed: {ex.Message}");
                throw;
            }
        }

        public async Task PrepareAndInstallUpdateAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(DownloadedPackagePath) || !File.Exists(DownloadedPackagePath))
            {
                throw new InvalidOperationException("Downloaded update package is not ready.");
            }

            Status = ApplicationUpdateStatus.Installing;
            Log($"Staging package: {DownloadedPackagePath}");

            try
            {
                StagedPackagePath = await _installer.StagePackageAsync(DownloadedPackagePath, cancellationToken);
                Status = ApplicationUpdateStatus.RestartRequired;
                Log($"Staging completed: {StagedPackagePath}. Launching external updater...");

                int currentPid = Environment.ProcessId;
                string currentExe = _versionProvider.ExecutablePath;

                _installer.LaunchUpdaterProcessAndExit(StagedPackagePath, currentExe, currentPid);
            }
            catch (Exception ex)
            {
                Status = ApplicationUpdateStatus.Failed;
                Log($"Update installation staging failed: {ex.Message}");
                throw;
            }
        }

        public void Log(string message)
        {
            try
            {
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }

                lock (LogLock)
                {
                    File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{Status}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }
    }
}
