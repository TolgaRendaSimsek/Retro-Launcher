using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IHttpClientProvider _clientProvider;
        private readonly IApplicationSettingsService _settings;
        private SemaphoreSlim _semaphore;
        private readonly HashSet<string> _activeDownloads = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        public DownloadManager(
            IHttpClientProvider? clientProvider = null,
            IApplicationSettingsService? settings = null)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
            _settings = settings ?? ApplicationSettingsService.Instance;
            _semaphore = new SemaphoreSlim(_settings.Download.MaxParallelDownloads);
        }

        public void SetMaxConcurrentDownloads(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            _semaphore = new SemaphoreSlim(max);
        }

        private bool TryRegisterActiveDownload(string id)
        {
            lock (_lock)
            {
                if (_activeDownloads.Contains(id)) return false;
                _activeDownloads.Add(id);
                return true;
            }
        }

        private void UnregisterActiveDownload(string id)
        {
            lock (_lock)
            {
                _activeDownloads.Remove(id);
            }
        }

        public async Task<DownloadResult> DownloadAsync(DownloadRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (!TryRegisterActiveDownload(request.EmulatorId))
            {
                return new DownloadResult
                {
                    Success = false,
                    FailureReason = DownloadFailureReason.DuplicateDownload,
                    ErrorMessage = $"A download task for emulator '{request.EmulatorId}' is already in progress."
                };
            }

            try
            {
                await _semaphore.WaitAsync(request.CancellationToken);
                try
                {
                    return await ExecuteDownloadAsync(request);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                return new DownloadResult
                {
                    Success = false,
                    FailureReason = DownloadFailureReason.Cancellation,
                    ErrorMessage = "Download cancelled by user."
                };
            }
            finally
            {
                UnregisterActiveDownload(request.EmulatorId);
            }
        }

        private async Task<DownloadResult> ExecuteDownloadAsync(DownloadRequest request)
        {
            string tempDir = Path.Combine(AppContext.BaseDirectory, _settings.Download.DownloadTempDir);
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string tempPartFile = Path.Combine(tempDir, $"{Guid.NewGuid():N}.part");
            RetroLogger.Log($"Starting stream download of {request.Url} to temp file: {tempPartFile}");

            try
            {
                var client = _clientProvider.GetClient("PackageDownloads");
                var httpReq = new HttpRequestMessage(HttpMethod.Get, request.Url);

                // Safe redirection check
                using (var response = await SafeRedirectHandler.SendWithRedirectsAsync(client, httpReq, request.CancellationToken))
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        return new DownloadResult { Success = false, FailureReason = DownloadFailureReason.HttpError, StatusCode = response.StatusCode, ErrorMessage = "Access Forbidden (403)." };
                    }
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new DownloadResult { Success = false, FailureReason = DownloadFailureReason.HttpError, StatusCode = response.StatusCode, ErrorMessage = "File not found (404)." };
                    }
                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        return new DownloadResult { Success = false, FailureReason = DownloadFailureReason.HttpError, StatusCode = response.StatusCode, ErrorMessage = "Too many requests (429)." };
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return new DownloadResult { Success = false, FailureReason = DownloadFailureReason.HttpError, StatusCode = response.StatusCode, ErrorMessage = $"Server returned HTTP status code: {response.StatusCode}" };
                    }

                    long? totalBytes = response.Content.Headers.ContentLength;
                    if (!totalBytes.HasValue && request.ExpectedSize.HasValue && request.ExpectedSize.Value > 0)
                    {
                        totalBytes = request.ExpectedSize;
                    }

                    using (var contentStream = await response.Content.ReadAsStreamAsync(request.CancellationToken))
                    using (var fileStream = new FileStream(tempPartFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        var startTime = DateTime.UtcNow;
                        var lastReportTime = startTime;
                        long lastReportRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, request.CancellationToken)) > 0)
                        {
                            request.CancellationToken.ThrowIfCancellationRequested();
                            await fileStream.WriteAsync(buffer, 0, bytesRead, request.CancellationToken);
                            totalRead += bytesRead;

                            var now = DateTime.UtcNow;
                            var elapsedSinceLastReport = now - lastReportTime;

                            if (elapsedSinceLastReport.TotalMilliseconds >= 500 || (totalBytes.HasValue && totalRead == totalBytes.Value))
                            {
                                double totalElapsedSeconds = (now - startTime).TotalSeconds;
                                double currentSpeed = elapsedSinceLastReport.TotalSeconds > 0 
                                    ? (totalRead - lastReportRead) / elapsedSinceLastReport.TotalSeconds 
                                    : 0;
                                double averageSpeed = totalElapsedSeconds > 0 
                                    ? totalRead / totalElapsedSeconds 
                                    : 0;

                                int percent = totalBytes.HasValue && totalBytes.Value > 0 
                                    ? (int)((double)totalRead / totalBytes.Value * 100) 
                                    : -1;

                                TimeSpan eta = TimeSpan.Zero;
                                if (totalBytes.HasValue && averageSpeed > 0)
                                {
                                    long remainingBytes = totalBytes.Value - totalRead;
                                    eta = TimeSpan.FromSeconds(remainingBytes / averageSpeed);
                                }

                                request.Progress?.Report(new DownloadProgress
                                {
                                    EmulatorId = request.EmulatorId,
                                    BytesDownloaded = totalRead,
                                    TotalBytes = totalBytes,
                                    Percentage = percent,
                                    CurrentSpeedBytesPerSecond = currentSpeed,
                                    AverageSpeedBytesPerSecond = averageSpeed,
                                    EstimatedRemainingTime = eta
                                });

                                lastReportTime = now;
                                lastReportRead = totalRead;
                            }
                        }
                    }

                    long actualSize = new FileInfo(tempPartFile).Length;
                    if (actualSize == 0)
                    {
                        throw new IOException("Downloaded file is empty.");
                    }
                    if (totalBytes.HasValue && totalBytes.Value > 0 && actualSize != totalBytes.Value)
                    {
                        throw new IOException($"File size mismatch. Expected {totalBytes.Value} bytes, but got {actualSize}.");
                    }

                    // Atomic finalization to target installation directory path
                    string? destDir = Path.GetDirectoryName(request.DestinationPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    if (File.Exists(request.DestinationPath))
                    {
                        File.Delete(request.DestinationPath);
                    }

                    File.Move(tempPartFile, request.DestinationPath);
                    RetroLogger.Log($"Download complete and moved to final destination: {request.DestinationPath}");

                    return new DownloadResult
                    {
                        Success = true,
                        DownloadedFilePath = request.DestinationPath
                    };
                }
            }
            catch (Exception ex)
            {
                string errorMsg;
                var reason = GetFailureReason(ex, out errorMsg);
                RetroLogger.Log($"Download failed for '{request.EmulatorId}': {errorMsg}", "ERROR");

                return new DownloadResult
                {
                    Success = false,
                    FailureReason = reason,
                    ErrorMessage = errorMsg
                };
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPartFile))
                    {
                        File.Delete(tempPartFile);
                    }
                }
                catch { }
            }
        }

        private static DownloadFailureReason GetFailureReason(Exception ex, out string message)
        {
            message = ex.Message;
            if (ex is OperationCanceledException)
            {
                return DownloadFailureReason.Cancellation;
            }
            if (ex is HttpRequestException httpEx)
            {
                if (httpEx.InnerException is System.Net.Sockets.SocketException)
                {
                    return DownloadFailureReason.NetworkUnavailable;
                }
                return DownloadFailureReason.HttpError;
            }
            if (ex is UnauthorizedAccessException)
            {
                return DownloadFailureReason.PermissionDenied;
            }
            if (ex is IOException ioEx)
            {
                int hResult = ioEx.HResult;
                if (hResult == unchecked((int)0x80070070) || hResult == unchecked((int)0x80070027))
                {
                    return DownloadFailureReason.DiskFull;
                }
                if (message.Contains("disk is full", StringComparison.OrdinalIgnoreCase) || 
                    message.Contains("no space", StringComparison.OrdinalIgnoreCase))
                {
                    return DownloadFailureReason.DiskFull;
                }
                if (message.Contains("mismatch", StringComparison.OrdinalIgnoreCase) || 
                    message.Contains("size", StringComparison.OrdinalIgnoreCase))
                {
                    return DownloadFailureReason.FileSizeMismatch;
                }
                if (message.Contains("empty", StringComparison.OrdinalIgnoreCase))
                {
                    return DownloadFailureReason.EmptyFile;
                }
            }
            if (ex is InvalidOperationException)
            {
                if (message.Contains("Disallowed", StringComparison.OrdinalIgnoreCase) || 
                    message.Contains("Insecure", StringComparison.OrdinalIgnoreCase))
                {
                    return DownloadFailureReason.DisallowedRedirect;
                }
                return DownloadFailureReason.InvalidResponse;
            }

            return DownloadFailureReason.UnknownFailure;
        }
    }
}
