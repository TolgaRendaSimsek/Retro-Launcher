using System;
using System.IO;
using System.Threading.Tasks;
using RetroLauncher.Services.Logging;
using RetroLauncher.Services.Updates;

namespace RetroLauncher.Tests.Unit
{
    public static class ApplicationUpdateServiceTests
    {
        public static async Task RunTestsAsync()
        {
            RetroLogger.Log("Starting ApplicationUpdateService Unit Tests...");

            TestVersionTagParsing();
            TestAssetSelection();
            TestVersionComparison();
            await TestChecksumVerificationAsync();

            RetroLogger.Log("All ApplicationUpdateService Unit Tests completed successfully!");
        }

        private static void TestVersionTagParsing()
        {
            Assert(GitHubApplicationReleaseClient.TryParseTagVersion("v1.0.1", out Version? v1) && v1 == new Version(1, 0, 1), "v1.0.1 tag parsing failed.");
            Assert(GitHubApplicationReleaseClient.TryParseTagVersion("1.2.3.4", out Version? v2) && v2 == new Version(1, 2, 3, 4), "1.2.3.4 tag parsing failed.");
            Assert(GitHubApplicationReleaseClient.TryParseTagVersion("release-2.0.0", out Version? v3) && v3 == new Version(2, 0, 0), "release-2.0.0 tag parsing failed.");
            Assert(!GitHubApplicationReleaseClient.TryParseTagVersion("invalid_tag", out _), "Invalid tag should return false.");

            RetroLogger.Log("Version tag parsing test passed.");
        }

        private static void TestAssetSelection()
        {
            var release = new GitHubAppReleaseDto
            {
                TagName = "v1.0.1",
                Assets = new System.Collections.Generic.List<GitHubAppReleaseAssetDto>
                {
                    new GitHubAppReleaseAssetDto { Name = "RetroLauncher-linux-x64.tar.gz", BrowserDownloadUrl = "https://example.com/linux.tar.gz" },
                    new GitHubAppReleaseAssetDto { Name = "RetroLauncher-win-x64-v1.0.1.pdb", BrowserDownloadUrl = "https://example.com/win.pdb" },
                    new GitHubAppReleaseAssetDto { Name = "RetroLauncher-win-x64-v1.0.1.zip", Size = 50000000, BrowserDownloadUrl = "https://example.com/RetroLauncher-win-x64-v1.0.1.zip" }
                }
            };

            var selected = GitHubApplicationReleaseClient.SelectCompatibleAsset(release);
            Assert(selected != null && selected.Name == "RetroLauncher-win-x64-v1.0.1.zip", "Compatible Windows x64 asset selection failed.");

            RetroLogger.Log("Compatible asset selection test passed.");
        }

        private static void TestVersionComparison()
        {
            Version current = new Version(1, 0, 0);
            Version newer = new Version(1, 0, 1);
            Version older = new Version(0, 9, 9);

            Assert(newer > current, "1.0.1 must be newer than 1.0.0.");
            Assert(older < current, "0.9.9 must be older than 1.0.0.");
            Assert(current == new Version(1, 0, 0), "1.0.0 must equal 1.0.0.");

            RetroLogger.Log("Version comparison test passed.");
        }

        private static async Task TestChecksumVerificationAsync()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"sha_test_{Guid.NewGuid():N}.tmp");
            try
            {
                await File.WriteAllTextAsync(tempFile, "RetroLauncher Checksum Test Content");
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                using var fs = File.OpenRead(tempFile);
                byte[] hash = await sha256.ComputeHashAsync(fs);
                string expectedHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                var downloader = new ApplicationUpdateDownloader();
                bool valid = await downloader.VerifySha256ChecksumAsync(tempFile, expectedHash);
                Assert(valid, "SHA-256 checksum verification failed.");
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }

            RetroLogger.Log("Checksum verification test passed.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"ApplicationUpdateService test failed: {message}");
            }
        }
    }
}
