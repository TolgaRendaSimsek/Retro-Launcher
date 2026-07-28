using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class GitHubReleaseProvider : IReleaseProvider
    {
        private readonly IHttpClientProvider _clientProvider;
        private readonly IApiResponseCache _cache;
        private readonly IRateLimitCoordinator _rateLimitCoordinator;
        private readonly IApplicationSettingsService _settings;
        private readonly IGitHubReleaseClient _gitHubReleaseClient;

        public GitHubReleaseProvider(
            IHttpClientProvider? clientProvider = null,
            IApiResponseCache? cache = null,
            IRateLimitCoordinator? rateLimitCoordinator = null,
            IApplicationSettingsService? settings = null)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
            _cache = cache ?? new FileApiResponseCache();
            _rateLimitCoordinator = rateLimitCoordinator ?? RateLimitCoordinator.Instance;
            _settings = settings ?? ApplicationSettingsService.Instance;
            _gitHubReleaseClient = new GitHubReleaseClient(_clientProvider);
        }

        public async Task<OperationResult<ReleaseInfo>> GetLatestReleaseAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = new ApiCacheKey($"/repos/{query.Owner}/{query.Repository}/releases/latest", ReleaseProviderType.GitHub);
            ApiCacheEntry? cachedEntry = null;

            try
            {
                var cacheResult = await _cache.GetAsync(cacheKey);
                cachedEntry = (cacheResult.Status == CacheFreshness.Fresh || cacheResult.Status == CacheFreshness.Stale) ? cacheResult.Entry : null;
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Cache read failure: {ex.Message}", "WARNING");
            }

            int maxRetries = _settings.Network.MaxRetryCount;
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                attempt++;
                try
                {
                    await _rateLimitCoordinator.WaitIfNeededAsync(cancellationToken);

                    string? etag = cachedEntry?.ETag;
                    var clientResult = await _gitHubReleaseClient.GetLatestReleaseAsync(query.Owner, query.Repository, etag, cancellationToken);

                    if (clientResult.RateLimit != null)
                    {
                        _rateLimitCoordinator.UpdateState(
                            clientResult.RateLimit.Limit,
                            clientResult.RateLimit.Remaining,
                            clientResult.RateLimit.ResetTime);
                    }

                    if (clientResult.StatusCode == HttpStatusCode.NotModified)
                    {
                        if (cachedEntry != null)
                        {
                            RetroLogger.Log($"GitHub conditional validation hit (304 Not Modified) for latest release.", "INFO");
                            cachedEntry.ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Cache.CacheDurationMinutes);
                            await _cache.SetAsync(cacheKey, cachedEntry);

                            var cachedRelease = JsonSerializer.Deserialize<GitHubRelease>(cachedEntry.ResponseBody);
                            if (cachedRelease != null)
                            {
                                var mapped = MapRelease(cachedRelease, query.Owner, query.Repository);
                                var okResult = OperationResult<ReleaseInfo>.Ok(mapped, isFromCache: true);
                                // Set validation flag
                                typeof(OperationResult<ReleaseInfo>).GetProperty("IsValidatedFromCache")?.SetValue(okResult, true);
                                return okResult;
                            }
                        }
                        else
                        {
                            // Stale cache body is missing, retry without etag
                            cachedEntry = null;
                            continue;
                        }
                    }

                    if (clientResult.Success && clientResult.Data != null)
                    {
                        var release = clientResult.Data;
                        var mapped = MapRelease(release, query.Owner, query.Repository);

                        // Save to cache
                        string responseBody = JsonSerializer.Serialize(release);
                        var entry = new ApiCacheEntry
                        {
                            ResponseBody = responseBody,
                            ETag = clientResult.ETag,
                            StoredAt = DateTime.UtcNow,
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Cache.CacheDurationMinutes)
                        };
                        await _cache.SetAsync(cacheKey, entry);

                        return OperationResult<ReleaseInfo>.Ok(mapped);
                    }

                    // Check if rate limit or transient
                    bool isRateLimit = clientResult.StatusCode == HttpStatusCode.Forbidden || clientResult.StatusCode == (HttpStatusCode)429;
                    bool isTransient = clientResult.StatusCode == HttpStatusCode.RequestTimeout || ((int?)clientResult.StatusCode >= 500 && (int?)clientResult.StatusCode < 600);

                    if ((isRateLimit || isTransient) && attempt <= maxRetries)
                    {
                        TimeSpan retryAfterDelay = TimeSpan.Zero;
                        TimeSpan finalDelay = retryAfterDelay > TimeSpan.Zero ? retryAfterDelay : delay;
                        RetroLogger.Log($"GitHub request failed with {clientResult.StatusCode}. Retrying ({attempt}/{maxRetries}) in {finalDelay.TotalSeconds:F1}s...", "WARNING");
                        
                        var delayProvider = ((RateLimitCoordinator)_rateLimitCoordinator).DelayProvider;
                        await delayProvider.DelayAsync(finalDelay, cancellationToken);
                        
                        delay = TimeSpan.FromTicks(delay.Ticks * 2);
                        continue;
                    }

                    var category = ErrorCategory.Internal;
                    if (clientResult.StatusCode == HttpStatusCode.NotFound) category = ErrorCategory.NotFound;
                    else if (isRateLimit) category = ErrorCategory.RateLimit;

                    return OperationResult<ReleaseInfo>.Fail(clientResult.ErrorMessage ?? "Failed to fetch latest release.", category);
                }
                catch (Exception ex)
                {
                    if (cachedEntry != null)
                    {
                        RetroLogger.Log($"Network failure: {ex.Message}. Falling back to stale cached metadata.", "WARNING");
                        try
                        {
                            var cachedRelease = JsonSerializer.Deserialize<GitHubRelease>(cachedEntry.ResponseBody);
                            if (cachedRelease != null)
                            {
                                var mapped = MapRelease(cachedRelease, query.Owner, query.Repository);
                                return OperationResult<ReleaseInfo>.Ok(mapped, isFromCache: true);
                            }
                        }
                        catch { }
                    }

                    var mappedErr = NetworkFailureMapper.MapException(ex);
                    if (attempt > maxRetries || mappedErr.Category == ErrorCategory.Internal)
                    {
                        return OperationResult<ReleaseInfo>.Fail(mappedErr.Message, mappedErr.Category, ex);
                    }

                    var jitter = new Random();
                    TimeSpan netDelay = delay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
                    RetroLogger.Log($"Network failure. Retrying ({attempt}/{maxRetries}) in {netDelay.TotalSeconds:F1}s...", "WARNING");
                    
                    var delayProvider = ((RateLimitCoordinator)_rateLimitCoordinator).DelayProvider;
                    await delayProvider.DelayAsync(netDelay, cancellationToken);
                    
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
            }
        }

        public async Task<OperationResult<IReadOnlyList<ReleaseInfo>>> GetReleasesAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            string relativeUri = $"/repos/{query.Owner}/{query.Repository}/releases?per_page=30";
            return await ExecuteRequestAsync<IReadOnlyList<ReleaseInfo>>(relativeUri, json =>
            {
                var dtos = JsonSerializer.Deserialize<List<GitHubReleaseDto>>(json);
                if (dtos == null) throw new JsonException("Deserialization returned null.");
                return dtos.Select(d => MapRelease(d, query.Owner, query.Repository)).ToList().AsReadOnly();
            }, cancellationToken);
        }

        public async Task<OperationResult<ReleaseInfo>> GetReleaseByTagAsync(ReleaseQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(query.Tag))
            {
                return OperationResult<ReleaseInfo>.Fail("Tag parameter cannot be null or empty for GetReleaseByTagAsync.", ErrorCategory.Validation);
            }
            string relativeUri = $"/repos/{query.Owner}/{query.Repository}/releases/tags/{query.Tag}";
            return await ExecuteRequestAsync(relativeUri, json =>
            {
                var dto = JsonSerializer.Deserialize<GitHubReleaseDto>(json);
                if (dto == null) throw new JsonException("Deserialization returned null.");
                return MapRelease(dto, query.Owner, query.Repository);
            }, cancellationToken);
        }

        public async Task<OperationResult<bool>> GetProviderStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = _clientProvider.GetClient("GitHubApi");
                var response = await client.GetAsync("/rate_limit", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                ParseAndLogHeaders(response);
                return OperationResult<bool>.Ok(response.IsSuccessStatusCode);
            }
            catch (Exception ex)
            {
                var err = NetworkFailureMapper.MapException(ex);
                return OperationResult<bool>.Fail(err.Message, err.Category, ex);
            }
        }

        private async Task<OperationResult<T>> ExecuteRequestAsync<T>(string relativeUri, Func<string, T> parseFunc, CancellationToken cancellationToken)
        {
            string coordinationKey = $"{ReleaseProviderType.GitHub}_{relativeUri}";
            return await _rateLimitCoordinator.CoordinateRequestAsync(coordinationKey, async () =>
            {
                return await ExecuteRequestInternalAsync(relativeUri, parseFunc, cancellationToken);
            });
        }

        private async Task<OperationResult<T>> ExecuteRequestInternalAsync<T>(string relativeUri, Func<string, T> parseFunc, CancellationToken cancellationToken, bool forceNoCache = false)
        {
            var cacheKey = new ApiCacheKey(relativeUri, ReleaseProviderType.GitHub);
            ApiCacheEntry? cachedEntry = null;

            if (!forceNoCache)
            {
                var cacheRes = await _cache.GetAsync(cacheKey);
                if (cacheRes.Status == CacheFreshness.Fresh && cacheRes.Entry != null)
                {
                    try
                    {
                        T parsed = parseFunc(cacheRes.Entry.ResponseBody);
                        return OperationResult<T>.Ok(parsed, isFromCache: true);
                    }
                    catch (Exception ex)
                    {
                        RetroLogger.Log($"Failed to parse fresh cache for {relativeUri}: {ex.Message}. Bypassing cache.", "WARNING");
                    }
                }
                else if (cacheRes.Status == CacheFreshness.Stale)
                {
                    cachedEntry = cacheRes.Entry;
                }
            }

            int maxRetries = _settings.Network.MaxRetryCount;
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                attempt++;
                try
                {
                    await _rateLimitCoordinator.WaitIfNeededAsync(cancellationToken);

                    var client = _clientProvider.GetClient("GitHubApi");
                    var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);

                    if (cachedEntry != null)
                    {
                        if (!string.IsNullOrEmpty(cachedEntry.ETag))
                        {
                            request.Headers.IfNoneMatch.ParseAdd(cachedEntry.ETag);
                        }
                        if (!string.IsNullOrEmpty(cachedEntry.LastModified))
                        {
                            request.Headers.IfModifiedSince = DateTimeOffset.Parse(cachedEntry.LastModified);
                        }
                    }

                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        ParseAndLogHeaders(response);

                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            if (cachedEntry != null)
                            {
                                RetroLogger.Log($"GitHub conditional validation hit (304 Not Modified) for {relativeUri}.", "INFO");
                                cachedEntry.ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Cache.CacheDurationMinutes);
                                await _cache.SetAsync(cacheKey, cachedEntry);

                                T parsed = parseFunc(cachedEntry.ResponseBody);
                                return OperationResult<T>.Ok(parsed, isFromCache: true);
                            }
                            else
                            {
                                RetroLogger.Log("Received 304 but cached body is missing. Retrying without condition headers.", "WARNING");
                                return await ExecuteRequestInternalAsync(relativeUri, parseFunc, cancellationToken, forceNoCache: true);
                            }
                        }

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                            T parsed = parseFunc(responseBody);

                            var entry = new ApiCacheEntry
                            {
                                ResponseBody = responseBody,
                                ETag = response.Headers.ETag?.ToString(),
                                LastModified = response.Content.Headers.LastModified?.ToString("R"),
                                StoredAt = DateTime.UtcNow,
                                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Cache.CacheDurationMinutes)
                            };
                            await _cache.SetAsync(cacheKey, entry);

                            return OperationResult<T>.Ok(parsed);
                        }

                        bool isRateLimit = response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429;
                        bool isTransient = response.StatusCode == HttpStatusCode.RequestTimeout || ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600);

                        if ((isRateLimit || isTransient) && attempt <= maxRetries)
                        {
                            TimeSpan retryAfterDelay = TimeSpan.Zero;
                            if (response.Headers.RetryAfter != null)
                            {
                                if (response.Headers.RetryAfter.Delta.HasValue)
                                {
                                    retryAfterDelay = response.Headers.RetryAfter.Delta.Value;
                                }
                                else if (response.Headers.RetryAfter.Date.HasValue)
                                {
                                    retryAfterDelay = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                                }
                            }

                            TimeSpan finalDelay = retryAfterDelay > TimeSpan.Zero ? retryAfterDelay : delay;
                            if (finalDelay > TimeSpan.FromSeconds(30))
                            {
                                return CreateErrorResult<T>($"GitHub API returned status code {response.StatusCode} and Retry-After is too long.", response.StatusCode);
                            }

                            var jitter = new Random();
                            finalDelay = finalDelay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));

                            RetroLogger.Log($"GitHub request failed with {response.StatusCode}. Retrying ({attempt}/{maxRetries}) in {finalDelay.TotalSeconds:F1}s...", "WARNING");
                            
                            var delayProvider = (RateLimitCoordinator.Instance).DelayProvider;
                            await delayProvider.DelayAsync(finalDelay, cancellationToken);
                            
                            delay = TimeSpan.FromTicks(delay.Ticks * 2);
                            continue;
                        }

                        return CreateErrorResult<T>($"GitHub API returned error status: {response.StatusCode} ({(int)response.StatusCode})", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    if (cachedEntry != null)
                    {
                        RetroLogger.Log($"Network failure: {ex.Message}. Falling back to stale cached metadata.", "WARNING");
                        try
                        {
                            T parsed = parseFunc(cachedEntry.ResponseBody);
                            return OperationResult<T>.Ok(parsed, isFromCache: true);
                        }
                        catch { }
                    }

                    var mappedErr = NetworkFailureMapper.MapException(ex);
                    if (attempt > maxRetries || mappedErr.Category == ErrorCategory.Internal)
                    {
                        return OperationResult<T>.Fail(mappedErr.Message, mappedErr.Category, ex);
                    }

                    var jitter = new Random();
                    TimeSpan netDelay = delay.Add(TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
                    RetroLogger.Log($"Network failure. Retrying ({attempt}/{maxRetries}) in {netDelay.TotalSeconds:F1}s...", "WARNING");
                    
                    var delayProvider = (RateLimitCoordinator.Instance).DelayProvider;
                    await delayProvider.DelayAsync(netDelay, cancellationToken);
                    
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
            }
        }

        private void ParseAndLogHeaders(HttpResponseMessage response)
        {
            int limit = 60;
            int remaining = 60;
            DateTime resetTime = DateTime.MinValue;

            if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limits) && int.TryParse(limits.FirstOrDefault(), out var limitVal))
            {
                limit = limitVal;
            }
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainings) && int.TryParse(remainings.FirstOrDefault(), out var remainingVal))
            {
                remaining = remainingVal;
            }
            if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resets) && long.TryParse(resets.FirstOrDefault(), out var resetVal))
            {
                resetTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(resetVal);
            }

            if (resets != null)
            {
                _rateLimitCoordinator.UpdateState(limit, remaining, resetTime);
            }
        }

        private OperationResult<T> CreateErrorResult<T>(string msg, HttpStatusCode code)
        {
            var category = ErrorCategory.Internal;
            if (code == HttpStatusCode.NotFound) category = ErrorCategory.NotFound;
            else if (code == HttpStatusCode.Forbidden && _rateLimitCoordinator.GetState().Remaining == 0) category = ErrorCategory.RateLimit;
            else if (code == HttpStatusCode.Forbidden || code == HttpStatusCode.Unauthorized) category = ErrorCategory.Unauthorized;
            else if (code == (HttpStatusCode)429) category = ErrorCategory.RateLimit;
            else if ((int)code >= 500) category = ErrorCategory.Network;

            return OperationResult<T>.Fail(msg, category);
        }

        private ReleaseInfo MapRelease(GitHubReleaseDto dto, string owner, string repo)
        {
            var info = new ReleaseInfo
            {
                Provider = ReleaseProviderType.GitHub,
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
                    ContentType = asset.ContentType,
                    CreatedAt = asset.CreatedAt,
                    UpdatedAt = asset.UpdatedAt
                });
            }

            return info;
        }

        private ReleaseInfo MapRelease(GitHubRelease dto, string owner, string repo)
        {
            var info = new ReleaseInfo
            {
                Provider = ReleaseProviderType.GitHub,
                RepositoryIdentifier = $"{owner}/{repo}",
                Tag = dto.TagName,
                Name = dto.Name,
                Description = dto.Name,
                IsDraft = dto.IsDraft,
                IsPrerelease = dto.IsPrerelease,
                PublishedAt = dto.PublishedAt,
                WebUrl = dto.HtmlUrl
            };

            foreach (var asset in dto.Assets)
            {
                info.Assets.Add(new ReleaseAssetInfo
                {
                    Id = asset.Name,
                    Name = asset.Name,
                    DownloadUrl = asset.BrowserDownloadUrl,
                    Size = asset.Size,
                    ContentType = asset.ContentType,
                    CreatedAt = asset.CreatedAt,
                    UpdatedAt = asset.UpdatedAt
                });
            }

            return info;
        }
    }
}
