using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Temel CRUD operasyonları
        T GetById(int id);
        T GetById(int id, int tenantId);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetByTenant(int tenantId);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        T FirstOrDefault(Expression<Func<T, bool>> expression);
        
        // Asenkron metodlar
        System.Threading.Tasks.Task<T> GetByIdAsync(int id);
        System.Threading.Tasks.Task<T> GetByIdAsync(int id, int tenantId);
        System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
        System.Threading.Tasks.Task<IEnumerable<T>> GetByTenantAsync(int tenantId);
        System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        System.Threading.Tasks.Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        
        // Ekleme, güncelleme, silme
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        
        // Sayfa bazlı sorgular
        IEnumerable<T> GetPaged(int pageNumber, int pageSize, int tenantId);
        IEnumerable<T> GetPaged(int pageNumber, int pageSize, int tenantId, Expression<Func<T, bool>> filter);
        System.Threading.Tasks.Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, int tenantId);
        System.Threading.Tasks.Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, int tenantId, Expression<Func<T, bool>> filter);
        
        // Sayma operasyonları
        int Count();
        int Count(Expression<Func<T, bool>> expression);
        int CountByTenant(int tenantId);
        System.Threading.Tasks.Task<int> CountAsync();
        System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>> expression);
        System.Threading.Tasks.Task<int> CountByTenantAsync(int tenantId);
    }
}