using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RetroLauncher.Core.Utilities;

namespace RetroLauncher.Services.Updates
{
    public interface IApplicationUpdateInstaller
    {
        Task<string> StagePackageAsync(string packageZipPath, CancellationToken cancellationToken = default);

        void LaunchUpdaterProcessAndExit(
            string stagingPath,
            string currentExePath,
            int currentProcessId);
    }

    public class ApplicationUpdateInstaller : IApplicationUpdateInstaller
    {
        private static readonly string StagingBaseDir = Path.Combine(ApplicationPaths.BaseDataDir, "Updates", "Staging");
        private static readonly string BackupBaseDir = Path.Combine(ApplicationPaths.BaseDataDir, "Updates", "Backups");

        public async Task<string> StagePackageAsync(string packageZipPath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(packageZipPath))
            {
                throw new FileNotFoundException("Update package file not found.", packageZipPath);
            }

            string stagingPath = Path.Combine(StagingBaseDir, Guid.NewGuid().ToString("N"));
            if (Directory.Exists(stagingPath))
            {
                Directory.Delete(stagingPath, true);
            }
            Directory.CreateDirectory(stagingPath);

            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(packageZipPath, stagingPath, overwriteFiles: true);
            }, cancellationToken);

            // Search for RetroLauncher.exe in staging (top-level or single subfolder)
            string stagedExe = Path.Combine(stagingPath, "RetroLauncher.exe");
            if (!File.Exists(stagedExe))
            {
                var subDirs = Directory.GetDirectories(stagingPath);
                if (subDirs.Length == 1 && File.Exists(Path.Combine(subDirs[0], "RetroLauncher.exe")))
                {
                    stagingPath = subDirs[0];
                    stagedExe = Path.Combine(stagingPath, "RetroLauncher.exe");
                }
            }

            if (!File.Exists(stagedExe))
            {
                throw new InvalidDataException("Staged update package does not contain RetroLauncher.exe.");
            }

            return stagingPath;
        }

        public void LaunchUpdaterProcessAndExit(
            string stagingPath,
            string currentExePath,
            int currentProcessId)
        {
            string targetDir = Path.GetDirectoryName(currentExePath)!;
            string backupPath = Path.Combine(BackupBaseDir, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}");

            // Arguments for updater mode
            string args = $"--updater --source \"{stagingPath}\" --target \"{targetDir}\" --process-id {currentProcessId} --restart \"{currentExePath}\" --backup \"{backupPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = currentExePath,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(psi);
            Environment.Exit(0);
        }

        public static bool RunUpdaterCLIIfRequested(string[] args)
        {
            if (args == null || args.Length == 0 || !args[0].Equals("--updater", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string source = "";
            string target = "";
            int pid = 0;
            string restartExe = "";
            string backup = "";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--source", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    source = args[i + 1];
                else if (args[i].Equals("--target", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    target = args[i + 1];
                else if (args[i].Equals("--process-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    int.TryParse(args[i + 1], out pid);
                else if (args[i].Equals("--restart", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    restartExe = args[i + 1];
                else if (args[i].Equals("--backup", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    backup = args[i + 1];
            }

            ExecuteUpdateReplacement(source, target, pid, restartExe, backup);
            return true;
        }

        private static void ExecuteUpdateReplacement(string sourceDir, string targetDir, int processId, string restartExe, string backupDir)
        {
            string logPath = Path.Combine(ApplicationPaths.LogsDir, "ApplicationUpdates", "updater_runner.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            void Log(string msg)
            {
                try { File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}"); } catch { }
            }

            Log($"Updater runner launched. PID={processId}, Source={sourceDir}, Target={targetDir}");

            // 1. Wait for main process to exit
            if (processId > 0)
            {
                try
                {
                    var proc = Process.GetProcessById(processId);
                    if (!proc.HasExited)
                    {
                        Log($"Waiting for process {processId} to exit...");
                        proc.WaitForExit(10000);
                    }
                }
                catch { }
            }

            Thread.Sleep(1000);

            try
            {
                // 2. Backup current target directory files
                if (!string.IsNullOrEmpty(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    foreach (var file in Directory.GetFiles(targetDir))
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            File.Copy(file, Path.Combine(backupDir, fileName), true);
                        }
                        catch { }
                    }
                }

                // 3. Copy staged files to target directory
                foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    string relPath = Path.GetRelativePath(sourceDir, file);
                    string destPath = Path.Combine(targetDir, relPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    File.Copy(file, destPath, true);
                }

                Log("Files successfully replaced.");

                // 4. Restart RetroLauncher
                if (File.Exists(restartExe))
                {
                    Log($"Restarting application: {restartExe}");
                    Process.Start(new ProcessStartInfo { FileName = restartExe, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Log($"Update replacement failed: {ex.Message}");
                // Rollback
                if (!string.IsNullOrEmpty(backupDir) && Directory.Exists(backupDir))
                {
                    try
                    {
                        foreach (var file in Directory.GetFiles(backupDir))
                        {
                            string fileName = Path.GetFileName(file);
                            File.Copy(file, Path.Combine(targetDir, fileName), true);
                        }
                        Log("Rollback completed.");
                    }
                    catch { }
                }
            }

            Environment.Exit(0);
        }
    }
}
