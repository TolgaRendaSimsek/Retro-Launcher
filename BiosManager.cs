using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class BiosManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bios.json");
        private static readonly object FileLock = new object();

        private BiosConfig _config = new();
        private static BiosManager? _instance;
        public static BiosManager Instance => _instance ??= new BiosManager();

        public BiosManager()
        {
            LoadConfig();
        }

        public List<BiosItem> BiosItems => _config.BiosItems;

        public void LoadConfig()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ConfigPath))
                    {
                        string json = File.ReadAllText(ConfigPath);
                        _config = JsonSerializer.Deserialize<BiosConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new BiosConfig();
                    }
                    else
                    {
                        SeedDefaultConfig();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading bios config: {ex.Message}");
                    SeedDefaultConfig();
                }

                DetectBiosStatus();
            }
        }

        public void SaveConfig()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving bios config: {ex.Message}");
                }
            }
        }

        public void DetectBiosStatus()
        {
            foreach (var item in _config.BiosItems)
            {
                if (string.IsNullOrEmpty(item.Path))
                {
                    item.Status = "Missing";
                    continue;
                }

                string resolved = ResolvePath(item.Path);
                if (File.Exists(resolved))
                {
                    item.Status = "Ready";
                }
                else
                {
                    item.Status = "Missing";
                }
            }
        }

        public bool ImportBiosFile(string console, string sourceFile)
        {
            var item = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item == null || !File.Exists(sourceFile)) return false;

            try
            {
                string fileName = Path.GetFileName(sourceFile);
                string defaultFolder = GetDefaultFolderForConsole(console);
                string destFile = Path.Combine(defaultFolder, fileName);

                string resolvedDestFolder = ResolvePath(defaultFolder);
                if (!Directory.Exists(resolvedDestFolder))
                {
                    Directory.CreateDirectory(resolvedDestFolder);
                }

                string resolvedDestFile = ResolvePath(destFile);
                File.Copy(sourceFile, resolvedDestFile, true);

                item.Path = destFile.Replace('\\', '/');
                item.FileName = fileName;
                item.Status = "Ready";

                SaveConfig();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to import BIOS for {console}: {ex.Message}");
                return false;
            }
        }

        public bool LocateBiosManually(string console, string fullPath)
        {
            var item = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item == null || !File.Exists(fullPath)) return false;

            item.Path = fullPath.Replace('\\', '/');
            item.FileName = Path.GetFileName(fullPath);
            item.Status = "Ready";

            SaveConfig();
            return true;
        }

        public async Task<bool> DownloadBiosFromApiAsync(string console, string apiEndpoint, Action<int> progressCallback)
        {
            string tempFile = "";
            try
            {
                var api = new ApiClient();

                // 1. Fetch package info from API
                var info = await api.GetBiosPackageAsync(apiEndpoint, console);
                if (info == null || string.IsNullOrEmpty(info.DownloadUrl))
                {
                    throw new Exception("Invalid API response or download URL is missing.");
                }

                // 2. Download and verify package
                tempFile = await api.DownloadAndVerifyPackageAsync(info.DownloadUrl, info.FileName, info.Sha256, progressCallback);

                // 3. Extract to target directory
                string targetFolder = string.IsNullOrEmpty(info.TargetFolder) ? GetDefaultFolderForConsole(console) : info.TargetFolder;
                string resolvedTargetFolder = ResolvePath(targetFolder);

                if (!Directory.Exists(resolvedTargetFolder))
                {
                    Directory.CreateDirectory(resolvedTargetFolder);
                }

                string archiveType = (info.ArchiveType ?? "").ToLower().Trim();
                if (archiveType == "zip" || tempFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                    archiveType == "7z" || tempFile.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                {
                    await api.ExtractPackageAsync(tempFile, archiveType, resolvedTargetFolder);
                }
                else
                {
                    // If it is raw file (not archive), just copy it directly
                    string targetFile = Path.Combine(resolvedTargetFolder, Path.GetFileName(tempFile));
                    File.Copy(tempFile, targetFile, true);
                }

                // 4. Update configuration
                var item = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    // Find a matching bin/rom/pup file in the extracted files or use the default filename
                    var files = Directory.GetFiles(resolvedTargetFolder, "*.*", SearchOption.AllDirectories);
                    string? detectedFile = files.FirstOrDefault(f => 
                        f.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".rom", StringComparison.OrdinalIgnoreCase) || 
                        f.EndsWith(".pup", StringComparison.OrdinalIgnoreCase));

                    if (detectedFile != null)
                    {
                        item.Path = MakeRelativePath(detectedFile).Replace('\\', '/');
                        item.FileName = Path.GetFileName(detectedFile);
                    }
                    else
                    {
                        item.Path = Path.Combine(targetFolder, item.FileName).Replace('\\', '/');
                    }

                    item.Status = "Ready";
                    item.InstalledVersion = info.Version;
                    item.Sha256 = info.Sha256;
                    SaveConfig();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BIOS download failed: {ex.Message}");
                throw;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile)) File.Delete(tempFile);
                }
                catch { }
            }
        }

        private void SeedDefaultConfig()
        {
            _config = new BiosConfig
            {
                BiosItems = new List<BiosItem>
                {
                    new BiosItem { Console = "Sony PlayStation 1", Path = "Emulators/PS1/bios/scph5501.bin", FileName = "scph5501.bin", Status = "Missing" },
                    new BiosItem { Console = "Sony PlayStation 2", Path = "Emulators/PS2/bios/scph39001.bin", FileName = "scph39001.bin", Status = "Missing" },
                    new BiosItem { Console = "Sony PlayStation 3", Path = "Emulators/PS3/dev_flash/PS3UPDAT.PUP", FileName = "PS3UPDAT.PUP", Status = "Missing" }
                }
            };
            SaveConfig();
        }

        private string GetDefaultFolderForConsole(string console)
        {
            return console switch
            {
                "Sony PlayStation 1" => "Emulators/PS1/bios",
                "Sony PlayStation 2" => "Emulators/PS2/bios",
                "Sony PlayStation 3" => "Emulators/PS3/dev_flash",
                _ => "Emulators/Common/bios"
            };
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }

        private string MakeRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            return fullPath;
        }
    }
}
