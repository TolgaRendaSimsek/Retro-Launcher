using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
            var definitionProvider = new JsonEmulatorPackageDefinitionProvider();
            var definition = definitionProvider.GetById(repo.ToLower()) ?? new EmulatorPackageDefinition
            {
                Id = repo.ToLower(),
                DisplayName = repo,
                InstallDirectoryName = $"Emulators/{repo}"
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
        private readonly IHttpClientProvider _clientProvider;
        private readonly HttpClient? _httpClient; // for direct unit test injection
        private readonly int _retryDelayMs;

        public HttpPackageDownloader(IHttpClientProvider? clientProvider = null, HttpClient? httpClient = null, int retryDelayMs = 1000)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
            _httpClient = httpClient;
            _retryDelayMs = retryDelayMs;
        }

        public async Task<string> DownloadAsync(
            string url,
            IProgress<PackageDownloadProgress>? progress,
            CancellationToken token,
            string packageId = "default",
            string operationId = "",
            string originalAssetName = "")
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            // Setup download paths
            string opId = string.IsNullOrWhiteSpace(operationId) ? Guid.NewGuid().ToString("N") : operationId;
            string assetName = string.IsNullOrWhiteSpace(originalAssetName) ? Path.GetFileName(new Uri(url).LocalPath) : originalAssetName;
            if (string.IsNullOrWhiteSpace(assetName)) assetName = "archive.zip";

            string downloadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "downloads", packageId, opId);
            Directory.CreateDirectory(downloadsDir);

            string partFilePath = Path.Combine(downloadsDir, $"{assetName}.part");
            string finalFilePath = Path.Combine(downloadsDir, assetName);

            // Clean up old files
            if (File.Exists(partFilePath)) File.Delete(partFilePath);
            if (File.Exists(finalFilePath)) File.Delete(finalFilePath);

            int maxRetries = 3;
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromMilliseconds(_retryDelayMs);

            while (true)
            {
                attempt++;
                try
                {
                    progress?.Report(new PackageDownloadProgress
                    {
                        BytesDownloaded = 0,
                        TotalBytes = null,
                        Percentage = 0,
                        SpeedBytesPerSecond = 0,
                        CurrentStage = attempt > 1 ? $"Retrying Download (Attempt {attempt}/{maxRetries + 1})" : "Connecting"
                    });

                    // Prepare HttpClient and request timeout
                    var client = _httpClient ?? _clientProvider.GetClient("PackageDownloads");
                    int timeoutSec = 100; // default timeout
                    try
                    {
                        var settings = ApplicationSettingsService.Instance;
                        if (settings?.Network != null)
                        {
                            timeoutSec = settings.Network.RequestTimeoutSeconds;
                        }
                    }
                    catch { }

                    using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec)))
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token))
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.UserAgent.ParseAdd("RetroLauncher");

                        using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token))
                        {
                            // 1. Validate HTTP Status Code
                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                throw new HttpRequestException(message: "File not found (404).", inner: null, statusCode: HttpStatusCode.NotFound);
                            }
                            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
                            {
                                throw new HttpRequestException(message: $"Access denied or rate limited ({response.StatusCode}).", inner: null, statusCode: response.StatusCode);
                            }
                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException(message: $"Server returned status code: {response.StatusCode}", inner: null, statusCode: response.StatusCode);
                            }

                            // 2. Validate final response URL (check for redirect errors or HTML logins)
                            var requestUri = response.RequestMessage?.RequestUri;
                            if (requestUri != null && requestUri.LocalPath.Contains("/login", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidDataException("Redirected to a login or proxy page. Download failed.");
                            }

                            // 3. Validate Content-Type
                            var contentType = response.Content.Headers.ContentType?.MediaType;
                            if (string.Equals(contentType, "text/html", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(contentType, "text/plain", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidDataException($"Server returned an invalid Content-Type '{contentType}'. Expected binary archive.");
                            }

                            // Read Content-Length
                            long? totalBytes = response.Content.Headers.ContentLength;

                            using (var contentStream = await response.Content.ReadAsStreamAsync(linkedCts.Token))
                            using (var fileStream = new FileStream(partFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                var buffer = new byte[8192];
                                long totalRead = 0;
                                int bytesRead;

                                var startTime = DateTime.UtcNow;
                                var lastReportTime = startTime;
                                long lastReportRead = 0;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, linkedCts.Token)) > 0)
                                {
                                    linkedCts.Token.ThrowIfCancellationRequested();
                                    await fileStream.WriteAsync(buffer, 0, bytesRead, linkedCts.Token);
                                    totalRead += bytesRead;

                                    var now = DateTime.UtcNow;
                                    var elapsedSinceLastReport = now - lastReportTime;

                                    if (elapsedSinceLastReport.TotalMilliseconds >= 250 || (totalBytes.HasValue && totalRead == totalBytes.Value))
                                    {
                                        double totalElapsedSec = (now - startTime).TotalSeconds;
                                        double speed = totalElapsedSec > 0 ? (double)totalRead / totalElapsedSec : 0;
                                        int percent = totalBytes.HasValue && totalBytes.Value > 0
                                            ? (int)((double)totalRead / totalBytes.Value * 100)
                                            : 0;

                                        progress?.Report(new PackageDownloadProgress
                                        {
                                            BytesDownloaded = totalRead,
                                            TotalBytes = totalBytes,
                                            Percentage = Math.Min(Math.Max(percent, 0), 100),
                                            SpeedBytesPerSecond = speed,
                                            CurrentStage = "Downloading"
                                        });

                                        lastReportTime = now;
                                        lastReportRead = totalRead;
                                    }
                                }
                            }

                            // 4. Validate non-zero file size and matching Content-Length
                            var info = new FileInfo(partFilePath);
                            if (info.Length == 0)
                            {
                                throw new InvalidDataException("Downloaded file is empty.");
                            }
                            if (totalBytes.HasValue && totalBytes.Value > 0 && info.Length != totalBytes.Value)
                            {
                                throw new InvalidDataException($"Downloaded file size ({info.Length} bytes) does not match Content-Length ({totalBytes.Value} bytes).");
                            }

                            // 5. Validate archive signature (magic bytes)
                            if (!ValidateArchiveSignature(partFilePath, assetName))
                            {
                                throw new InvalidDataException("Downloaded file is not a valid ZIP or 7z archive (invalid signature magic bytes).");
                            }

                            // Success: Rename part to final target file
                            File.Move(partFilePath, finalFilePath);
                            return finalFilePath;
                        }
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // Clean up partial files
                    CleanFile(partFilePath);
                    throw;
                }
                catch (Exception ex)
                {
                    CleanFile(partFilePath);

                    // Re-throw if error is fatal (404, invalid signatures, or we exceeded retry budget)
                    bool isFatal = ex is InvalidDataException || 
                                   (ex is HttpRequestException httpEx && (httpEx.StatusCode == HttpStatusCode.NotFound || httpEx.StatusCode == HttpStatusCode.Forbidden));

                    if (isFatal || attempt > maxRetries)
                    {
                        throw;
                    }

                    // Transient retry with exponential backoff
                    var jitter = new Random();
                    TimeSpan backoff = delay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
                    RetroLogger.Log($"Download transient error (attempt {attempt}/{maxRetries + 1}): {ex.Message}. Retrying in {backoff.TotalSeconds:F1}s...", "WARNING");
                    
                    await Task.Delay(backoff, token);
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
            }
        }

        private static bool ValidateArchiveSignature(string filePath, string assetName)
        {
            try
            {
                byte[] buffer = new byte[6];
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead < 4) return false;
                }

                // 7z magic: 37 7A BC AF 27 1C
                if (buffer[0] == 0x37 && buffer[1] == 0x7A && buffer[2] == 0xBC && buffer[3] == 0xAF && buffer[4] == 0x27 && buffer[5] == 0x1C)
                {
                    return true;
                }
                // ZIP magic: PK\x03\x04
                if (buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private static void CleanFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
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
