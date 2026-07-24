using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class BiosManager
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "bios.json");
        private static readonly object FileLock = new object();

        private BiosConfig _config = new();
        private static BiosManager? _instance;
        public static BiosManager Instance => _instance ??= new BiosManager();

        public BiosManager()
        {
            LoadConfig();
            EnsureCentralizedFoldersCreated();
        }

        public static string GetCentralizedBiosRoot()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "RetroLauncher", "BIOS");
        }

        public void LogDiagnostic(string message)
        {
            try
            {
                string logDir = Path.Combine(AppContext.BaseDirectory, "logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                string logFile = Path.Combine(logDir, "package_manager.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.AppendAllText(logFile, $"[{timestamp}] [BIOS_MANAGER] {message}\n");
            }
            catch { }
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void EnsureCentralizedFoldersCreated()
        {
            string biosRoot = GetCentralizedBiosRoot();
            string[] subfolders = new[]
            {
                "DuckStation/PS1",
                "PCSX2/PS2",
                "RPCS3/PS3",
                "PPSSPP/PSP",
                "Dolphin/GameCube",
                "Dolphin/Wii",
                "RetroArch"
            };

            foreach (var sf in subfolders)
            {
                string dir = Path.Combine(biosRoot, sf.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(dir))
                {
                    try { Directory.CreateDirectory(dir); } catch { }
                }
            }
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

                        // Merge default items for missing consoles/emulators
                        var defaultItems = GetDefaultBiosItems();
                        bool updated = false;
                        foreach (var def in defaultItems)
                        {
                            var existing = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, def.Console, StringComparison.OrdinalIgnoreCase));
                            if (existing == null)
                            {
                                _config.BiosItems.Add(def);
                                updated = true;
                            }
                            else
                            {
                                // Migrate fields if empty (like Emulator and Platform)
                                if (string.IsNullOrEmpty(existing.Emulator))
                                {
                                    existing.Emulator = def.Emulator;
                                    updated = true;
                                }
                                if (string.IsNullOrEmpty(existing.Platform))
                                {
                                    existing.Platform = def.Platform;
                                    updated = true;
                                }
                                // If path was the old path "Emulators/...", update it to new centralized path if not modified or is missing
                                if (existing.Path.StartsWith("Emulators/", StringComparison.OrdinalIgnoreCase) && existing.Status == "Missing")
                                {
                                    existing.Path = def.Path;
                                    updated = true;
                                }
                            }
                        }

                        if (updated)
                        {
                            SaveConfig();
                        }
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

        private List<BiosItem> GetDefaultBiosItems()
        {
            return new List<BiosItem>
            {
                new BiosItem { Emulator = "DuckStation", Platform = "PS1", Console = "Sony PlayStation 1", Path = "BIOS/DuckStation/PS1/scph5501.bin", FileName = "scph5501.bin", Status = "Missing" },
                new BiosItem { Emulator = "PCSX2", Platform = "PS2", Console = "Sony PlayStation 2", Path = "BIOS/PCSX2/PS2/scph39001.bin", FileName = "scph39001.bin", Status = "Missing" },
                new BiosItem { Emulator = "RPCS3", Platform = "PS3", Console = "Sony PlayStation 3", Path = "BIOS/RPCS3/PS3/PS3UPDAT.PUP", FileName = "PS3UPDAT.PUP", Status = "Missing" },
                new BiosItem { Emulator = "PPSSPP", Platform = "PSP", Console = "Sony PlayStation Portable", Path = "BIOS/PPSSPP/PSP/gptokernel.prx", FileName = "gptokernel.prx", Status = "Missing" },
                new BiosItem { Emulator = "Dolphin", Platform = "GameCube", Console = "Nintendo GameCube", Path = "BIOS/Dolphin/GameCube/ipl_usa.bin", FileName = "ipl_usa.bin", Status = "Missing" },
                new BiosItem { Emulator = "Dolphin", Platform = "Wii", Console = "Nintendo Wii", Path = "BIOS/Dolphin/Wii/rvl.bin", FileName = "rvl.bin", Status = "Missing" },
                new BiosItem { Emulator = "RetroArch", Platform = "Common", Console = "RetroArch", Path = "BIOS/RetroArch/system.bin", FileName = "system.bin", Status = "Missing" }
            };
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
                string biosFolder = GetDefaultFolderForConsole(item.Console);
                string resolvedFolder = ResolvePath(biosFolder);
                bool found = false;

                // Auto-create directory
                if (!Directory.Exists(resolvedFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(resolvedFolder);
                        LogDiagnostic($"Created BIOS directory: {resolvedFolder}");
                    }
                    catch (Exception ex)
                    {
                        LogDiagnostic($"Failed to create BIOS directory '{resolvedFolder}': {ex.Message}");
                    }
                }

                if (Directory.Exists(resolvedFolder))
                {
                    try
                    {
                        // Recursively scan directories
                        var files = Directory.GetFiles(resolvedFolder, "*.*", SearchOption.AllDirectories);

                        LogDiagnostic($"Scanning BIOS folder for {item.Console}: '{resolvedFolder}'");
                        LogDiagnostic($"Found {files.Length} files in target folder.");
                        foreach (var file in files)
                        {
                            LogDiagnostic($"Detected BIOS File: '{Path.GetFileName(file)}' at '{file}'");
                        }

                        // Accept .bin and .rom case-insensitively (along with .pup and .prx)
                        var validFile = files.FirstOrDefault(f => 
                            f.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) || 
                            f.EndsWith(".rom", StringComparison.OrdinalIgnoreCase) || 
                            f.EndsWith(".pup", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".prx", StringComparison.OrdinalIgnoreCase));

                        if (validFile != null)
                        {
                            // Save only after a real file is found
                            item.Path = MakeRelativePath(validFile).Replace('\\', '/');
                            item.FileName = Path.GetFileName(validFile);
                            item.Status = "Ready";
                            found = true;
                            LogDiagnostic($"Discovered active BIOS for {item.Console}: '{item.FileName}' ({item.Path})");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDiagnostic($"Error scanning BIOS directory '{resolvedFolder}': {ex.Message}");
                    }
                }

                if (!found)
                {
                    if (!string.IsNullOrEmpty(item.Path))
                    {
                        string resolvedFile = ResolvePath(item.Path);
                        if (File.Exists(resolvedFile))
                        {
                            item.Status = "Ready";
                            found = true;
                            LogDiagnostic($"Fallback active BIOS for {item.Console}: '{item.FileName}' ({item.Path})");
                        }
                    }
                }

                if (!found)
                {
                    item.Status = "Missing";
                }
            }
            SaveConfig();
        }

        public bool CheckRealBiosExists(string console)
        {
            string folder = GetDefaultFolderForConsole(console);
            string resolved = ResolvePath(folder);
            if (!Directory.Exists(resolved)) return false;

            try
            {
                var files = Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories);
                return files.Any(f => 
                    f.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) || 
                    f.EndsWith(".rom", StringComparison.OrdinalIgnoreCase) || 
                    f.EndsWith(".pup", StringComparison.OrdinalIgnoreCase)
                );
            }
            catch
            {
                return false;
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

                if (!File.Exists(resolvedDestFile))
                {
                    return false;
                }

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
            if (string.Equals(console, "Sony PlayStation 3", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("PlayStation 3 system firmware cannot be downloaded automatically. You must import your own legally obtained PS3UPDAT.PUP firmware update package.");
            }

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
                BiosItems = GetDefaultBiosItems()
            };
            SaveConfig();
        }

        public string GetDefaultFolderForConsole(string console)
        {
            var item = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                if (string.Equals(item.Emulator, "RetroArch", StringComparison.OrdinalIgnoreCase))
                {
                    return "BIOS/RetroArch";
                }
                return $"BIOS/{item.Emulator}/{item.Platform}";
            }
            return console switch
            {
                "Sony PlayStation 1" => "BIOS/DuckStation/PS1",
                "Sony PlayStation 2" => "BIOS/PCSX2/PS2",
                "Sony PlayStation 3" => "BIOS/RPCS3/PS3",
                "Sony PlayStation Portable" => "BIOS/PPSSPP/PSP",
                "Nintendo GameCube" => "BIOS/Dolphin/GameCube",
                "Nintendo Wii" => "BIOS/Dolphin/Wii",
                "RetroArch" => "BIOS/RetroArch",
                _ => "BIOS/RetroArch"
            };
        }

        public string GetEmulatorExpectedFolder(string emulator, string platform)
        {
            return emulator.ToLower() switch
            {
                "duckstation" => "Emulators/PS1/bios",
                "pcsx2" => "Emulators/PS2/bios",
                "rpcs3" => "Emulators/PS3/dev_flash",
                "ppsspp" => "Emulators/PSP/bios",
                "dolphin" => platform.ToLower() == "gamecube" ? "Emulators/Dolphin/User/GC" : "Emulators/Dolphin/User/Wii",
                "retroarch" => "Emulators/RetroArch/system",
                _ => $"Emulators/{emulator}/bios"
            };
        }

        public bool SyncBiosToEmulator(BiosItem item)
        {
            string centralFolder = Path.GetDirectoryName(ResolvePath(item.Path)) ?? "";
            if (!Directory.Exists(centralFolder)) return false;

            try
            {
                var files = Directory.GetFiles(centralFolder, "*.*", SearchOption.AllDirectories);
                var validFiles = files.Where(f => 
                    f.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) || 
                    f.EndsWith(".rom", StringComparison.OrdinalIgnoreCase) || 
                    f.EndsWith(".pup", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".prx", StringComparison.OrdinalIgnoreCase)).ToList();

                if (!validFiles.Any()) return false;

                string destFolder = ResolvePath(GetEmulatorExpectedFolder(item.Emulator, item.Platform));
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }

                foreach (var file in validFiles)
                {
                    string destFile = Path.Combine(destFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);

                    // For DuckStation, create portable.txt beside execution folder
                    if (string.Equals(item.Emulator, "DuckStation", StringComparison.OrdinalIgnoreCase))
                    {
                        string emuDir = Path.GetDirectoryName(destFolder) ?? "";
                        if (!string.IsNullOrEmpty(emuDir) && Directory.Exists(emuDir))
                        {
                            var exes = Directory.GetFiles(emuDir, "*.exe", SearchOption.AllDirectories);
                            foreach (var exe in exes)
                            {
                                string portableFile = Path.Combine(Path.GetDirectoryName(exe) ?? "", "portable.txt");
                                if (!File.Exists(portableFile))
                                {
                                    File.WriteAllText(portableFile, "");
                                }
                            }
                        }
                    }

                    // For RPCS3, silently install firmware PUP package to generate dev_flash
                    if (string.Equals(item.Emulator, "RPCS3", StringComparison.OrdinalIgnoreCase))
                    {
                        string emuDir = Path.GetFullPath(Path.Combine(destFolder, ".."));
                        string rpcs3Exe = Path.Combine(emuDir, "rpcs3.exe");
                        if (File.Exists(rpcs3Exe))
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = rpcs3Exe,
                                Arguments = $"--installfw \"{destFile}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            try
                            {
                                using (var p = Process.Start(psi))
                                {
                                    p?.WaitForExit(15000);
                                }
                            }
                            catch { }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to sync BIOS for {item.Emulator}: {ex.Message}");
                return false;
            }
        }

        public bool RemoveBiosFile(string console)
        {
            var item = _config.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item == null) return false;

            string biosFolder = GetDefaultFolderForConsole(console);
            string resolvedFolder = ResolvePath(biosFolder);

            try
            {
                if (Directory.Exists(resolvedFolder))
                {
                    var files = Directory.GetFiles(resolvedFolder, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        if (ext == ".bin" || ext == ".rom" || ext == ".pup" || ext == ".prx")
                        {
                            File.Delete(file);
                        }
                    }
                }

                item.Path = "";
                item.FileName = "";
                item.Status = "Missing";
                SaveConfig();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to remove BIOS for {console}: {ex.Message}");
                return false;
            }
        }

        public string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            if (path.StartsWith("BIOS/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("BIOS\\", StringComparison.OrdinalIgnoreCase))
            {
                string relative = path.Substring(5);
                return Path.GetFullPath(Path.Combine(GetCentralizedBiosRoot(), relative));
            }
            if (string.Equals(path, "BIOS", StringComparison.OrdinalIgnoreCase))
            {
                return GetCentralizedBiosRoot();
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }

        private string MakeRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";

            string localRoot = GetCentralizedBiosRoot();
            if (fullPath.StartsWith(localRoot, StringComparison.OrdinalIgnoreCase))
            {
                string relative = fullPath.Substring(localRoot.Length).TrimStart(Path.DirectorySeparatorChar);
                return Path.Combine("BIOS", relative).Replace('\\', '/');
            }

            string baseDir = AppContext.BaseDirectory;
            if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar).Replace('\\', '/');
            }
            return fullPath;
        }
    }
}
