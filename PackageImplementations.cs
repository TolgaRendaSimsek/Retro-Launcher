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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher");
                string json = await client.GetStringAsync($"https://api.github.com/repos/{source}/releases/latest", token);
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    string version = root.GetProperty("tag_name").GetString() ?? "";
                    string downloadUrl = "";
                    string fileName = "";
                    long size = 0;

                    if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var asset in assets.EnumerateArray())
                        {
                            string name = asset.GetProperty("name").GetString() ?? "";
                            string nameLower = name.ToLower();
                            if ((nameLower.Contains("win") || nameLower.Contains("x64") || nameLower.Contains("x86_64") || nameLower.Contains("windows")) &&
                                (nameLower.EndsWith(".zip") || nameLower.EndsWith(".7z")))
                            {
                                downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                fileName = name;
                                size = asset.GetProperty("size").GetInt64();
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        var manifest = new PackageManifest
                        {
                            id = source.Split('/').Last().ToLower(),
                            name = source.Split('/').Last(),
                            description = $"Latest release of {source}",
                            packageType = PackageType.Emulator,
                            version = version,
                            downloadUrl = downloadUrl,
                            fileName = fileName,
                            archiveType = fileName.EndsWith(".7z", StringComparison.OrdinalIgnoreCase) ? "7z" : "zip",
                            downloadSize = size,
                            installFolder = source.Split('/').Last()
                        };
                        return new List<PackageManifest> { manifest };
                    }
                }
            }
            return new List<PackageManifest>();
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

    public class ZipArchiveExtractor : IArchiveExtractor
    {
        public bool CanExtract(string archiveType) => string.Equals(archiveType, "zip", StringComparison.OrdinalIgnoreCase);

        public async Task<bool> ExtractAsync(string archivePath, string destinationPath, CancellationToken token)
        {
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, destinationPath, true), token);
            return true;
        }
    }

    public class SevenZipArchiveExtractor : IArchiveExtractor
    {
        public bool CanExtract(string archiveType) => string.Equals(archiveType, "7z", StringComparison.OrdinalIgnoreCase);

        public async Task<bool> ExtractAsync(string archivePath, string destinationPath, CancellationToken token)
        {
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            return await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "tar.exe",
                        Arguments = $"-xf \"{archivePath}\" -C \"{destinationPath}\"",
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
            }, token);
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
