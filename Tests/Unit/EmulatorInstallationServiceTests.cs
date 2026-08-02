using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Tests.Unit
{
    public static class EmulatorInstallationServiceTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting EmulatorInstallationService Unit Tests...");

            TestRollbackOnFinalizationFailureAsync().GetAwaiter().GetResult();

            RetroLogger.Log("All EmulatorInstallationService Unit Tests completed successfully!");
        }

        private static async Task TestRollbackOnFinalizationFailureAsync()
        {
            RetroLogger.Log("Test Case: Rollback on Finalization (Registry Update) Failure.");

            string tempDir = Path.Combine(Path.GetTempPath(), $"emu_install_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // 1. Create a dummy emulator installation (existing files & user config/saves)
                string installDir = Path.Combine(tempDir, "Emulators", "TestEmu");
                Directory.CreateDirectory(installDir);

                string oldExePath = Path.Combine(installDir, "emulator.exe");
                File.WriteAllText(oldExePath, "old_version_binary");

                // Create user configuration and save folder
                string biosDir = Path.Combine(installDir, "bios");
                Directory.CreateDirectory(biosDir);
                File.WriteAllText(Path.Combine(biosDir, "scph1001.bin"), "bios_content");

                string saveDir = Path.Combine(installDir, "savestates");
                Directory.CreateDirectory(saveDir);
                File.WriteAllText(Path.Combine(saveDir, "state1.state"), "save_state_content");

                string portableFile = Path.Combine(installDir, "portable.txt");
                File.WriteAllText(portableFile, "portable_mode_enabled");

                // Create non-preserved file
                string oldExtraFile = Path.Combine(installDir, "extra_old_file.txt");
                File.WriteAllText(oldExtraFile, "extra");

                // 2. Create the update ZIP package (representing new version download)
                string downloadDir = Path.Combine(tempDir, "downloads");
                Directory.CreateDirectory(downloadDir);
                string zipPath = Path.Combine(downloadDir, "update.zip");

                using (var fs = new FileStream(zipPath, FileMode.Create))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    var exeEntry = archive.CreateEntry("emulator.exe");
                    using (var writer = new StreamWriter(exeEntry.Open()))
                    {
                        writer.Write("new_version_binary");
                    }

                    var newFileEntry = archive.CreateEntry("new_file.txt");
                    using (var writer = new StreamWriter(newFileEntry.Open()))
                    {
                        writer.Write("some_new_feature_data");
                    }
                }

                // 3. Define the package details
                var definition = new EmulatorPackageDefinition
                {
                    Id = "testemu",
                    DisplayName = "Test Emulator",
                    ConsoleName = "Test Console",
                    GitHubOwner = "testowner",
                    GitHubRepository = "testrepo",
                    InstallDirectoryName = Path.GetRelativePath(AppContext.BaseDirectory, installDir).Replace('\\', '/'),
                    ExecutableCandidates = new List<string> { "emulator.exe" },
                    IncludeAssetPatterns = new List<string> { "update.zip" },
                    SupportedArchiveTypes = new List<string> { "zip" },
                    PreservedDirectories = new List<string> { "bios", "savestates" },
                    PreservedFiles = new List<string> { "portable.txt" }
                };

                // 4. Set up mock services
                var defProvider = new MockDefinitionProvider(definition);
                var releaseProvider = new MockReleaseProvider(zipPath);
                var downloadManager = new MockDownloadManager(zipPath);
                var verifier = new MockVerifier();

                var service = new EmulatorInstallationService(
                    defProvider,
                    releaseProvider,
                    new ReleaseAssetSelector(),
                    downloadManager,
                    new SecureArchiveExtractor(),
                    verifier
                );

                // 5. Trigger update.
                // Since "testemu" is NOT in EmulatorManager.Instance.Config.Emulators (registry), 
                // the registration step (UpdateLauncherRegistry) will return false, triggering rollback!
                var req = new EmulatorInstallationRequest
                {
                    EmulatorId = "testemu",
                    Operation = EmulatorInstallationOperation.Update,
                    CancellationToken = CancellationToken.None
                };

                var result = await service.InstallAsync(req);

                // Assert failure occurred (since registration failed)
                Assert(!result.Success, "Operation should fail due to registration failure.");
                Assert(result.FailedStage == PackageInstallStage.Registering, $"Failed stage should be Registering, but was {result.FailedStage}");

                // Assert rollback worked: old files and user config/saves must be restored and intact
                Assert(Directory.Exists(installDir), "Installation folder must be restored.");
                Assert(File.Exists(oldExePath), "Old executable must be restored.");
                Assert(File.ReadAllText(oldExePath) == "old_version_binary", "Old executable content must match.");
                
                Assert(Directory.Exists(biosDir), "Bios folder must be restored.");
                Assert(File.ReadAllText(Path.Combine(biosDir, "scph1001.bin")) == "bios_content", "Bios file must match.");

                Assert(Directory.Exists(saveDir), "Save folder must be restored.");
                Assert(File.ReadAllText(Path.Combine(saveDir, "state1.state")) == "save_state_content", "Save file must match.");

                Assert(File.Exists(portableFile), "Portable mode file must be restored.");

                // Check that update files (new_file.txt) were rolled back and do not exist in install folder
                Assert(!File.Exists(Path.Combine(installDir, "new_file.txt")), "New file must not exist in final folder after rollback.");

                RetroLogger.Log("Rollback test case passed successfully!");
            }
            finally
            {
                // Cleanup temp test directory
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Test assertion failed: {message}");
            }
            RetroLogger.Log($"Test Case passed: {message}");
        }

        // Mock Classes for Testing
        private class MockDefinitionProvider : IEmulatorPackageDefinitionProvider
        {
            private readonly EmulatorPackageDefinition _def;

            public MockDefinitionProvider(EmulatorPackageDefinition def)
            {
                _def = def;
            }

            public IReadOnlyList<EmulatorPackageDefinition> GetAll() => new List<EmulatorPackageDefinition> { _def }.AsReadOnly();
            public EmulatorPackageDefinition? GetById(string id) => string.Equals(id, _def.Id, StringComparison.OrdinalIgnoreCase) ? _def : null;
            public IReadOnlyList<EmulatorPackageDefinition> GetByConsole(string consoleName) => new List<EmulatorPackageDefinition>().AsReadOnly();
            public void Validate(EmulatorPackageDefinition definition) { }
        }

        private class MockReleaseProvider : IReleaseProvider
        {
            private readonly string _zipPath;

            public MockReleaseProvider(string zipPath)
            {
                _zipPath = zipPath;
            }

            public Task<OperationResult<ReleaseInfo>> GetLatestReleaseAsync(ReleaseQuery query, CancellationToken cancellationToken)
            {
                var release = new ReleaseInfo
                {
                    Tag = "v1.2.3",
                    Name = "Release 1.2.3",
                    PublishedAt = DateTime.UtcNow,
                    Assets = new List<ReleaseAssetInfo>
                    {
                        new ReleaseAssetInfo
                        {
                            Name = "update.zip",
                            DownloadUrl = "https://github.com/testowner/testrepo/releases/download/v1.2.3/update.zip",
                            Size = new FileInfo(_zipPath).Length
                        }
                    }
                };
                return Task.FromResult(OperationResult<ReleaseInfo>.Ok(release));
            }

            public Task<OperationResult<IReadOnlyList<ReleaseInfo>>> GetReleasesAsync(ReleaseQuery query, CancellationToken cancellationToken)
            {
                IReadOnlyList<ReleaseInfo> list = new List<ReleaseInfo>();
                return Task.FromResult(OperationResult<IReadOnlyList<ReleaseInfo>>.Ok(list));
            }

            public Task<OperationResult<ReleaseInfo>> GetReleaseByTagAsync(ReleaseQuery query, CancellationToken cancellationToken)
            {
                var release = new ReleaseInfo
                {
                    Tag = query.Tag ?? "v1.2.3",
                    Name = $"Release {query.Tag}",
                    PublishedAt = DateTime.UtcNow,
                    Assets = new List<ReleaseAssetInfo>
                    {
                        new ReleaseAssetInfo
                        {
                            Name = "update.zip",
                            DownloadUrl = "https://github.com/testowner/testrepo/releases/download/v1.2.3/update.zip",
                            Size = new FileInfo(_zipPath).Length
                        }
                    }
                };
                return Task.FromResult(OperationResult<ReleaseInfo>.Ok(release));
            }

            public Task<OperationResult<bool>> GetProviderStatusAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(OperationResult<bool>.Ok(true));
            }
        }

        private class MockDownloadManager : IDownloadManager
        {
            private readonly string _zipPath;

            public MockDownloadManager(string zipPath)
            {
                _zipPath = zipPath;
            }

            public Task<DownloadResult> DownloadAsync(DownloadRequest request)
            {
                try
                {
                    string dir = Path.GetDirectoryName(request.DestinationPath)!;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    File.Copy(_zipPath, request.DestinationPath, true);
                    return Task.FromResult(new DownloadResult
                    {
                        Success = true,
                        DownloadedFilePath = request.DestinationPath,
                        StatusCode = HttpStatusCode.OK
                    });
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new DownloadResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        FailureReason = DownloadFailureReason.UnknownFailure
                    });
                }
            }

            public void SetMaxConcurrentDownloads(int max) { }
        }

        private class MockVerifier : IEmuPackageVerifier
        {
            public Task<VerificationResult> VerifyPackageAsync(string packagePath, long expectedSize, string? expectedHash, CancellationToken cancellationToken)
            {
                return Task.FromResult(new VerificationResult
                {
                    Success = true,
                    Status = VerificationStatus.SizeVerified,
                    CalculatedHash = "mock_hash"
                });
            }
        }
    }
}
