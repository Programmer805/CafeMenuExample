using Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IUserService
    {
        // Temel CRUD operasyonları
        UserDto GetById(int id, int tenantId);
        Task<UserDto> GetByIdAsync(int id, int tenantId);
        
        IEnumerable<UserDto> GetAll(int tenantId);
        Task<IEnumerable<UserDto>> GetAllAsync(int tenantId);
        
        UserDto GetByUsername(string username, int tenantId);
        Task<UserDto> GetByUsernameAsync(string username, int tenantId);
        
        // CRUD metodları
        UserDto Create(UserCreateDto userCreateDto);
        Task<UserDto> CreateAsync(UserCreateDto userCreateDto);
        
        UserDto Update(UserUpdateDto userUpdateDto, int tenantId);
        Task<UserDto> UpdateAsync(UserUpdateDto userUpdateDto, int tenantId);
        
        bool Delete(int id, int tenantId);
        Task<bool> DeleteAsync(int id, int tenantId);
        
        // Authentication metodları
        UserDto Login(UserLoginDto userLoginDto);
        Task<UserDto> LoginAsync(UserLoginDto userLoginDto);
        
        bool ChangePassword(UserPasswordChangeDto passwordChangeDto);
        Task<bool> ChangePasswordAsync(UserPasswordChangeDto passwordChangeDto);
        
        // Validation metodları
        bool IsUsernameExists(string username, int tenantId);
        Task<bool> IsUsernameExistsAsync(string username, int tenantId);
        
        bool IsUsernameExists(string username, int tenantId, int excludeUserId);
        Task<bool> IsUsernameExistsAsync(string username, int tenantId, int excludeUserId);
        
        // Sayfalama
        IEnumerable<UserDto> GetPaged(int pageNumber, int pageSize, int tenantId);
        Task<IEnumerable<UserDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId);
        
        int GetTotalCount(int tenantId);
        Task<int> GetTotalCountAsync(int tenantId);
    }
}