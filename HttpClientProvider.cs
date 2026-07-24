using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private static HttpClientProvider? _instance;
        public static HttpClientProvider Instance => _instance ??= new HttpClientProvider();

        private readonly ConcurrentDictionary<string, HttpClient> _clients = new();
        private readonly IApplicationSettingsService _settings = ApplicationSettingsService.Instance;

        public HttpClient GetClient(string name)
        {
            return _clients.GetOrAdd(name, CreateClient);
        }

        public void ResetClients()
        {
            // Clear current cache to force recreation with new settings
            _clients.Clear();
            RetroLogger.Log("HTTP Clients cleared and reset according to new configuration.", "INFO");
        }

        private HttpClient CreateClient(string name)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var net = _settings.Network;
            if (net.ProxyMode == "NoProxy")
            {
                handler.UseProxy = false;
                handler.Proxy = null;
            }
            else if (net.ProxyMode == "SystemDefault")
            {
                handler.UseProxy = true;
                handler.Proxy = WebRequest.DefaultWebProxy;
            }
            else if (net.ProxyMode == "ManualProxy" && !string.IsNullOrEmpty(net.ProxyUri))
            {
                try
                {
                    var proxy = new WebProxy(net.ProxyUri)
                    {
                        BypassProxyOnLocal = net.BypassLocalAddresses
                    };

                    if (net.BypassList != null && net.BypassList.Any())
                    {
                        proxy.BypassList = net.BypassList.ToArray();
                    }

                    if (!string.IsNullOrEmpty(net.ProxyUsername))
                    {
                        proxy.Credentials = new NetworkCredential(net.ProxyUsername, net.GetProxyPassword());
                    }

                    handler.Proxy = proxy;
                    handler.UseProxy = true;
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to configure manual proxy '{net.ProxyUri}': {ex.Message}. Falling back to default proxy.", "ERROR");
                    handler.UseProxy = true;
                    handler.Proxy = WebRequest.DefaultWebProxy;
                }
            }

            var client = new HttpClient(handler);

            if (name == "GitHubApi")
            {
                client.BaseAddress = new Uri(_settings.GitHub.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(_settings.GitHub.RequestTimeoutSeconds);
                
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher/1.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

                string? token = _settings.GitHub.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            else if (name == "PackageDownloads")
            {
                client.Timeout = TimeSpan.FromSeconds(_settings.Network.RequestTimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher/1.0");
            }
            else
            {
                client.Timeout = TimeSpan.FromSeconds(_settings.Network.RequestTimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher/1.0");
            }

            return client;
        }

        public async Task<bool> TestConnectionAsync(string endpoint, CancellationToken cancellationToken)
        {
            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(10)); // Force 10 second timeout for connection tests

                    // Always test using a fresh client handler to verify current proxy status
                    var tempClient = CreateClient("PackageDownloads");
                    using (var response = await tempClient.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                    {
                        return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden;
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Connection test failed for '{endpoint}': {ex.Message}", "WARNING");
                return false;
            }
        }
    }
}
