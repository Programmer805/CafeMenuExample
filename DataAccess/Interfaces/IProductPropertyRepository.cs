using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IProductPropertyRepository : IGenericRepository<ProductProperty>
    {
        // Ürün-özellik ilişki metodları
        IEnumerable<ProductProperty> GetByProduct(int productId, int tenantId);
        Task<IEnumerable<ProductProperty>> GetByProductAsync(int productId, int tenantId);
        
        IEnumerable<ProductProperty> GetByProperty(int propertyId, int tenantId);
        Task<IEnumerable<ProductProperty>> GetByPropertyAsync(int propertyId, int tenantId);
        
        ProductProperty GetByProductAndProperty(int productId, int propertyId, int tenantId);
        Task<ProductProperty> GetByProductAndPropertyAsync(int productId, int propertyId, int tenantId);
        
        bool IsProductPropertyExists(int productId, int propertyId, int tenantId);
        Task<bool> IsProductPropertyExistsAsync(int productId, int propertyId, int tenantId);
        
        void RemoveByProduct(int productId, int tenantId);
        Task RemoveByProductAsync(int productId, int tenantId);
        
        void RemoveByProperty(int propertyId, int tenantId);
        Task RemoveByPropertyAsync(int propertyId, int tenantId);
    }
}