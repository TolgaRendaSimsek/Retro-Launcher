using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Services
{
    public class SaveMetadata
    {
        public string GameId { get; set; } = "";
        public string EmulatorId { get; set; } = "";
        public string LastBackupDate { get; set; } = ""; // "yyyy-MM-dd HH:mm:ss"
        public string LocalPath { get; set; } = "";
        public string BackupPath { get; set; } = "";
        public string Status { get; set; } = "No Backup";
    }

    public enum SaveComparisonResult
    {
        InSync,
        LocalNewer,
        BackupNewer,
        NoBackup,
        NoLocal,
        Different
    }

    public class SaveManager
    {
        private static readonly string SavesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves.json");
        private static readonly string BackupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", "saves");
        private static readonly object FileLock = new object();

        private Dictionary<string, SaveMetadata> _metadata = new();
        private static SaveManager? _instance;

        public string ActiveProvider { get; set; } = "Local Backup";
        public Dictionary<string, string> CustomSavePaths { get; set; } = new();

        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveManager();
                }
                return _instance;
            }
        }

        public SaveManager()
        {
            LoadMetadata();
        }

        public void LoadMetadata()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(SavesJsonPath))
                    {
                        string json = File.ReadAllText(SavesJsonPath);
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                if (doc.RootElement.TryGetProperty("ActiveProvider", out var provProp))
                                {
                                    ActiveProvider = provProp.GetString() ?? "Local Backup";
                                }

                                if (doc.RootElement.TryGetProperty("CustomSavePaths", out var pathsProp) && pathsProp.ValueKind == JsonValueKind.Object)
                                {
                                    CustomSavePaths = JsonSerializer.Deserialize<Dictionary<string, string>>(pathsProp.GetRawText()) ?? new();
                                }

                                JsonElement metaArray;
                                if (doc.RootElement.TryGetProperty("Metadata", out var listProp) && listProp.ValueKind == JsonValueKind.Array)
                                {
                                    metaArray = listProp;
                                }
                                else
                                {
                                    metaArray = doc.RootElement;
                                }

                                if (metaArray.ValueKind == JsonValueKind.Array)
                                {
                                    _metadata = new Dictionary<string, SaveMetadata>();
                                    foreach (var element in metaArray.EnumerateArray())
                                    {
                                        var meta = JsonSerializer.Deserialize<SaveMetadata>(element.GetRawText());
                                        if (meta != null && !string.IsNullOrEmpty(meta.GameId))
                                        {
                                            _metadata[meta.GameId] = meta;
                                        }
                                    }
                                }
                            }
                            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                // Legacy fallback directly to array
                                var list = JsonSerializer.Deserialize<List<SaveMetadata>>(json);
                                _metadata = new Dictionary<string, SaveMetadata>();
                                if (list != null)
                                {
                                    foreach (var meta in list)
                                    {
                                        if (!string.IsNullOrEmpty(meta.GameId))
                                        {
                                            _metadata[meta.GameId] = meta;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading saves.json: {ex.Message}");
                }
            }
        }

        public void SaveMetadata()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var wrapper = new
                    {
                        ActiveProvider = ActiveProvider,
                        CustomSavePaths = CustomSavePaths,
                        Metadata = _metadata.Values.ToList()
                    };
                    string json = JsonSerializer.Serialize(wrapper, options);
                    File.WriteAllText(SavesJsonPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving saves.json: {ex.Message}");
                }
            }
        }

        public SaveMetadata GetOrCreateMetadata(string gameId, string emulatorId = "")
        {
            if (!_metadata.TryGetValue(gameId, out var meta))
            {
                meta = new SaveMetadata { GameId = gameId, EmulatorId = emulatorId };
                _metadata[gameId] = meta;
            }
            if (string.IsNullOrEmpty(meta.EmulatorId) && !string.IsNullOrEmpty(emulatorId))
            {
                meta.EmulatorId = emulatorId;
            }
            return meta;
        }

        public void SetCustomSaveFolder(string emulatorId, string path)
        {
            CustomSavePaths[emulatorId] = path;
            SaveMetadata();
        }

        public string DetectSaveFolder(string emulatorId)
        {
            if (CustomSavePaths.TryGetValue(emulatorId, out var customPath) && !string.IsNullOrEmpty(customPath))
            {
                return customPath;
            }

            // Normalize path relative to executable
            string defaultPath = "";
            string cleanId = Path.GetFileNameWithoutExtension(emulatorId).ToLower();

            if (cleanId.Contains("duckstation"))
            {
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emulators", "PS1", "memcards");
            }
            else if (cleanId.Contains("pcsx2"))
            {
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emulators", "PS2", "memcards");
            }
            else if (cleanId.Contains("rpcs3"))
            {
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emulators", "PS3", "dev_hdd0", "home", "00000001", "savedata");
            }
            else
            {
                // General fallback
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", cleanId);
            }

            return defaultPath;
        }

        public string[] GetGameSaveFiles(string gameId, string saveFolder)
        {
            if (!Directory.Exists(saveFolder)) return Array.Empty<string>();

            // Retrieve game title from library
            var lib = new GameLibraryManager();
            var game = lib.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return Array.Empty<string>();

            // Try to find specific files containing game ID or title
            var allFiles = Directory.GetFiles(saveFolder, "*", SearchOption.AllDirectories);
            var gameFiles = allFiles.Where(f =>
                Path.GetFileName(f).ToLower().Contains(gameId.ToLower()) ||
                Path.GetFileName(f).ToLower().Contains(game.Title.ToLower().Replace(":", ""))
            ).ToArray();

            // Fallback: if no specific files match, return all files in folder (e.g. shared memory card)
            if (gameFiles.Length == 0)
            {
                return allFiles;
            }
            return gameFiles;
        }

        public bool BackupSaves(string gameId)
        {
            try
            {
                var lib = new GameLibraryManager();
                var game = lib.Games.FirstOrDefault(g => g.Id == gameId);
                if (game == null) return false;

                string saveFolder = DetectSaveFolder(game.EmulatorId);
                if (!Directory.Exists(saveFolder))
                {
                    return false;
                }

                var filesToBackup = GetGameSaveFiles(gameId, saveFolder);
                if (filesToBackup.Length == 0)
                {
                    return false;
                }

                // Prepare backup directory
                string gameBackupDir = Path.Combine(BackupDir, gameId);
                if (Directory.Exists(gameBackupDir))
                {
                    Directory.Delete(gameBackupDir, true);
                }
                Directory.CreateDirectory(gameBackupDir);

                // Copy files retaining structure relative to saveFolder
                foreach (var file in filesToBackup)
                {
                    string relativePath = Path.GetRelativePath(saveFolder, file);
                    string destFile = Path.Combine(gameBackupDir, relativePath);
                    string? destFolder = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destFolder) && !Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    File.Copy(file, destFile, true);
                }

                // Update metadata
                var meta = GetOrCreateMetadata(gameId, game.EmulatorId);
                meta.LastBackupDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                meta.LocalPath = saveFolder;
                meta.BackupPath = gameBackupDir;
                meta.Status = "Synced";
                SaveMetadata();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup error: {ex.Message}");
                return false;
            }
        }

        public bool RestoreSaves(string gameId)
        {
            try
            {
                var meta = GetOrCreateMetadata(gameId);
                if (string.IsNullOrEmpty(meta.BackupPath) || !Directory.Exists(meta.BackupPath))
                {
                    return false;
                }

                string localPath = meta.LocalPath;
                if (string.IsNullOrEmpty(localPath))
                {
                    var lib = new GameLibraryManager();
                    var game = lib.Games.FirstOrDefault(g => g.Id == gameId);
                    if (game == null) return false;
                    localPath = DetectSaveFolder(game.EmulatorId);
                }

                var backupFiles = Directory.GetFiles(meta.BackupPath, "*", SearchOption.AllDirectories);
                if (backupFiles.Length == 0) return false;

                // Confirm Overwrite if files exist locally!
                bool hasExisting = false;
                foreach (var file in backupFiles)
                {
                    string relativePath = Path.GetRelativePath(meta.BackupPath, file);
                    string localTarget = Path.Combine(localPath, relativePath);
                    if (File.Exists(localTarget))
                    {
                        hasExisting = true;
                        break;
                    }
                }

                if (hasExisting)
                {
                    var result = MessageBox.Show(
                        $"Restoring saves will overwrite existing local save files.\n\nAre you sure you want to continue?",
                        "Confirm Restore Overwrite",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result != DialogResult.Yes)
                    {
                        return false;
                    }
                }

                // Copy files back
                foreach (var file in backupFiles)
                {
                    string relativePath = Path.GetRelativePath(meta.BackupPath, file);
                    string destFile = Path.Combine(localPath, relativePath);
                    string? destFolder = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destFolder) && !Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    File.Copy(file, destFile, true);
                }

                meta.Status = "Synced";
                SaveMetadata();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Restore error: {ex.Message}");
                return false;
            }
        }

        public void BackupAllSaves()
        {
            var lib = new GameLibraryManager();
            foreach (var game in lib.Games)
            {
                BackupSaves(game.Id);
            }
        }

        public void RestoreAllSaves()
        {
            var lib = new GameLibraryManager();
            foreach (var game in lib.Games)
            {
                RestoreSaves(game.Id);
            }
        }

        public SaveComparisonResult CompareLocalAndBackupSaves(string gameId)
        {
            var meta = GetOrCreateMetadata(gameId);
            if (string.IsNullOrEmpty(meta.BackupPath) || !Directory.Exists(meta.BackupPath))
            {
                return SaveComparisonResult.NoBackup;
            }

            string localPath = meta.LocalPath;
            if (string.IsNullOrEmpty(localPath))
            {
                var lib = new GameLibraryManager();
                var game = lib.Games.FirstOrDefault(g => g.Id == gameId);
                if (game == null) return SaveComparisonResult.NoBackup;
                localPath = DetectSaveFolder(game.EmulatorId);
            }

            if (!Directory.Exists(localPath))
            {
                return SaveComparisonResult.NoLocal;
            }

            var backupFiles = Directory.GetFiles(meta.BackupPath, "*", SearchOption.AllDirectories);
            var localFiles = GetGameSaveFiles(gameId, localPath);

            if (backupFiles.Length == 0 && localFiles.Length == 0) return SaveComparisonResult.InSync;
            if (backupFiles.Length == 0) return SaveComparisonResult.NoBackup;
            if (localFiles.Length == 0) return SaveComparisonResult.NoLocal;

            DateTime maxLocalWrite = localFiles.Max(f => File.GetLastWriteTime(f));
            DateTime maxBackupWrite = backupFiles.Max(f => File.GetLastWriteTime(f));

            // Simple date comparison
            TimeSpan diff = maxLocalWrite - maxBackupWrite;
            if (Math.Abs(diff.TotalSeconds) < 2)
            {
                return SaveComparisonResult.InSync;
            }
            else if (diff.TotalSeconds > 0)
            {
                return SaveComparisonResult.LocalNewer;
            }
            else
            {
                return SaveComparisonResult.BackupNewer;
            }
        }

        public DialogResult ShowConflictDialog(string gameTitle, SaveComparisonResult comparison)
        {
            string message = "";
            if (comparison == SaveComparisonResult.LocalNewer)
            {
                message = $"Conflict detected for '{gameTitle}':\n\nYour local saves are NEWER than the backup saves.\n\nDo you want to overwrite the Backup with your Local saves?\n\n- Click YES to Overwrite Backup (Upload)\n- Click NO to Overwrite Local with Backup (Download)\n- Click CANCEL to abort sync.";
            }
            else if (comparison == SaveComparisonResult.BackupNewer)
            {
                message = $"Conflict detected for '{gameTitle}':\n\nYour backup saves are NEWER than the local saves.\n\nDo you want to overwrite your Local saves with the Backup?\n\n- Click YES to Overwrite Local with Backup (Download)\n- Click NO to Overwrite Backup with Local (Upload)\n- Click CANCEL to abort sync.";
            }
            else
            {
                message = $"Conflict detected for '{gameTitle}':\n\nLocal and backup saves differ.\n\nOverwrite Local with Backup?";
                return MessageBox.Show(message, "Save Sync Conflict", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }

            return MessageBox.Show(message, "Save Sync Conflict Detected", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }
    }
}
