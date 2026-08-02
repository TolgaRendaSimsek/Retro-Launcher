using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Services
{
    public class LocalSaveFileInfo
    {
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public DateTime LastModified { get; set; }
        public long SizeInBytes { get; set; }
        public string SizeDisplay => FormatSize(SizeInBytes);

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1048576) return $"{(bytes / 1048576.0):F2} MB";
            if (bytes >= 1024) return $"{(bytes / 1024.0):F1} KB";
            return $"{bytes} B";
        }
    }

    public class LocalBackupInfo
    {
        public string BackupName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public long TotalSizeInBytes { get; set; }
        public string SizeDisplay => FormatSize(TotalSizeInBytes);

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1048576) return $"{(bytes / 1048576.0):F2} MB";
            if (bytes >= 1024) return $"{(bytes / 1024.0):F1} KB";
            return $"{bytes} B";
        }
    }

    public static class LocalSaveManager
    {
        private static readonly string LocalBackupBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", "local");

        public static List<LocalSaveFileInfo> GetActiveSaveFiles(Game game)
        {
            List<LocalSaveFileInfo> list = new List<LocalSaveFileInfo>();
            string saveFolder = SaveManager.Instance.DetectSaveFolder(game.EmulatorId);

            if (!Directory.Exists(saveFolder))
            {
                return list;
            }

            var matchedFiles = SaveManager.Instance.GetGameSaveFiles(game.Id, saveFolder);
            foreach (var file in matchedFiles)
            {
                if (File.Exists(file))
                {
                    FileInfo fi = new FileInfo(file);
                    list.Add(new LocalSaveFileInfo
                    {
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        LastModified = fi.LastWriteTime,
                        SizeInBytes = fi.Length
                    });
                }
            }

            return list;
        }

        public static List<LocalBackupInfo> GetLocalBackups(string gameId)
        {
            List<LocalBackupInfo> list = new List<LocalBackupInfo>();
            string gameBackupDir = Path.Combine(LocalBackupBaseDir, gameId);

            if (!Directory.Exists(gameBackupDir))
            {
                return list;
            }

            var subDirs = Directory.GetDirectories(gameBackupDir);
            foreach (var dir in subDirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                long totalSize = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                                          .Sum(f => new FileInfo(f).Length);

                list.Add(new LocalBackupInfo
                {
                    BackupName = di.Name,
                    FullPath = dir,
                    CreatedDate = di.CreationTime,
                    TotalSizeInBytes = totalSize
                });
            }

            return list.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public static bool CreateBackup(Game game, string backupName)
        {
            try
            {
                string saveFolder = SaveManager.Instance.DetectSaveFolder(game.EmulatorId);
                if (!Directory.Exists(saveFolder))
                {
                    MessageBox.Show("Active save folder does not exist. Nothing to backup.", "Backup Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                var files = SaveManager.Instance.GetGameSaveFiles(game.Id, saveFolder);
                if (files.Length == 0)
                {
                    MessageBox.Show("No active save files detected for this game.", "Backup Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                string gameBackupDir = Path.Combine(LocalBackupBaseDir, game.Id);
                string destBackupDir = Path.Combine(gameBackupDir, backupName);

                if (Directory.Exists(destBackupDir))
                {
                    var result = MessageBox.Show($"A backup named '{backupName}' already exists. Overwrite?", "Backup Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return false;
                    Directory.Delete(destBackupDir, true);
                }

                Directory.CreateDirectory(destBackupDir);

                foreach (var file in files)
                {
                    string relativePath = Path.GetRelativePath(saveFolder, file);
                    string destPath = Path.Combine(destBackupDir, relativePath);
                    string? destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }
                    File.Copy(file, destPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool RestoreBackup(Game game, string backupName)
        {
            try
            {
                string gameBackupDir = Path.Combine(LocalBackupBaseDir, game.Id);
                string sourceBackupDir = Path.Combine(gameBackupDir, backupName);

                if (!Directory.Exists(sourceBackupDir))
                {
                    MessageBox.Show("Source backup directory not found.", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string saveFolder = SaveManager.Instance.DetectSaveFolder(game.EmulatorId);

                // Overwrite confirmation
                if (Directory.Exists(saveFolder))
                {
                    var files = Directory.GetFiles(saveFolder, "*", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        var confirm = MessageBox.Show(
                            $"Are you sure you want to restore '{backupName}'?\n\nThis will OVERWRITE your current active save files for this game.",
                            "Confirm Overwrite",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirm != DialogResult.Yes) return false;
                    }
                }

                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                var backupFiles = Directory.GetFiles(sourceBackupDir, "*", SearchOption.AllDirectories);
                foreach (var file in backupFiles)
                {
                    string relativePath = Path.GetRelativePath(sourceBackupDir, file);
                    string destPath = Path.Combine(saveFolder, relativePath);
                    string? destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }
                    File.Copy(file, destPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring backup: {ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool DeleteBackup(string gameId, string backupName)
        {
            try
            {
                string backupDir = Path.Combine(LocalBackupBaseDir, gameId, backupName);
                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting backup: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool RenameBackup(string gameId, string oldName, string newName)
        {
            try
            {
                string baseDir = Path.Combine(LocalBackupBaseDir, gameId);
                string oldDir = Path.Combine(baseDir, oldName);
                string newDir = Path.Combine(baseDir, newName);

                if (!Directory.Exists(oldDir)) return false;
                if (Directory.Exists(newDir))
                {
                    MessageBox.Show($"A backup named '{newName}' already exists.", "Rename Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                Directory.Move(oldDir, newDir);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming backup: {ex.Message}", "Rename Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExportBackup(string gameId, string backupName, string destZipPath)
        {
            try
            {
                string backupDir = Path.Combine(LocalBackupBaseDir, gameId, backupName);
                if (!Directory.Exists(backupDir)) return false;

                if (File.Exists(destZipPath))
                {
                    File.Delete(destZipPath);
                }

                ZipFile.CreateFromDirectory(backupDir, destZipPath, CompressionLevel.Optimal, false);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting backup: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ImportBackup(string gameId, string sourceZipPath, string backupName)
        {
            try
            {
                string destBackupDir = Path.Combine(LocalBackupBaseDir, gameId, backupName);
                if (Directory.Exists(destBackupDir))
                {
                    var result = MessageBox.Show($"A backup named '{backupName}' already exists. Overwrite?", "Import Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return false;
                    Directory.Delete(destBackupDir, true);
                }

                Directory.CreateDirectory(destBackupDir);
                ZipFile.ExtractToDirectory(sourceZipPath, destBackupDir);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing backup: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
