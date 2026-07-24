using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace RetroLauncher
{
    public class EmulatorUpdateService : IEmulatorUpdateService
    {
        private static readonly ConcurrentDictionary<string, EmulatorUpdateInfo> _lastRemoteResults = new();
        
        private readonly IEmulatorDefinitionProvider _definitionProvider;
        private readonly IReleaseProvider _releaseProvider;
        private readonly IReleaseAssetSelector _assetSelector;
        
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(10);

        public EmulatorUpdateService(
            IEmulatorDefinitionProvider? definitionProvider = null,
            IReleaseProvider? releaseProvider = null,
            IReleaseAssetSelector? assetSelector = null)
        {
            _definitionProvider = definitionProvider ?? new JsonEmulatorDefinitionProvider();
            _releaseProvider = releaseProvider ?? new GitHubReleaseProvider();
            _assetSelector = assetSelector ?? new ReleaseAssetSelector();
            
            LoadLastResults();
        }

        public async Task<EmulatorUpdateInfo> CheckForUpdateAsync(string emulatorId, EmulatorReleaseChannel channel, CancellationToken cancellationToken)
        {
            var definition = _definitionProvider.GetById(emulatorId);
            if (definition == null)
            {
                return new EmulatorUpdateInfo
                {
                    EmulatorId = emulatorId,
                    Error = "Emulator definition not found.",
                    DisplayStatus = "Unable to check"
                };
            }

            var emuItem = EmulatorManager.Instance.Config.Emulators.FirstOrDefault(x => string.Equals(x.Id, emulatorId, StringComparison.OrdinalIgnoreCase));
            if (emuItem != null && !string.IsNullOrEmpty(emuItem.ReleaseChannel))
            {
                if (Enum.TryParse<EmulatorReleaseChannel>(emuItem.ReleaseChannel, out var parsedChannel))
                {
                    definition.ReleaseChannel = parsedChannel;
                }
            }
            else
            {
                definition.ReleaseChannel = channel;
            }

            string localVersion = "Not Detected";
            string localPath = emuItem?.Path ?? "";
            string localStatus = "Not installed";

            if (emuItem != null && !string.IsNullOrEmpty(localPath))
            {
                string resolved = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, localPath));
                if (File.Exists(resolved))
                {
                    try
                    {
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(resolved);
                        localVersion = versionInfo.ProductVersion ?? versionInfo.FileVersion ?? emuItem.InstalledVersion;
                    }
                    catch
                    {
                        localVersion = emuItem.InstalledVersion;
                    }
                    if (string.IsNullOrEmpty(localVersion)) localVersion = "Detected";

                    if (!string.IsNullOrEmpty(emuItem.InstallFolder))
                    {
                        string standardPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emuItem.InstallFolder));
                        string resolvedDir = Path.GetDirectoryName(resolved) ?? "";
                        if (resolvedDir.StartsWith(standardPath, StringComparison.OrdinalIgnoreCase))
                        {
                            localStatus = "Installed";
                        }
                        else
                        {
                            localStatus = "Manually configured";
                        }
                    }
                    else
                    {
                        localStatus = "Manually configured";
                    }
                }
                else
                {
                    localStatus = "Executable missing";
                }
            }

            var updateInfo = new EmulatorUpdateInfo
            {
                EmulatorId = emulatorId,
                InstalledVersion = localVersion,
                InstalledReleaseTag = emuItem?.InstalledVersion ?? "",
                CurrentChannel = definition.ReleaseChannel.ToString(),
                DisplayStatus = localStatus,
                CheckedAt = DateTime.UtcNow
            };

            if (localStatus == "Executable missing" || localStatus == "Not installed")
            {
                return updateInfo;
            }

            ReleaseInfo? latestRelease = null;
            
            try
            {
                var query = new ReleaseQuery
                {
                    Owner = definition.RepositoryOwner,
                    Repository = definition.RepositoryName,
                    Channel = definition.ReleaseChannel == EmulatorReleaseChannel.Stable ? ReleaseChannel.Stable : ReleaseChannel.Preview
                };

                if (definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubBinaryRepository ||
                    definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubReleaseList)
                {
                    var listRes = await _releaseProvider.GetReleasesAsync(query, cancellationToken);
                    if (listRes.Success && listRes.Data != null && listRes.Data.Any())
                    {
                        var selectNew = (IReleaseAssetSelectorNew)_assetSelector;
                        foreach (var rel in listRes.Data)
                        {
                            var selectResult = selectNew.SelectAsset(definition, rel);
                            if (selectResult.Success && selectResult.SelectedAsset != null)
                            {
                                latestRelease = rel;
                                updateInfo.SelectedAsset = selectResult.SelectedAsset.Name;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var latestRes = await _releaseProvider.GetLatestReleaseAsync(query, cancellationToken);
                    if (latestRes.Success) latestRelease = latestRes.Data;
                }
            }
            catch (Exception ex)
            {
                updateInfo.Error = $"GitHub offline or API rate limited: {ex.Message}";
                if (_lastRemoteResults.TryGetValue(emulatorId, out var lastResult))
                {
                    updateInfo.AvailableVersion = lastResult.AvailableVersion;
                    updateInfo.AvailableReleaseTag = lastResult.AvailableReleaseTag;
                    updateInfo.PublishedAt = lastResult.PublishedAt;
                    updateInfo.DisplayStatus = "Unable to check";
                }
                else
                {
                    updateInfo.DisplayStatus = "Unable to check";
                }
                return updateInfo;
            }

            if (latestRelease != null)
            {
                updateInfo.AvailableVersion = latestRelease.Tag;
                updateInfo.AvailableReleaseTag = latestRelease.Tag;
                updateInfo.PublishedAt = latestRelease.PublishedAt;

                IEmulatorVersionStrategy strategy = GetVersionStrategy(emulatorId);
                
                DateTime? installedTime = null;
                string manifestPath = Path.Combine(AppContext.BaseDirectory, emuItem?.InstallFolder ?? "", "install.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        string manifestJson = File.ReadAllText(manifestPath);
                        var manifest = JsonSerializer.Deserialize<InstalledEmulatorInfo>(manifestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        installedTime = manifest?.InstalledAt;
                    }
                    catch { }
                }

                bool isNewer = strategy.IsNewer(updateInfo.InstalledVersion, updateInfo.AvailableVersion, installedTime, updateInfo.PublishedAt);

                if (isNewer)
                {
                    updateInfo.IsUpdateAvailable = true;
                    updateInfo.DisplayStatus = "Update available";
                }
                else
                {
                    updateInfo.IsUpdateAvailable = false;
                    updateInfo.DisplayStatus = "Up to date";
                }

                _lastRemoteResults[emulatorId] = updateInfo;
                SaveLastResults();
            }
            else
            {
                updateInfo.DisplayStatus = "Unable to check";
            }

            return updateInfo;
        }

        private IEmulatorVersionStrategy GetVersionStrategy(string emulatorId)
        {
            if (string.Equals(emulatorId, "rpcs3", StringComparison.OrdinalIgnoreCase))
            {
                return new RollingBuildStrategy();
            }
            if (string.Equals(emulatorId, "pcsx2", StringComparison.OrdinalIgnoreCase))
            {
                return new RollingBuildStrategy(); // PCSX2 uses dev/rolling builds too
            }
            return new SemanticVersionStrategy();
        }

        private void LoadLastResults()
        {
            try
            {
                string cachePath = Path.Combine(AppContext.BaseDirectory, "Cache", "UpdateResults.json");
                if (File.Exists(cachePath))
                {
                    string json = File.ReadAllText(cachePath);
                    var list = JsonSerializer.Deserialize<ConcurrentDictionary<string, EmulatorUpdateInfo>>(json);
                    if (list != null)
                    {
                        foreach (var kvp in list)
                        {
                            _lastRemoteResults[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveLastResults()
        {
            try
            {
                string cacheDir = Path.Combine(AppContext.BaseDirectory, "Cache");
                if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

                string cachePath = Path.Combine(cacheDir, "UpdateResults.json");
                string json = JsonSerializer.Serialize(_lastRemoteResults, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(cachePath, json);
            }
            catch { }
        }
    }
}
