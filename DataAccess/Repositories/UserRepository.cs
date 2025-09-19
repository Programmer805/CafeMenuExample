using DataAccess.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace DataAccess.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CafeMenuEntities context) : base(context)
        {
        }

        public override User GetById(int id, int tenantId)
        {
            return _dbSet.Where(u => u.ID == id && u.TenantID == tenantId && !u.IsDeleted)
                         .Include(u => u.Tenant)
                         .FirstOrDefault();
        }

        public override async Task<User> GetByIdAsync(int id, int tenantId)
        {
            return await _dbSet.Where(u => u.ID == id && u.TenantID == tenantId && !u.IsDeleted)
                              .Include(u => u.Tenant)
                              .FirstOrDefaultAsync();
        }

        public override System.Collections.Generic.IEnumerable<User> GetByTenant(int tenantId)
        {
            return _dbSet.Where(u => u.TenantID == tenantId && !u.IsDeleted)
                         .Include(u => u.Tenant)
                         .OrderBy(u => u.Name)
                         .ThenBy(u => u.Surname)
                         .ToList();
        }

        public override async Task<System.Collections.Generic.IEnumerable<User>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet.Where(u => u.TenantID == tenantId && !u.IsDeleted)
                              .Include(u => u.Tenant)
                              .OrderBy(u => u.Name)
                              .ThenBy(u => u.Surname)
                              .ToListAsync();
        }

        public User GetByUsername(string username, int tenantId)
        {
            return _dbSet.Where(u => u.Username == username && u.TenantID == tenantId && !u.IsDeleted)
                         .Include(u => u.Tenant)
                         .FirstOrDefault();
        }

        public async Task<User> GetByUsernameAsync(string username, int tenantId)
        {
            return await _dbSet.Where(u => u.Username == username && u.TenantID == tenantId && !u.IsDeleted)
                              .Include(u => u.Tenant)
                              .FirstOrDefaultAsync();
        }

        public bool IsUsernameExists(string username, int tenantId)
        {
            return _dbSet.Any(u => u.Username == username && u.TenantID == tenantId && !u.IsDeleted);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int tenantId)
        {
            return await _dbSet.AnyAsync(u => u.Username == username && u.TenantID == tenantId && !u.IsDeleted);
        }

        public bool IsUsernameExists(string username, int tenantId, int excludeUserId)
        {
            return _dbSet.Any(u => u.Username == username && u.TenantID == tenantId && u.ID != excludeUserId && !u.IsDeleted);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int tenantId, int excludeUserId)
        {
            return await _dbSet.AnyAsync(u => u.Username == username && u.TenantID == tenantId && u.ID != excludeUserId && !u.IsDeleted);
        }

        public int CreateUserWithHashedPassword(int tenantId, string name, string surname, string username, string password)
        {
            var isSuccessParam = new ObjectParameter("IsSuccess", typeof(int));

            var result = _context.sp_CreateUserWithHashedPassword(tenantId, name, surname, username, password, isSuccessParam);
            
            // isSuccess OUTPUT parametresini kontrol et
            var isSuccess = (bool)isSuccessParam.Value;
            
            // Başarılı ise pozitif değer, değilse 0 döndür
            return isSuccess ? 1 : 0;
        }

        public async Task<int> CreateUserWithHashedPasswordAsync(int tenantId, string name, string surname, string username, string password)
        {
            var isSuccessParam = new ObjectParameter("IsSuccess", typeof(int));

            var result = await Task.Run(() => _context.sp_CreateUserWithHashedPassword(tenantId, name, surname, username, password, isSuccessParam));

            // isSuccess OUTPUT parametresini kontrol et
            var isSuccess = (bool)isSuccessParam.Value;
            
            // Başarılı ise pozitif değer, değilse 0 döndür
            return isSuccess ? 1 : 0;
        }

        public bool VerifyUserPassword(string username, string password)
        {
            var isVerifiedParam = new ObjectParameter("IsVerified", typeof(int));
            _context.sp_VerifyUserPassword(username, password, isVerifiedParam);
            
            var result = isVerifiedParam.Value;
            return (bool)result;
        }

        public async Task<bool> VerifyUserPasswordAsync(string username, string password)
        {
            return await Task.Run(() => VerifyUserPassword(username, password));
        }

        protected override Expression<Func<User, bool>> GetTenantFilter(int tenantId)
        {
            return u => u.TenantID == tenantId && !u.IsDeleted;
        }
    }
}