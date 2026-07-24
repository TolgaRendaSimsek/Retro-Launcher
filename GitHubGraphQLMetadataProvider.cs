using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class GitHubGraphQLMetadataProvider : IGitHubGraphQLMetadataProvider
    {
        private readonly IHttpClientProvider _clientProvider;
        private readonly IApplicationSettingsService _settings;
        private readonly Dictionary<string, RepositoryMetadata> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _cacheLock = new();

        public GitHubGraphQLMetadataProvider(
            IHttpClientProvider? clientProvider = null,
            IApplicationSettingsService? settings = null)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
            _settings = settings ?? ApplicationSettingsService.Instance;
        }

        public async Task<OperationResult<Dictionary<string, RepositoryMetadata>>> FetchBatchMetadataAsync(
            List<(string Owner, string Name)> repositories,
            CancellationToken cancellationToken)
        {
            string? token = _settings.GitHub.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return OperationResult<Dictionary<string, RepositoryMetadata>>.Fail(
                    "GraphQL requires authentication; token is not configured.",
                    ErrorCategory.Unauthorized);
            }

            lock (_cacheLock)
            {
                var cachedResult = new Dictionary<string, RepositoryMetadata>();
                bool allCached = true;
                foreach (var repo in repositories)
                {
                    string key = $"{repo.Owner}/{repo.Name}";
                    if (_cache.TryGetValue(key, out var cachedData))
                    {
                        cachedResult[key] = cachedData;
                    }
                    else
                    {
                        allCached = false;
                    }
                }
                if (allCached)
                {
                    return OperationResult<Dictionary<string, RepositoryMetadata>>.Ok(cachedResult);
                }
            }

            var variables = new Dictionary<string, object>();
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("query(");

            for (int i = 0; i < repositories.Count; i++)
            {
                queryBuilder.AppendLine($"  $owner{i}: String!, $name{i}: String!{(i == repositories.Count - 1 ? "" : ",")}");
                variables[$"owner{i}"] = repositories[i].Owner;
                variables[$"name{i}"] = repositories[i].Name;
            }
            queryBuilder.AppendLine(") {");

            for (int i = 0; i < repositories.Count; i++)
            {
                queryBuilder.AppendLine($"  repo{i}: repository(owner: $owner{i}, name: $name{i}) {{");
                queryBuilder.AppendLine("    name");
                queryBuilder.AppendLine("    description");
                queryBuilder.AppendLine("    licenseInfo { spdxId }");
                queryBuilder.AppendLine("    stargazerCount");
                queryBuilder.AppendLine("    url");
                queryBuilder.AppendLine("  }");
            }

            queryBuilder.AppendLine("  rateLimit {");
            queryBuilder.AppendLine("    limit");
            queryBuilder.AppendLine("    remaining");
            queryBuilder.AppendLine("    resetAt");
            queryBuilder.AppendLine("  }");
            queryBuilder.AppendLine("}");

            var payload = new
            {
                query = queryBuilder.ToString(),
                variables = variables
            };

            try
            {
                var client = _clientProvider.GetClient("GitHubApi");
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/graphql");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using (var response = await client.SendAsync(request, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return OperationResult<Dictionary<string, RepositoryMetadata>>.Fail(
                            $"GraphQL request failed with status: {response.StatusCode}",
                            ErrorCategory.Network);
                    }

                    string json = await response.Content.ReadAsStringAsync(cancellationToken);
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;

                        if (root.TryGetProperty("errors", out var errorsProp))
                        {
                            RetroLogger.Log($"GraphQL partial error: {errorsProp.GetRawText()}", "WARNING");
                        }

                        if (!root.TryGetProperty("data", out var dataProp))
                        {
                            return OperationResult<Dictionary<string, RepositoryMetadata>>.Fail(
                                "GraphQL response contains no data.",
                                ErrorCategory.Parser);
                        }

                        var resultDict = new Dictionary<string, RepositoryMetadata>();

                        for (int i = 0; i < repositories.Count; i++)
                        {
                            string key = $"{repositories[i].Owner}/{repositories[i].Name}";
                            string alias = $"repo{i}";

                            if (dataProp.TryGetProperty(alias, out var repoProp) && repoProp.ValueKind == JsonValueKind.Object)
                            {
                                var meta = new RepositoryMetadata
                                {
                                    Name = repoProp.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                                    Description = repoProp.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                                    Url = repoProp.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "",
                                    Stargazers = repoProp.TryGetProperty("stargazerCount", out var s) ? s.GetInt32() : 0,
                                    License = repoProp.TryGetProperty("licenseInfo", out var lic) && lic.ValueKind == JsonValueKind.Object && lic.TryGetProperty("spdxId", out var spdx) ? spdx.GetString() ?? "" : "None"
                                };

                                lock (_cacheLock)
                                {
                                    _cache[key] = meta;
                                }
                                resultDict[key] = meta;
                            }
                        }

                        return OperationResult<Dictionary<string, RepositoryMetadata>>.Ok(resultDict);
                    }
                }
            }
            catch (Exception ex)
            {
                return OperationResult<Dictionary<string, RepositoryMetadata>>.Fail(
                    $"GraphQL request failed: {ex.Message}",
                    ErrorCategory.Internal,
                    ex);
            }
        }
    }
}
