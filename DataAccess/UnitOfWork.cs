using DataAccess.Interfaces;
using DataAccess.Repositories;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CafeMenuEntities _context;
        private DbContextTransaction _transaction;

        // Repository instances
        private ICategoryRepository _categories;
        private IProductRepository _products;
        private IUserRepository _users;
        private IPropertyRepository _properties;
        private IProductPropertyRepository _productProperties;
        private ITenantRepository _tenants;

        public UnitOfWork(CafeMenuEntities context)
        {
            _context = context;
        }

        // Repository properties with lazy loading
        public ICategoryRepository Categories
        {
            get
            {
                return _categories ?? (_categories = new CategoryRepository(_context));
            }
        }

        public IProductRepository Products
        {
            get
            {
                return _products ?? (_products = new ProductRepository(_context));
            }
        }

        public IUserRepository Users
        {
            get
            {
                return _users ?? (_users = new UserRepository(_context));
            }
        }

        public IPropertyRepository Properties
        {
            get
            {
                return _properties ?? (_properties = new PropertyRepository(_context));
            }
        }

        public IProductPropertyRepository ProductProperties
        {
            get
            {
                return _productProperties ?? (_productProperties = new ProductPropertyRepository(_context));
            }
        }

        public ITenantRepository Tenants
        {
            get
            {
                return _tenants ?? (_tenants = new TenantRepository(_context));
            }
        }

        // Commit operations
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Transaction management
        public void BeginTransaction()
        {
            if (_transaction == null)
            {
                _transaction = _context.Database.BeginTransaction();
            }
        }

        public void Commit()
        {
            try
            {
                Complete();
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = _context.Database.BeginTransaction();
            }
            await Task.CompletedTask;
        }

        public async Task CommitAsync()
        {
            try
            {
                await CompleteAsync();
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
            await Task.CompletedTask;
        }

        // Dispose pattern
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}