using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace DataAccess.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override Product GetById(int id, int tenantId)
        {
            return _dbSet.Where(p => p.ID == id && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .FirstOrDefault();
        }

        public override async Task<Product> GetByIdAsync(int id, int tenantId)
        {
            return await _dbSet.Where(p => p.ID == id && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .FirstOrDefaultAsync();
        }

        public override IEnumerable<Product> GetByTenant(int tenantId)
        {
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderBy(p => p.ProductName)
                         .ToList();
        }

        public override async Task<IEnumerable<Product>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderBy(p => p.ProductName)
                              .ToListAsync();
        }

        public IEnumerable<Product> GetByCategory(int categoryId, int tenantId)
        {
            return _dbSet.Where(p => p.CategoryID == categoryId && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderBy(p => p.ProductName)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, int tenantId)
        {
            return await _dbSet.Where(p => p.CategoryID == categoryId && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderBy(p => p.ProductName)
                              .ToListAsync();
        }

        public IEnumerable<Product> GetByCategoryWithSubCategories(int categoryId, int tenantId)
        {
            // Get category and all its subcategories
            var categoryIds = GetCategoryIdsWithSubCategories(categoryId, tenantId);

            return _dbSet.Where(p => categoryIds.Contains(p.CategoryID) && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderBy(p => p.ProductName)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> GetByCategoryWithSubCategoriesAsync(int categoryId, int tenantId)
        {
            var categoryIds = GetCategoryIdsWithSubCategories(categoryId, tenantId);

            return await _dbSet.Where(p => categoryIds.Contains(p.CategoryID) && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderBy(p => p.ProductName)
                              .ToListAsync();
        }

        public Product GetProductWithProperties(int productId, int tenantId)
        {
            return _dbSet.Where(p => p.ID == productId && p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .Include(p => p.ProductProperties.Select(pp => pp.Property))
                         .FirstOrDefault();
        }

        public async Task<Product> GetProductWithPropertiesAsync(int productId, int tenantId)
        {
            return await _dbSet.Where(p => p.ID == productId && p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .Include(p => p.ProductProperties.Select(pp => pp.Property))
                              .FirstOrDefaultAsync();
        }

        public IEnumerable<Product> GetProductsWithProperties(int tenantId)
        {
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .Include(p => p.ProductProperties.Select(pp => pp.Property))
                         .OrderBy(p => p.ProductName)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> GetProductsWithPropertiesAsync(int tenantId)
        {
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .Include(p => p.ProductProperties.Select(pp => pp.Property))
                              .OrderBy(p => p.ProductName)
                              .ToListAsync();
        }

        public IEnumerable<Product> SearchProducts(string searchTerm, int tenantId)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted &&
                               (p.ProductName.ToLower().Contains(lowerSearchTerm) ||
                                p.Category.CategoryName.ToLower().Contains(lowerSearchTerm)))
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderBy(p => p.ProductName)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int tenantId)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted &&
                                     (p.ProductName.ToLower().Contains(lowerSearchTerm) ||
                                      p.Category.CategoryName.ToLower().Contains(lowerSearchTerm)))
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderBy(p => p.ProductName)
                              .ToListAsync();
        }

        public IEnumerable<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice, int tenantId)
        {
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted &&
                               p.Price >= minPrice && p.Price <= maxPrice)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderBy(p => p.Price)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int tenantId)
        {
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted &&
                                     p.Price >= minPrice && p.Price <= maxPrice)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderBy(p => p.Price)
                              .ToListAsync();
        }

        public IEnumerable<Product> GetRecentProducts(int count, int tenantId)
        {
            return _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                         .Include(p => p.Category)
                         .Include(p => p.User)
                         .OrderByDescending(p => p.CreatedDate)
                         .Take(count)
                         .ToList();
        }

        public async Task<IEnumerable<Product>> GetRecentProductsAsync(int count, int tenantId)
        {
            return await _dbSet.Where(p => p.TenantID == tenantId && !p.IsDeleted)
                              .Include(p => p.Category)
                              .Include(p => p.User)
                              .OrderByDescending(p => p.CreatedDate)
                              .Take(count)
                              .ToListAsync();
        }

        protected override Expression<Func<Product, bool>> GetTenantFilter(int tenantId)
        {
            return p => p.TenantID == tenantId && !p.IsDeleted;
        }

        private List<int> GetCategoryIdsWithSubCategories(int categoryId, int tenantId)
        {
            var categoryIds = new List<int> { categoryId };

            // Get all subcategories recursively
            var subCategories = _context.Categories
                .Where(c => c.ParentCategoryID == categoryId && c.TenantID == tenantId && !c.IsDeleted)
                .ToList();

            foreach (var subCategory in subCategories)
            {
                categoryIds.AddRange(GetCategoryIdsWithSubCategories(subCategory.ID, tenantId));
            }

            return categoryIds;
        }
    }
}