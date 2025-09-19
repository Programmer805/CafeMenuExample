using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace DataAccess.Repositories
{
    public class ProductPropertyRepository : GenericRepository<ProductProperty>, IProductPropertyRepository
    {
        public ProductPropertyRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override ProductProperty GetById(int id, int tenantId)
        {
            return _dbSet.Where(pp => pp.ID == id && pp.TenantID == tenantId)
                         .Include(pp => pp.Product)
                         .Include(pp => pp.Property)
                         .Include(pp => pp.Tenant)
                         .FirstOrDefault();
        }

        public override async Task<ProductProperty> GetByIdAsync(int id, int tenantId)
        {
            return await _dbSet.Where(pp => pp.ID == id && pp.TenantID == tenantId)
                              .Include(pp => pp.Product)
                              .Include(pp => pp.Property)
                              .Include(pp => pp.Tenant)
                              .FirstOrDefaultAsync();
        }

        public override IEnumerable<ProductProperty> GetByTenant(int tenantId)
        {
            return _dbSet.Where(pp => pp.TenantID == tenantId)
                         .Include(pp => pp.Product)
                         .Include(pp => pp.Property)
                         .Include(pp => pp.Tenant)
                         .OrderBy(pp => pp.Product.ProductName)
                         .ThenBy(pp => pp.Property.Key)
                         .ToList();
        }

        public override async Task<IEnumerable<ProductProperty>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet.Where(pp => pp.TenantID == tenantId)
                              .Include(pp => pp.Product)
                              .Include(pp => pp.Property)
                              .Include(pp => pp.Tenant)
                              .OrderBy(pp => pp.Product.ProductName)
                              .ThenBy(pp => pp.Property.Key)
                              .ToListAsync();
        }

        public IEnumerable<ProductProperty> GetByProduct(int productId, int tenantId)
        {
            return _dbSet.Where(pp => pp.ProductID == productId && pp.TenantID == tenantId)
                         .Include(pp => pp.Product)
                         .Include(pp => pp.Property)
                         .Include(pp => pp.Tenant)
                         .OrderBy(pp => pp.Property.Key)
                         .ToList();
        }

        public async Task<IEnumerable<ProductProperty>> GetByProductAsync(int productId, int tenantId)
        {
            return await _dbSet.Where(pp => pp.ProductID == productId && pp.TenantID == tenantId)
                              .Include(pp => pp.Product)
                              .Include(pp => pp.Property)
                              .Include(pp => pp.Tenant)
                              .OrderBy(pp => pp.Property.Key)
                              .ToListAsync();
        }

        public IEnumerable<ProductProperty> GetByProperty(int propertyId, int tenantId)
        {
            return _dbSet.Where(pp => pp.PropertyID == propertyId && pp.TenantID == tenantId)
                         .Include(pp => pp.Product)
                         .Include(pp => pp.Property)
                         .Include(pp => pp.Tenant)
                         .OrderBy(pp => pp.Product.ProductName)
                         .ToList();
        }

        public async Task<IEnumerable<ProductProperty>> GetByPropertyAsync(int propertyId, int tenantId)
        {
            return await _dbSet.Where(pp => pp.PropertyID == propertyId && pp.TenantID == tenantId)
                              .Include(pp => pp.Product)
                              .Include(pp => pp.Property)
                              .Include(pp => pp.Tenant)
                              .OrderBy(pp => pp.Product.ProductName)
                              .ToListAsync();
        }

        public ProductProperty GetByProductAndProperty(int productId, int propertyId, int tenantId)
        {
            return _dbSet.Where(pp => pp.ProductID == productId && pp.PropertyID == propertyId && pp.TenantID == tenantId)
                         .Include(pp => pp.Product)
                         .Include(pp => pp.Property)
                         .Include(pp => pp.Tenant)
                         .FirstOrDefault();
        }

        public async Task<ProductProperty> GetByProductAndPropertyAsync(int productId, int propertyId, int tenantId)
        {
            return await _dbSet.Where(pp => pp.ProductID == productId && pp.PropertyID == propertyId && pp.TenantID == tenantId)
                              .Include(pp => pp.Product)
                              .Include(pp => pp.Property)
                              .Include(pp => pp.Tenant)
                              .FirstOrDefaultAsync();
        }

        public bool IsProductPropertyExists(int productId, int propertyId, int tenantId)
        {
            return _dbSet.Any(pp => pp.ProductID == productId && pp.PropertyID == propertyId && pp.TenantID == tenantId);
        }

        public async Task<bool> IsProductPropertyExistsAsync(int productId, int propertyId, int tenantId)
        {
            return await _dbSet.AnyAsync(pp => pp.ProductID == productId && pp.PropertyID == propertyId && pp.TenantID == tenantId);
        }

        public void RemoveByProduct(int productId, int tenantId)
        {
            var productProperties = _dbSet.Where(pp => pp.ProductID == productId && pp.TenantID == tenantId).ToList();
            _dbSet.RemoveRange(productProperties);
        }

        public async Task RemoveByProductAsync(int productId, int tenantId)
        {
            var productProperties = await _dbSet.Where(pp => pp.ProductID == productId && pp.TenantID == tenantId).ToListAsync();
            _dbSet.RemoveRange(productProperties);
        }

        public void RemoveByProperty(int propertyId, int tenantId)
        {
            var productProperties = _dbSet.Where(pp => pp.PropertyID == propertyId && pp.TenantID == tenantId).ToList();
            _dbSet.RemoveRange(productProperties);
        }

        public async Task RemoveByPropertyAsync(int propertyId, int tenantId)
        {
            var productProperties = await _dbSet.Where(pp => pp.PropertyID == propertyId && pp.TenantID == tenantId).ToListAsync();
            _dbSet.RemoveRange(productProperties);
        }

        protected override Expression<Func<ProductProperty, bool>> GetTenantFilter(int tenantId)
        {
            return pp => pp.TenantID == tenantId;
        }
    }
}