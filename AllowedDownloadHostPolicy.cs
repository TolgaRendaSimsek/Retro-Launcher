using System;
using System.Collections.Generic;

namespace RetroLauncher
{
    public static class AllowedDownloadHostPolicy
    {
        private static readonly HashSet<string> TrustedHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "github.com",
            "api.github.com",
            "objects.githubusercontent.com",
            "github-releases.githubusercontent.com",
            "rpcs3.net",
            "pcsx2.net",
            "stenzek.github.io",
            "gitlab.com",
            "codeberg.org"
        };

        public static bool IsHostAllowed(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return TrustedHosts.Contains(uri.Host);
            }
            return false;
        }
    }
}
