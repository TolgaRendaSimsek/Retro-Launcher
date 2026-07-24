using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public static class ArchiveExtractorTests
    {
        public static async Task RunTestsAsync()
        {
            RetroLogger.Log("Starting SecureArchiveExtractor Unit Tests...");

            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "test_archives");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            var extractor = new SecureArchiveExtractor();

            // Setup temporary file paths
            string traversalZip = Path.Combine(tempDir, "traversal.zip");
            string nestedZip = Path.Combine(tempDir, "nested.zip");
            string normalZip = Path.Combine(tempDir, "normal.zip");

            try
            {
                // Create Mock Zip for Traversal Check
                CreateZipWithEntry(traversalZip, "../outside_path.txt", "traversal content");

                // Create Mock Zip for Nested Root Check
                CreateZipWithEntry(nestedZip, "nested_folder/duckstation.exe", "emulator binary");

                // Create Mock Zip for normal check
                CreateZipWithEntry(normalZip, "duckstation.exe", "emulator binary");

                // Test 1: Path Traversal Attack Mitigation
                var req1 = new ArchiveExtractionRequest
                {
                    ArchivePath = traversalZip,
                    DestinationPath = Path.Combine(tempDir, "dest_traversal"),
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "outside_path.txt" }
                };

                var res1 = await extractor.ExtractAsync(req1);
                Debug.Assert(res1.Success == false, "Extraction of a traversal archive should fail.");
                Debug.Assert(res1.FailureReason == ExtractionFailureReason.PathTraversalAttempt, "Failure reason should be PathTraversalAttempt.");
                RetroLogger.Log("Test Case 1 passed: Path traversal (Zip Slip) correctly detected and blocked.");

                // Test 2: Oversized File / Limit Exceeded Check
                var req2 = new ArchiveExtractionRequest
                {
                    ArchivePath = normalZip,
                    DestinationPath = Path.Combine(tempDir, "dest_limits"),
                    CancellationToken = CancellationToken.None,
                    MaxSingleFileSize = 5, // Set very low limit to trigger constraint
                    ExecutableCandidates = new List<string> { "duckstation.exe" }
                };

                var res2 = await extractor.ExtractAsync(req2);
                Debug.Assert(res2.Success == false, "Extraction should fail when size limits are exceeded.");
                Debug.Assert(res2.FailureReason == ExtractionFailureReason.LimitExceededSingleFileSize, "Failure reason should be LimitExceededSingleFileSize.");
                RetroLogger.Log("Test Case 2 passed: Archive size limits correctly enforced.");

                // Test 3: Nested Root Folder Normalization
                var req3 = new ArchiveExtractionRequest
                {
                    ArchivePath = nestedZip,
                    DestinationPath = Path.Combine(tempDir, "dest_nested"),
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "duckstation.exe" }
                };

                var res3 = await extractor.ExtractAsync(req3);
                Debug.Assert(res3.Success == true, "Nested extraction should succeed.");
                Debug.Assert(res3.MainExecutablePath != null && res3.MainExecutablePath.EndsWith("duckstation.exe"), "MainExecutablePath should target duckstation.exe.");
                RetroLogger.Log("Test Case 3 passed: Nested archive root folders correctly normalized.");

                RetroLogger.Log("All SecureArchiveExtractor Unit Tests completed successfully!");
            }
            finally
            {
                // Cleanup temp files
                try
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        private static void CreateZipWithEntry(string zipPath, string entryPath, string content)
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            using (var fileStream = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry(entryPath);
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write(content);
                }
            }
        }
    }
}
