using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class JsonPackageCatalogProvider : IPackageCatalogProvider
    {
        public async Task<List<PackageManifest>> GetCatalogAsync(string source, CancellationToken token)
        {
            if (File.Exists(source))
            {
                string json = await File.ReadAllTextAsync(source, token);
                using (var doc = JsonDocument.Parse(json))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        return JsonSerializer.Deserialize<List<PackageManifest>>(json, options) ?? new();
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("packages", out var packagesProp))
                    {
                        return JsonSerializer.Deserialize<List<PackageManifest>>(packagesProp.GetRawText(), options) ?? new();
                    }
                }
            }
            return new List<PackageManifest>();
        }
    }

    public class GitHubReleaseCatalogProvider : IPackageCatalogProvider
    {
        public async Task<List<PackageManifest>> GetCatalogAsync(string source, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("Source repository string cannot be null or empty.");
            }

            string[] parts = source.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid source repository format: '{source}'. Expected 'owner/repo'.");
            }

            string owner = parts[0];
            string repo = parts[1];

            var client = GitHubReleaseClient.Instance;
            var result = await client.GetLatestReleaseAsync(owner, repo, null, token);
            if (!result.Success || result.Data == null)
            {
                throw new Exception($"Failed to retrieve package catalog from GitHub: {result.ErrorMessage}");
            }

            var release = result.Data;
            
            // Map GitHubRelease to ReleaseInfo
            var releaseInfo = new ReleaseInfo
            {
                Provider = ReleaseProviderType.GitHub,
                RepositoryIdentifier = source,
                Tag = release.TagName,
                Name = release.Name,
                Description = release.Name,
                IsDraft = release.IsDraft,
                IsPrerelease = release.IsPrerelease,
                PublishedAt = release.PublishedAt,
                WebUrl = release.HtmlUrl
            };

            foreach (var asset in release.Assets)
            {
                releaseInfo.Assets.Add(new ReleaseAssetInfo
                {
                    Id = asset.Name,
                    Name = asset.Name,
                    DownloadUrl = asset.BrowserDownloadUrl,
                    Size = asset.Size,
                    ContentType = asset.ContentType,
                    CreatedAt = asset.CreatedAt,
                    UpdatedAt = asset.UpdatedAt
                });
            }

            // Get configured emulator definition for better rules mapping, or fallback
            var definitionProvider = new JsonEmulatorDefinitionProvider();
            var definition = definitionProvider.GetById(repo.ToLower()) ?? new EmulatorDefinition
            {
                Id = repo.ToLower(),
                DisplayName = repo,
                InstallationDirectoryName = $"Emulators/{repo}"
            };

            var selector = new ReleaseAssetSelector();
            var selectResult = selector.SelectAsset(definition, releaseInfo);
            if (!selectResult.Success || selectResult.SelectedAsset == null)
            {
                throw new Exception($"Failed to find a compatible release asset for repository '{source}': {selectResult.Message}");
            }

            var selectedAsset = selectResult.SelectedAsset;
            string ext = Path.GetExtension(selectedAsset.Name).ToLower();
            string archiveType = ext == ".7z" ? "7z" : "zip";

            var manifest = new PackageManifest
            {
                id = repo.ToLower(),
                name = repo,
                description = $"Latest release of {source}",
                packageType = PackageType.Emulator,
                version = release.TagName,
                downloadUrl = selectedAsset.DownloadUrl,
                fileName = selectedAsset.Name,
                archiveType = archiveType,
                downloadSize = selectedAsset.Size,
                installFolder = repo
            };

            return new List<PackageManifest> { manifest };
        }
    }

    public class HttpPackageDownloader : IPackageDownloader
    {
        public async Task<string> DownloadAsync(string url, IProgress<int>? progress, CancellationToken token)
        {
            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string fileExtension = url.Contains(".7z") ? "7z" : "zip";
            string tempFile = Path.Combine(tempDir, $"{Guid.NewGuid():N}.{fileExtension}");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher");
                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    response.EnsureSuccessStatusCode();
                    long? totalBytes = response.Content.Headers.ContentLength;

                    using (var contentStream = await response.Content.ReadAsStreamAsync(token))
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                            totalRead += bytesRead;

                            if (totalBytes.HasValue)
                            {
                                int percent = (int)((double)totalRead / totalBytes.Value * 100);
                                progress?.Report(percent);
                            }
                        }
                    }
                }
            }
            return tempFile;
        }
    }

    public class Sha256PackageVerifier : IPackageVerifier
    {
        public async Task<bool> VerifyAsync(string filePath, string expectedHash, CancellationToken token)
        {
            if (string.IsNullOrEmpty(expectedHash)) return true; // Skip if no hash provided

            using (var sha = SHA256.Create())
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
            {
                byte[] hash = await sha.ComputeHashAsync(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                string computed = sb.ToString();
                return string.Equals(computed, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
        }
    }



    public class JsonInstalledPackageRepository : IPackageRepository
    {
        private static readonly string RegistryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installed_packages.json");
        private static readonly object FileLock = new object();
        private InstalledPackagesConfig _config = new();

        public JsonInstalledPackageRepository()
        {
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(RegistryPath))
                    {
                        string json = File.ReadAllText(RegistryPath);
                        _config = JsonSerializer.Deserialize<InstalledPackagesConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new InstalledPackagesConfig();
                    }
                    else
                    {
                        _config = new InstalledPackagesConfig();
                    }
                }
                catch
                {
                    _config = new InstalledPackagesConfig();
                }
            }
        }

        public void Save()
        {
            lock (FileLock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(RegistryPath, json);
                }
                catch { }
            }
        }

        public List<InstalledPackage> GetAll() => _config.InstalledPackages;

        public InstalledPackage? GetById(string packageId)
        {
            return _config.InstalledPackages.FirstOrDefault(p => string.Equals(p.packageId, packageId, StringComparison.OrdinalIgnoreCase));
        }

        public void AddOrUpdate(InstalledPackage package)
        {
            var existing = GetById(package.packageId);
            if (existing != null)
            {
                _config.InstalledPackages.Remove(existing);
            }
            _config.InstalledPackages.Add(package);
            Save();
        }

        public void Remove(string packageId)
        {
            var existing = GetById(packageId);
            if (existing != null)
            {
                _config.InstalledPackages.Remove(existing);
                Save();
            }
        }
    }

    public class PackageUpdateService : IPackageUpdateService
    {
        public bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            if (currentVersion == latestVersion) return false;
            try
            {
                if (Version.TryParse(currentVersion, out var current) && Version.TryParse(latestVersion, out var latest))
                {
                    return latest > current;
                }
                return string.Compare(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
