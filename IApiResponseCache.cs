using System;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public enum CacheFreshness
    {
        Fresh,
        Stale,
        Missing,
        Invalid
    }

    public class ApiCacheKey
    {
        public string RequestUri { get; }
        public ReleaseProviderType Provider { get; }

        public ApiCacheKey(string requestUri, ReleaseProviderType provider)
        {
            RequestUri = requestUri;
            Provider = provider;
        }

        public string GetHashKey()
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes($"{Provider}_{RequestUri}");
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Replace("=", "");
            }
        }
    }

    public class ApiCacheEntry
    {
        public string RequestUri { get; set; } = "";
        public ReleaseProviderType Provider { get; set; }
        public string ResponseBody { get; set; } = "";
        public string? ETag { get; set; }
        public string? LastModified { get; set; }
        public DateTime StoredAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
        public int SchemaVersion { get; set; } = 1;
    }

    public class CacheReadResult
    {
        public CacheFreshness Status { get; set; }
        public ApiCacheEntry? Entry { get; set; }
    }

    public interface IApiResponseCache
    {
        Task<CacheReadResult> GetAsync(ApiCacheKey key);
        Task SetAsync(ApiCacheKey key, ApiCacheEntry entry);
        Task ClearAsync(ReleaseProviderType? provider = null);
    }
}
