using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Ürüne özel metodlar
        IEnumerable<Product> GetByCategory(int categoryId, int tenantId);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, int tenantId);
        
        IEnumerable<Product> GetByCategoryWithSubCategories(int categoryId, int tenantId);
        Task<IEnumerable<Product>> GetByCategoryWithSubCategoriesAsync(int categoryId, int tenantId);
        
        Product GetProductWithProperties(int productId, int tenantId);
        Task<Product> GetProductWithPropertiesAsync(int productId, int tenantId);
        
        IEnumerable<Product> GetProductsWithProperties(int tenantId);
        Task<IEnumerable<Product>> GetProductsWithPropertiesAsync(int tenantId);
        
        IEnumerable<Product> SearchProducts(string searchTerm, int tenantId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int tenantId);
        
        IEnumerable<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, int tenantId);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int tenantId);
        
        IEnumerable<Product> GetRecentProducts(int count, int tenantId);
        Task<IEnumerable<Product>> GetRecentProductsAsync(int count, int tenantId);
    }
}