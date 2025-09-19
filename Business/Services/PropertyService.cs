using AutoMapper;
using Domain.Common;
using Domain.DTOs;
using DataAccess.Interfaces;
using Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public PropertyService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public PropertyDto GetById(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetPropertyById(id, tenantId);
            var cachedProperty = _cacheService.Get<PropertyDto>(cacheKey);
            
            if (cachedProperty != null)
                return cachedProperty;

            var property = _unitOfWork.Properties.GetById(id, tenantId);
            if (property == null)
                return null;

            var propertyDto = _mapper.Map<PropertyDto>(property);
            _cacheService.Set(cacheKey, propertyDto, TimeSpan.FromMinutes(Constants.PROPERTY_CACHE_EXPIRATION));
            
            return propertyDto;
        }

        public async Task<PropertyDto> GetByIdAsync(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetPropertyById(id, tenantId);
            var cachedProperty = await _cacheService.GetAsync<PropertyDto>(cacheKey);
            
            if (cachedProperty != null)
                return cachedProperty;

            var property = await _unitOfWork.Properties.GetByIdAsync(id, tenantId);
            if (property == null)
                return null;

            var propertyDto = _mapper.Map<PropertyDto>(property);
            await _cacheService.SetAsync(cacheKey, propertyDto, TimeSpan.FromMinutes(Constants.PROPERTY_CACHE_EXPIRATION));
            
            return propertyDto;
        }

        public IEnumerable<PropertyDto> GetAll(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllProperties(tenantId);
            var cachedProperties = _cacheService.Get<IEnumerable<PropertyDto>>(cacheKey);
            
            if (cachedProperties != null)
                return cachedProperties;

            var properties = _unitOfWork.Properties.GetByTenant(tenantId);
            var propertyDtos = _mapper.Map<IEnumerable<PropertyDto>>(properties);
            
            _cacheService.Set(cacheKey, propertyDtos, TimeSpan.FromMinutes(Constants.PROPERTY_CACHE_EXPIRATION));
            
            return propertyDtos;
        }

        public async Task<IEnumerable<PropertyDto>> GetAllAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllProperties(tenantId);
            var cachedProperties = await _cacheService.GetAsync<IEnumerable<PropertyDto>>(cacheKey);
            
            if (cachedProperties != null)
                return cachedProperties;

            var properties = await _unitOfWork.Properties.GetByTenantAsync(tenantId);
            var propertyDtos = _mapper.Map<IEnumerable<PropertyDto>>(properties);
            
            await _cacheService.SetAsync(cacheKey, propertyDtos, TimeSpan.FromMinutes(Constants.PROPERTY_CACHE_EXPIRATION));
            
            return propertyDtos;
        }

        public IEnumerable<PropertyDto> GetByKey(string key, int tenantId)
        {
            var properties = _unitOfWork.Properties.GetByKey(key, tenantId);
            return _mapper.Map<IEnumerable<PropertyDto>>(properties);
        }

        public async Task<IEnumerable<PropertyDto>> GetByKeyAsync(string key, int tenantId)
        {
            var properties = await _unitOfWork.Properties.GetByKeyAsync(key, tenantId);
            return _mapper.Map<IEnumerable<PropertyDto>>(properties);
        }

        public PropertyDto GetByKeyValue(string key, string value, int tenantId)
        {
            var property = _unitOfWork.Properties.GetByKeyValue(key, value, tenantId);
            return _mapper.Map<PropertyDto>(property);
        }

        public async Task<PropertyDto> GetByKeyValueAsync(string key, string value, int tenantId)
        {
            var property = await _unitOfWork.Properties.GetByKeyValueAsync(key, value, tenantId);
            return _mapper.Map<PropertyDto>(property);
        }

        public PropertyDto Create(PropertyCreateDto propertyCreateDto)
        {
            var property = _mapper.Map<DataAccess.Property>(propertyCreateDto);
            
            _unitOfWork.Properties.Add(property);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidatePropertyCache(propertyCreateDto.TenantID);

            return _mapper.Map<PropertyDto>(property);
        }

        public async Task<PropertyDto> CreateAsync(PropertyCreateDto propertyCreateDto)
        {
            var property = _mapper.Map<DataAccess.Property>(propertyCreateDto);
            
            _unitOfWork.Properties.Add(property);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidatePropertyCacheAsync(propertyCreateDto.TenantID);

            return _mapper.Map<PropertyDto>(property);
        }

        public PropertyDto Update(PropertyUpdateDto propertyUpdateDto, int tenantId)
        {
            var existingProperty = _unitOfWork.Properties.GetById(propertyUpdateDto.ID, tenantId);
            if (existingProperty == null)
                return null;

            _mapper.Map(propertyUpdateDto, existingProperty);
            
            _unitOfWork.Properties.Update(existingProperty);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidatePropertyCache(tenantId);

            return _mapper.Map<PropertyDto>(existingProperty);
        }

        public async Task<PropertyDto> UpdateAsync(PropertyUpdateDto propertyUpdateDto, int tenantId)
        {
            var existingProperty = await _unitOfWork.Properties.GetByIdAsync(propertyUpdateDto.ID, tenantId);
            if (existingProperty == null)
                return null;

            _mapper.Map(propertyUpdateDto, existingProperty);
            
            _unitOfWork.Properties.Update(existingProperty);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidatePropertyCacheAsync(tenantId);

            return _mapper.Map<PropertyDto>(existingProperty);
        }

        public bool Delete(int id, int tenantId)
        {
            var property = _unitOfWork.Properties.GetById(id, tenantId);
            if (property == null)
                return false;

            // Check if property is used by products
            var productProperties = _unitOfWork.ProductProperties.GetByProperty(id, tenantId);
            if (productProperties.Any())
                return false; // Cannot delete property that is in use

            property.IsDeleted = true;
            _unitOfWork.Properties.Update(property);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidatePropertyCache(tenantId);

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(id, tenantId);
            if (property == null)
                return false;

            // Check if property is used by products
            var productProperties = await _unitOfWork.ProductProperties.GetByPropertyAsync(id, tenantId);
            if (productProperties.Any())
                return false; // Cannot delete property that is in use

            property.IsDeleted = true;
            _unitOfWork.Properties.Update(property);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidatePropertyCacheAsync(tenantId);

            return true;
        }

        public bool IsKeyExists(string key, int tenantId)
        {
            return _unitOfWork.Properties.IsKeyExists(key, tenantId);
        }

        public async Task<bool> IsKeyExistsAsync(string key, int tenantId)
        {
            return await _unitOfWork.Properties.IsKeyExistsAsync(key, tenantId);
        }

        public bool IsKeyExists(string key, int tenantId, int excludePropertyId)
        {
            return _unitOfWork.Properties.IsKeyExists(key, tenantId, excludePropertyId);
        }

        public async Task<bool> IsKeyExistsAsync(string key, int tenantId, int excludePropertyId)
        {
            return await _unitOfWork.Properties.IsKeyExistsAsync(key, tenantId, excludePropertyId);
        }

        public IEnumerable<PropertyDto> GetPaged(int pageNumber, int pageSize, int tenantId)
        {
            var properties = _unitOfWork.Properties.GetPaged(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<PropertyDto>>(properties);
        }

        public async Task<IEnumerable<PropertyDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId)
        {
            var properties = await _unitOfWork.Properties.GetPagedAsync(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<PropertyDto>>(properties);
        }

        public int GetTotalCount(int tenantId)
        {
            return _unitOfWork.Properties.CountByTenant(tenantId);
        }

        public async Task<int> GetTotalCountAsync(int tenantId)
        {
            return await _unitOfWork.Properties.CountByTenantAsync(tenantId);
        }

        private void InvalidatePropertyCache(int tenantId)
        {
            _cacheService.RemoveByPattern(CacheKeys.GetPropertyPattern(tenantId));
        }

        private async Task InvalidatePropertyCacheAsync(int tenantId)
        {
            await _cacheService.RemoveByPatternAsync(CacheKeys.GetPropertyPattern(tenantId));
        }
    }
}