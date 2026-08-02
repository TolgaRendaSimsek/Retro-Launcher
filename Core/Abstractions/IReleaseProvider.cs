using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Core.Abstractions
{
    public enum ReleaseProviderType
    {
        GitHub,
        GitLab,
        Codeberg,
        DirectDownload
    }

    public enum ReleaseChannel
    {
        Stable,
        Preview,
        Rolling
    }

    public struct ReleaseQuery
    {
        public string Owner { get; set; }
        public string Repository { get; set; }
        public ReleaseChannel Channel { get; set; }
        public string? Tag { get; set; }
    }

    public class ReleaseAssetInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long Size { get; set; }
        public string ContentType { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Sha256 { get; set; }
    }

    public class ReleaseInfo
    {
        public ReleaseProviderType Provider { get; set; }
        public string RepositoryIdentifier { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsDraft { get; set; }
        public bool IsPrerelease { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string WebUrl { get; set; } = "";
        public List<ReleaseAssetInfo> Assets { get; set; } = new();
    }

    public interface IReleaseProvider
    {
        Task<OperationResult<ReleaseInfo>> GetLatestReleaseAsync(ReleaseQuery query, CancellationToken cancellationToken);
        Task<OperationResult<IReadOnlyList<ReleaseInfo>>> GetReleasesAsync(ReleaseQuery query, CancellationToken cancellationToken);
        Task<OperationResult<ReleaseInfo>> GetReleaseByTagAsync(ReleaseQuery query, CancellationToken cancellationToken);
        Task<OperationResult<bool>> GetProviderStatusAsync(CancellationToken cancellationToken);
    }
}
