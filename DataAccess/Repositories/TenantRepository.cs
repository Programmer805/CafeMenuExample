using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
    {
        public TenantRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override IEnumerable<Tenant> GetAll()
        {
            return _dbSet.OrderBy(t => t.TenantName).ToList();
        }

        public override async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            return await _dbSet.OrderBy(t => t.TenantName).ToListAsync();
        }

        public Tenant GetByName(string tenantName)
        {
            return _dbSet.FirstOrDefault(t => t.TenantName == tenantName);
        }

        public async Task<Tenant> GetByNameAsync(string tenantName)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.TenantName == tenantName);
        }

        public bool IsTenantNameExists(string tenantName)
        {
            return _dbSet.Any(t => t.TenantName == tenantName);
        }

        public async Task<bool> IsTenantNameExistsAsync(string tenantName)
        {
            return await _dbSet.AnyAsync(t => t.TenantName == tenantName);
        }

        public bool IsTenantNameExists(string tenantName, int excludeTenantId)
        {
            return _dbSet.Any(t => t.TenantName == tenantName && t.ID != excludeTenantId);
        }

        public async Task<bool> IsTenantNameExistsAsync(string tenantName, int excludeTenantId)
        {
            return await _dbSet.AnyAsync(t => t.TenantName == tenantName && t.ID != excludeTenantId);
        }

        public IEnumerable<Tenant> GetActiveTenants()
        {
            return _dbSet.Where(t => t.IsActive)
                         .OrderBy(t => t.TenantName)
                         .ToList();
        }

        public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
        {
            return await _dbSet.Where(t => t.IsActive)
                              .OrderBy(t => t.TenantName)
                              .ToListAsync();
        }
    }
}