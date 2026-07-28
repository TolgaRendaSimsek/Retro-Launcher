using System;
using System.Collections.Generic;

namespace RetroLauncher
{
    public enum AssetRejectionReason
    {
        DraftRelease,
        PrereleaseExcluded,
        NotWindowsPlatform,
        IncompatibleArchitecture,
        NonWindowsAsset,
        ExcludedFileType, // source code, symbols, apk, dmg, appimage, checksums, etc.
        PatternMismatch,
        InsecureUrl,
        DisallowedDownloadDomain,
        DuplicateHighScores,
        ReleaseExcluded
    }

    public class AssetRejectionDetail
    {
        public string AssetName { get; set; } = "";
        public AssetRejectionReason Reason { get; set; }
        public string Explanation { get; set; } = "";
    }

    public enum SelectionStatus
    {
        Success,
        NoCompatiblePackage,
        UnsupportedArchitecture,
        AmbiguousPackages,
        ReleaseHasNoAssets,
        OnlySourceCodeFound,
        DownloadDomainNotAllowed,
        InsecureUrl
    }

    public class AssetSelectionResult
    {
        public SelectionStatus Status { get; set; }
        public GitHubReleaseAsset? SelectedAsset { get; set; }
        public string? SelectedReleaseTag { get; set; }
        public string? UserMessage { get; set; }
        public List<AssetRejectionDetail> Rejections { get; set; } = new();
    }

    public interface IReleaseAssetSelector
    {
        AssetSelectionResult SelectAsset(EmulatorPackageDefinition definition, IEnumerable<GitHubRelease> releases);
    }
}
