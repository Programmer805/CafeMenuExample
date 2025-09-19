using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly CafeMenuEntities _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(CafeMenuEntities context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public virtual T GetById(int id, int tenantId)
        {
            // Multi-tenancy için override edilecek
            return GetById(id);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual IEnumerable<T> GetByTenant(int tenantId)
        {
            // Multi-tenancy için override edilecek
            return GetAll();
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression).ToList();
        }

        public virtual T FirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return _dbSet.FirstOrDefault(expression);
        }

        // Async methods
        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> GetByIdAsync(int id, int tenantId)
        {
            return await GetByIdAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetByTenantAsync(int tenantId)
        {
            return await GetAllAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).ToListAsync();
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual IEnumerable<T> GetPaged(int pageNumber, int pageSize, int tenantId)
        {
            return GetByTenant(tenantId).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public virtual IEnumerable<T> GetPaged(int pageNumber, int pageSize, int tenantId, Expression<Func<T, bool>> filter)
        {
            return Find(filter).Where(GetTenantFilter(tenantId).Compile()).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, int tenantId)
        {
            var items = await GetByTenantAsync(tenantId);
            return items.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, int tenantId, Expression<Func<T, bool>> filter)
        {
            var items = await FindAsync(filter);
            return items.Where(GetTenantFilter(tenantId).Compile()).Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public virtual int Count()
        {
            return _dbSet.Count();
        }

        public virtual int Count(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Count(expression);
        }

        public virtual int CountByTenant(int tenantId)
        {
            return Count(GetTenantFilter(tenantId));
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.CountAsync(expression);
        }

        public virtual async Task<int> CountByTenantAsync(int tenantId)
        {
            return await CountAsync(GetTenantFilter(tenantId));
        }

        protected virtual Expression<Func<T, bool>> GetTenantFilter(int tenantId)
        {
            // Base class'ta varsayılan implementasyon
            // Her entity için özelleştirilecek
            return x => true;
        }
    }
}