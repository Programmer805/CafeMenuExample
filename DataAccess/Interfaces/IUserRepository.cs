using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Kullanıcıya özel metodlar
        User GetByUsername(string username, int tenantId);
        Task<User> GetByUsernameAsync(string username, int tenantId);
        
        bool IsUsernameExists(string username, int tenantId);
        Task<bool> IsUsernameExistsAsync(string username, int tenantId);
        
        bool IsUsernameExists(string username, int tenantId, int excludeUserId);
        Task<bool> IsUsernameExistsAsync(string username, int tenantId, int excludeUserId);
        
        // Stored procedure metodları
        int CreateUserWithHashedPassword(int tenantId, string name, string surname, string username, string password);
        Task<int> CreateUserWithHashedPasswordAsync(int tenantId, string name, string surname, string username, string password);
        
        bool VerifyUserPassword(string username, string password);
        Task<bool> VerifyUserPasswordAsync(string username, string password);
    }
}