using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroLauncher
{
    public static class ReleaseAssetSelectorTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting ReleaseAssetSelector Unit Tests...");

            var provider = new JsonEmulatorDefinitionProvider();
            var selector = new ReleaseAssetSelector();

            // Test Case 1: DuckStation
            var duckDef = provider.GetById("duckstation");
            if (duckDef != null)
            {
                var duckReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v0.1.3000",
                        IsDraft = false,
                        IsPrerelease = false,
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "duckstation-windows-x64-release.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release.zip", Size = 15000000 },
                            new GitHubReleaseAsset { Name = "duckstation-windows-x64-debug.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-debug.zip", Size = 20000000 },
                            new GitHubReleaseAsset { Name = "duckstation-android.apk", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-android.apk", Size = 12000000 }
                        }
                    }
                };

                var duckResult = selector.SelectAsset(duckDef, duckReleases);
                Debug.Assert(duckResult.Status == SelectionStatus.Success, "DuckStation selection should succeed.");
                Debug.Assert(duckResult.SelectedAsset?.Name == "duckstation-windows-x64-release.zip", "DuckStation should select the release ZIP.");
                RetroLogger.Log("Test Case 1 passed: DuckStation asset selected successfully.");
            }

            // Test Case 2: PCSX2
            var pcsxDef = provider.GetById("pcsx2");
            if (pcsxDef != null)
            {
                var pcsxReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v1.7.5348",
                        IsDraft = false,
                        IsPrerelease = true, // PCSX2 dev channel permits prereleases
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "pcsx2-v1.7.5348-windows-x64-Qt.7z", BrowserDownloadUrl = "https://github.com/PCSX2/pcsx2/releases/download/v1.7.5348/pcsx2-v1.7.5348-windows-x64-Qt.7z", Size = 25000000 },
                            new GitHubReleaseAsset { Name = "pcsx2-v1.7.5348-windows-x64-Qt-symbols.7z", BrowserDownloadUrl = "https://github.com/PCSX2/pcsx2/releases/download/v1.7.5348/pcsx2-v1.7.5348-windows-x64-Qt-symbols.7z", Size = 80000000 },
                            new GitHubReleaseAsset { Name = "pcsx2-v1.7.5348-linux-x64-AppImage.tar.xz", BrowserDownloadUrl = "https://github.com/PCSX2/pcsx2/releases/download/v1.7.5348/pcsx2-v1.7.5348-linux-x64-AppImage.tar.xz", Size = 30000000 }
                        }
                    }
                };

                var pcsxResult = selector.SelectAsset(pcsxDef, pcsxReleases);
                Debug.Assert(pcsxResult.Status == SelectionStatus.Success, "PCSX2 selection should succeed.");
                Debug.Assert(pcsxResult.SelectedAsset?.Name == "pcsx2-v1.7.5348-windows-x64-Qt.7z", "PCSX2 should select the Windows Qt 7z.");
                RetroLogger.Log("Test Case 2 passed: PCSX2 asset selected successfully.");
            }

            // Test Case 3: RPCS3
            var rpcsDef = provider.GetById("rpcs3");
            if (rpcsDef != null)
            {
                var rpcsReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v0.0.30-16016",
                        IsDraft = false,
                        IsPrerelease = false,
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "rpcs3-v0.0.30-16016-5645367d_win64.7z", BrowserDownloadUrl = "https://github.com/RPCS3/rpcs3-binaries-win/releases/download/v0.0.30-16016/rpcs3-v0.0.30-16016-5645367d_win64.7z", Size = 35000000 },
                            new GitHubReleaseAsset { Name = "rpcs3-v0.0.30-16016-5645367d_linux64.AppImage", BrowserDownloadUrl = "https://github.com/RPCS3/rpcs3-binaries-win/releases/download/v0.0.30-16016/rpcs3-v0.0.30-16016-5645367d_linux64.AppImage", Size = 40000000 }
                        }
                    }
                };

                var rpcsResult = selector.SelectAsset(rpcsDef, rpcsReleases);
                Debug.Assert(rpcsResult.Status == SelectionStatus.Success, "RPCS3 selection should succeed.");
                Debug.Assert(rpcsResult.SelectedAsset?.Name == "rpcs3-v0.0.30-16016-5645367d_win64.7z", "RPCS3 should select the Windows 7z binary.");
                RetroLogger.Log("Test Case 3 passed: RPCS3 asset selected successfully.");
            }

            // Test Case 4: Ambiguous Match Rejector
            if (duckDef != null)
            {
                var originalRules = duckDef.AssetSelectionRules;
                duckDef.AssetSelectionRules = new List<string> { "duckstation-windows-x64-*.zip" };

                try
                {
                    var ambiguousReleases = new List<GitHubRelease>
                    {
                        new GitHubRelease
                        {
                            TagName = "v0.1.3000",
                            IsDraft = false,
                            IsPrerelease = false,
                            Assets = new List<GitHubReleaseAsset>
                            {
                                new GitHubReleaseAsset { Name = "duckstation-windows-x64-release.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release.zip", Size = 15000000 },
                                new GitHubReleaseAsset { Name = "duckstation-windows-x64-release-qt.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release-qt.zip", Size = 15000000 }
                            }
                        }
                    };

                    var ambResult = selector.SelectAsset(duckDef, ambiguousReleases);
                    Debug.Assert(ambResult.Status == SelectionStatus.AmbiguousPackages, "Selection should fail due to ambiguity.");
                    RetroLogger.Log("Test Case 4 passed: Ambiguous assets correctly rejected.");
                }
                finally
                {
                    duckDef.AssetSelectionRules = originalRules;
                }
            }

            RetroLogger.Log("All ReleaseAssetSelector Unit Tests completed successfully!");
        }
    }
}
