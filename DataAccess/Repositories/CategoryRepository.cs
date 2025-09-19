using DataAccess.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace DataAccess.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override Category GetById(int id, int tenantId)
        {
            return _dbSet.FirstOrDefault(c => c.ID == id && c.TenantID == tenantId && !c.IsDeleted);
        }

        public override async Task<Category> GetByIdAsync(int id, int tenantId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ID == id && c.TenantID == tenantId && !c.IsDeleted);
        }

        public override IEnumerable<Category> GetByTenant(int tenantId)
        {
            return _dbSet.Where(c => c.TenantID == tenantId && !c.IsDeleted)
                         .Include(c => c.Category1) // Parent category
                         .Include(c => c.User) // Creator user
                         .OrderBy(c => c.CategoryName)
                         .ToList();
        }

        public override async Task<IEnumerable<Category>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet.Where(c => c.TenantID == tenantId && !c.IsDeleted)
                              .Include(c => c.Category1)
                              .Include(c => c.User)
                              .OrderBy(c => c.CategoryName)
                              .ToListAsync();
        }

        public IEnumerable<Category> GetByParentCategory(int? parentCategoryId, int tenantId)
        {
            return _dbSet.Where(c => c.ParentCategoryID == parentCategoryId && c.TenantID == tenantId && !c.IsDeleted)
                         .Include(c => c.Category1)
                         .Include(c => c.User)
                         .OrderBy(c => c.CategoryName)
                         .ToList();
        }

        public async Task<IEnumerable<Category>> GetByParentCategoryAsync(int? parentCategoryId, int tenantId)
        {
            return await _dbSet.Where(c => c.ParentCategoryID == parentCategoryId && c.TenantID == tenantId && !c.IsDeleted)
                              .Include(c => c.Category1)
                              .Include(c => c.User)
                              .OrderBy(c => c.CategoryName)
                              .ToListAsync();
        }

        public IEnumerable<Category> GetRootCategories(int tenantId)
        {
            return GetByParentCategory(null, tenantId);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync(int tenantId)
        {
            return await GetByParentCategoryAsync(null, tenantId);
        }

        public IEnumerable<Category> GetCategoryHierarchy(int tenantId)
        {
            return _dbSet.Where(c => c.TenantID == tenantId && !c.IsDeleted)
                         .Include(c => c.Category1)
                         .Include(c => c.Categories1) // Sub categories
                         .Include(c => c.User)
                         .OrderBy(c => c.ParentCategoryID)
                         .ThenBy(c => c.CategoryName)
                         .ToList();
        }

        public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int tenantId)
        {
            return await _dbSet.Where(c => c.TenantID == tenantId && !c.IsDeleted)
                              .Include(c => c.Category1)
                              .Include(c => c.Categories1)
                              .Include(c => c.User)
                              .OrderBy(c => c.ParentCategoryID)
                              .ThenBy(c => c.CategoryName)
                              .ToListAsync();
        }

        public Category GetCategoryWithProducts(int categoryId, int tenantId)
        {
            return _dbSet.Where(c => c.ID == categoryId && c.TenantID == tenantId && !c.IsDeleted)
                         .Include(c => c.Products.Where(p => !p.IsDeleted))
                         .Include(c => c.Category1)
                         .Include(c => c.User)
                         .FirstOrDefault();
        }

        public async Task<Category> GetCategoryWithProductsAsync(int categoryId, int tenantId)
        {
            return await _dbSet.Where(c => c.ID == categoryId && c.TenantID == tenantId && !c.IsDeleted)
                              .Include(c => c.Products.Where(p => !p.IsDeleted))
                              .Include(c => c.Category1)
                              .Include(c => c.User)
                              .FirstOrDefaultAsync();
        }

        public bool HasSubCategories(int categoryId, int tenantId)
        {
            return _dbSet.Any(c => c.ParentCategoryID == categoryId && c.TenantID == tenantId && !c.IsDeleted);
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId, int tenantId)
        {
            return await _dbSet.AnyAsync(c => c.ParentCategoryID == categoryId && c.TenantID == tenantId && !c.IsDeleted);
        }

        public bool HasProducts(int categoryId, int tenantId)
        {
            return _context.Products.Any(p => p.CategoryID == categoryId && p.TenantID == tenantId && !p.IsDeleted);
        }

        public async Task<bool> HasProductsAsync(int categoryId, int tenantId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryID == categoryId && p.TenantID == tenantId && !p.IsDeleted);
        }

        protected override Expression<Func<Category, bool>> GetTenantFilter(int tenantId)
        {
            return c => c.TenantID == tenantId && !c.IsDeleted;
        }
    }
}