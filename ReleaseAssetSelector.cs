using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RetroLauncher
{
    public enum SelectionConfidence
    {
        High,
        Medium,
        Low,
        Ambiguous,
        None
    }

    public class CandidateScore
    {
        public string Name { get; set; } = "";
        public int Score { get; set; }
        public string Explanation { get; set; } = "";
    }

    public class CandidateRejection
    {
        public string Name { get; set; } = "";
        public string Reason { get; set; } = "";
    }

    public class ReleaseSelectionResult
    {
        public bool Success { get; set; }
        public ReleaseInfo? SelectedRelease { get; set; }
        public SelectionConfidence Confidence { get; set; } = SelectionConfidence.None;
        public string Message { get; set; } = "";
    }

    public class ReleaseAssetSelectionResult
    {
        public bool Success { get; set; }
        public ReleaseAssetInfo? SelectedAsset { get; set; }
        public SelectionConfidence Confidence { get; set; } = SelectionConfidence.None;
        public string Message { get; set; } = "";
        public List<CandidateScore> Scores { get; set; } = new();
        public List<CandidateRejection> Rejections { get; set; } = new();
    }

    public interface IReleaseSelector
    {
        ReleaseSelectionResult SelectRelease(EmulatorDefinition definition, IEnumerable<ReleaseInfo> releases);
    }

    public interface IReleaseAssetSelectorNew
    {
        ReleaseAssetSelectionResult SelectAsset(EmulatorDefinition definition, ReleaseInfo release);
    }

    public class ReleaseSelector : IReleaseSelector
    {
        public ReleaseSelectionResult SelectRelease(EmulatorDefinition definition, IEnumerable<ReleaseInfo> releases)
        {
            var result = new ReleaseSelectionResult();

            if (releases == null || !releases.Any())
            {
                result.Success = false;
                result.Message = "No releases available to select.";
                return result;
            }

            var candidates = new List<ReleaseInfo>();
            foreach (var release in releases)
            {
                if (release.IsDraft) continue;

                // Channel filtering
                if (definition.ReleaseChannel == EmulatorReleaseChannel.Stable && release.IsPrerelease)
                {
                    continue;
                }

                candidates.Add(release);
            }

            if (!candidates.Any())
            {
                result.Success = false;
                result.Message = "No compatible release found for the configured channel.";
                return result;
            }

            var chosen = candidates.OrderByDescending(r => r.PublishedAt ?? DateTime.MinValue).First();

            result.Success = true;
            result.SelectedRelease = chosen;
            result.Confidence = SelectionConfidence.High;
            result.Message = $"Selected release '{chosen.Tag}' published at '{chosen.PublishedAt}'.";
            return result;
        }
    }

    public class ReleaseAssetSelector : IReleaseAssetSelector, IReleaseAssetSelectorNew
    {
        public ReleaseAssetSelectionResult SelectAsset(EmulatorDefinition definition, ReleaseInfo release)
        {
            var result = new ReleaseAssetSelectionResult();

            if (release == null || release.Assets == null || !release.Assets.Any())
            {
                result.Success = false;
                result.Message = "The release does not contain any assets.";
                return result;
            }

            var candidates = new List<(ReleaseAssetInfo Asset, int Score)>();

            // Terms that must be rejected
            var rejectTerms = new[]
            {
                "source", "src", "symbols", "pdb", "debug", "checksum", "sha256", 
                "signature", "asc", "arm", "arm64", "linux", "macos", "osx", 
                "flatpak", "appimage", "qt6symbols"
            };

            // Terms that are preferred
            var preferTerms = new[]
            {
                "windows", "win", "x64", "amd64", "x86_64", "portable", "qt"
            };

            foreach (var asset in release.Assets)
            {
                string nameLower = asset.Name.ToLower();

                // 1. Reject terms check
                bool rejected = false;
                foreach (var term in rejectTerms)
                {
                    if (nameLower.Contains(term))
                    {
                        result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = $"contains excluded term '{term}'" });
                        rejected = true;
                        break;
                    }
                }
                if (rejected) continue;

                // 2. Archive format check (ZIP and 7Z only)
                string ext = Path.GetExtension(asset.Name).ToLower();
                if (ext != ".zip" && ext != ".7z")
                {
                    result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = "unsupported archive format (only ZIP/7Z allowed)" });
                    continue;
                }

                // 3. browser_download_url validation
                if (string.IsNullOrWhiteSpace(asset.DownloadUrl))
                {
                    result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = "empty download URL" });
                    continue;
                }

                if (!Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var uri))
                {
                    result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = "invalid absolute download URL" });
                    continue;
                }

                if (uri.Scheme != Uri.UriSchemeHttps)
                {
                    result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = "insecure URL protocol (HTTPS required)" });
                    continue;
                }

                // GitHub or GitHub release asset host
                string host = uri.Host;
                bool isGitHubHost = host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                                    host.EndsWith(".github.com", StringComparison.OrdinalIgnoreCase) ||
                                    host.Equals("githubusercontent.com", StringComparison.OrdinalIgnoreCase) ||
                                    host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase);

                if (!isGitHubHost)
                {
                    result.Rejections.Add(new CandidateRejection { Name = asset.Name, Reason = "disallowed download host domain (GitHub only)" });
                    continue;
                }

                // 4. Calculate Score
                int score = 0;

                // Match definition inclusion patterns (glob match)
                bool matchesRule = false;
                if (definition.AssetSelectionRules != null && definition.AssetSelectionRules.Any())
                {
                    foreach (var rule in definition.AssetSelectionRules)
                    {
                        if (MatchesPattern(asset.Name, rule))
                        {
                            matchesRule = true;
                            break;
                        }
                    }
                }

                if (matchesRule)
                {
                    score += 1000;
                }

                // Preference scoring
                foreach (var term in preferTerms)
                {
                    if (nameLower.Contains(term))
                    {
                        score += 20;
                    }
                }

                // Small tie-breaker preference for ZIP over 7Z if scores are equal
                if (ext == ".zip")
                {
                    score += 5;
                }

                result.Scores.Add(new CandidateScore { Name = asset.Name, Score = score, Explanation = $"Base score calculated. Matches rules: {matchesRule}." });
                candidates.Add((asset, score));
            }

            if (!candidates.Any())
            {
                result.Success = false;
                result.Message = "No compatible Windows package was found after filtering assets.";
                return result;
            }

            int maxScore = candidates.Max(c => c.Score);
            var bestCandidates = candidates.Where(c => c.Score == maxScore).ToList();

            if (bestCandidates.Count > 1)
            {
                result.Success = false;
                result.Confidence = SelectionConfidence.Ambiguous;
                result.Message = $"Found {bestCandidates.Count} ambiguous packages with matching high scores.";
                return result;
            }

            var chosen = bestCandidates.First().Asset;
            result.Success = true;
            result.SelectedAsset = chosen;
            result.Confidence = SelectionConfidence.High;
            result.Message = $"Selected asset '{chosen.Name}' with score {maxScore}.";
            return result;
        }

        public AssetSelectionResult SelectAsset(EmulatorDefinition definition, IEnumerable<GitHubRelease> releases)
        {
            var releaseList = releases.Select(ConvertToReleaseInfo).ToList();
            var releaseSelector = new ReleaseSelector();
            var releaseSel = releaseSelector.SelectRelease(definition, releaseList);

            if (!releaseSel.Success || releaseSel.SelectedRelease == null)
            {
                return new AssetSelectionResult
                {
                    Status = SelectionStatus.NoCompatiblePackage,
                    UserMessage = releaseSel.Message
                };
            }

            var assetSel = SelectAsset(definition, releaseSel.SelectedRelease);
            var oldResult = new AssetSelectionResult
            {
                Status = assetSel.Success ? SelectionStatus.Success : SelectionStatus.NoCompatiblePackage,
                SelectedReleaseTag = releaseSel.SelectedRelease.Tag,
                UserMessage = assetSel.Message
            };

            if (assetSel.SelectedAsset != null)
            {
                oldResult.SelectedAsset = new GitHubReleaseAsset
                {
                    Name = assetSel.SelectedAsset.Name,
                    BrowserDownloadUrl = assetSel.SelectedAsset.DownloadUrl,
                    Size = assetSel.SelectedAsset.Size,
                    ContentType = assetSel.SelectedAsset.ContentType
                };
            }

            if (assetSel.Confidence == SelectionConfidence.Ambiguous)
            {
                oldResult.Status = SelectionStatus.AmbiguousPackages;
            }

            return oldResult;
        }

        private ReleaseInfo ConvertToReleaseInfo(GitHubRelease gh)
        {
            var info = new ReleaseInfo
            {
                Provider = ReleaseProviderType.GitHub,
                Tag = gh.TagName,
                Name = gh.Name,
                Description = gh.Name,
                IsDraft = gh.IsDraft,
                IsPrerelease = gh.IsPrerelease,
                PublishedAt = gh.PublishedAt,
                WebUrl = gh.HtmlUrl
            };
            if (gh.Assets != null)
            {
                foreach (var asset in gh.Assets)
                {
                    info.Assets.Add(new ReleaseAssetInfo
                    {
                        Id = asset.Name,
                        Name = asset.Name,
                        DownloadUrl = asset.BrowserDownloadUrl,
                        Size = asset.Size,
                        ContentType = asset.ContentType
                    });
                }
            }
            return info;
        }

        private bool MatchesPattern(string filename, string globPattern)
        {
            string regexPattern = "^" + Regex.Escape(globPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(filename, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}
