using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public class FileApiResponseCache : IApiResponseCache
    {
        private readonly string _cacheDir;
        private const int CurrentSchemaVersion = 1;
        private static readonly ConcurrentDictionary<string, object> FileLocks = new();

        public FileApiResponseCache(string? cacheDir = null)
        {
            _cacheDir = cacheDir ?? Path.Combine(AppContext.BaseDirectory, "Cache", "Metadata");
        }

        private object GetLock(string key)
        {
            return FileLocks.GetOrAdd(key, _ => new object());
        }

        public Task<CacheReadResult> GetAsync(ApiCacheKey key)
        {
            string hash = key.GetHashKey();
            string filePath = Path.Combine(_cacheDir, $"{hash}.json");
            object lockObj = GetLock(hash);

            lock (lockObj)
            {
                if (!File.Exists(filePath))
                {
                    return Task.FromResult(new CacheReadResult { Status = CacheFreshness.Missing });
                }

                try
                {
                    string json = File.ReadAllText(filePath);
                    var entry = JsonSerializer.Deserialize<ApiCacheEntry>(json);

                    if (entry == null || entry.SchemaVersion != CurrentSchemaVersion)
                    {
                        try { File.Delete(filePath); } catch { }
                        return Task.FromResult(new CacheReadResult { Status = CacheFreshness.Invalid });
                    }

                    bool isExpired = DateTime.UtcNow > entry.ExpiresAt;
                    var freshness = isExpired ? CacheFreshness.Stale : CacheFreshness.Fresh;

                    return Task.FromResult(new CacheReadResult
                    {
                        Status = freshness,
                        Entry = entry
                    });
                }
                catch
                {
                    try { File.Delete(filePath); } catch { }
                    return Task.FromResult(new CacheReadResult { Status = CacheFreshness.Invalid });
                }
            }
        }

        public Task SetAsync(ApiCacheKey key, ApiCacheEntry entry)
        {
            string hash = key.GetHashKey();
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }

            string filePath = Path.Combine(_cacheDir, $"{hash}.json");
            string tempPath = Path.Combine(_cacheDir, $"{hash}.tmp");
            object lockObj = GetLock(hash);

            lock (lockObj)
            {
                try
                {
                    entry.SchemaVersion = CurrentSchemaVersion;
                    entry.RequestUri = key.RequestUri;
                    entry.Provider = key.Provider;

                    string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(tempPath, json);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    File.Move(tempPath, filePath);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to write cache entry atomically: {ex.Message}", "ERROR");
                    try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                }
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync(ReleaseProviderType? provider = null)
        {
            if (!Directory.Exists(_cacheDir)) return Task.CompletedTask;

            try
            {
                foreach (string file in Directory.GetFiles(_cacheDir, "*.json"))
                {
                    string hash = Path.GetFileNameWithoutExtension(file);
                    object lockObj = GetLock(hash);

                    lock (lockObj)
                    {
                        try
                        {
                            if (provider.HasValue)
                            {
                                string json = File.ReadAllText(file);
                                var entry = JsonSerializer.Deserialize<ApiCacheEntry>(json);
                                if (entry != null && entry.Provider == provider.Value)
                                {
                                    File.Delete(file);
                                }
                            }
                            else
                            {
                                File.Delete(file);
                            }
                        }
                        catch
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Failed to clear metadata cache: {ex.Message}", "ERROR");
            }

            return Task.CompletedTask;
        }
    }
}
