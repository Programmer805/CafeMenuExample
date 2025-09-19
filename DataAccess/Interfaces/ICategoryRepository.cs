using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        // Kategoriye özel metodlar
        IEnumerable<Category> GetByParentCategory(int? parentCategoryId, int tenantId);
        Task<IEnumerable<Category>> GetByParentCategoryAsync(int? parentCategoryId, int tenantId);
        
        IEnumerable<Category> GetRootCategories(int tenantId);
        Task<IEnumerable<Category>> GetRootCategoriesAsync(int tenantId);
        
        IEnumerable<Category> GetCategoryHierarchy(int tenantId);
        Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int tenantId);
        
        Category GetCategoryWithProducts(int categoryId, int tenantId);
        Task<Category> GetCategoryWithProductsAsync(int categoryId, int tenantId);
        
        bool HasSubCategories(int categoryId, int tenantId);
        Task<bool> HasSubCategoriesAsync(int categoryId, int tenantId);
        
        bool HasProducts(int categoryId, int tenantId);
        Task<bool> HasProductsAsync(int categoryId, int tenantId);
    }
}