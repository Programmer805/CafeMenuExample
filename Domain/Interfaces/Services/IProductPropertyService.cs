using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IProductPropertyService
    {
        // Temel CRUD operasyonları
        ProductPropertyDto GetById(int id, int tenantId);
        Task<ProductPropertyDto> GetByIdAsync(int id, int tenantId);
        
        IEnumerable<ProductPropertyDto> GetByProduct(int productId, int tenantId);
        Task<IEnumerable<ProductPropertyDto>> GetByProductAsync(int productId, int tenantId);
        
        IEnumerable<ProductPropertyDto> GetByProperty(int propertyId, int tenantId);
        Task<IEnumerable<ProductPropertyDto>> GetByPropertyAsync(int propertyId, int tenantId);
        
        ProductPropertyDto GetByProductAndProperty(int productId, int propertyId, int tenantId);
        Task<ProductPropertyDto> GetByProductAndPropertyAsync(int productId, int propertyId, int tenantId);
        
        // CRUD metodları
        ProductPropertyDto Create(ProductPropertyCreateDto productPropertyCreateDto);
        Task<ProductPropertyDto> CreateAsync(ProductPropertyCreateDto productPropertyCreateDto);
        
        bool Delete(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        
        bool DeleteByProductAndProperty(int productId, int propertyId, int tenantId);
        Task<bool> DeleteByProductAndPropertyAsync(int productId, int propertyId, int tenantId);
        
        void DeleteByProduct(int productId, int tenantId);
        Task DeleteByProductAsync(int productId, int tenantId);
        
        void DeleteByProperty(int propertyId, int tenantId);
        Task DeleteByPropertyAsync(int propertyId, int tenantId);
        
        // Bulk operations
        bool CreateMultiple(IEnumerable<ProductPropertyCreateDto> productPropertyCreateDtos);
        Task<bool> CreateMultipleAsync(IEnumerable<ProductPropertyCreateDto> productPropertyCreateDtos);
        
        // Validation metodları
        bool IsProductPropertyExists(int productId, int propertyId, int tenantId);
        Task<bool> IsProductPropertyExistsAsync(int productId, int propertyId, int tenantId);
    }
}