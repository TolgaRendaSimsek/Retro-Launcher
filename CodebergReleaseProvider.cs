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
    public class GiteaReleaseDto
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = "";

        [JsonPropertyName("assets")]
        public List<GiteaAssetDto> Assets { get; set; } = new();
    }

    public class GiteaAssetDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }

    public class CodebergReleaseProvider : IReleaseProvider
    {
        private readonly IHttpClientProvider _clientProvider;
        private readonly IApiResponseCache _cache;
        private readonly IRateLimitCoordinator _rateLimitCoordinator;
        private readonly IApplicationSettingsService _settings;

        public CodebergReleaseProvider(
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
            return OperationResult<ReleaseInfo>.Fail("No releases found on Codeberg.", ErrorCategory.NotFound);
        }

        public async Task<OperationResult<IReadOnlyList<ReleaseInfo>>> GetReleasesAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            string relativeUri = $"https://codeberg.org/api/v1/repos/{query.Owner}/{query.Repository}/releases";
            
            return await ExecuteRequestAsync<IReadOnlyList<ReleaseInfo>>(relativeUri, json =>
            {
                var dtos = JsonSerializer.Deserialize<List<GiteaReleaseDto>>(json);
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
            string relativeUri = $"https://codeberg.org/api/v1/repos/{query.Owner}/{query.Repository}/releases/tags/{query.Tag}";

            return await ExecuteRequestAsync(relativeUri, json =>
            {
                var dto = JsonSerializer.Deserialize<GiteaReleaseDto>(json);
                if (dto == null) throw new JsonException("Deserialization returned null.");
                return MapRelease(dto, query.Owner, query.Repository);
            }, cancellationToken);
        }

        public async Task<OperationResult<bool>> GetProviderStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = _clientProvider.GetClient("PackageDownloads");
                using (var response = await client.GetAsync("https://codeberg.org/api/v1/repos/rpcs3/rpcs3", HttpCompletionOption.ResponseHeadersRead, cancellationToken))
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
            string coordinationKey = $"{ReleaseProviderType.Codeberg}_{relativeUri}";
            return await _rateLimitCoordinator.CoordinateRequestAsync(coordinationKey, async () =>
            {
                return await ExecuteRequestInternalAsync(relativeUri, parseFunc, cancellationToken);
            });
        }

        private async Task<OperationResult<T>> ExecuteRequestInternalAsync<T>(string relativeUri, Func<string, T> parseFunc, CancellationToken cancellationToken)
        {
            var cacheKey = new ApiCacheKey(relativeUri, ReleaseProviderType.Codeberg);
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

                        return OperationResult<T>.Fail($"Codeberg API returned: {response.StatusCode}", ErrorCategory.Internal);
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

        private ReleaseInfo MapRelease(GiteaReleaseDto dto, string owner, string repo)
        {
            var info = new ReleaseInfo
            {
                Provider = ReleaseProviderType.Codeberg,
                RepositoryIdentifier = $"{owner}/{repo}",
                Tag = dto.TagName,
                Name = dto.Name,
                Description = dto.Body,
                IsDraft = dto.Draft,
                IsPrerelease = dto.Prerelease,
                PublishedAt = dto.PublishedAt,
                WebUrl = dto.HtmlUrl
            };

            foreach (var asset in dto.Assets)
            {
                info.Assets.Add(new ReleaseAssetInfo
                {
                    Id = asset.Id.ToString(),
                    Name = asset.Name,
                    DownloadUrl = asset.BrowserDownloadUrl,
                    Size = asset.Size,
                    ContentType = "application/octet-stream"
                });
            }

            return info;
        }
    }
}
