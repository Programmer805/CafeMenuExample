using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace DataAccess.Repositories
{
    public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
    {
        public PropertyRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override Property GetById(int id, int tenantId)
        {
            return _dbSet.Where(p => p.ID == id && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.User)
                         .Include(p => p.Tenant)
                         .FirstOrDefault();
        }

        public override async Task<Property> GetByIdAsync(int id, int tenantId)
        {
            return await _dbSet.Where(p => p.ID == id && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.User)
                              .Include(p => p.Tenant)
                              .FirstOrDefaultAsync();
        }

        public override IEnumerable<Property> GetByTenant(int tenantId)
        {
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.User)
                         .Include(p => p.Tenant)
                         .OrderBy(p => p.Key)
                         .ThenBy(p => p.Value)
                         .ToList();
        }

        public override async Task<IEnumerable<Property>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.User)
                              .Include(p => p.Tenant)
                              .OrderBy(p => p.Key)
                              .ThenBy(p => p.Value)
                              .ToListAsync();
        }

        public IEnumerable<Property> GetByKey(string key, int tenantId)
        {
            return _dbSet.Where(p => p.Key == key && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.User)
                         .Include(p => p.Tenant)
                         .OrderBy(p => p.Value)
                         .ToList();
        }

        public async Task<IEnumerable<Property>> GetByKeyAsync(string key, int tenantId)
        {
            return await _dbSet.Where(p => p.Key == key && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.User)
                              .Include(p => p.Tenant)
                              .OrderBy(p => p.Value)
                              .ToListAsync();
        }

        public Property GetByKeyValue(string key, string value, int tenantId)
        {
            return _dbSet.Where(p => p.Key == key && p.Value == value && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.User)
                         .Include(p => p.Tenant)
                         .FirstOrDefault();
        }

        public async Task<Property> GetByKeyValueAsync(string key, string value, int tenantId)
        {
            return await _dbSet.Where(p => p.Key == key && p.Value == value && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.User)
                              .Include(p => p.Tenant)
                              .FirstOrDefaultAsync();
        }

        public bool IsKeyExists(string key, int tenantId)
        {
            return _dbSet.Any(p => p.Key == key && p.TenantID == tenantId && !p.IsDeleted);
        }

        public async Task<bool> IsKeyExistsAsync(string key, int tenantId)
        {
            return await _dbSet.AnyAsync(p => p.Key == key && p.TenantID == tenantId && !p.IsDeleted);
        }

        public bool IsKeyExists(string key, int tenantId, int excludePropertyId)
        {
            return _dbSet.Any(p => p.Key == key && p.TenantID == tenantId && p.ID != excludePropertyId && !p.IsDeleted);
        }

        public async Task<bool> IsKeyExistsAsync(string key, int tenantId, int excludePropertyId)
        {
            return await _dbSet.AnyAsync(p => p.Key == key && p.TenantID == tenantId && p.ID != excludePropertyId && !p.IsDeleted);
        }

        protected override Expression<Func<Property, bool>> GetTenantFilter(int tenantId)
        {
            return p => p.TenantID == tenantId && !p.IsDeleted;
        }
    }
}