using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace RetroLauncherUpdater
{
    class Program
    {
        private static string _logPath = "";

        static int Main(string[] args)
        {
            int parentPid = 0;
            string zipPath = "";
            string targetDir = "";

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--pid" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out parentPid);
                }
                else if (args[i] == "--zip" && i + 1 < args.Length)
                {
                    zipPath = args[i + 1];
                }
                else if (args[i] == "--target" && i + 1 < args.Length)
                {
                    targetDir = args[i + 1];
                }
            }

            if (string.IsNullOrEmpty(targetDir))
            {
                targetDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            string logDir = Path.Combine(targetDir, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            _logPath = Path.Combine(logDir, "update.log");

            Log("========================================");
            Log("RetroLauncher Updater Started");
            Log($"Target Directory: {targetDir}");
            Log($"Update Zip Path: {zipPath}");
            Log($"Parent PID: {parentPid}");

            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                Log("Error: Downloaded ZIP file does not exist.");
                return 1;
            }

            // Wait for parent process to exit
            if (parentPid > 0)
            {
                Log($"Waiting for parent process {parentPid} to close...");
                try
                {
                    var parentProcess = Process.GetProcessById(parentPid);
                    parentProcess.WaitForExit(15000); // Wait up to 15 seconds
                    if (!parentProcess.HasExited)
                    {
                        Log("Error: Parent process did not exit within the timeout.");
                        return 1;
                    }
                    Log("Parent process closed.");
                }
                catch (ArgumentException)
                {
                    Log("Parent process already closed.");
                }
                catch (Exception ex)
                {
                    Log($"Warning waiting for process: {ex.Message}");
                }
            }

            // Perform update with rollback
            string backupDir = Path.Combine(targetDir, "backup_update");
            bool backupCreated = false;

            try
            {
                // Create backup directory
                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, true);
                }
                Directory.CreateDirectory(backupDir);
                Log("Created backup directory.");

                // Back up executable files and DLL libraries
                var filesToBackup = Directory.EnumerateFiles(targetDir, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                foreach (var file in filesToBackup)
                {
                    string name = Path.GetFileName(file);
                    // Do not backup or restore active databases to avoid overwriting newer configs in failure
                    if (name.Equals("games.json", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("emulators.json", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("profiles.json", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("friends.json", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("updater_settings.json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string dest = Path.Combine(backupDir, name);
                    File.Copy(file, dest, true);
                    Log($"Backed up: {name}");
                }
                backupCreated = true;

                // Extract Zip contents with filtering to preserve configurations
                Log("Extracting update zip package...");
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string fullName = entry.FullName.Replace('\\', '/');

                        // Directory check
                        if (fullName.EndsWith("/"))
                        {
                            string dirPath = Path.Combine(targetDir, fullName);
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(dirPath);
                            }
                            continue;
                        }

                        // Preserved Files and folders filter check
                        if (fullName.Equals("games.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.Equals("emulators.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.Equals("bios.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.Equals("profiles.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.Equals("friends.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.Equals("updater_settings.json", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("saves/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("media/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("screenshots/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("configs/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("games/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("emulators/", StringComparison.OrdinalIgnoreCase) ||
                            fullName.StartsWith("logs/", StringComparison.OrdinalIgnoreCase))
                        {
                            Log($"Skipping extraction (preserved): {entry.FullName}");
                            continue;
                        }

                        string destPath = Path.Combine(targetDir, entry.FullName);
                        string? destParent = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destParent) && !Directory.Exists(destParent))
                        {
                            Directory.CreateDirectory(destParent);
                        }

                        // Extract file (overwriting existing launcher binaries)
                        entry.ExtractToFile(destPath, true);
                        Log($"Extracted: {entry.FullName}");
                    }
                }

                Log("Update extraction completed successfully.");

                // Clean up backup and downloaded ZIP
                try
                {
                    if (Directory.Exists(backupDir))
                    {
                        Directory.Delete(backupDir, true);
                    }
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }
                    Log("Cleaned up backup and temp files.");
                }
                catch (Exception cleanupEx)
                {
                    Log($"Warning cleaning up temp folders: {cleanupEx.Message}");
                }

                // Restart launcher
                string launcherExe = Path.Combine(targetDir, "RetroLauncher.exe");
                Log($"Restarting launcher: {launcherExe}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = launcherExe,
                    UseShellExecute = true
                });

                Log("Update workflow finished successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                Log($"Critical Update Error: {ex.Message}");

                // Rollback if backup is available
                if (backupCreated)
                {
                    Log("Initiating rollback recovery...");
                    try
                    {
                        var backupFiles = Directory.EnumerateFiles(backupDir, "*.*", SearchOption.TopDirectoryOnly);
                        foreach (var file in backupFiles)
                        {
                            string name = Path.GetFileName(file);
                            string dest = Path.Combine(targetDir, name);
                            File.Copy(file, dest, true);
                            Log($"Restored backup: {name}");
                        }

                        Log("Rollback completed successfully. Restarting old version...");
                    }
                    catch (Exception rbEx)
                    {
                        Log($"CRITICAL ROLLBACK FAILURE: {rbEx.Message}");
                    }
                }

                // Restart old application anyway
                try
                {
                    string launcherExe = Path.Combine(targetDir, "RetroLauncher.exe");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launcherExe,
                        UseShellExecute = true
                    });
                }
                catch (Exception restartEx)
                {
                    Log($"Failed to restart old executable: {restartEx.Message}");
                }

                return 1;
            }
        }

        private static void Log(string message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(logLine);
            try
            {
                File.AppendAllText(_logPath, logLine + Environment.NewLine);
            }
            catch
            {
                // Silently ignore log write failure
            }
        }
    }
}
