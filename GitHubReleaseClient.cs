using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface IGitHubReleaseClient
    {
        Task<GitHubApiResult<GitHubRelease>> GetLatestReleaseAsync(string owner, string repository, string? etag, CancellationToken cancellationToken);
    }

    public class GitHubReleaseClient : IGitHubReleaseClient
    {
        private static GitHubReleaseClient? _instance;
        public static GitHubReleaseClient Instance => _instance ??= new GitHubReleaseClient();

        private readonly IHttpClientProvider _clientProvider;

        public GitHubReleaseClient(IHttpClientProvider? clientProvider = null)
        {
            _clientProvider = clientProvider ?? HttpClientProvider.Instance;
        }

        public async Task<GitHubApiResult<GitHubRelease>> GetLatestReleaseAsync(string owner, string repository, string? etag, CancellationToken cancellationToken)
        {
            var result = new GitHubApiResult<GitHubRelease> { Success = false };

            try
            {
                var client = _clientProvider.GetClient("GitHubApi");
                
                // Ensure proper version headers are present
                if (!client.DefaultRequestHeaders.Contains("X-GitHub-Api-Version"))
                {
                    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                }

                string url = $"/repos/{owner}/{repository}/releases/latest";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(etag))
                {
                    request.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(etag));
                }

                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    result.StatusCode = response.StatusCode;
                    
                    // Parse ETag response header
                    if (response.Headers.TryGetValues("ETag", out var etags))
                    {
                        result.ETag = etags.FirstOrDefault();
                    }

                    // Parse rate limit headers
                    var rateLimit = ParseRateLimitHeaders(response);
                    result.RateLimit = rateLimit;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string body = await response.Content.ReadAsStringAsync(cancellationToken);
                        try
                        {
                            var release = JsonSerializer.Deserialize<GitHubRelease>(body);
                            if (release == null)
                            {
                                result.ErrorMessage = "Failed to deserialize GitHub release (received empty body).";
                                return result;
                            }

                            result.Success = true;
                            result.Data = release;
                            return result;
                        }
                        catch (JsonException ex)
                        {
                            result.ErrorMessage = $"Malformed JSON response from GitHub API: {ex.Message}";
                            return result;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        result.ErrorMessage = "Release not modified.";
                        return result;
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
                    {
                        string resetMsg = rateLimit != null 
                            ? $". Rate limit will reset at {rateLimit.ResetTime.ToLocalTime()}."
                            : "";
                        result.ErrorMessage = $"GitHub API rate limit exceeded or access forbidden{resetMsg}";
                        return result;
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        result.ErrorMessage = $"GitHub repository or latest release not found: '{owner}/{repository}'.";
                        return result;
                    }
                    else
                    {
                        result.ErrorMessage = $"GitHub API returned error status: {response.StatusCode} ({(int)response.StatusCode})";
                        return result;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result.ErrorMessage = "The request was cancelled by the user.";
                    throw;
                }
                else
                {
                    result.ErrorMessage = "The request timed out.";
                }
            }
            catch (HttpRequestException ex)
            {
                result.ErrorMessage = $"Network or DNS resolution failure: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }

            return result;
        }

        private GitHubRateLimitInfo? ParseRateLimitHeaders(HttpResponseMessage response)
        {
            try
            {
                int limit = 0;
                int remaining = 0;
                DateTime resetTime = DateTime.MinValue;
                bool hasHeaders = false;

                if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limits) && int.TryParse(limits.FirstOrDefault(), out var limitVal))
                {
                    limit = limitVal;
                    hasHeaders = true;
                }
                if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainings) && int.TryParse(remainings.FirstOrDefault(), out var remainingVal))
                {
                    remaining = remainingVal;
                    hasHeaders = true;
                }
                if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resets) && long.TryParse(resets.FirstOrDefault(), out var resetVal))
                {
                    resetTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(resetVal);
                    hasHeaders = true;
                }

                if (hasHeaders)
                {
                    return new GitHubRateLimitInfo
                    {
                        Limit = limit,
                        Remaining = remaining,
                        ResetTime = resetTime
                    };
                }
            }
            catch { }
            return null;
        }
    }
}
