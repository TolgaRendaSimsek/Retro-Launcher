using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class RepositoryMetadata
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string License { get; set; } = "";
        public int Stargazers { get; set; }
        public string Url { get; set; } = "";
    }

    public interface IGitHubGraphQLMetadataProvider
    {
        Task<OperationResult<Dictionary<string, RepositoryMetadata>>> FetchBatchMetadataAsync(
            List<(string Owner, string Name)> repositories,
            CancellationToken cancellationToken);
    }
}
