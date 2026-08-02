using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Infrastructure.Http
{
    public static class SafeRedirectHandler
    {
        public static async Task<HttpResponseMessage> SendWithRedirectsAsync(
            HttpClient client,
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            int maxRedirects = 5)
        {
            var currentRequest = request;
            int redirectCount = 0;

            while (true)
            {
                var response = await client.SendAsync(currentRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther ||
                    response.StatusCode == (HttpStatusCode)307 ||
                    response.StatusCode == (HttpStatusCode)308)
                {
                    redirectCount++;
                    if (redirectCount > maxRedirects)
                    {
                        response.Dispose();
                        throw new HttpRequestException("Too many redirects.");
                    }

                    var redirectUri = response.Headers.Location;
                    if (redirectUri == null)
                    {
                        return response;
                    }

                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(currentRequest.RequestUri!, redirectUri);
                    }

                    // Check hosts allowlist
                    if (!AllowedDownloadHostPolicy.IsHostAllowed(redirectUri.AbsoluteUri))
                    {
                        response.Dispose();
                        throw new HttpRequestException($"Redirect block: Host '{redirectUri.Host}' is not in the trusted host allowlist.");
                    }

                    // Enforce HTTPS policy and reject HTTP downgrade
                    if (currentRequest.RequestUri!.Scheme == "https" && redirectUri.Scheme != "https")
                    {
                        response.Dispose();
                        throw new HttpRequestException("Redirect block: Downgrade from HTTPS to HTTP is not allowed.");
                    }

                    // Prepare redirect request
                    var newRequest = new HttpRequestMessage(currentRequest.Method, redirectUri);
                    
                    // Copy headers
                    foreach (var header in currentRequest.Headers)
                    {
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        {
                            // Strip token on cross-host redirect
                            if (currentRequest.RequestUri!.Host.Equals(redirectUri.Host, StringComparison.OrdinalIgnoreCase))
                            {
                                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                        }
                        else
                        {
                            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    response.Dispose();
                    currentRequest = newRequest;
                }
                else
                {
                    return response;
                }
            }
        }
    }
}
