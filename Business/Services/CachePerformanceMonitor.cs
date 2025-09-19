using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    /// <summary>
    /// 5 milyon kayıt için cache performansını izler ve optimize eder.
    /// Memory kullanımını kontrol eder ve gerektiğinde otomatik temizlik yapar.
    /// </summary>
    public class CachePerformanceMonitor
    {
        private readonly ICacheService _cacheService;
        private readonly PerformanceCounter _memoryCounter;
        private readonly Dictionary<string, CacheMetrics> _metrics;
        private readonly object _metricsLock = new object();
        
        // Performans eşikleri
        private const long MAX_MEMORY_USAGE_MB = 512; // 512 MB cache limiti
        private const double MIN_HIT_RATIO = 0.7; // Minimum %70 hit ratio
        private const int MAX_CACHE_ITEMS = 100000; // Maximum 100.000 cache item
        
        public CachePerformanceMonitor(ICacheService cacheService)
        {
            _cacheService = cacheService;
            _metrics = new Dictionary<string, CacheMetrics>();
            
            try
            {
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch
            {
                // Performance counter kullanılamıyorsa null bırak
                _memoryCounter = null;
            }
            
            // Performans izleme task'ini başlat
            Task.Run(MonitorPerformanceAsync);
        }

        /// <summary>
        /// Cache performansını sürekli izler
        /// </summary>
        private async Task MonitorPerformanceAsync()
        {
            while (true)
            {
                try
                {
                    await CheckMemoryUsageAsync();
                    await CheckCacheEfficiencyAsync();
                    await CleanupIfNeededAsync();
                    
                    // Her 30 saniyede bir kontrol et
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                catch (Exception)
                {
                    // Log error in real implementation
                    await Task.Delay(TimeSpan.FromMinutes(1)); // Hata durumunda 1 dakika bekle
                }
            }
        }

        /// <summary>
        /// Memory kullanımını kontrol eder
        /// </summary>
        private async Task CheckMemoryUsageAsync()
        {
            var cacheStats = await _cacheService.GetCacheStatisticsAsync();
            var cacheSize = (long)cacheStats["TotalItems"];
            
            if (cacheSize > MAX_CACHE_ITEMS)
            {
                await PerformEmergencyCleanupAsync();
            }
            
            // Memory counter varsa sistem memory'sini de kontrol et
            if (_memoryCounter != null)
            {
                var availableMemory = _memoryCounter.NextValue();
                if (availableMemory < 100) // 100 MB'dan az memory kaldıysa
                {
                    await PerformEmergencyCleanupAsync();
                }
            }
        }

        /// <summary>
        /// Cache efficiency'sini kontrol eder
        /// </summary>
        private async Task CheckCacheEfficiencyAsync()
        {
            var cacheStats = await _cacheService.GetCacheStatisticsAsync();
            var hitRatio = (double)cacheStats["CacheHitRatio"];
            
            if (hitRatio < MIN_HIT_RATIO)
            {
                // Hit ratio düşükse cache stratejisini gözden geçir
                await OptimizeCacheStrategyAsync();
            }
        }

        /// <summary>
        /// Gerektiğinde cache temizliği yapar
        /// </summary>
        private async Task CleanupIfNeededAsync()
        {
            var cacheStats = await _cacheService.GetCacheStatisticsAsync();
            var expiredItems = (int)cacheStats["ExpiredItems"];
            
            if (expiredItems > 1000) // 1000'den fazla expired item varsa temizle
            {
                await CleanExpiredItemsAsync();
            }
        }

        /// <summary>
        /// Emergency cache temizliği yapar
        /// </summary>
        private async Task PerformEmergencyCleanupAsync()
        {
            // Önce expired item'ları temizle
            await CleanExpiredItemsAsync();
            
            // Hala fazla item varsa en az kullanılan chunk'ları temizle
            await RemoveLeastUsedChunksAsync();
        }

        /// <summary>
        /// Expired cache item'larını temizler
        /// </summary>
        private async Task CleanExpiredItemsAsync()
        {
            // Memory cache service'de bu işlem otomatik yapılıyor
            // Burada ekstra temizlik logic'i eklenebilir
            await Task.CompletedTask;
        }

        /// <summary>
        /// En az kullanılan chunk'ları temizler
        /// </summary>
        private async Task RemoveLeastUsedChunksAsync()
        {
            // Chunk key'lerini al
            var chunkKeys = await _cacheService.GetKeysAsync("product_chunk:*");
            
            // Metrics'e göre en az kullanılanları bul
            var leastUsedKeys = new List<string>();
            
            lock (_metricsLock)
            {
                foreach (var key in chunkKeys)
                {
                    if (_metrics.ContainsKey(key) && _metrics[key].AccessCount < 5)
                    {
                        leastUsedKeys.Add(key);
                    }
                }
            }
            
            // En az kullanılan chunk'ları temizle
            foreach (var key in leastUsedKeys.Take(10)) // En fazla 10 chunk temizle
            {
                await _cacheService.RemoveAsync(key);
            }
        }

        /// <summary>
        /// Cache stratejisini optimize eder
        /// </summary>
        private async Task OptimizeCacheStrategyAsync()
        {
            // Hit ratio düşükse:
            // 1. Daha sık kullanılan kategorileri öncelikle cache'le
            // 2. Chunk size'ını ayarla
            // 3. Cache expiration time'ını optimize et
            
            await Task.CompletedTask; // Placeholder
        }

        /// <summary>
        /// Cache access'ini metric olarak kaydeder
        /// </summary>
        public void RecordCacheAccess(string key, bool isHit)
        {
            lock (_metricsLock)
            {
                if (!_metrics.ContainsKey(key))
                {
                    _metrics[key] = new CacheMetrics();
                }
                
                _metrics[key].AccessCount++;
                _metrics[key].LastAccessTime = DateTime.UtcNow;
                
                if (isHit)
                {
                    _metrics[key].HitCount++;
                }
            }
        }

        /// <summary>
        /// Cache performans raporunu döndürür
        /// </summary>
        public async Task<CachePerformanceReport> GetPerformanceReportAsync()
        {
            var cacheStats = await _cacheService.GetCacheStatisticsAsync();
            
            var report = new CachePerformanceReport
            {
                TotalCacheItems = (long)cacheStats["TotalItems"],
                ActiveItems = (long)cacheStats["ActiveItems"],
                ExpiredItems = (long)cacheStats["ExpiredItems"],
                OverallHitRatio = (double)cacheStats["CacheHitRatio"],
                MemoryUsageMB = CalculateMemoryUsage(),
                RecommendedActions = GetRecommendedActions(cacheStats)
            };
            
            return report;
        }

        /// <summary>
        /// Memory kullanımını hesaplar
        /// </summary>
        private double CalculateMemoryUsage()
        {
            // Basit tahmin: Her cache item ortalama 1KB
            var cacheSize = _cacheService.GetCacheSize();
            return (double)cacheSize / 1024; // MB cinsinden
        }

        /// <summary>
        /// Cache durumuna göre önerilen aksiyonları döndürür
        /// </summary>
        private List<string> GetRecommendedActions(Dictionary<string, object> cacheStats)
        {
            var actions = new List<string>();
            
            var totalItems = (long)cacheStats["TotalItems"];
            var hitRatio = (double)cacheStats["CacheHitRatio"];
            
            if (totalItems > MAX_CACHE_ITEMS)
            {
                actions.Add("Cache item sayısı çok yüksek. Temizlik önerilir.");
            }
            
            if (hitRatio < MIN_HIT_RATIO)
            {
                actions.Add("Cache hit ratio düşük. Strateji optimizasyonu gerekli.");
            }
            
            var memoryUsage = CalculateMemoryUsage();
            if (memoryUsage > MAX_MEMORY_USAGE_MB)
            {
                actions.Add("Memory kullanımı yüksek. Chunk size azaltılması önerilir.");
            }
            
            if (actions.Count == 0)
            {
                actions.Add("Cache performansı optimal durumda.");
            }
            
            return actions;
        }

        /// <summary>
        /// Cache metrics sınıfı
        /// </summary>
        private class CacheMetrics
        {
            public int AccessCount { get; set; }
            public int HitCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            
            public double HitRatio => AccessCount > 0 ? (double)HitCount / AccessCount : 0;
        }

        /// <summary>
        /// Cache performans raporu
        /// </summary>
        public class CachePerformanceReport
        {
            public long TotalCacheItems { get; set; }
            public long ActiveItems { get; set; }
            public long ExpiredItems { get; set; }
            public double OverallHitRatio { get; set; }
            public double MemoryUsageMB { get; set; }
            public List<string> RecommendedActions { get; set; }
        }
    }
}