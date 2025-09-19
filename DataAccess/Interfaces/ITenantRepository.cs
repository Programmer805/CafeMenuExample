namespace DataAccess.Interfaces
{
    public interface ITenantRepository : IGenericRepository<Tenant>
    {
        // Tenant'a özel metodlar
        Tenant GetByName(string tenantName);
        System.Threading.Tasks.Task<Tenant> GetByNameAsync(string tenantName);
        
        bool IsTenantNameExists(string tenantName);
        System.Threading.Tasks.Task<bool> IsTenantNameExistsAsync(string tenantName);
        
        bool IsTenantNameExists(string tenantName, int excludeTenantId);
        System.Threading.Tasks.Task<bool> IsTenantNameExistsAsync(string tenantName, int excludeTenantId);
        
        System.Collections.Generic.IEnumerable<Tenant> GetActiveTenants();
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Tenant>> GetActiveTenantsAsync();
    }
}