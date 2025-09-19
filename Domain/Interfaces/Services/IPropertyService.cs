using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IPropertyService
    {
        // Temel CRUD operasyonları
        PropertyDto GetById(int id, int tenantId);
        Task<PropertyDto> GetByIdAsync(int id, int tenantId);
        
        IEnumerable<PropertyDto> GetAll(int tenantId);
        Task<IEnumerable<PropertyDto>> GetAllAsync(int tenantId);
        
        IEnumerable<PropertyDto> GetByKey(string key, int tenantId);
        Task<IEnumerable<PropertyDto>> GetByKeyAsync(string key, int tenantId);
        
        PropertyDto GetByKeyValue(string key, string value, int tenantId);
        Task<PropertyDto> GetByKeyValueAsync(string key, string value, int tenantId);
        
        // CRUD metodları
        PropertyDto Create(PropertyCreateDto propertyCreateDto);
        Task<PropertyDto> CreateAsync(PropertyCreateDto propertyCreateDto);
        
        PropertyDto Update(PropertyUpdateDto propertyUpdateDto, int tenantId);
        Task<PropertyDto> UpdateAsync(PropertyUpdateDto propertyUpdateDto, int tenantId);
        
        bool Delete(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        
        // Validation metodları
        bool IsKeyExists(string key, int tenantId);
        Task<bool> IsKeyExistsAsync(string key, int tenantId);
        
        bool IsKeyExists(string key, int tenantId, int excludePropertyId);
        Task<bool> IsKeyExistsAsync(string key, int tenantId, int excludePropertyId);
        
        // Sayfalama
        IEnumerable<PropertyDto> GetPaged(int pageNumber, int pageSize, int tenantId);
        Task<IEnumerable<PropertyDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId);
        
        int GetTotalCount(int tenantId);
        Task<int> GetTotalCountAsync(int tenantId);
    }
}