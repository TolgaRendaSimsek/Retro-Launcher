using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher
{
    public class ScannedRomItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = "";
        public string RomPath { get; set; } = "";
        public string Platform { get; set; } = "Unknown";
        public string FileFormat { get; set; } = "Unknown";
        public string Status { get; set; } = "Unknown Format";
        public string EmulatorId { get; set; } = "";
        public string AssociatedGameId { get; set; } = ""; // Maps to library game ID if registered
    }

    public class RomManager
    {
        private static readonly string ScanResultsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rom_scan.json");
        private static readonly object FileLock = new object();
        private List<ScannedRomItem> _scanResults = new();
        private readonly GameLibraryManager _libraryManager;

        private static RomManager? _instance;
        public static RomManager Instance => _instance ??= new RomManager();

        public RomManager()
        {
            _libraryManager = new GameLibraryManager();
        }

        public RomManager(GameLibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public List<ScannedRomItem> ScanResults => _scanResults;

        public void ScanFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

            try
            {
                // Retrieve all files in folder and subfolders
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    // Ignore common non-rom metadata/system files
                    if (ext == ".txt" || ext == ".png" || ext == ".jpg" || ext == ".json" || ext == ".xml" || ext == ".db")
                        continue;

                    string format = DetectFileFormat(file);
                    string? detectedPlatform = DetectPlatform(file);
                    
                    string platformName = detectedPlatform ?? "Unknown";
                    string status = "Ready";

                    if (detectedPlatform == null)
                    {
                        // Ambiguous/unrecognized format
                        if (ext == ".iso" || ext == ".bin" || ext == ".img")
                        {
                            status = "Needs Manual Platform Selection";
                        }
                        else
                        {
                            status = "Unknown Format";
                        }
                    }

                    var item = new ScannedRomItem
                    {
                        FileName = Path.GetFileName(file),
                        RomPath = file.Replace('\\', '/'),
                        Platform = platformName,
                        FileFormat = format,
                        Status = status,
                        EmulatorId = detectedPlatform != null ? GetDefaultEmulatorForConsole(detectedPlatform) : ""
                    };

                    // Check if this ROM is already added to RetroLauncher library
                    var matchingLibraryGame = _libraryManager.Games.FirstOrDefault(g => 
                        string.Equals(g.RomPath.Replace('\\', '/'), item.RomPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingLibraryGame != null)
                    {
                        item.AssociatedGameId = matchingLibraryGame.Id;
                    }

                    _scanResults.Add(item);
                }

                // Post-process scan lists
                FindDuplicates();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning folder {folderPath}: {ex.Message}");
            }
        }

        public void ScanMultipleFolders(IEnumerable<string> folderPaths)
        {
            if (folderPaths == null) return;
            foreach (var path in folderPaths)
            {
                ScanFolder(path);
            }
        }

        public string? DetectPlatform(string filePath)
        {
            return RomDetector.DetectConsole(filePath);
        }

        public string DetectFileFormat(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "Unknown";
            string ext = Path.GetExtension(filePath).TrimStart('.').ToUpper();
            return string.IsNullOrEmpty(ext) ? "Unknown" : ext;
        }

        public void FindDuplicates()
        {
            // Group by normalized rom paths
            var pathGroups = _scanResults
                .Where(r => r.Status != "Missing")
                .GroupBy(r => r.RomPath.ToLower())
                .Where(g => g.Count() > 1);

            foreach (var group in pathGroups)
            {
                foreach (var item in group)
                {
                    item.Status = "Duplicate";
                }
            }

            // Group by filename for naming duplication checks
            var nameGroups = _scanResults
                .Where(r => r.Status != "Missing" && r.Status != "Duplicate")
                .GroupBy(r => r.FileName.ToLower())
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var item in group)
                {
                    item.Status = "Duplicate";
                }
            }
        }

        public void CheckMissingFiles()
        {
            // Checks RetroLauncher library games and appends items that are missing ROMs on disk
            foreach (var game in _libraryManager.Games)
            {
                if (!ValidateRomPath(game.Id))
                {
                    // Prevent duplicates in scan results
                    if (!_scanResults.Any(r => r.AssociatedGameId == game.Id && r.Status == "Missing"))
                    {
                        _scanResults.Add(new ScannedRomItem
                        {
                            FileName = Path.GetFileName(game.RomPath),
                            RomPath = game.RomPath.Replace('\\', '/'),
                            Platform = game.Platform,
                            FileFormat = DetectFileFormat(game.RomPath),
                            Status = "Missing",
                            AssociatedGameId = game.Id,
                            EmulatorId = game.EmulatorId
                        });
                    }
                }
            }
        }

        public bool ValidateRomPath(string gameId)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null || string.IsNullOrEmpty(game.RomPath)) return false;

            string resolved = ResolvePath(game.RomPath);
            return File.Exists(resolved) || Directory.Exists(resolved);
        }

        public bool AssignPlatform(string scannedItemId, string platform)
        {
            var item = _scanResults.FirstOrDefault(r => r.Id == scannedItemId);
            if (item != null)
            {
                item.Platform = platform;
                item.EmulatorId = GetDefaultEmulatorForConsole(platform);
                if (item.Status == "Needs Manual Platform Selection" || item.Status == "Unknown Format")
                {
                    item.Status = "Ready";
                }
                FindDuplicates(); // Recheck duplicates after update
                return true;
            }
            return false;
        }

        public bool AssignEmulator(string scannedItemId, string emulatorIdOrPath)
        {
            var item = _scanResults.FirstOrDefault(r => r.Id == scannedItemId);
            if (item != null)
            {
                item.EmulatorId = emulatorIdOrPath;
                return true;
            }
            return false;
        }

        public void SaveScanResults()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_scanResults, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ScanResultsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving scan results: {ex.Message}");
                }
            }
        }

        public void LoadScanResults()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ScanResultsPath))
                    {
                        string json = File.ReadAllText(ScanResultsPath);
                        _scanResults = JsonSerializer.Deserialize<List<ScannedRomItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ScannedRomItem>();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading scan results: {ex.Message}");
                }
            }
        }

        private string GetDefaultEmulatorForConsole(string console)
        {
            var emuConfig = EmulatorManager.LoadConfig();
            if (emuConfig.DefaultEmulators.TryGetValue(console, out string? path))
            {
                return path;
            }
            return "";
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }
    }
}
