using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Infrastructure.Http
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

                string fullUrl = client.BaseAddress != null ? new Uri(client.BaseAddress, url).ToString() : url;

                bool userAgentPresent = client.DefaultRequestHeaders.UserAgent.Any(u => u.Product != null && u.Product.Name == "RetroLauncher");
                bool acceptPresent = client.DefaultRequestHeaders.Accept.Any(a => a.MediaType == "application/vnd.github+json");
                bool isHttps = fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

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

                    string body = await response.Content.ReadAsStringAsync(cancellationToken);
                    result.ResponseBody = body;

                    // Format response headers
                    string headersStr = string.Join("; ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"));
                    string bodySnippet = body.Length > 500 ? body.Substring(0, 500) + "..." : body;

                    // Log connection diagnostic information before JSON parsing
                    RetroLogger.Log($"[GitHub API Request] URL: {fullUrl} | HTTPS: {isHttps} | UserAgent: {userAgentPresent} | Accept: {acceptPresent}");
                    RetroLogger.Log($"[GitHub API Response] Status: {(int)response.StatusCode} ({response.StatusCode})");
                    RetroLogger.Log($"[GitHub API Response] Headers: {headersStr}");
                    RetroLogger.Log($"[GitHub API Response] Body (first 500 chars): {bodySnippet}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var release = JsonSerializer.Deserialize<GitHubRelease>(body);
                            if (release == null)
                            {
                                RetroLogger.Log($"[GitHub API Error] Deserialization returned null for '{fullUrl}'. Raw response body: {body}", "ERROR");
                                result.ErrorMessage = "Failed to deserialize GitHub release (received null body).";
                                return result;
                            }

                            result.Success = true;
                            result.Data = release;
                            return result;
                        }
                        catch (JsonException ex)
                        {
                            RetroLogger.Log($"[GitHub API Error] JSON parsing failed for '{fullUrl}': {ex.Message}. Raw response body: {body}", "ERROR");
                            result.ErrorMessage = $"Malformed JSON response from GitHub API: {ex.Message}";
                            return result;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        result.ErrorMessage = "Release not modified.";
                        return result;
                    }
                    else
                    {
                        RetroLogger.Log($"[GitHub API Error] Request to '{fullUrl}' failed with HTTP {(int)response.StatusCode} ({response.StatusCode}). Raw response body: {body}", "ERROR");
                        result.ErrorMessage = $"HTTP {(int)response.StatusCode} ({response.StatusCode}): {body}";
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
