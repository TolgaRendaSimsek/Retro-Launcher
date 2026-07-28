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
            string validZip = Path.Combine(tempDir, "valid.zip");
            string valid7z = Path.Combine(tempDir, "valid.7z");
            string corruptZip = Path.Combine(tempDir, "corrupt.zip");
            string corrupt7z = Path.Combine(tempDir, "corrupt.7z");
            string emptyFile = Path.Combine(tempDir, "empty.zip");
            string traversalZip = Path.Combine(tempDir, "traversal.zip");
            string nestedZip = Path.Combine(tempDir, "nested.zip");

            try
            {
                // Create Mock Archives
                CreateZipWithEntry(validZip, "emulator.exe", "emulator binary payload");
                Create7zWithEntry(valid7z, "emulator.exe", "emulator binary payload");
                CreateZipWithEntry(traversalZip, "../outside.txt", "traversal file content");
                CreateZipWithEntry(nestedZip, "nested_root/emulator.exe", "nested emulator binary");

                // Create Corrupt Files
                File.WriteAllText(corruptZip, "not a real zip file PK");
                File.WriteAllText(corrupt7z, "not a real 7z file 7z");
                File.WriteAllText(emptyFile, "");

                // Test 1: Valid ZIP Extraction
                string destDir1 = Path.Combine(tempDir, "dest_valid_zip");
                var req1 = new ArchiveExtractionRequest
                {
                    ArchivePath = validZip,
                    DestinationPath = destDir1,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_valid_zip",
                    OperationId = "op1"
                };
                var res1 = await extractor.ExtractAsync(req1);
                Debug.Assert(res1.Success == true, "Valid ZIP extraction should succeed.");
                Debug.Assert(File.Exists(Path.Combine(destDir1, "emulator.exe")), "Extracted emulator.exe should exist.");
                RetroLogger.Log("Test Case 1 passed: Valid ZIP extraction.");

                // Test 2: Valid 7z Extraction
                string destDir2 = Path.Combine(tempDir, "dest_valid_7z");
                var req2 = new ArchiveExtractionRequest
                {
                    ArchivePath = valid7z,
                    DestinationPath = destDir2,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_valid_7z",
                    OperationId = "op2"
                };
                var res2 = await extractor.ExtractAsync(req2);
                Debug.Assert(res2.Success == true, "Valid 7z extraction should succeed.");
                Debug.Assert(File.Exists(Path.Combine(destDir2, "emulator.exe")), "Extracted emulator.exe from 7z should exist.");
                RetroLogger.Log("Test Case 2 passed: Valid 7z extraction.");

                // Test 3: Corrupt ZIP Verification
                string destDir3 = Path.Combine(tempDir, "dest_corrupt_zip");
                var req3 = new ArchiveExtractionRequest
                {
                    ArchivePath = corruptZip,
                    DestinationPath = destDir3,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_corrupt_zip",
                    OperationId = "op3"
                };
                var res3 = await extractor.ExtractAsync(req3);
                Debug.Assert(res3.Success == false, "Corrupt ZIP extraction should fail.");
                Debug.Assert(res3.FailureReason == ExtractionFailureReason.InvalidArchive, "Should fail with InvalidArchive.");
                RetroLogger.Log("Test Case 3 passed: Corrupt ZIP correctly rejected.");

                // Test 4: Corrupt 7z Verification
                string destDir4 = Path.Combine(tempDir, "dest_corrupt_7z");
                var req4 = new ArchiveExtractionRequest
                {
                    ArchivePath = corrupt7z,
                    DestinationPath = destDir4,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_corrupt_7z",
                    OperationId = "op4"
                };
                var res4 = await extractor.ExtractAsync(req4);
                Debug.Assert(res4.Success == false, "Corrupt 7z extraction should fail.");
                Debug.Assert(res4.FailureReason == ExtractionFailureReason.InvalidArchive, "Should fail with InvalidArchive.");
                RetroLogger.Log("Test Case 4 passed: Corrupt 7z correctly rejected.");

                // Test 5: Empty File Verification
                string destDir5 = Path.Combine(tempDir, "dest_empty_file");
                var req5 = new ArchiveExtractionRequest
                {
                    ArchivePath = emptyFile,
                    DestinationPath = destDir5,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_empty_file",
                    OperationId = "op5"
                };
                var res5 = await extractor.ExtractAsync(req5);
                Debug.Assert(res5.Success == false, "Empty archive file extraction should fail.");
                Debug.Assert(res5.FailureReason == ExtractionFailureReason.InvalidArchive, "Should fail with InvalidArchive.");
                RetroLogger.Log("Test Case 5 passed: Empty file correctly rejected.");

                // Test 6: Path Traversal (Zip Slip) Attempt
                string destDir6 = Path.Combine(tempDir, "dest_traversal");
                var req6 = new ArchiveExtractionRequest
                {
                    ArchivePath = traversalZip,
                    DestinationPath = destDir6,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "outside.txt" },
                    PackageId = "test_traversal",
                    OperationId = "op6"
                };
                var res6 = await extractor.ExtractAsync(req6);
                Debug.Assert(res6.Success == false, "Traversal ZIP extraction should fail.");
                Debug.Assert(res6.FailureReason == ExtractionFailureReason.PathTraversalAttempt, "Should fail with PathTraversalAttempt.");
                RetroLogger.Log("Test Case 6 passed: Zip Slip attempt correctly blocked.");

                // Test 7: Nested Root Folder Normalization
                string destDir7 = Path.Combine(tempDir, "dest_nested");
                var req7 = new ArchiveExtractionRequest
                {
                    ArchivePath = nestedZip,
                    DestinationPath = destDir7,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_nested",
                    OperationId = "op7"
                };
                var res7 = await extractor.ExtractAsync(req7);
                Debug.Assert(res7.Success == true, "Nested archive extraction should succeed.");
                Debug.Assert(File.Exists(Path.Combine(destDir7, "emulator.exe")), "Nested directory should be normalized and emulator.exe should exist directly at target.");
                RetroLogger.Log("Test Case 7 passed: Nested top-level directory normalized.");

                // Test 8: Extraction Cancellation
                string destDir8 = Path.Combine(tempDir, "dest_cancel");
                var cts = new CancellationTokenSource();
                cts.Cancel(); // Pre-cancel

                var req8 = new ArchiveExtractionRequest
                {
                    ArchivePath = validZip,
                    DestinationPath = destDir8,
                    CancellationToken = cts.Token,
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    PackageId = "test_cancel",
                    OperationId = "op8"
                };
                var res8 = await extractor.ExtractAsync(req8);
                Debug.Assert(res8.Success == false, "Cancelled extraction should return failure.");
                Debug.Assert(res8.FailureReason == ExtractionFailureReason.Cancellation, "Should fail with Cancellation.");
                RetroLogger.Log("Test Case 8 passed: Cancellation handled successfully.");

                // Test 9: Executable Discovery and Scoring
                string destDir9 = Path.Combine(tempDir, "dest_scoring");
                string scoringZip = Path.Combine(tempDir, "scoring.zip");

                if (File.Exists(scoringZip)) File.Delete(scoringZip);
                using (var fileStream = new FileStream(scoringZip, FileMode.Create))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    var entries = new[]
                    {
                        ("subdir/updater/pcsx2.exe", "payload"),
                        ("pcsx2-crash.exe", "payload"),
                        ("uninstall.exe", "payload"),
                        ("sub/deep/folder/pcsx2.exe", "payload"),
                        ("first/pcsx2-qt.exe", "payload"),
                        ("tie/first/pcsx2-qt.exe", "payload"),
                        ("other/unknown.exe", "payload")
                    };

                    foreach (var entry in entries)
                    {
                        var zipEntry = archive.CreateEntry(entry.Item1);
                        using (var es = zipEntry.Open())
                        using (var writer = new StreamWriter(es))
                        {
                            writer.Write(entry.Item2);
                        }
                    }
                }

                var req9 = new ArchiveExtractionRequest
                {
                    ArchivePath = scoringZip,
                    DestinationPath = destDir9,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "pcsx2-qt.exe", "pcsx2.exe" },
                    PackageId = "test_scoring",
                    OperationId = "op9"
                };

                var res9 = await extractor.ExtractAsync(req9);
                Debug.Assert(res9.Success == true, "Scoring extraction should succeed.");
                string expectedExe = Path.GetFullPath(Path.Combine(destDir9, "first", "pcsx2-qt.exe"));
                Debug.Assert(string.Equals(Path.GetFullPath(res9.MainExecutablePath ?? ""), expectedExe, StringComparison.OrdinalIgnoreCase), $"Expected chosen executable: {expectedExe}, but got: {res9.MainExecutablePath}");
                RetroLogger.Log("Test Case 9 passed: Executable discovery, scoring, and rejection rules validated successfully.");

                // Test 10: Clear Error Message listing discovered files if no match
                string destDir10 = Path.Combine(tempDir, "dest_no_match");
                string noMatchZip = Path.Combine(tempDir, "nomatch.zip");

                if (File.Exists(noMatchZip)) File.Delete(noMatchZip);
                using (var fileStream = new FileStream(noMatchZip, FileMode.Create))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    var zipEntry = archive.CreateEntry("some_folder/dummy.exe");
                    using (var es = zipEntry.Open())
                    using (var writer = new StreamWriter(es))
                    {
                        writer.Write("payload");
                    }
                }

                var req10 = new ArchiveExtractionRequest
                {
                    ArchivePath = noMatchZip,
                    DestinationPath = destDir10,
                    CancellationToken = CancellationToken.None,
                    ExecutableCandidates = new List<string> { "nonexistent.exe" },
                    PackageId = "test_nomatch",
                    OperationId = "op10"
                };

                var res10 = await extractor.ExtractAsync(req10);
                Debug.Assert(res10.Success == false, "Extraction should fail when no candidate matches.");
                Debug.Assert(res10.ErrorMessage != null && res10.ErrorMessage.Contains("Discovered executables:"), "Error message should list discovered executables.");
                Debug.Assert(res10.ErrorMessage.Contains("dummy.exe"), "Error message should list dummy.exe.");
                RetroLogger.Log("Test Case 10 passed: Discovered executables error list reporting.");

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

        private static void Create7zWithEntry(string archivePath, string entryPath, string content)
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            using (var fileStream = new FileStream(archivePath, FileMode.Create))
            using (var writer = new SharpCompress.Writers.SevenZip.SevenZipWriter(fileStream, new SharpCompress.Writers.SevenZip.SevenZipWriterOptions()))
            {
                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
                {
                    writer.Write(entryPath, ms, DateTime.Now);
                }
            }
        }
    }
}
