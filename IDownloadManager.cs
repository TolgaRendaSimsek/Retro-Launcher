using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public enum DownloadFailureReason
    {
        None,
        NetworkUnavailable,
        DnsFailure,
        Timeout,
        Cancellation,
        HttpError,
        DiskFull,
        PermissionDenied,
        InvalidResponse,
        FileSizeMismatch,
        DisallowedRedirect,
        EmptyFile,
        DuplicateDownload,
        UnknownFailure
    }

    public class DownloadProgress
    {
        public string EmulatorId { get; set; } = "";
        public long BytesDownloaded { get; set; }
        public long? TotalBytes { get; set; }
        public int Percentage { get; set; } // -1 if indeterminate
        public double CurrentSpeedBytesPerSecond { get; set; }
        public double AverageSpeedBytesPerSecond { get; set; }
        public TimeSpan EstimatedRemainingTime { get; set; }
    }

    public class DownloadRequest
    {
        public string EmulatorId { get; set; } = "";
        public string OperationId { get; set; } = "";
        public string Url { get; set; } = "";
        public string DestinationPath { get; set; } = "";
        public long? ExpectedSize { get; set; }
        public IProgress<DownloadProgress>? Progress { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public class DownloadResult
    {
        public bool Success { get; set; }
        public string? DownloadedFilePath { get; set; }
        public DownloadFailureReason FailureReason { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IDownloadManager
    {
        Task<DownloadResult> DownloadAsync(DownloadRequest request);
        void SetMaxConcurrentDownloads(int max);
    }
}
