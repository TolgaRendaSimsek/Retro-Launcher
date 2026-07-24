using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public enum ExtractionFailureReason
    {
        None,
        FileNotFound,
        InvalidArchive,
        PathTraversalAttempt, // Zip Slip
        LimitExceededFileCount,
        LimitExceededTotalSize,
        LimitExceededSingleFileSize,
        LimitExceededCompressionRatio,
        UnsafeSymbolicLink,
        StagingCleanupFailed,
        ExtractionException,
        NoExecutableFound,
        Cancellation
    }

    public class ArchiveExtractionProgress
    {
        public int FilesExtracted { get; set; }
        public int TotalFiles { get; set; }
        public long BytesExtracted { get; set; }
        public long TotalBytes { get; set; }
        public int Percentage { get; set; }
        public string CurrentFileName { get; set; } = "";
    }

    public class ArchiveExtractionRequest
    {
        public string ArchivePath { get; set; } = "";
        public string DestinationPath { get; set; } = ""; // final target path
        public CancellationToken CancellationToken { get; set; }
        public IProgress<ArchiveExtractionProgress>? Progress { get; set; }

        // Security limits
        public int MaxFileCount { get; set; } = 10000;
        public long MaxTotalSize { get; set; } = 1024 * 1024 * 1024; // 1 GB default
        public long MaxSingleFileSize { get; set; } = 500 * 1024 * 1024; // 500 MB default
        public double MaxCompressionRatio { get; set; } = 100.0; // 100x default

        public bool PreserveStagingForDiagnostics { get; set; } = false;
        public System.Collections.Generic.List<string> ExecutableCandidates { get; set; } = new();
    }

    public class ArchiveExtractionResult
    {
        public bool Success { get; set; }
        public string? ExtractedRootPath { get; set; } // The staging sub-folder if single nested, or staging root itself
        public string? MainExecutablePath { get; set; } // Path to executable relative to the staging folder or absolute
        public ExtractionFailureReason FailureReason { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IArchiveExtractor
    {
        bool CanExtract(string archiveType);
        Task<ArchiveExtractionResult> ExtractAsync(ArchiveExtractionRequest request);
    }
}
