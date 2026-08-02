using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Core.Abstractions
{
    public interface IGitHubReleaseService
    {
        void ConfigureToken(string? token);
        Task<GitHubApiResult<GitHubRelease>> GetLatestReleaseAsync(string owner, string repo, CancellationToken token);
        Task<GitHubApiResult<IReadOnlyList<GitHubRelease>>> GetReleasesAsync(string owner, string repo, CancellationToken token);
        Task<GitHubApiResult<GitHubRelease>> GetReleaseByTagAsync(string owner, string repo, string tag, CancellationToken token);
    }

    public static class GitHubAssetFilters
    {
        /// <summary>
        /// Finds the first release asset that matches the wildcard pattern (e.g. "pcsx2-v1.7.*-windows-x64-Qt.7z").
        /// Case-insensitive.
        /// </summary>
        public static GitHubReleaseAsset? FindMatchingAsset(IEnumerable<GitHubReleaseAsset> assets, string pattern)
        {
            if (assets == null || string.IsNullOrEmpty(pattern)) return null;

            // Convert glob wildcard to regular expression pattern
            string escapedPattern = Regex.Escape(pattern);
            // Replace wildcards
            string regexPattern = "^" + escapedPattern.Replace("\\*", ".*") + "$";

            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            return assets.FirstOrDefault(asset => regex.IsMatch(asset.Name));
        }
    }
}
