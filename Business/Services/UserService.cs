using AutoMapper;
using Domain.Common;
using Domain.DTOs;
using DataAccess.Interfaces;
using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public UserDto GetById(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetUserById(id, tenantId);
            var cachedUser = _cacheService.Get<UserDto>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;

            var user = _unitOfWork.Users.GetById(id, tenantId);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            _cacheService.Set(cacheKey, userDto, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDto;
        }

        public async Task<UserDto> GetByIdAsync(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetUserById(id, tenantId);
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;

            var user = await _unitOfWork.Users.GetByIdAsync(id, tenantId);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDto;
        }

        public IEnumerable<UserDto> GetAll(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllUsers(tenantId);
            var cachedUsers = _cacheService.Get<IEnumerable<UserDto>>(cacheKey);
            
            if (cachedUsers != null)
                return cachedUsers;

            var users = _unitOfWork.Users.GetByTenant(tenantId);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            
            _cacheService.Set(cacheKey, userDtos, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDtos;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllUsers(tenantId);
            var cachedUsers = await _cacheService.GetAsync<IEnumerable<UserDto>>(cacheKey);
            
            if (cachedUsers != null)
                return cachedUsers;

            var users = await _unitOfWork.Users.GetByTenantAsync(tenantId);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            
            await _cacheService.SetAsync(cacheKey, userDtos, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDtos;
        }

        public UserDto GetByUsername(string username, int tenantId)
        {
            var cacheKey = CacheKeys.GetUserByUsername(username, tenantId);
            var cachedUser = _cacheService.Get<UserDto>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;

            var user = _unitOfWork.Users.GetByUsername(username, tenantId);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            _cacheService.Set(cacheKey, userDto, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDto;
        }

        public async Task<UserDto> GetByUsernameAsync(string username, int tenantId)
        {
            var cacheKey = CacheKeys.GetUserByUsername(username, tenantId);
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            
            if (cachedUser != null)
                return cachedUser;

            var user = await _unitOfWork.Users.GetByUsernameAsync(username, tenantId);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(Constants.USER_CACHE_EXPIRATION));
            
            return userDto;
        }

        public UserDto Create(UserCreateDto userCreateDto)
        {
            // Check if username already exists
            if (IsUsernameExists(userCreateDto.Username, userCreateDto.TenantID))
                return null;

            // Use stored procedure to create user with hashed password
            var result = _unitOfWork.Users.CreateUserWithHashedPassword(
                userCreateDto.TenantID,
                userCreateDto.Name,
                userCreateDto.Surname,
                userCreateDto.Username,
                userCreateDto.Password
            );

            if (result > 0)
            {
                // Get the created user
                var createdUser = _unitOfWork.Users.GetByUsername(userCreateDto.Username, userCreateDto.TenantID);
                
                // Cache invalidation
                InvalidateUserCache(userCreateDto.TenantID);
                
                return _mapper.Map<UserDto>(createdUser);
            }

            return null;
        }

        public async Task<UserDto> CreateAsync(UserCreateDto userCreateDto)
        {
            // Check if username already exists
            if (await IsUsernameExistsAsync(userCreateDto.Username, userCreateDto.TenantID))
                return null;

            // Use stored procedure to create user with hashed password
            var result = await _unitOfWork.Users.CreateUserWithHashedPasswordAsync(
                userCreateDto.TenantID,
                userCreateDto.Name,
                userCreateDto.Surname,
                userCreateDto.Username,
                userCreateDto.Password
            );

            if (result > 0)
            {
                // Get the created user
                var createdUser = await _unitOfWork.Users.GetByUsernameAsync(userCreateDto.Username, userCreateDto.TenantID);
                
                // Cache invalidation
                await InvalidateUserCacheAsync(userCreateDto.TenantID);
                
                return _mapper.Map<UserDto>(createdUser);
            }

            return null;
        }

        public UserDto Update(UserUpdateDto userUpdateDto, int tenantId)
        {
            var existingUser = _unitOfWork.Users.GetById(userUpdateDto.ID, tenantId);
            if (existingUser == null)
                return null;

            // Check if username already exists for another user
            if (IsUsernameExists(userUpdateDto.Username, tenantId, userUpdateDto.ID))
                return null;

            _mapper.Map(userUpdateDto, existingUser);
            
            _unitOfWork.Users.Update(existingUser);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateUserCache(tenantId);

            return _mapper.Map<UserDto>(existingUser);
        }

        public async Task<UserDto> UpdateAsync(UserUpdateDto userUpdateDto, int tenantId)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userUpdateDto.ID, tenantId);
            if (existingUser == null)
                return null;

            // Check if username already exists for another user
            if (await IsUsernameExistsAsync(userUpdateDto.Username, tenantId, userUpdateDto.ID))
                return null;

            _mapper.Map(userUpdateDto, existingUser);
            
            _unitOfWork.Users.Update(existingUser);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateUserCacheAsync(tenantId);

            return _mapper.Map<UserDto>(existingUser);
        }

        public bool Delete(int id, int tenantId)
        {
            var user = _unitOfWork.Users.GetById(id, tenantId);
            if (user == null)
                return false;

            user.IsDeleted = true;
            _unitOfWork.Users.Update(user);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateUserCache(tenantId);

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, tenantId);
            if (user == null)
                return false;

            user.IsDeleted = true;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateUserCacheAsync(tenantId);

            return true;
        }

        public UserDto Login(UserLoginDto userLoginDto)
        {
            // Verify password using stored procedure
            var isPasswordValid = _unitOfWork.Users.VerifyUserPassword(userLoginDto.Username, userLoginDto.Password);
            
            if (!isPasswordValid)
                return null;

            // Get user by username from all tenants (login can be across tenants)
            // Note: In a real implementation, you might want to specify tenant during login
            var user = _unitOfWork.Users.FirstOrDefault(u => u.Username == userLoginDto.Username && !u.IsDeleted);
            
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> LoginAsync(UserLoginDto userLoginDto)
        {
            // Verify password using stored procedure
            var isPasswordValid = await _unitOfWork.Users.VerifyUserPasswordAsync(userLoginDto.Username, userLoginDto.Password);
            
            if (!isPasswordValid)
                return null;

            // Get user by username from all tenants
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == userLoginDto.Username && !u.IsDeleted);
            
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public bool ChangePassword(UserPasswordChangeDto passwordChangeDto)
        {
            // First verify current password
            var user = _unitOfWork.Users.GetById(passwordChangeDto.UserID);
            if (user == null)
                return false;

            var isCurrentPasswordValid = _unitOfWork.Users.VerifyUserPassword(user.Username, passwordChangeDto.CurrentPassword);
            if (!isCurrentPasswordValid)
                return false;

            // Create new user with new password (using stored procedure)
            // Note: This is a simplified approach. In practice, you might want to create a separate stored procedure for password updates
            var result = _unitOfWork.Users.CreateUserWithHashedPassword(
                user.TenantID,
                user.Name,
                user.Surname,
                user.Username + "_temp", // Temporary username
                passwordChangeDto.NewPassword
            );

            if (result > 0)
            {
                // Get the temporary user to extract hash and salt
                var tempUser = _unitOfWork.Users.GetByUsername(user.Username + "_temp", user.TenantID);
                
                // Update original user with new hash and salt
                user.HashPassword = tempUser.HashPassword;
                user.SaltPassword = tempUser.SaltPassword;
                
                _unitOfWork.Users.Update(user);
                
                // Remove temporary user
                _unitOfWork.Users.Remove(tempUser);
                
                _unitOfWork.Complete();

                // Cache invalidation
                InvalidateUserCache(user.TenantID);

                return true;
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(UserPasswordChangeDto passwordChangeDto)
        {
            // This is a simplified async version - in practice, you'd want proper async password handling
            return await Task.Run(() => ChangePassword(passwordChangeDto));
        }

        public bool IsUsernameExists(string username, int tenantId)
        {
            return _unitOfWork.Users.IsUsernameExists(username, tenantId);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int tenantId)
        {
            return await _unitOfWork.Users.IsUsernameExistsAsync(username, tenantId);
        }

        public bool IsUsernameExists(string username, int tenantId, int excludeUserId)
        {
            return _unitOfWork.Users.IsUsernameExists(username, tenantId, excludeUserId);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int tenantId, int excludeUserId)
        {
            return await _unitOfWork.Users.IsUsernameExistsAsync(username, tenantId, excludeUserId);
        }

        public IEnumerable<UserDto> GetPaged(int pageNumber, int pageSize, int tenantId)
        {
            var users = _unitOfWork.Users.GetPaged(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId)
        {
            var users = await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public int GetTotalCount(int tenantId)
        {
            return _unitOfWork.Users.CountByTenant(tenantId);
        }

        public async Task<int> GetTotalCountAsync(int tenantId)
        {
            return await _unitOfWork.Users.CountByTenantAsync(tenantId);
        }

        private void InvalidateUserCache(int tenantId)
        {
            _cacheService.RemoveByPattern(CacheKeys.GetUserPattern(tenantId));
        }

        private async Task InvalidateUserCacheAsync(int tenantId)
        {
            await _cacheService.RemoveByPatternAsync(CacheKeys.GetUserPattern(tenantId));
        }
    }
}