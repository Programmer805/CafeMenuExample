using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IPropertyRepository : IGenericRepository<Property>
    {
        // Özellik metodları
        IEnumerable<Property> GetByKey(string key, int tenantId);
        Task<IEnumerable<Property>> GetByKeyAsync(string key, int tenantId);
        
        Property GetByKeyValue(string key, string value, int tenantId);
        Task<Property> GetByKeyValueAsync(string key, string value, int tenantId);
        
        bool IsKeyExists(string key, int tenantId);
        Task<bool> IsKeyExistsAsync(string key, int tenantId);
        
        bool IsKeyExists(string key, int tenantId, int excludePropertyId);
        Task<bool> IsKeyExistsAsync(string key, int tenantId, int excludePropertyId);
    }
}