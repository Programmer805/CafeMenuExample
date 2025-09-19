using System;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository'ler
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IUserRepository Users { get; }
        IPropertyRepository Properties { get; }
        IProductPropertyRepository ProductProperties { get; }
        ITenantRepository Tenants { get; }
        
        // Commit operasyonları
        int Complete();
        Task<int> CompleteAsync();
        
        // Transaction yönetimi
        void BeginTransaction();
        void Commit();
        void Rollback();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}