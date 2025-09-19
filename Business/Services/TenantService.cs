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
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public TenantService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public TenantDto GetById(int id)
        {
            var cacheKey = CacheKeys.GetTenantById(id);
            var cachedTenant = _cacheService.Get<TenantDto>(cacheKey);
            
            if (cachedTenant != null)
                return cachedTenant;

            var tenant = _unitOfWork.Tenants.GetById(id);
            if (tenant == null)
                return null;

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            _cacheService.Set(cacheKey, tenantDto, TimeSpan.FromMinutes(Constants.TENANT_CACHE_EXPIRATION));
            
            return tenantDto;
        }

        public async Task<TenantDto> GetByIdAsync(int id)
        {
            var cacheKey = CacheKeys.GetTenantById(id);
            var cachedTenant = await _cacheService.GetAsync<TenantDto>(cacheKey);
            
            if (cachedTenant != null)
                return cachedTenant;

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(id);
            if (tenant == null)
                return null;

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            await _cacheService.SetAsync(cacheKey, tenantDto, TimeSpan.FromMinutes(Constants.TENANT_CACHE_EXPIRATION));
            
            return tenantDto;
        }

        public IEnumerable<TenantDto> GetAll()
        {
            var tenants = _unitOfWork.Tenants.GetAll();
            return _mapper.Map<IEnumerable<TenantDto>>(tenants);
        }

        public async Task<IEnumerable<TenantDto>> GetAllAsync()
        {
            var tenants = await _unitOfWork.Tenants.GetAllAsync();
            return _mapper.Map<IEnumerable<TenantDto>>(tenants);
        }

        public IEnumerable<TenantDto> GetActiveTenants()
        {
            var cacheKey = CacheKeys.GetActiveTenants();
            var cachedTenants = _cacheService.Get<IEnumerable<TenantDto>>(cacheKey);
            
            if (cachedTenants != null)
                return cachedTenants;

            var tenants = _unitOfWork.Tenants.GetActiveTenants();
            var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);
            
            _cacheService.Set(cacheKey, tenantDtos, TimeSpan.FromMinutes(Constants.TENANT_CACHE_EXPIRATION));
            
            return tenantDtos;
        }

        public async Task<IEnumerable<TenantDto>> GetActiveTenantsAsync()
        {
            var cacheKey = CacheKeys.GetActiveTenants();
            var cachedTenants = await _cacheService.GetAsync<IEnumerable<TenantDto>>(cacheKey);
            
            if (cachedTenants != null)
                return cachedTenants;

            var tenants = await _unitOfWork.Tenants.GetActiveTenantsAsync();
            var tenantDtos = _mapper.Map<IEnumerable<TenantDto>>(tenants);
            
            await _cacheService.SetAsync(cacheKey, tenantDtos, TimeSpan.FromMinutes(Constants.TENANT_CACHE_EXPIRATION));
            
            return tenantDtos;
        }

        public TenantDto GetByName(string tenantName)
        {
            var tenant = _unitOfWork.Tenants.GetByName(tenantName);
            return _mapper.Map<TenantDto>(tenant);
        }

        public async Task<TenantDto> GetByNameAsync(string tenantName)
        {
            var tenant = await _unitOfWork.Tenants.GetByNameAsync(tenantName);
            return _mapper.Map<TenantDto>(tenant);
        }

        public TenantDto Create(TenantCreateDto tenantCreateDto)
        {
            // Check if tenant name already exists
            if (IsTenantNameExists(tenantCreateDto.TenantName))
                return null;

            var tenant = _mapper.Map<DataAccess.Tenant>(tenantCreateDto);
            
            _unitOfWork.Tenants.Add(tenant);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateTenantCache();

            return _mapper.Map<TenantDto>(tenant);
        }

        public async Task<TenantDto> CreateAsync(TenantCreateDto tenantCreateDto)
        {
            // Check if tenant name already exists
            if (await IsTenantNameExistsAsync(tenantCreateDto.TenantName))
                return null;

            var tenant = _mapper.Map<DataAccess.Tenant>(tenantCreateDto);
            
            _unitOfWork.Tenants.Add(tenant);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateTenantCacheAsync();

            return _mapper.Map<TenantDto>(tenant);
        }

        public TenantDto Update(TenantUpdateDto tenantUpdateDto)
        {
            var existingTenant = _unitOfWork.Tenants.GetById(tenantUpdateDto.ID);
            if (existingTenant == null)
                return null;

            // Check if tenant name already exists for another tenant
            if (IsTenantNameExists(tenantUpdateDto.TenantName, tenantUpdateDto.ID))
                return null;

            _mapper.Map(tenantUpdateDto, existingTenant);
            
            _unitOfWork.Tenants.Update(existingTenant);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateTenantCache();

            return _mapper.Map<TenantDto>(existingTenant);
        }

        public async Task<TenantDto> UpdateAsync(TenantUpdateDto tenantUpdateDto)
        {
            var existingTenant = await _unitOfWork.Tenants.GetByIdAsync(tenantUpdateDto.ID);
            if (existingTenant == null)
                return null;

            // Check if tenant name already exists for another tenant
            if (await IsTenantNameExistsAsync(tenantUpdateDto.TenantName, tenantUpdateDto.ID))
                return null;

            _mapper.Map(tenantUpdateDto, existingTenant);
            
            _unitOfWork.Tenants.Update(existingTenant);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateTenantCacheAsync();

            return _mapper.Map<TenantDto>(existingTenant);
        }

        public bool IsTenantNameExists(string tenantName)
        {
            return _unitOfWork.Tenants.IsTenantNameExists(tenantName);
        }

        public async Task<bool> IsTenantNameExistsAsync(string tenantName)
        {
            return await _unitOfWork.Tenants.IsTenantNameExistsAsync(tenantName);
        }

        public bool IsTenantNameExists(string tenantName, int excludeTenantId)
        {
            return _unitOfWork.Tenants.IsTenantNameExists(tenantName, excludeTenantId);
        }

        public async Task<bool> IsTenantNameExistsAsync(string tenantName, int excludeTenantId)
        {
            return await _unitOfWork.Tenants.IsTenantNameExistsAsync(tenantName, excludeTenantId);
        }

        private void InvalidateTenantCache()
        {
            _cacheService.RemoveByPattern("tenant:*");
        }

        private async Task InvalidateTenantCacheAsync()
        {
            await _cacheService.RemoveByPatternAsync("tenant:*");
        }
    }
}