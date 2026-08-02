using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroLauncher.Tests.Unit
{
    public static class ReleaseAssetSelectorTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting ReleaseAssetSelector Unit Tests...");

            var provider = new JsonEmulatorPackageDefinitionProvider();
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
                var originalRules = duckDef.IncludeAssetPatterns;
                duckDef.IncludeAssetPatterns = new List<string> { "duckstation-windows-x64-*.zip" };

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
                                new GitHubReleaseAsset { Name = "duckstation-windows-x64-release-v2.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release-v2.zip", Size = 15000000 }
                            }
                        }
                    };

                    var ambResult = selector.SelectAsset(duckDef, ambiguousReleases);
                    Debug.Assert(ambResult.Status == SelectionStatus.AmbiguousPackages, "Selection should fail due to ambiguity.");
                    RetroLogger.Log("Test Case 4 passed: Ambiguous assets correctly rejected.");
                }
                finally
                {
                    duckDef.IncludeAssetPatterns = originalRules;
                }
            }

            // Test Case 5: Release containing source archives
            if (duckDef != null)
            {
                var sourceReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v0.1.3000",
                        IsDraft = false,
                        IsPrerelease = false,
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "duckstation-src.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-src.zip", Size = 5000000 },
                            new GitHubReleaseAsset { Name = "source.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/source.zip", Size = 5000000 },
                            new GitHubReleaseAsset { Name = "duckstation-windows-x64-release.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release.zip", Size = 15000000 }
                        }
                    }
                };

                var srcResult = selector.SelectAsset(duckDef, sourceReleases);
                Debug.Assert(srcResult.Status == SelectionStatus.Success, "Source selection should succeed because a valid Windows package exists.");
                Debug.Assert(srcResult.SelectedAsset?.Name == "duckstation-windows-x64-release.zip", "Should select the release ZIP and skip source archives.");
                RetroLogger.Log("Test Case 5 passed: Source archives correctly ignored and valid package selected.");
            }

            // Test Case 6: Release containing ARM and x64 files
            if (duckDef != null)
            {
                var armReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v0.1.3000",
                        IsDraft = false,
                        IsPrerelease = false,
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "duckstation-windows-arm64-release.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-arm64-release.zip", Size = 15000000 },
                            new GitHubReleaseAsset { Name = "duckstation-windows-x64-release.zip", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-windows-x64-release.zip", Size = 15000000 }
                        }
                    }
                };

                var armResult = selector.SelectAsset(duckDef, armReleases);
                Debug.Assert(armResult.Status == SelectionStatus.Success, "ARM vs x64 selection should succeed.");
                Debug.Assert(armResult.SelectedAsset?.Name == "duckstation-windows-x64-release.zip", "Should select x64 over ARM.");
                RetroLogger.Log("Test Case 6 passed: x64 asset selected successfully over ARM.");
            }

            // Test Case 7: Release without supported Windows assets
            if (duckDef != null)
            {
                var noWinReleases = new List<GitHubRelease>
                {
                    new GitHubRelease
                    {
                        TagName = "v0.1.3000",
                        IsDraft = false,
                        IsPrerelease = false,
                        Assets = new List<GitHubReleaseAsset>
                        {
                            new GitHubReleaseAsset { Name = "duckstation-linux-x64.tar.gz", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-linux-x64.tar.gz", Size = 15000000 },
                            new GitHubReleaseAsset { Name = "duckstation-macos.dmg", BrowserDownloadUrl = "https://github.com/stenzek/duckstation/releases/download/v0.1.3000/duckstation-macos.dmg", Size = 15000000 }
                        }
                    }
                };

                var noWinResult = selector.SelectAsset(duckDef, noWinReleases);
                Debug.Assert(noWinResult.Status == SelectionStatus.NoCompatiblePackage, "Selection should fail since no Windows assets are available.");
                RetroLogger.Log("Test Case 7 passed: No supported Windows assets detected successfully.");
            }

            RetroLogger.Log("All ReleaseAssetSelector Unit Tests completed successfully!");
        }
    }
}
