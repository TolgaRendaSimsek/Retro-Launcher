using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroLauncher.Infrastructure.Http
{
    public class ApiClient
    {
        private static readonly HttpClient _client;

        static ApiClient()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher");
        }

        public async Task<DuckStationPackageInfo?> GetDuckStationPackageAsync(string apiEndpoint)
        {
            try
            {
                string response = await _client.GetStringAsync(apiEndpoint);
                return JsonSerializer.Deserialize<DuckStationPackageInfo>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve DuckStation package info from API: {ex.Message}", ex);
            }
        }

        public async Task<BiosPackageInfo?> GetBiosPackageAsync(string apiEndpoint, string platform)
        {
            try
            {
                string requestUrl = apiEndpoint;
                if (!requestUrl.Contains("?"))
                {
                    requestUrl += $"?console={Uri.EscapeDataString(platform)}";
                }
                else
                {
                    requestUrl += $"&console={Uri.EscapeDataString(platform)}";
                }

                string response = await _client.GetStringAsync(requestUrl);
                return JsonSerializer.Deserialize<BiosPackageInfo>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve BIOS package info from API for {platform}: {ex.Message}", ex);
            }
        }

        public async Task<string> DownloadAndVerifyPackageAsync(string downloadUrl, string fileName, string expectedSha256, Action<int> progressCallback)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), string.IsNullOrEmpty(fileName) ? "download_temp.tmp" : fileName);
            try
            {
                using (var response = await _client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    long? totalBytes = response.Content.Headers.ContentLength;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;

                            if (totalBytes.HasValue && totalBytes.Value > 0)
                            {
                                int progress = (int)((double)totalReadBytes / totalBytes.Value * 100);
                                progressCallback?.Invoke(progress);
                            }
                        }
                    }
                }

                // Verify SHA256 checksum
                if (!string.IsNullOrEmpty(expectedSha256))
                {
                    string calculatedHash;
                    using (var sha256 = SHA256.Create())
                    using (var stream = File.OpenRead(tempFile))
                    {
                        byte[] hashBytes = sha256.ComputeHash(stream);
                        calculatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }

                    if (!string.Equals(calculatedHash, expectedSha256.Trim().ToLower(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(tempFile)) File.Delete(tempFile);
                        throw new Exception("SHA256 checksum verification failed! The downloaded file might be corrupted or tampered with.");
                    }
                }

                return tempFile;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
                throw new Exception($"Failed to download or verify package: {ex.Message}", ex);
            }
        }

        public async Task ExtractPackageAsync(string archivePath, string archiveType, string targetFolder)
        {
            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string type = (archiveType ?? "").ToLower().Trim();
                if (type == "zip" || archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Run(() => System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, targetFolder, true));
                }
                else if (type == "7z" || archivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                {
                    bool extractSuccess = await Task.Run(() => Extract7zUsingTar(archivePath, targetFolder));
                    if (!extractSuccess)
                    {
                        throw new Exception("Failed to extract 7z archive using system tar.exe tool.");
                    }
                }
                else
                {
                    throw new Exception($"Unsupported archive type format: {archiveType}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract installation package: {ex.Message}", ex);
            }
        }

        private static bool Extract7zUsingTar(string archivePath, string destDir)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tar.exe",
                    Arguments = $"-xf \"{archivePath}\" -C \"{destDir}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = System.Diagnostics.Process.Start(psi))
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
    }
}
