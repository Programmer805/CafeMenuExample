using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface ICacheService
    {
        // Temel cache operasyonları
        T Get<T>(string key) where T : class;
        Task<T> GetAsync<T>(string key) where T : class;
        
        void Set<T>(string key, T value) where T : class;
        void Set<T>(string key, T value, TimeSpan expiration) where T : class;
        Task SetAsync<T>(string key, T value) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
        
        bool Remove(string key);
        Task<bool> RemoveAsync(string key);
        
        bool Exists(string key);
        Task<bool> ExistsAsync(string key);
        
        void Clear();
        Task ClearAsync();
        
        // Pattern-based operations
        void RemoveByPattern(string pattern);
        Task RemoveByPatternAsync(string pattern);
        
        IEnumerable<string> GetKeys(string pattern = "*");
        Task<IEnumerable<string>> GetKeysAsync(string pattern = "*");
        
        // Cache statistics
        long GetCacheSize();
        Task<long> GetCacheSizeAsync();
        
        Dictionary<string, object> GetCacheStatistics();
        Task<Dictionary<string, object>> GetCacheStatisticsAsync();
    }
}