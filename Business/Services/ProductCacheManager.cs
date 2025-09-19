using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    /// <summary>
    /// Bu sınıf 5 milyon ürün kaydı için optimize edilmiş cache stratejilerini içerir.
    /// Büyük veri setlerini parçalı olarak cache'ler ve memory kullanımını optimize eder.
    /// </summary>
    public class ProductCacheManager
    {
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;
        
        // Cache stratejileri için sabitler
        private const int CHUNK_SIZE = 1000; // Her chunk'ta 1000 ürün
        private const int MAX_CACHED_CHUNKS = 50; // Maksimum 50 chunk (50.000 ürün) bellekte
        private const string CHUNK_KEY_PREFIX = "product_chunk:";
        private const string CHUNK_INDEX_KEY = "product_chunk_index:";
        private const string POPULAR_PRODUCTS_KEY = "popular_products:";
        
        public ProductCacheManager(ICacheService cacheService, IProductService productService)
        {
            _cacheService = cacheService;
            _productService = productService;
        }

        /// <summary>
        /// Ürünleri chunk'lar halinde cache'e alır. Bu sayede 5 milyon kayıt parça parça işlenir.
        /// </summary>
        public async Task WarmupProductCacheAsync(int tenantId)
        {
            try
            {
                // İlk olarak en popüler/sık kullanılan ürünleri cache'le
                await CachePopularProductsAsync(tenantId);
                
                // Sonra kategoriye göre chunk'ları oluştur
                await CreateCategoryBasedChunksAsync(tenantId);
                
                // Index'leri oluştur
                await CreateChunkIndexAsync(tenantId);
            }
            catch (Exception ex)
            {
                // Log error in real implementation
                throw new InvalidOperationException($"Cache warmup failed for tenant {tenantId}: {ex.Message}");
            }
        }

        /// <summary>
        /// En sık erişilen ürünleri öncelikli olarak cache'ler
        /// </summary>
        private async Task CachePopularProductsAsync(int tenantId)
        {
            // Son eklenen ürünler (muhtemelen daha popüler)
            var recentProducts = await _productService.GetRecentProductsAsync(CHUNK_SIZE, tenantId);
            var cacheKey = $"{POPULAR_PRODUCTS_KEY}{tenantId}";
            
            await _cacheService.SetAsync(cacheKey, recentProducts, TimeSpan.FromHours(2));
        }

        /// <summary>
        /// Kategoriye göre ürünleri chunk'lar halinde organize eder
        /// </summary>
        private async Task CreateCategoryBasedChunksAsync(int tenantId)
        {
            // Kategorileri al
            var categoryService = GetCategoryService(); // Dependency injection ile alınacak
            var categories = await categoryService.GetAllAsync(tenantId);
            
            foreach (var category in categories)
            {
                await CacheCategoryProductsInChunksAsync(category.ID, tenantId);
            }
        }

        /// <summary>
        /// Belirli bir kategorinin ürünlerini chunk'lar halinde cache'ler
        /// </summary>
        private async Task CacheCategoryProductsInChunksAsync(int categoryId, int tenantId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId, tenantId);
            var productList = products.ToList();
            
            // Ürünleri chunk'lara böl
            for (int i = 0; i < productList.Count; i += CHUNK_SIZE)
            {
                var chunk = productList.Skip(i).Take(CHUNK_SIZE).ToList();
                var chunkKey = $"{CHUNK_KEY_PREFIX}{tenantId}:category:{categoryId}:chunk:{i / CHUNK_SIZE}";
                
                // 30 dakika cache süresi (sık erişilen veriler için)
                await _cacheService.SetAsync(chunkKey, chunk, TimeSpan.FromMinutes(30));
            }
        }

        /// <summary>
        /// Chunk'ların index'ini oluşturur. Bu sayede hangi chunk'ta hangi ürünlerin olduğu bilinir.
        /// </summary>
        private async Task CreateChunkIndexAsync(int tenantId)
        {
            var chunkIndex = new Dictionary<string, ChunkInfo>();
            var cacheKeys = await _cacheService.GetKeysAsync($"{CHUNK_KEY_PREFIX}{tenantId}:*");
            
            foreach (var key in cacheKeys)
            {
                var chunkInfo = ExtractChunkInfoFromKey(key);
                if (chunkInfo != null)
                {
                    chunkIndex[key] = chunkInfo;
                }
            }
            
            var indexKey = $"{CHUNK_INDEX_KEY}{tenantId}";
            await _cacheService.SetAsync(indexKey, chunkIndex, TimeSpan.FromHours(1));
        }

        /// <summary>
        /// Ürün arama işlemini cache optimizasyonu ile yapar
        /// </summary>
        public async Task<IEnumerable<Domain.DTOs.ProductDto>> SearchProductsWithCacheAsync(string searchTerm, int tenantId)
        {
            // Önce popüler ürünlerde ara
            var popularProducts = await GetPopularProductsAsync(tenantId);
            var popularResults = popularProducts?.Where(p => 
                p.ProductName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.CategoryName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
            ).Take(20); // İlk 20 sonucu popüler ürünlerden al
            
            if (popularResults?.Any() == true)
            {
                return popularResults;
            }
            
            // Eğer popüler ürünlerde bulunamadıysa, chunk'larda ara
            return await SearchInChunksAsync(searchTerm, tenantId);
        }

        /// <summary>
        /// Chunk'larda arama yapar (lazy loading)
        /// </summary>
        private async Task<IEnumerable<Domain.DTOs.ProductDto>> SearchInChunksAsync(string searchTerm, int tenantId)
        {
            var indexKey = $"{CHUNK_INDEX_KEY}{tenantId}";
            var chunkIndex = await _cacheService.GetAsync<Dictionary<string, ChunkInfo>>(indexKey);
            
            if (chunkIndex == null)
            {
                // Index yoksa database'den direk ara
                return await _productService.SearchProductsAsync(searchTerm, tenantId);
            }
            
            var results = new List<Domain.DTOs.ProductDto>();
            
            // Her chunk'ı kontrol et (en fazla ilk 10 chunk'ı)
            var chunksToCheck = chunkIndex.Take(10);
            
            foreach (var chunkEntry in chunksToCheck)
            {
                var chunkProducts = await _cacheService.GetAsync<IEnumerable<Domain.DTOs.ProductDto>>(chunkEntry.Key);
                if (chunkProducts != null)
                {
                    var matchingProducts = chunkProducts.Where(p => 
                        p.ProductName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        p.CategoryName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                    );
                    
                    results.AddRange(matchingProducts);
                    
                    // Yeterli sonuç bulunduysa dur
                    if (results.Count >= 50)
                        break;
                }
            }
            
            return results.Take(50); // Maksimum 50 sonuç döndür
        }

        /// <summary>
        /// Popüler ürünleri cache'den alır
        /// </summary>
        private async Task<IEnumerable<Domain.DTOs.ProductDto>> GetPopularProductsAsync(int tenantId)
        {
            var cacheKey = $"{POPULAR_PRODUCTS_KEY}{tenantId}";
            return await _cacheService.GetAsync<IEnumerable<Domain.DTOs.ProductDto>>(cacheKey);
        }

        /// <summary>
        /// Cache key'inden chunk bilgisini çıkarır
        /// </summary>
        private ChunkInfo ExtractChunkInfoFromKey(string key)
        {
            try
            {
                // product_chunk:1:category:5:chunk:0 formatından bilgileri çıkar
                var parts = key.Split(':');
                if (parts.Length >= 6)
                {
                    return new ChunkInfo
                    {
                        TenantId = int.Parse(parts[1]),
                        CategoryId = int.Parse(parts[3]),
                        ChunkIndex = int.Parse(parts[5])
                    };
                }
            }
            catch
            {
                // Parse edilemezse null döndür
            }
            
            return null;
        }

        /// <summary>
        /// Cache temizleme işlemi (yeni ürün eklendiğinde çağrılır)
        /// </summary>
        public async Task InvalidateProductCacheAsync(int tenantId, int? categoryId = null)
        {
            if (categoryId.HasValue)
            {
                // Sadece belirli kategori chunk'larını temizle
                var pattern = $"{CHUNK_KEY_PREFIX}{tenantId}:category:{categoryId}:*";
                await _cacheService.RemoveByPatternAsync(pattern);
            }
            else
            {
                // Tüm ürün cache'ini temizle
                var pattern = $"{CHUNK_KEY_PREFIX}{tenantId}:*";
                await _cacheService.RemoveByPatternAsync(pattern);
                
                // Popüler ürünler cache'ini de temizle
                var popularKey = $"{POPULAR_PRODUCTS_KEY}{tenantId}";
                await _cacheService.RemoveAsync(popularKey);
            }
            
            // Index'i de temizle
            var indexKey = $"{CHUNK_INDEX_KEY}{tenantId}";
            await _cacheService.RemoveAsync(indexKey);
        }

        // Dependency injection için geçici method (gerçek implementasyonda DI container'dan alınacak)
        private ICategoryService GetCategoryService()
        {
            // Bu method gerçek implementasyonda dependency injection ile değiştirilecek
            throw new NotImplementedException("CategoryService dependency injection ile sağlanmalı");
        }

        /// <summary>
        /// Chunk bilgilerini tutan helper sınıf
        /// </summary>
        private class ChunkInfo
        {
            public int TenantId { get; set; }
            public int CategoryId { get; set; }
            public int ChunkIndex { get; set; }
        }
    }
}