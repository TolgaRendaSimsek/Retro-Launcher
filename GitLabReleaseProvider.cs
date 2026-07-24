using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class GitLabReleaseDto
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("released_at")]
        public DateTime? ReleasedAt { get; set; }

        [JsonPropertyName("assets")]
        public GitLabAssetsDto? Assets { get; set; }
    }

    public class GitLabAssetsDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("links")]
        public List<GitLabLinkDto> Links { get; set; } = new();
    }

    public class GitLabLinkDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("direct_asset_url")]
        public string DirectAssetUrl { get; set; } = "";
    }

    public class GitLabReleaseProvider : IReleaseProvider
    {
        private readonly IHttpClientProvider _clientProvider;
        private readonly IApiResponseCache _cache;
        private readonly IRateLimitCoordinator _rateLimitCoordinator;
        private readonly IApplicationSettingsService _settings;

        public GitLabReleaseProvider(
            IHttpClientProvider? clientProvider = null,
            IApiResponseCache? cache = null,
            IRateLimitCoordinator? rateLimitCoordinator = null,
            IApplicationSettingsService? settings = null)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
            _cache = cache ?? new FileApiResponseCache();
            _rateLimitCoordinator = rateLimitCoordinator ?? RateLimitCoordinator.Instance;
            _settings = settings ?? ApplicationSettingsService.Instance;
        }

        public async Task<OperationResult<ReleaseInfo>> GetLatestReleaseAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            var res = await GetReleasesAsync(query, cancellationToken);
            if (res.Success && res.Data != null && res.Data.Any())
            {
                return OperationResult<ReleaseInfo>.Ok(res.Data.First());
            }
            return OperationResult<ReleaseInfo>.Fail("No releases found on GitLab.", ErrorCategory.NotFound);
        }

        public async Task<OperationResult<IReadOnlyList<ReleaseInfo>>> GetReleasesAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            string urlEncodedRepo = Uri.EscapeDataString($"{query.Owner}/{query.Repository}");
            string relativeUri = $"https://gitlab.com/api/v4/projects/{urlEncodedRepo}/releases";
            
            return await ExecuteRequestAsync<IReadOnlyList<ReleaseInfo>>(relativeUri, json =>
            {
                var dtos = JsonSerializer.Deserialize<List<GitLabReleaseDto>>(json);
                if (dtos == null) throw new JsonException("Deserialization returned null.");
                return dtos.Select(d => MapRelease(d, query.Owner, query.Repository)).ToList().AsReadOnly();
            }, cancellationToken);
        }

        public async Task<OperationResult<ReleaseInfo>> GetReleaseByTagAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(query.Tag))
            {
                return OperationResult<ReleaseInfo>.Fail("Tag parameter cannot be null or empty.", ErrorCategory.Validation);
            }
            string urlEncodedRepo = Uri.EscapeDataString($"{query.Owner}/{query.Repository}");
            string relativeUri = $"https://gitlab.com/api/v4/projects/{urlEncodedRepo}/releases/{query.Tag}";

            return await ExecuteRequestAsync(relativeUri, json =>
            {
                var dto = JsonSerializer.Deserialize<GitLabReleaseDto>(json);
                if (dto == null) throw new JsonException("Deserialization returned null.");
                return MapRelease(dto, query.Owner, query.Repository);
            }, cancellationToken);
        }

        public async Task<OperationResult<bool>> GetProviderStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = _clientProvider.GetClient("PackageDownloads");
                using (var response = await client.GetAsync("https://gitlab.com/api/v4/projects", HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    return OperationResult<bool>.Ok(response.IsSuccessStatusCode);
                }
            }
            catch (Exception ex)
            {
                var err = NetworkFailureMapper.MapException(ex);
                return OperationResult<bool>.Fail(err.Message, err.Category, ex);
            }
        }

        private async Task<OperationResult<T>> ExecuteRequestAsync<T>(string relativeUri, Func<string, T> parseFunc, CancellationToken cancellationToken)
        {
            string coordinationKey = $"{ReleaseProviderType.GitLab}_{relativeUri}";
            return await _rateLimitCoordinator.CoordinateRequestAsync(coordinationKey, async () =>
            {
                return await ExecuteRequestInternalAsync(relativeUri, parseFunc, cancellationToken);
            });
        }

        private async Task<OperationResult<T>> ExecuteRequestInternalAsync<T>(string relativeUri, Func<string, T> parseFunc, CancellationToken cancellationToken)
        {
            var cacheKey = new ApiCacheKey(relativeUri, ReleaseProviderType.GitLab);
            var cacheRes = await _cache.GetAsync(cacheKey);

            if (cacheRes.Status == CacheFreshness.Fresh && cacheRes.Entry != null)
            {
                try
                {
                    T parsed = parseFunc(cacheRes.Entry.ResponseBody);
                    return OperationResult<T>.Ok(parsed, isFromCache: true);
                }
                catch { }
            }

            int maxRetries = _settings.Network.MaxRetryCount;
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                attempt++;
                try
                {
                    var client = _clientProvider.GetClient("PackageDownloads");
                    var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);

                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                            T parsed = parseFunc(responseBody);

                            var entry = new ApiCacheEntry
                            {
                                ResponseBody = responseBody,
                                StoredAt = DateTime.UtcNow,
                                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Cache.CacheDurationMinutes)
                            };
                            await _cache.SetAsync(cacheKey, entry);

                            return OperationResult<T>.Ok(parsed);
                        }

                        if ((response.StatusCode == (HttpStatusCode)429 || (int)response.StatusCode >= 500) && attempt <= maxRetries)
                        {
                            var jitter = new Random();
                            var finalDelay = delay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
                            await (RateLimitCoordinator.Instance).DelayProvider.DelayAsync(finalDelay, cancellationToken);
                            delay = TimeSpan.FromTicks(delay.Ticks * 2);
                            continue;
                        }

                        return OperationResult<T>.Fail($"GitLab API returned: {response.StatusCode}", ErrorCategory.Internal);
                    }
                }
                catch (Exception ex)
                {
                    var mappedErr = NetworkFailureMapper.MapException(ex);
                    if (attempt > maxRetries || mappedErr.Category == ErrorCategory.Internal)
                    {
                        return OperationResult<T>.Fail(mappedErr.Message, mappedErr.Category, ex);
                    }

                    var jitter = new Random();
                    TimeSpan netDelay = delay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
                    await (RateLimitCoordinator.Instance).DelayProvider.DelayAsync(netDelay, cancellationToken);
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
            }
        }

        private ReleaseInfo MapRelease(GitLabReleaseDto dto, string owner, string repo)
        {
            var info = new ReleaseInfo
            {
                Provider = ReleaseProviderType.GitLab,
                RepositoryIdentifier = $"{owner}/{repo}",
                Tag = dto.TagName,
                Name = dto.Name,
                Description = dto.Description,
                PublishedAt = dto.ReleasedAt,
                WebUrl = $"https://gitlab.com/{owner}/{repo}/-/releases/{dto.TagName}"
            };

            if (dto.Assets?.Links != null)
            {
                foreach (var link in dto.Assets.Links)
                {
                    info.Assets.Add(new ReleaseAssetInfo
                    {
                        Id = link.Id.ToString(),
                        Name = link.Name,
                        DownloadUrl = link.DirectAssetUrl ?? link.Url,
                        Size = 0,
                        ContentType = "application/octet-stream"
                    });
                }
            }

            return info;
        }
    }
}
