using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IProductService
    {
        // Temel CRUD operasyonları
        ProductDto GetById(int id, int tenantId);
        Task<ProductDto> GetByIdAsync(int id, int tenantId);
        
        IEnumerable<ProductDto> GetAll(int tenantId);
        Task<IEnumerable<ProductDto>> GetAllAsync(int tenantId);
        
        IEnumerable<ProductListDto> GetAllForList(int tenantId);
        Task<IEnumerable<ProductListDto>> GetAllForListAsync(int tenantId);
        
        IEnumerable<ProductDto> GetByCategory(int categoryId, int tenantId);
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId, int tenantId);
        
        IEnumerable<ProductDto> GetByCategoryWithSubCategories(int categoryId, int tenantId);
        Task<IEnumerable<ProductDto>> GetByCategoryWithSubCategoriesAsync(int categoryId, int tenantId);
        
        ProductDto GetProductWithProperties(int productId, int tenantId);
        Task<ProductDto> GetProductWithPropertiesAsync(int productId, int tenantId);
        
        IEnumerable<ProductDto> GetProductsWithProperties(int tenantId);
        Task<IEnumerable<ProductDto>> GetProductsWithPropertiesAsync(int tenantId);
        
        // CRUD metodları
        ProductDto Create(ProductCreateDto productCreateDto);
        Task<ProductDto> CreateAsync(ProductCreateDto productCreateDto);
        
        ProductDto Update(ProductUpdateDto productUpdateDto, int tenantId);
        Task<ProductDto> UpdateAsync(ProductUpdateDto productUpdateDto, int tenantId);
        
        bool Delete(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        
        // Arama ve filtreleme
        IEnumerable<ProductDto> SearchProducts(string searchTerm, int tenantId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int tenantId);
        
        IEnumerable<ProductDto> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, int tenantId);
        Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int tenantId);
        
        IEnumerable<ProductDto> GetRecentProducts(int count, int tenantId);
        Task<IEnumerable<ProductDto>> GetRecentProductsAsync(int count, int tenantId);
        
        // Döviz kuru ile fiyat hesaplama
        IEnumerable<ProductDto> GetProductsWithExchangeRate(int tenantId, decimal exchangeRate = 1);
        Task<IEnumerable<ProductDto>> GetProductsWithExchangeRateAsync(int tenantId, decimal exchangeRate = 1);
        
        // Sayfalama
        IEnumerable<ProductDto> GetPaged(int pageNumber, int pageSize, int tenantId);
        Task<IEnumerable<ProductDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId);
        
        int GetTotalCount(int tenantId);
        Task<int> GetTotalCountAsync(int tenantId);
    }
}