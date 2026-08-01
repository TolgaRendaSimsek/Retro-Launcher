using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public static class GitHubReleaseProviderTests
    {
        public static void RunTests()
        {
            RetroLogger.Log("Starting GitHubReleaseProvider, Cache & Rate Coordinator Unit Tests...");

            TestUnsafeCacheKeys();
            TestAtomicReplacementAndCorruption();
            TestMockedHttpFlowsAsync().GetAwaiter().GetResult();
            TestMockedGitLabAndCodebergFlowsAsync().GetAwaiter().GetResult();
            TestGraphQLProviderAsync().GetAwaiter().GetResult();
            TestGitHubApiConnectionDiagnosticsAsync().GetAwaiter().GetResult();

            RetroLogger.Log("All GitHubReleaseProvider, Cache & Rate Coordinator Unit Tests completed successfully!");
        }

        private static async Task TestGitHubApiConnectionDiagnosticsAsync()
        {
            RetroLogger.Log("--- Starting Real GitHub API Connection Diagnostics ---");

            var defProvider = new JsonEmulatorPackageDefinitionProvider();
            var duckDef = defProvider.GetById("duckstation");
            string duckOwner = duckDef?.GitHubOwner ?? "stenzek";
            string duckRepo = duckDef?.GitHubRepository ?? "duckstation";

            var reposToTest = new List<(string Owner, string Repo)>
            {
                ("PCSX2", "pcsx2"),
                ("RPCS3", "rpcs3"),
                (duckOwner, duckRepo)
            };

            var client = GitHubReleaseClient.Instance;

            foreach (var (owner, repo) in reposToTest)
            {
                RetroLogger.Log($"Testing GitHub API connection for repository '{owner}/{repo}'...");
                try
                {
                    var res = await client.GetLatestReleaseAsync(owner, repo, etag: null, CancellationToken.None);
                    if (res.Success && res.Data != null)
                    {
                        RetroLogger.Log($"[GitHub API Diagnostic Success] '{owner}/{repo}' resolved latest tag: '{res.Data.TagName}', release name: '{res.Data.Name}', assets count: {res.Data.Assets.Count}");
                    }
                    else
                    {
                        RetroLogger.Log($"[GitHub API Diagnostic Result] '{owner}/{repo}' returned Success=false. Status: {res.StatusCode}. ErrorMessage: {res.ErrorMessage}", "WARNING");
                    }
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"[GitHub API Diagnostic Exception] Error querying '{owner}/{repo}': {ex.Message}", "ERROR");
                }
            }

            RetroLogger.Log("--- Finished Real GitHub API Connection Diagnostics ---");
        }

        private static void TestUnsafeCacheKeys()
        {
            var key1 = new ApiCacheKey("../escaped/path", ReleaseProviderType.GitHub);
            string hash1 = key1.GetHashKey();
            Assert(!hash1.Contains("..") && !hash1.Contains("/") && !hash1.Contains("\\"), "Cache keys must block path traversal characters.");
        }

        private static void TestAtomicReplacementAndCorruption()
        {
            string testDir = Path.Combine(AppContext.BaseDirectory, "TestCacheTemp");
            if (Directory.Exists(testDir)) Directory.Delete(testDir, true);

            var cache = new FileApiResponseCache(testDir);
            var key = new ApiCacheKey("https://api.github.com/repos/test/test/releases", ReleaseProviderType.GitHub);

            var getRes1 = cache.GetAsync(key).GetAwaiter().GetResult();
            Assert(getRes1.Status == CacheFreshness.Missing, "Cache must report Missing for nonexistent keys.");

            var entry = new ApiCacheEntry
            {
                ResponseBody = "{\"status\":\"ok\"}",
                ETag = "\"w/1234\"",
                LastModified = "Wed, 21 Oct 2015 07:28:00 GMT",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };
            cache.SetAsync(key, entry).GetAwaiter().GetResult();

            var getRes2 = cache.GetAsync(key).GetAwaiter().GetResult();
            Assert(getRes2.Status == CacheFreshness.Fresh, "Cache must report Fresh when inside expiration duration.");
            Assert(getRes2.Entry?.ResponseBody == "{\"status\":\"ok\"}", "Cache must return exact written body.");

            entry.ExpiresAt = DateTime.UtcNow.AddMinutes(-5);
            cache.SetAsync(key, entry).GetAwaiter().GetResult();
            var getRes3 = cache.GetAsync(key).GetAwaiter().GetResult();
            Assert(getRes3.Status == CacheFreshness.Stale, "Cache must report Stale after expiration.");

            string hash = key.GetHashKey();
            string filePath = Path.Combine(testDir, $"{hash}.json");
            File.WriteAllText(filePath, "invalid-corrupted-json-content");

            var getRes4 = cache.GetAsync(key).GetAwaiter().GetResult();
            Assert(getRes4.Status == CacheFreshness.Invalid, "Cache must report Invalid when parsing fails.");
            Assert(!File.Exists(filePath), "Corrupted cache files must be deleted safely.");

            if (Directory.Exists(testDir)) Directory.Delete(testDir, true);
        }

        private static async Task TestMockedHttpFlowsAsync()
        {
            var settings = new MockApplicationSettingsService();
            var coordinator = new RateLimitCoordinator { DelayProvider = new MockAsyncDelay() };
            var cacheDir = Path.Combine(AppContext.BaseDirectory, "TestCacheHttp");
            if (Directory.Exists(cacheDir)) Directory.Delete(cacheDir, true);
            var cache = new FileApiResponseCache(cacheDir);

            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com") };
            var mockClientProvider = new MockHttpClientProvider(client);

            var provider = new GitHubReleaseProvider(mockClientProvider, cache, coordinator, settings);
            var query = new ReleaseQuery { Owner = "test", Repository = "test" };

            // Scenario 1: Successful 200 OK
            handler.ResponseFunc = req =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent("{\"tag_name\":\"v1.0.0\",\"name\":\"Release 1.0.0\"}");
                resp.Headers.Add("ETag", "\"tag123\"");
                resp.Headers.Add("X-RateLimit-Limit", "60");
                resp.Headers.Add("X-RateLimit-Remaining", "50");
                resp.Headers.Add("X-RateLimit-Reset", "1999999999");
                return resp;
            };

            var res1 = await provider.GetLatestReleaseAsync(query, CancellationToken.None);
            Assert(res1.Success && res1.Data != null, "Request should succeed with 200 OK.");
            Assert(res1.Data?.Tag == "v1.0.0", "Mapped release tag should match.");
            Assert(coordinator.GetState().Remaining == 50, "Rate coordinator state must update from response headers.");

            // Scenario 2: 304 Not Modified
            int requestCount = 0;
            handler.ResponseFunc = req =>
            {
                requestCount++;
                var resp = new HttpResponseMessage(HttpStatusCode.NotModified);
                resp.Headers.Add("X-RateLimit-Limit", "60");
                resp.Headers.Add("X-RateLimit-Remaining", "49");
                return resp;
            };

            var res2 = await provider.GetLatestReleaseAsync(query, CancellationToken.None);
            Assert(res2.Success && res2.Data != null, "Request should succeed on 304.");
            Assert(res2.IsValidatedFromCache, "304 result must be flagged as validated from cache.");
            Assert(res2.Data?.Tag == "v1.0.0", "Mapped release tag should match cache payload.");

            // Scenario 3: 429 followed by transient success
            requestCount = 0;
            handler.ResponseFunc = req =>
            {
                requestCount++;
                if (requestCount == 1)
                {
                    var resp = new HttpResponseMessage((HttpStatusCode)429);
                    resp.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
                    return resp;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new StringContent("{\"tag_name\":\"v1.1.0\",\"name\":\"Release 1.1.0\"}");
                    return resp;
                }
            };

            await cache.ClearAsync();

            var res3 = await provider.GetLatestReleaseAsync(query, CancellationToken.None);
            Assert(res3.Success && res3.Data != null, "Provider must retry on transient failures and succeed.");
            Assert(res3.Data?.Tag == "v1.1.0", "Tag name should match retry success payload.");
            Assert(requestCount == 2, "Should execute exactly 2 request attempts.");

            if (Directory.Exists(cacheDir)) Directory.Delete(cacheDir, true);
        }

        private static async Task TestMockedGitLabAndCodebergFlowsAsync()
        {
            var settings = new MockApplicationSettingsService();
            var coordinator = new RateLimitCoordinator { DelayProvider = new MockAsyncDelay() };
            var cache = new FileApiResponseCache(Path.Combine(AppContext.BaseDirectory, "TestCacheGitLab"));

            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler);
            var mockClientProvider = new MockHttpClientProvider(client);

            // Test GitLab
            var gitLabProvider = new GitLabReleaseProvider(mockClientProvider, cache, coordinator, settings);
            handler.ResponseFunc = req =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent("[{\"tag_name\":\"v2.0.0\",\"name\":\"GitLab Release\",\"description\":\"GitLab\",\"released_at\":\"2026-07-24T12:00:00Z\",\"assets\":{\"count\":1,\"links\":[{\"id\":123,\"name\":\"gitlab-asset.zip\",\"url\":\"https://gitlab.com/test/link\"}]}}]");
                return resp;
            };

            var glQuery = new ReleaseQuery { Owner = "gitlab_owner", Repository = "gitlab_repo" };
            var glRes = await gitLabProvider.GetReleasesAsync(glQuery, CancellationToken.None);
            Assert(glRes.Success && glRes.Data != null, "GitLab provider should fetch releases.");
            Assert(glRes.Data.First().Tag == "v2.0.0", "GitLab release tag should match.");

            // Test Codeberg
            var codebergProvider = new CodebergReleaseProvider(mockClientProvider, cache, coordinator, settings);
            handler.ResponseFunc = req =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent("[{\"tag_name\":\"v3.0.0\",\"name\":\"Codeberg Release\",\"body\":\"Codeberg\",\"draft\":false,\"prerelease\":false,\"published_at\":\"2026-07-24T12:00:00Z\",\"html_url\":\"https://codeberg.org/url\",\"assets\":[{\"id\":456,\"name\":\"codeberg-asset.zip\",\"browser_download_url\":\"https://codeberg.org/download\",\"size\":1024}]}]");
                return resp;
            };

            var cbQuery = new ReleaseQuery { Owner = "codeberg_owner", Repository = "codeberg_repo" };
            var cbRes = await codebergProvider.GetReleasesAsync(cbQuery, CancellationToken.None);
            Assert(cbRes.Success && cbRes.Data != null, "Codeberg provider should fetch releases.");
            Assert(cbRes.Data.First().Tag == "v3.0.0", "Codeberg release tag should match.");
            Assert(cbRes.Data.First().Assets.First().Size == 1024, "Codeberg asset size should match.");

            // Cleanup
            try { Directory.Delete(Path.Combine(AppContext.BaseDirectory, "TestCacheGitLab"), true); } catch { }
        }

        private static async Task TestGraphQLProviderAsync()
        {
            var settings = new MockApplicationSettingsService();
            var handler = new MockHttpMessageHandler();
            var client = new HttpClient(handler);
            var mockClientProvider = new MockHttpClientProvider(client);

            var gqlProvider = new GitHubGraphQLMetadataProvider(mockClientProvider, settings);

            // 1. Test when no token configured (should fail gracefully)
            var batchList = new List<(string, string)> { ("testOwner", "testRepo") };
            var gqlRes1 = await gqlProvider.FetchBatchMetadataAsync(batchList, CancellationToken.None);
            Assert(!gqlRes1.Success, "GraphQL provider must fail when token is not configured.");

            // 2. Configure a token and test successful query
            settings.GitHub.SetToken("dummy_token");
            handler.ResponseFunc = req =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent("{\"data\":{\"repo0\":{\"name\":\"testRepo\",\"description\":\"GQL Desc\",\"url\":\"https://github.com/testOwner/testRepo\",\"stargazerCount\":999,\"licenseInfo\":{\"spdxId\":\"MIT\"}}}}");
                return resp;
            };

            var gqlRes2 = await gqlProvider.FetchBatchMetadataAsync(batchList, CancellationToken.None);
            Assert(gqlRes2.Success && gqlRes2.Data != null, "GraphQL provider should fetch batch metadata successfully with token.");
            Assert(gqlRes2.Data["testOwner/testRepo"].Description == "GQL Desc", "GraphQL description should match.");
            Assert(gqlRes2.Data["testOwner/testRepo"].Stargazers == 999, "GraphQL stargazers count should match.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Test assertion failed: {message}");
            }
            RetroLogger.Log($"Test Case passed: {message}");
        }
    }

    public class MockApplicationSettingsService : IApplicationSettingsService
    {
        public NetworkSettings Network { get; } = new NetworkSettings { MaxRetryCount = 3 };
        public GitHubSettings GitHub { get; } = new GitHubSettings();
        public CacheSettings Cache { get; } = new CacheSettings { CacheDurationMinutes = 1 };
        public DownloadSettings Download { get; } = new DownloadSettings();
        public InstallationSettings Installation { get; } = new InstallationSettings();

        public void SaveSettings() { }
        public List<string> ValidateSettings() => new();
    }

    public class MockHttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _client;
        public MockHttpClientProvider(HttpClient client) { _client = client; }
        public HttpClient GetClient(string name) => _client;
    }

    public class MockAsyncDelay : IAsyncDelay
    {
        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFunc { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ResponseFunc == null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return Task.FromResult(ResponseFunc(request));
        }
    }
}
