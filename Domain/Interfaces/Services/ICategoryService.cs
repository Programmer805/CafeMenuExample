using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface ICategoryService
    {
        // Temel CRUD operasyonları
        CategoryDto GetById(int id, int tenantId);
        Task<CategoryDto> GetByIdAsync(int id, int tenantId);
        
        IEnumerable<CategoryDto> GetAll(int tenantId);
        Task<IEnumerable<CategoryDto>> GetAllAsync(int tenantId);
        
        IEnumerable<CategoryDto> GetByParentCategory(int? parentCategoryId, int tenantId);
        Task<IEnumerable<CategoryDto>> GetByParentCategoryAsync(int? parentCategoryId, int tenantId);
        
        IEnumerable<CategoryDto> GetRootCategories(int tenantId);
        Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(int tenantId);
        
        IEnumerable<CategoryDto> GetCategoryHierarchy(int tenantId);
        Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync(int tenantId);
        
        CategoryDto GetCategoryWithProducts(int categoryId, int tenantId);
        Task<CategoryDto> GetCategoryWithProductsAsync(int categoryId, int tenantId);
        
        // CRUD metodları
        CategoryDto Create(CategoryCreateDto categoryCreateDto);
        Task<CategoryDto> CreateAsync(CategoryCreateDto categoryCreateDto);
        
        CategoryDto Update(CategoryUpdateDto categoryUpdateDto, int tenantId);
        Task<CategoryDto> UpdateAsync(CategoryUpdateDto categoryUpdateDto, int tenantId);
        
        bool Delete(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        
        // Validation metodları
        bool HasSubCategories(int categoryId, int tenantId);
        Task<bool> HasSubCategoriesAsync(int categoryId, int tenantId);
        
        bool HasProducts(int categoryId, int tenantId);
        Task<bool> HasProductsAsync(int categoryId, int tenantId);
        
        // Sayfalama
        IEnumerable<CategoryDto> GetPaged(int pageNumber, int pageSize, int tenantId);
        Task<IEnumerable<CategoryDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId);
        
        int GetTotalCount(int tenantId);
        Task<int> GetTotalCountAsync(int tenantId);
    }
}