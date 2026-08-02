using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class EmulatorItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> SupportedPlatforms { get; set; } = new();
        public string GithubRepository { get; set; } = "";
        public string ReleaseAssetPattern { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string InstallFolder { get; set; } = "";
        public string ArchiveType { get; set; } = ""; // e.g. "zip", "7z"
        public string InstalledVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string Status { get; set; } = "Missing";
        public bool RequiresBIOS { get; set; } = false;
        public bool RequiresFirmware { get; set; } = false;
        public string DefaultLaunchArguments { get; set; } = "";
        public string ReleaseChannel { get; set; } = "Stable";
        public string SelectedAssetName { get; set; } = "";
        public DateTime? InstallationTimestamp { get; set; }
        public bool AutoSyncController { get; set; } = false;

        // Backward compatibility properties
        public string Path { get => ExecutablePath; set => ExecutablePath = value; }
        public string Version { get => InstalledVersion; set => InstalledVersion = value; }
        public string Repo { get => GithubRepository; set => GithubRepository = value; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(InstalledVersion) ? Name : $"{Name} ({InstalledVersion})";
        }
    }

    public class EmulatorConfig
    {
        public List<EmulatorItem> Emulators { get; set; } = new();
        public Dictionary<string, string> DefaultEmulators { get; set; } = new();
    }

    public class EmulatorManager
    {
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "emulators.json");
        private static EmulatorManager? _instance;
        public static EmulatorManager Instance => _instance ??= new EmulatorManager();

        public EmulatorConfig Config { get; private set; } = new();

        public EmulatorManager()
        {
            LoadEmulators();
        }

        public void LoadEmulators()
        {
            Config = LoadConfig();
        }

        public void SaveEmulators()
        {
            SaveConfig(Config);
        }

        public List<EmulatorItem> DetectInstalledEmulators()
        {
            return Config.Emulators.Where(emu => IsEmulatorInstalled(emu)).ToList();
        }

        public static bool IsEmulatorInstalled(EmulatorItem emu)
        {
            if (emu == null) return false;
            if (emu.Status == "Missing" || string.IsNullOrEmpty(emu.Path)) return false;

            string resolvedExe = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.Path));
            string resolvedDir = !string.IsNullOrEmpty(emu.InstallFolder)
                ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.InstallFolder))
                : (Path.GetDirectoryName(resolvedExe) ?? "");

            bool exeExists = File.Exists(resolvedExe);
            bool dirExists = Directory.Exists(resolvedDir);

            return exeExists && dirExists;
        }

        public bool VerifyExecutable(string emulatorId)
        {
            var emu = FindEmulator(emulatorId);
            if (emu == null || string.IsNullOrWhiteSpace(emu.Path)) return false;

            string resolved = ResolvePath(emu.Path);
            bool primaryValid = false;

            if (File.Exists(resolved))
            {
                try
                {
                    var fileInfo = new FileInfo(resolved);
                    if (fileInfo.Length > 0)
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(resolved);
                        primaryValid = true;
                    }
                }
                catch { }
            }

            if (primaryValid) return true;

            // Fallback recursive search if not found or invalid
            string? searchFolder = null;
            if (!string.IsNullOrEmpty(emu.InstallFolder))
            {
                searchFolder = ResolvePath(emu.InstallFolder);
            }
            else if (!string.IsNullOrEmpty(emu.Path))
            {
                searchFolder = Path.GetDirectoryName(ResolvePath(emu.Path));
            }

            if (searchFolder != null && Directory.Exists(searchFolder))
            {
                string? foundExe = null;
                if (string.Equals(emulatorId, "duckstation", StringComparison.OrdinalIgnoreCase))
                {
                    foundExe = FindDuckStationExecutable(searchFolder);
                }
                else
                {
                    foundExe = FindExecutableInFolder(searchFolder);
                }

                if (foundExe != null && File.Exists(foundExe))
                {
                    try
                    {
                        var fileInfo = new FileInfo(foundExe);
                        if (fileInfo.Length > 0)
                        {
                            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(foundExe);
                            emu.Path = MakeRelativePath(foundExe);
                            SaveEmulators();
                            return true;
                        }
                    }
                    catch { }
                }
            }

            return false;
        }

        public async Task<bool> CheckForUpdates(string emulatorId)
        {
            var emu = FindEmulator(emulatorId);
            if (emu == null || string.IsNullOrWhiteSpace(emu.Repo)) return false;

            try
            {
                var info = await GetLatestReleaseInfoAsync(emu.Repo);
                if (info != null)
                {
                    string latestTag = info.Value.TagName;
                    return IsUpdateAvailable(emu.Version, latestTag);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to check updates for {emulatorId}: {ex.Message}");
            }
            return false;
        }

        public async Task<PackageInstallResult> InstallEmulator(string emulatorId, IProgress<int>? progress = null)
        {
            try
            {
                var service = new EmulatorInstallationService();
                var serviceProgress = new Progress<EmulatorInstallationProgress>(p =>
                {
                    progress?.Report(p.Percentage);
                });

                var req = new EmulatorInstallationRequest
                {
                    EmulatorId = emulatorId,
                    Progress = serviceProgress,
                    CancellationToken = CancellationToken.None
                };

                return await service.InstallAsync(req);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to install emulator {emulatorId}: {ex.Message}");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = emulatorId,
                    FailedStage = PackageInstallStage.ResolvingRelease,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<PackageInstallResult> UpdateEmulator(string emulatorId, IProgress<int>? progress = null)
        {
            return await InstallEmulator(emulatorId, progress);
        }

        public async Task<PackageInstallResult> RepairEmulator(string emulatorId, IProgress<int>? progress = null)
        {
            return await InstallEmulator(emulatorId, progress);
        }

        public void RemoveEmulator(string emulatorId)
        {
            var emu = FindEmulator(emulatorId);
            if (emu != null)
            {
                // Remove default console mappings
                List<string> keysToRemove = Config.DefaultEmulators
                    .Where(pair => string.Equals(pair.Value, emu.Path, StringComparison.OrdinalIgnoreCase))
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    Config.DefaultEmulators.Remove(key);
                }

                Config.Emulators.Remove(emu);
                SaveEmulators();
            }
        }

        public bool BrowseManualExecutable(string emulatorId, string? manualPath = null)
        {
            var emu = FindEmulator(emulatorId);
            if (emu == null) return false;

            if (!string.IsNullOrEmpty(manualPath))
            {
                emu.Path = MakeRelativePath(manualPath);
                SaveEmulators();
                return true;
            }

            // Launch UI dialog on UI thread
            bool result = false;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Emulator Executable";
                ofd.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath("Emulators");

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    emu.Path = MakeRelativePath(ofd.FileName);
                    SaveEmulators();
                    result = true;
                }
            }
            return result;
        }

        public EmulatorItem? FindEmulator(string emulatorId)
        {
            return Config.Emulators.FirstOrDefault(e => 
                string.Equals(e.Id, emulatorId, StringComparison.OrdinalIgnoreCase) || 
                string.Equals(e.Name, emulatorId, StringComparison.OrdinalIgnoreCase));
        }

        public static string? FindDuckStationExecutable(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return null;
            try
            {
                var files = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string name = Path.GetFileName(file);
                    if (name.Contains("duckstation", StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }
            }
            catch { }
            return null;
        }

        public string ResolveAndRegisterEmulatorId(string exePath, string platform)
        {
            if (string.IsNullOrEmpty(exePath)) return "";

            string normalizedPath = exePath.Replace('\\', '/').ToLower();
            string fileName = Path.GetFileName(normalizedPath);

            // 1. Check if path contains duckstation
            if (normalizedPath.Contains("duckstation") || fileName.Contains("duckstation"))
            {
                var duck = Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, "duckstation", StringComparison.OrdinalIgnoreCase));
                if (duck != null)
                {
                    duck.Path = exePath;
                    SaveEmulators();
                }
                return "duckstation";
            }

            // 2. Check if path contains pcsx2
            if (normalizedPath.Contains("pcsx2") || fileName.Contains("pcsx2"))
            {
                var pcsx = Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, "pcsx2", StringComparison.OrdinalIgnoreCase));
                if (pcsx != null)
                {
                    pcsx.Path = exePath;
                    SaveEmulators();
                }
                return "pcsx2";
            }

            // 3. Check if path contains rpcs3
            if (normalizedPath.Contains("rpcs3") || fileName.Contains("rpcs3"))
            {
                var rpcs = Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, "rpcs3", StringComparison.OrdinalIgnoreCase));
                if (rpcs != null)
                {
                    rpcs.Path = exePath;
                    SaveEmulators();
                }
                return "rpcs3";
            }

            // 4. Match exact path
            var matchedEmu = Config.Emulators.FirstOrDefault(e => string.Equals(e.Path, exePath, StringComparison.OrdinalIgnoreCase));
            if (matchedEmu != null)
            {
                return matchedEmu.Id;
            }

            // 5. Register new custom emulator
            string name = Path.GetFileNameWithoutExtension(exePath);
            string id = name.ToLower().Replace(" ", "_");
            if (!Config.Emulators.Any(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                var newEmu = new EmulatorItem
                {
                    Id = id,
                    Name = name,
                    Path = exePath,
                    SupportedPlatforms = new List<string> { platform },
                    Status = "Installed"
                };
                Config.Emulators.Add(newEmu);
                SaveEmulators();
            }
            return id;
        }

        // Backward compatibility static methods
        public static EmulatorConfig LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    string localPath = Path.Combine(Directory.GetCurrentDirectory(), "emulators.json");
                    if (File.Exists(localPath))
                    {
                        string jsonText = File.ReadAllText(localPath);
                        return JsonSerializer.Deserialize<EmulatorConfig>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? CreateDefaultConfig();
                    }
                    var defaultConfig = CreateDefaultConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<EmulatorConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public static void SaveConfig(EmulatorConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }

        private static EmulatorConfig CreateDefaultConfig()
        {
            return new EmulatorConfig
            {
                Emulators = new List<EmulatorItem>
                {
                    new EmulatorItem
                    {
                        Id = "duckstation",
                        Name = "DuckStation",
                        SupportedPlatforms = new List<string> { "Sony PlayStation 1" },
                        GithubRepository = "stenzek/duckstation",
                        ReleaseAssetPattern = "duckstation-windows-x64-release.zip",
                        ExecutablePath = "Emulators/PS1/duckstation.exe",
                        InstallFolder = "Emulators/PS1",
                        ArchiveType = "zip",
                        InstalledVersion = "",
                        LatestVersion = "",
                        Status = "Missing",
                        RequiresBIOS = true,
                        RequiresFirmware = false,
                        DefaultLaunchArguments = "-fullscreen"
                    },
                    new EmulatorItem
                    {
                        Id = "pcsx2",
                        Name = "PCSX2",
                        SupportedPlatforms = new List<string> { "Sony PlayStation 2" },
                        GithubRepository = "PCSX2/pcsx2",
                        ReleaseAssetPattern = "pcsx2-v1.7.*-windows-x64-Qt.7z",
                        ExecutablePath = "Emulators/PS2/pcsx2.exe",
                        InstallFolder = "Emulators/PS2",
                        ArchiveType = "7z",
                        InstalledVersion = "",
                        LatestVersion = "",
                        Status = "Missing",
                        RequiresBIOS = true,
                        RequiresFirmware = false,
                        DefaultLaunchArguments = "-fullscreen"
                    },
                    new EmulatorItem
                    {
                        Id = "rpcs3",
                        Name = "RPCS3",
                        SupportedPlatforms = new List<string> { "Sony PlayStation 3" },
                        GithubRepository = "RPCS3/rpcs3-binaries-win",
                        ReleaseAssetPattern = "rpcs3-v0.0.*_win64.7z",
                        ExecutablePath = "Emulators/PS3/rpcs3.exe",
                        InstallFolder = "Emulators/PS3",
                        ArchiveType = "7z",
                        InstalledVersion = "",
                        LatestVersion = "",
                        Status = "Missing",
                        RequiresBIOS = false,
                        RequiresFirmware = true,
                        DefaultLaunchArguments = "--fullscreen"
                    }
                },
                DefaultEmulators = new Dictionary<string, string>
                {
                    { "Sony PlayStation 1", "Emulators/PS1/duckstation.exe" },
                    { "Sony PlayStation 2", "Emulators/PS2/pcsx2.exe" },
                    { "Sony PlayStation 3", "Emulators/PS3/rpcs3.exe" }
                }
            };
        }

        // Helper Utilities
        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }

        private string MakeRelativePath(string fullPath)
        {
            string baseDir = AppContext.BaseDirectory;
            if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            return fullPath;
        }

        private static string? FindExecutableInFolder(string folder)
        {
            // First search top-level folder
            var exes = Directory.GetFiles(folder, "*.exe", SearchOption.TopDirectoryOnly);
            if (exes.Length > 0) return exes[0];

            // Search recursively
            exes = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
            return exes.Length > 0 ? exes[0] : null;
        }

        private static bool Extract7zUsingTar(string archivePath, string destDir)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "tar.exe",
                    Arguments = $"-xf \"{archivePath}\" -C \"{destDir}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc?.WaitForExit();
                    return proc?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<(string TagName, string DownloadUrl, long Size)?> GetLatestReleaseInfoAsync(string repo)
        {
            if (string.IsNullOrWhiteSpace(repo)) return null;
            string[] parts = repo.Split('/');
            if (parts.Length != 2) return null;

            try
            {
                var clientResult = await GitHubReleaseClient.Instance.GetLatestReleaseAsync(parts[0], parts[1], null, CancellationToken.None);
                if (clientResult.Success && clientResult.Data != null)
                {
                    var release = clientResult.Data;
                    var definitionProvider = new JsonEmulatorPackageDefinitionProvider();
                    var definition = definitionProvider.GetById(parts[1].ToLower()) ?? new EmulatorPackageDefinition
                    {
                        Id = parts[1].ToLower(),
                        DisplayName = parts[1],
                        InstallDirectoryName = $"Emulators/{parts[1]}"
                    };

                    var releaseInfo = new ReleaseInfo
                    {
                        Tag = release.TagName,
                        Name = release.Name
                    };
                    foreach (var a in release.Assets)
                    {
                        releaseInfo.Assets.Add(new ReleaseAssetInfo
                        {
                            Name = a.Name,
                            DownloadUrl = a.BrowserDownloadUrl,
                            Size = a.Size
                        });
                    }

                    var selector = new ReleaseAssetSelector();
                    var selectResult = selector.SelectAsset(definition, releaseInfo);
                    if (selectResult.Success && selectResult.SelectedAsset != null)
                    {
                        return (release.TagName, selectResult.SelectedAsset.DownloadUrl, selectResult.SelectedAsset.Size);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to fetch release info for '{repo}': {ex.Message}");
            }
            return null;
        }

        public static bool IsUpdateAvailable(string emuId, string currentVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion)) return false;
            if (currentVersion == latestVersion) return false;

            if (string.Equals(emuId, "rpcs3", StringComparison.OrdinalIgnoreCase))
            {
                int currentBuild = ParseRpcs3BuildNumber(currentVersion);
                int latestBuild = ParseRpcs3BuildNumber(latestVersion);
                if (currentBuild > 0 && latestBuild > 0)
                {
                    return latestBuild > currentBuild;
                }
            }

            string cleanCurrent = CleanVersionString(currentVersion);
            string cleanLatest = CleanVersionString(latestVersion);

            if (Version.TryParse(cleanCurrent, out Version? valCurrent) && 
                Version.TryParse(cleanLatest, out Version? valLatest))
            {
                return valLatest > valCurrent;
            }
            return string.Compare(cleanLatest, cleanCurrent, StringComparison.OrdinalIgnoreCase) > 0;
        }

        private static int ParseRpcs3BuildNumber(string version)
        {
            if (string.IsNullOrEmpty(version)) return 0;
            var match = System.Text.RegularExpressions.Regex.Match(version, @"-(\d+)(?:-|$)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int buildNum))
            {
                return buildNum;
            }
            return 0;
        }

        private static bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            return IsUpdateAvailable("", currentVersion, latestVersion);
        }

        private static string CleanVersionString(string ver)
        {
            ver = ver.Trim().ToLower().TrimStart('v').TrimStart('r');
            int dashIndex = ver.IndexOf('-');
            if (dashIndex > 0) ver = ver.Substring(0, dashIndex);
            return ver;
        }

        public async Task<bool> InstallDuckStationFromApiAsync(string apiEndpoint, Action<int> progressCallback)
        {
            string tempFile = "";
            string tempExtractDir = "";
            try
            {
                var api = new ApiClient();

                // 1. Fetch package info from API
                var info = await api.GetDuckStationPackageAsync(apiEndpoint);
                if (info == null || string.IsNullOrEmpty(info.DownloadUrl))
                {
                    throw new Exception("Invalid API response or download URL is missing.");
                }

                // 2. Download and verify package
                tempFile = await api.DownloadAndVerifyPackageAsync(info.DownloadUrl, info.FileName, info.Sha256, progressCallback);

                // 3. Safe Extract
                string destDir = ResolvePath("Emulators/DuckStation");
                tempExtractDir = Path.Combine(Path.GetTempPath(), "DuckStation_Extract_Temp_" + Guid.NewGuid().ToString("N"));

                await api.ExtractPackageAsync(tempFile, info.ArchiveType, tempExtractDir);

                // Verify executable exists in the temp directory
                string? exeInTemp = FindExecutableInFolder(tempExtractDir);
                if (string.IsNullOrEmpty(exeInTemp) || !File.Exists(exeInTemp))
                {
                    throw new Exception("DuckStation executable not found in the extracted package.");
                }

                // Overwrite destination without deleting saves/bios/configs
                CopyDirectory(tempExtractDir, destDir);

                // Find where the executable is in destination
                string relativeExePath = MakeRelativePath(Path.Combine(destDir, Path.GetFileName(exeInTemp)));

                // 4. Update configuration
                var item = Config.Emulators.FirstOrDefault(e => string.Equals(e.Id, "duckstation", StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    item.InstalledVersion = info.Version;
                    item.LatestVersion = info.Version;
                    item.Status = "Installed";
                    item.ExecutablePath = relativeExePath.Replace('\\', '/');
                    SaveEmulators();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DuckStation API installation failed: {ex.Message}");
                throw;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile)) File.Delete(tempFile);
                    if (!string.IsNullOrEmpty(tempExtractDir) && Directory.Exists(tempExtractDir)) Directory.Delete(tempExtractDir, true);
                }
                catch { }
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir).ToLower();
                if (dirName == "bios" || dirName == "saves" || dirName == "configs" || dirName == "screenshots" || dirName == "games" || dirName == "roms")
                {
                    continue;
                }

                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
