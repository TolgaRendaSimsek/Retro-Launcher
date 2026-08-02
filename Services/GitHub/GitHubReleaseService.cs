using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Services.GitHub
{
    public class GitHubReleaseService : IGitHubReleaseService
    {
        private static readonly HttpClient _httpClient;
        private string? _token;

        static GitHubReleaseService()
        {
            _httpClient = new HttpClient();
            // Recommended default headers for GitHub REST API v3
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }

        public void ConfigureToken(string? token)
        {
            _token = token;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(_token))
            {
                // Authenticate request without writing to logs
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            return request;
        }

        public async Task<GitHubApiResult<GitHubRelease>> GetLatestReleaseAsync(string owner, string repo, CancellationToken token)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            RetroLogger.Log($"Querying latest release for {owner}/{repo}...");

            try
            {
                using (var request = CreateRequest(HttpMethod.Get, url))
                using (var response = await _httpClient.SendAsync(request, token))
                {
                    var rateLimit = ParseRateLimit(response.Headers);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        RetroLogger.Log($"Release not found for {owner}/{repo}.", "WARNING");
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = "Repository or latest release not found.",
                            RateLimit = rateLimit
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden && rateLimit.Remaining == 0)
                    {
                        string resetMsg = $"Rate limit exceeded. Resets at {rateLimit.ResetTime}.";
                        RetroLogger.Log(resetMsg, "ERROR");
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = resetMsg,
                            RateLimit = rateLimit
                        };
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorMsg = $"GitHub API returned error status: {response.StatusCode} ({(int)response.StatusCode})";
                        RetroLogger.Log(errorMsg, "ERROR");
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = errorMsg,
                            RateLimit = rateLimit
                        };
                    }

                    string json = await response.Content.ReadAsStringAsync(token);
                    var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                    if (release == null)
                    {
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = "Failed to deserialize JSON response.",
                            RateLimit = rateLimit
                        };
                    }

                    return new GitHubApiResult<GitHubRelease>
                    {
                        Success = true,
                        Data = release,
                        StatusCode = response.StatusCode,
                        RateLimit = rateLimit
                    };
                }
            }
            catch (JsonException ex)
            {
                string msg = $"Invalid JSON payload in release response: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
            catch (HttpRequestException ex)
            {
                string msg = $"Network/HTTP request failed: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
            catch (OperationCanceledException)
            {
                RetroLogger.Log("GetLatestReleaseAsync operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Unexpected error retrieving release: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
        }

        public async Task<GitHubApiResult<IReadOnlyList<GitHubRelease>>> GetReleasesAsync(string owner, string repo, CancellationToken token)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            RetroLogger.Log($"Querying all releases for {owner}/{repo}...");

            try
            {
                using (var request = CreateRequest(HttpMethod.Get, url))
                using (var response = await _httpClient.SendAsync(request, token))
                {
                    var rateLimit = ParseRateLimit(response.Headers);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new GitHubApiResult<IReadOnlyList<GitHubRelease>>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = "Repository not found.",
                            RateLimit = rateLimit
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden && rateLimit.Remaining == 0)
                    {
                        string resetMsg = $"Rate limit exceeded. Resets at {rateLimit.ResetTime}.";
                        return new GitHubApiResult<IReadOnlyList<GitHubRelease>>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = resetMsg,
                            RateLimit = rateLimit
                        };
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return new GitHubApiResult<IReadOnlyList<GitHubRelease>>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = $"GitHub API returned error status: {response.StatusCode}",
                            RateLimit = rateLimit
                        };
                    }

                    string json = await response.Content.ReadAsStringAsync(token);
                    var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(json);

                    if (releases == null)
                    {
                        return new GitHubApiResult<IReadOnlyList<GitHubRelease>>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = "Failed to deserialize JSON releases list.",
                            RateLimit = rateLimit
                        };
                    }

                    return new GitHubApiResult<IReadOnlyList<GitHubRelease>>
                    {
                        Success = true,
                        Data = releases.AsReadOnly(),
                        StatusCode = response.StatusCode,
                        RateLimit = rateLimit
                    };
                }
            }
            catch (JsonException ex)
            {
                string msg = $"Invalid JSON payload in releases response: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<IReadOnlyList<GitHubRelease>> { Success = false, ErrorMessage = msg };
            }
            catch (HttpRequestException ex)
            {
                string msg = $"Network/HTTP request failed: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<IReadOnlyList<GitHubRelease>> { Success = false, ErrorMessage = msg };
            }
            catch (OperationCanceledException)
            {
                RetroLogger.Log("GetReleasesAsync operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Unexpected error retrieving releases list: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<IReadOnlyList<GitHubRelease>> { Success = false, ErrorMessage = msg };
            }
        }

        public async Task<GitHubApiResult<GitHubRelease>> GetReleaseByTagAsync(string owner, string repo, string tag, CancellationToken token)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/tags/{tag}";
            RetroLogger.Log($"Querying release for {owner}/{repo} with tag {tag}...");

            try
            {
                using (var request = CreateRequest(HttpMethod.Get, url))
                using (var response = await _httpClient.SendAsync(request, token))
                {
                    var rateLimit = ParseRateLimit(response.Headers);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = $"Release with tag '{tag}' not found.",
                            RateLimit = rateLimit
                        };
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden && rateLimit.Remaining == 0)
                    {
                        string resetMsg = $"Rate limit exceeded. Resets at {rateLimit.ResetTime}.";
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = resetMsg,
                            RateLimit = rateLimit
                        };
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = $"GitHub API returned error status: {response.StatusCode}",
                            RateLimit = rateLimit
                        };
                    }

                    string json = await response.Content.ReadAsStringAsync(token);
                    var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                    if (release == null)
                    {
                        return new GitHubApiResult<GitHubRelease>
                        {
                            Success = false,
                            StatusCode = response.StatusCode,
                            ErrorMessage = "Failed to deserialize JSON response.",
                            RateLimit = rateLimit
                        };
                    }

                    return new GitHubApiResult<GitHubRelease>
                    {
                        Success = true,
                        Data = release,
                        StatusCode = response.StatusCode,
                        RateLimit = rateLimit
                    };
                }
            }
            catch (JsonException ex)
            {
                string msg = $"Invalid JSON payload in release tag response: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
            catch (HttpRequestException ex)
            {
                string msg = $"Network/HTTP request failed: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
            catch (OperationCanceledException)
            {
                RetroLogger.Log("GetReleaseByTagAsync operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Unexpected error retrieving release by tag: {ex.Message}";
                RetroLogger.Log(msg, "ERROR");
                return new GitHubApiResult<GitHubRelease> { Success = false, ErrorMessage = msg };
            }
        }

        private static GitHubRateLimitInfo ParseRateLimit(HttpResponseHeaders headers)
        {
            var info = new GitHubRateLimitInfo();
            if (headers.TryGetValues("X-RateLimit-Limit", out var limits) && int.TryParse(limits.FirstOrDefault(), out var limitVal))
            {
                info.Limit = limitVal;
            }
            if (headers.TryGetValues("X-RateLimit-Remaining", out var remainings) && int.TryParse(remainings.FirstOrDefault(), out var remainingVal))
            {
                info.Remaining = remainingVal;
            }
            if (headers.TryGetValues("X-RateLimit-Reset", out var resets) && long.TryParse(resets.FirstOrDefault(), out var resetVal))
            {
                info.ResetTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(resetVal).ToLocalTime();
            }
            return info;
        }
    }
}
