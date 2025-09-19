using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface ITenantService
    {
        // Temel CRUD operasyonları
        TenantDto GetById(int id);
        Task<TenantDto> GetByIdAsync(int id);
        
        IEnumerable<TenantDto> GetAll();
        Task<IEnumerable<TenantDto>> GetAllAsync();
        
        IEnumerable<TenantDto> GetActiveTenants();
        Task<IEnumerable<TenantDto>> GetActiveTenantsAsync();
        
        TenantDto GetByName(string tenantName);
        Task<TenantDto> GetByNameAsync(string tenantName);
        
        // CRUD metodları
        TenantDto Create(TenantCreateDto tenantCreateDto);
        Task<TenantDto> CreateAsync(TenantCreateDto tenantCreateDto);
        
        TenantDto Update(TenantUpdateDto tenantUpdateDto);
        Task<TenantDto> UpdateAsync(TenantUpdateDto tenantUpdateDto);
        
        // Validation metodları
        bool IsTenantNameExists(string tenantName);
        Task<bool> IsTenantNameExistsAsync(string tenantName);
        
        bool IsTenantNameExists(string tenantName, int excludeTenantId);
        Task<bool> IsTenantNameExistsAsync(string tenantName, int excludeTenantId);
    }
}