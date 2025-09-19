using Domain.Interfaces.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private readonly object _lockObject = new object();

        public MemoryCacheService()
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
            
            // Start a background task to clean expired items
            Task.Run(CleanupExpiredItems);
        }

        public T Get<T>(string key) where T : class
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return null;
                }
                
                cacheItem.LastAccessed = DateTime.UtcNow;
                return cacheItem.Value as T;
            }
            
            return null;
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var result = await Task.FromResult(Get<T>(key));
            System.Diagnostics.Debug.WriteLine($"[CACHE] GetAsync - Key: {key}, Result: {(result != null ? "HIT" : "MISS")}");
            return result;
        }

        public void Set<T>(string key, T value) where T : class
        {
            Set(key, value, TimeSpan.FromHours(1)); // Default 1 hour expiration
        }

        public void Set<T>(string key, T value, TimeSpan expiration) where T : class
        {
            var cacheItem = new CacheItem
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };
            
            _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        }

        public async Task SetAsync<T>(string key, T value) where T : class
        {
            await Task.Run(() => Set(key, value));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            System.Diagnostics.Debug.WriteLine($"[CACHE] SetAsync - Key: {key}, Type: {typeof(T).Name}");
            await Task.Run(() => Set(key, value, expiration));
        }

        public bool Remove(string key)
        {
            return _cache.TryRemove(key, out _);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await Task.FromResult(Remove(key));
        }

        public bool Exists(string key)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return false;
                }
                return true;
            }
            return false;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await Task.FromResult(Exists(key));
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public async Task ClearAsync()
        {
            await Task.Run(() => Clear());
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CACHE] RemoveByPattern called with pattern: {pattern}");
                System.Diagnostics.Debug.WriteLine($"[CACHE] Total cache keys before removal: {_cache.Count}");
                
                // Convert simple wildcard pattern to regex
                var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                
                System.Diagnostics.Debug.WriteLine($"[CACHE] Regex pattern: {regexPattern}");

                var keysToRemove = _cache.Keys.Where(key => regex.IsMatch(key)).ToList();
                
                System.Diagnostics.Debug.WriteLine($"[CACHE] Found {keysToRemove.Count} keys to remove");
                foreach (var key in keysToRemove)
                {
                    System.Diagnostics.Debug.WriteLine($"[CACHE] Removing key: {key}");
                    _cache.TryRemove(key, out _);
                }
                
                System.Diagnostics.Debug.WriteLine($"[CACHE] Total cache keys after removal: {_cache.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CACHE ERROR] Regex failed: {ex.Message}");
                // If regex fails, fall back to simple contains check
                var keysToRemove = _cache.Keys.Where(key => key.Contains(pattern.Replace("*", ""))).ToList();
                
                System.Diagnostics.Debug.WriteLine($"[CACHE FALLBACK] Found {keysToRemove.Count} keys to remove with contains");
                foreach (var key in keysToRemove)
                {
                    System.Diagnostics.Debug.WriteLine($"[CACHE FALLBACK] Removing key: {key}");
                    _cache.TryRemove(key, out _);
                }
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            await Task.Run(() => RemoveByPattern(pattern));
        }

        public IEnumerable<string> GetKeys(string pattern = "*")
        {
            if (pattern == "*")
                return _cache.Keys.ToList();

            try
            {
                var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                return _cache.Keys.Where(key => regex.IsMatch(key)).ToList();
            }
            catch (Exception)
            {
                return _cache.Keys.Where(key => key.Contains(pattern.Replace("*", ""))).ToList();
            }
        }

        public async Task<IEnumerable<string>> GetKeysAsync(string pattern = "*")
        {
            return await Task.FromResult(GetKeys(pattern));
        }

        public long GetCacheSize()
        {
            return _cache.Count;
        }

        public async Task<long> GetCacheSizeAsync()
        {
            return await Task.FromResult(GetCacheSize());
        }

        public Dictionary<string, object> GetCacheStatistics()
        {
            var now = DateTime.UtcNow;
            var totalItems = _cache.Count;
            var expiredItems = _cache.Values.Count(item => item.IsExpired);
            var activeItems = totalItems - expiredItems;
            
            var oldestItem = _cache.Values.Where(item => !item.IsExpired)
                                         .OrderBy(item => item.CreatedAt)
                                         .FirstOrDefault();
                                         
            var newestItem = _cache.Values.Where(item => !item.IsExpired)
                                         .OrderByDescending(item => item.CreatedAt)
                                         .FirstOrDefault();

            return new Dictionary<string, object>
            {
                ["TotalItems"] = totalItems,
                ["ActiveItems"] = activeItems,
                ["ExpiredItems"] = expiredItems,
                ["OldestItemAge"] = oldestItem != null ? now.Subtract(oldestItem.CreatedAt).TotalMinutes : 0,
                ["NewestItemAge"] = newestItem != null ? now.Subtract(newestItem.CreatedAt).TotalMinutes : 0,
                ["CacheHitRatio"] = CalculateCacheHitRatio()
            };
        }

        public async Task<Dictionary<string, object>> GetCacheStatisticsAsync()
        {
            return await Task.FromResult(GetCacheStatistics());
        }

        private double CalculateCacheHitRatio()
        {
            // This is a simplified implementation
            // In a real cache, you would track hits and misses
            return 0.85; // Placeholder value
        }

        private async void CleanupExpiredItems()
        {
            while (true)
            {
                try
                {
                    var expiredKeys = _cache
                        .Where(kvp => kvp.Value.IsExpired)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        _cache.TryRemove(key, out _);
                    }

                    // Run cleanup every 5 minutes
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
                catch
                {
                    // Log the error in a real implementation
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }

        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime ExpiresAt { get; set; }
            
            public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        }
    }
}