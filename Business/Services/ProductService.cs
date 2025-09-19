using AutoMapper;
using Domain.Common;
using Domain.DTOs;
using DataAccess.Interfaces;
using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public ProductDto GetById(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetProductById(id, tenantId);
            var cachedProduct = _cacheService.Get<ProductDto>(cacheKey);
            
            if (cachedProduct != null)
                return cachedProduct;

            var product = _unitOfWork.Products.GetById(id, tenantId);
            if (product == null)
                return null;

            var productDto = _mapper.Map<ProductDto>(product);
            _cacheService.Set(cacheKey, productDto, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDto;
        }

        public async Task<ProductDto> GetByIdAsync(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetProductById(id, tenantId);
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
            
            if (cachedProduct != null)
                return cachedProduct;

            var product = await _unitOfWork.Products.GetByIdAsync(id, tenantId);
            if (product == null)
                return null;

            var productDto = _mapper.Map<ProductDto>(product);
            await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDto;
        }

        public IEnumerable<ProductDto> GetAll(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllProducts(tenantId);
            var cachedProducts = _cacheService.Get<IEnumerable<ProductDto>>(cacheKey);
            
            if (cachedProducts != null)
                return cachedProducts;

            var products = _unitOfWork.Products.GetByTenant(tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _cacheService.Set(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllProducts(tenantId);
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
            
            System.Diagnostics.Debug.WriteLine($"[PRODUCT CACHE] GetAllAsync - TenantId: {tenantId}, CacheKey: {cacheKey}");
            
            if (cachedProducts != null)
            {
                System.Diagnostics.Debug.WriteLine($"[PRODUCT CACHE] Cache HIT - Found {cachedProducts.Count()} products in cache");
                return cachedProducts;
            }

            System.Diagnostics.Debug.WriteLine($"[PRODUCT CACHE] Cache MISS - Loading from database");
            var products = await _unitOfWork.Products.GetByTenantAsync(tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            System.Diagnostics.Debug.WriteLine($"[PRODUCT CACHE] Loaded {productDtos.Count()} products from database - Adding to cache");
            await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public IEnumerable<ProductListDto> GetAllForList(int tenantId)
        {
            var products = _unitOfWork.Products.GetByTenant(tenantId);
            return _mapper.Map<IEnumerable<ProductListDto>>(products);
        }

        public async Task<IEnumerable<ProductListDto>> GetAllForListAsync(int tenantId)
        {
            var products = await _unitOfWork.Products.GetByTenantAsync(tenantId);
            return _mapper.Map<IEnumerable<ProductListDto>>(products);
        }

        public IEnumerable<ProductDto> GetByCategory(int categoryId, int tenantId)
        {
            var cacheKey = CacheKeys.GetProductsByCategory(categoryId, tenantId);
            var cachedProducts = _cacheService.Get<IEnumerable<ProductDto>>(cacheKey);
            
            if (cachedProducts != null)
                return cachedProducts;

            var products = _unitOfWork.Products.GetByCategory(categoryId, tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _cacheService.Set(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId, int tenantId)
        {
            var cacheKey = CacheKeys.GetProductsByCategory(categoryId, tenantId);
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
            
            if (cachedProducts != null)
                return cachedProducts;

            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId, tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public IEnumerable<ProductDto> GetByCategoryWithSubCategories(int categoryId, int tenantId)
        {
            var products = _unitOfWork.Products.GetByCategoryWithSubCategories(categoryId, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryWithSubCategoriesAsync(int categoryId, int tenantId)
        {
            var products = await _unitOfWork.Products.GetByCategoryWithSubCategoriesAsync(categoryId, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public ProductDto GetProductWithProperties(int productId, int tenantId)
        {
            var product = _unitOfWork.Products.GetProductWithProperties(productId, tenantId);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetProductWithPropertiesAsync(int productId, int tenantId)
        {
            var product = await _unitOfWork.Products.GetProductWithPropertiesAsync(productId, tenantId);
            return _mapper.Map<ProductDto>(product);
        }

        public IEnumerable<ProductDto> GetProductsWithProperties(int tenantId)
        {
            var cacheKey = CacheKeys.GetProductsWithProperties(tenantId);
            var cachedProducts = _cacheService.Get<IEnumerable<ProductDto>>(cacheKey);
            
            if (cachedProducts != null)
                return cachedProducts;

            var products = _unitOfWork.Products.GetProductsWithProperties(tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _cacheService.Set(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsWithPropertiesAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetProductsWithProperties(tenantId);
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
            
            if (cachedProducts != null)
                return cachedProducts;

            var products = await _unitOfWork.Products.GetProductsWithPropertiesAsync(tenantId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(Constants.PRODUCT_CACHE_EXPIRATION));
            
            return productDtos;
        }

        public ProductDto Create(ProductCreateDto productCreateDto)
        {
            var product = _mapper.Map<DataAccess.Product>(productCreateDto);
            
            _unitOfWork.Products.Add(product);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateProductCache(productCreateDto.TenantID);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto productCreateDto)
        {
            var product = _mapper.Map<DataAccess.Product>(productCreateDto);
            
            _unitOfWork.Products.Add(product);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateProductCacheAsync(productCreateDto.TenantID);

            return _mapper.Map<ProductDto>(product);
        }

        public ProductDto Update(ProductUpdateDto productUpdateDto, int tenantId)
        {
            var existingProduct = _unitOfWork.Products.GetById(productUpdateDto.ID, tenantId);
            if (existingProduct == null)
                return null;

            _mapper.Map(productUpdateDto, existingProduct);
            
            _unitOfWork.Products.Update(existingProduct);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateProductCache(tenantId);

            return _mapper.Map<ProductDto>(existingProduct);
        }

        public async Task<ProductDto> UpdateAsync(ProductUpdateDto productUpdateDto, int tenantId)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productUpdateDto.ID, tenantId);
            if (existingProduct == null)
                return null;

            _mapper.Map(productUpdateDto, existingProduct);
            
            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateProductCacheAsync(tenantId);

            return _mapper.Map<ProductDto>(existingProduct);
        }

        public bool Delete(int id, int tenantId)
        {
            var product = _unitOfWork.Products.GetById(id, tenantId);
            if (product == null)
                return false;

            product.IsDeleted = true;
            _unitOfWork.Products.Update(product);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateProductCache(tenantId);

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, tenantId);
            if (product == null)
                return false;

            product.IsDeleted = true;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateProductCacheAsync(tenantId);

            return true;
        }

        public IEnumerable<ProductDto> SearchProducts(string searchTerm, int tenantId)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < Constants.MIN_SEARCH_LENGTH)
                return new List<ProductDto>();

            var products = _unitOfWork.Products.SearchProducts(searchTerm, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int tenantId)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < Constants.MIN_SEARCH_LENGTH)
                return new List<ProductDto>();

            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public IEnumerable<ProductDto> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, int tenantId)
        {
            var products = _unitOfWork.Products.GetProductsByPriceRange(minPrice, maxPrice, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int tenantId)
        {
            var products = await _unitOfWork.Products.GetProductsByPriceRangeAsync(minPrice, maxPrice, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public IEnumerable<ProductDto> GetRecentProducts(int count, int tenantId)
        {
            var products = _unitOfWork.Products.GetRecentProducts(count, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetRecentProductsAsync(int count, int tenantId)
        {
            var products = await _unitOfWork.Products.GetRecentProductsAsync(count, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public IEnumerable<ProductDto> GetProductsWithExchangeRate(int tenantId, decimal exchangeRate = 1)
        {
            var products = GetAll(tenantId);
            
            // Apply exchange rate to prices
            foreach (var product in products)
            {
                product.PriceWithExchangeRate = product.Price * exchangeRate;
            }
            
            return products;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsWithExchangeRateAsync(int tenantId, decimal exchangeRate = 1)
        {
            var products = await GetAllAsync(tenantId);
            
            // Apply exchange rate to prices
            foreach (var product in products)
            {
                product.PriceWithExchangeRate = product.Price * exchangeRate;
            }
            
            return products;
        }

        public IEnumerable<ProductDto> GetPaged(int pageNumber, int pageSize, int tenantId)
        {
            var products = _unitOfWork.Products.GetPaged(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId)
        {
            var products = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public int GetTotalCount(int tenantId)
        {
            return _unitOfWork.Products.CountByTenant(tenantId);
        }

        public async Task<int> GetTotalCountAsync(int tenantId)
        {
            return await _unitOfWork.Products.CountByTenantAsync(tenantId);
        }

        private void InvalidateProductCache(int tenantId)
        {
            _cacheService.RemoveByPattern(CacheKeys.GetProductPattern(tenantId));
        }

        private async Task InvalidateProductCacheAsync(int tenantId)
        {
            try
            {
                var pattern = CacheKeys.GetProductPattern(tenantId);
                
                // Debug: Cache'de hangi key'ler var?
                var cacheKeys = await _cacheService.GetKeysAsync();
                var productKeys = cacheKeys.Where(k => k.Contains($"product:") && k.Contains($":{tenantId}")).ToList();
                
                System.Diagnostics.Debug.WriteLine($"[CACHE DEBUG] Tenant {tenantId} - Pattern: {pattern}");
                System.Diagnostics.Debug.WriteLine($"[CACHE DEBUG] Tenant {tenantId} - Found {productKeys.Count} product keys to remove");
                
                foreach (var key in productKeys)
                {
                    System.Diagnostics.Debug.WriteLine($"[CACHE DEBUG] Removing key: {key}");
                }
                
                await _cacheService.RemoveByPatternAsync(pattern);
                
                // Ekstra güvenlik: Manuel olarak da temizle
                foreach (var key in productKeys)
                {
                    await _cacheService.RemoveAsync(key);
                }
                
                // Category cache'ini de temizle çünkü ürün sayıları değişebilir
                var categoryPattern = CacheKeys.GetCategoryPattern(tenantId);
                await _cacheService.RemoveByPatternAsync(categoryPattern);
                
                System.Diagnostics.Debug.WriteLine($"[CACHE DEBUG] Tenant {tenantId} - Cache invalidation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CACHE ERROR] Tenant {tenantId} - Cache invalidation failed: {ex.Message}");
                
                // Son çare: Tüm cache'i temizle
                await _cacheService.ClearAsync();
                System.Diagnostics.Debug.WriteLine($"[CACHE ERROR] Tenant {tenantId} - Cleared entire cache as fallback");
            }
        }
    }
}