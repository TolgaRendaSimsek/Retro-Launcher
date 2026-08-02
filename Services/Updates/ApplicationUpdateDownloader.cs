using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RetroLauncher.Core.Utilities;

namespace RetroLauncher.Services.Updates
{
    public interface IApplicationUpdateDownloader
    {
        Task<string> DownloadUpdatePackageAsync(
            ApplicationUpdateCheckResult updateResult,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default);

        Task<bool> VerifySha256ChecksumAsync(
            string packageFilePath,
            string expectedChecksum,
            CancellationToken cancellationToken = default);
    }

    public class ApplicationUpdateDownloader : IApplicationUpdateDownloader
    {
        private readonly HttpClient _httpClient;
        private static readonly string DownloadDir = Path.Combine(ApplicationPaths.BaseDataDir, "Updates", "Downloads");

        public ApplicationUpdateDownloader(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> DownloadUpdatePackageAsync(
            ApplicationUpdateCheckResult updateResult,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (updateResult == null || updateResult.DownloadUri == null || string.IsNullOrWhiteSpace(updateResult.AssetName))
            {
                throw new ArgumentException("Invalid update check result or download URL.");
            }

            if (!Directory.Exists(DownloadDir))
            {
                Directory.CreateDirectory(DownloadDir);
            }

            string finalFilePath = Path.Combine(DownloadDir, updateResult.AssetName);
            string partFilePath = finalFilePath + ".part";

            if (File.Exists(partFilePath))
            {
                try { File.Delete(partFilePath); } catch { }
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, updateResult.DownloadUri);
            request.Headers.UserAgent.ParseAdd("RetroLauncher");
            request.Headers.Accept.ParseAdd("application/octet-stream");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength ?? updateResult.AssetSize;

            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            using (var fileStream = new FileStream(partFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                byte[] buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalRead += bytesRead;

                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        int percent = (int)((double)totalRead / totalBytes.Value * 100);
                        progress?.Report(Math.Min(100, Math.Max(0, percent)));
                    }
                }
            }

            // Validation Checks
            var fileInfo = new FileInfo(partFilePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                throw new InvalidDataException("Downloaded package file is empty or missing.");
            }

            if (totalBytes.HasValue && totalBytes.Value > 0 && fileInfo.Length != totalBytes.Value)
            {
                throw new InvalidDataException($"Downloaded package size ({fileInfo.Length} bytes) does not match expected size ({totalBytes.Value} bytes).");
            }

            // Validate ZIP signature if .zip
            if (updateResult.AssetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var fs = File.OpenRead(partFilePath);
                byte[] header = new byte[4];
                if (fs.Read(header, 0, 4) < 4 || header[0] != 0x50 || header[1] != 0x4B)
                {
                    throw new InvalidDataException("Downloaded package file does not have a valid ZIP header signature.");
                }
            }

            if (File.Exists(finalFilePath))
            {
                try { File.Delete(finalFilePath); } catch { }
            }

            File.Move(partFilePath, finalFilePath);
            progress?.Report(100);
            return finalFilePath;
        }

        public async Task<bool> VerifySha256ChecksumAsync(
            string packageFilePath,
            string expectedChecksum,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(packageFilePath) || string.IsNullOrWhiteSpace(expectedChecksum))
            {
                return false;
            }

            using var sha256 = SHA256.Create();
            using var fileStream = File.OpenRead(packageFilePath);
            byte[] hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
            string calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return string.Equals(calculatedHash, expectedChecksum.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
