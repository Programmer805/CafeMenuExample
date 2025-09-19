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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public CategoryDto GetById(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetCategoryById(id, tenantId);
            var cachedCategory = _cacheService.Get<CategoryDto>(cacheKey);
            
            if (cachedCategory != null)
                return cachedCategory;

            var category = _unitOfWork.Categories.GetById(id, tenantId);
            if (category == null)
                return null;

            var categoryDto = _mapper.Map<CategoryDto>(category);
            _cacheService.Set(cacheKey, categoryDto, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDto;
        }

        public async Task<CategoryDto> GetByIdAsync(int id, int tenantId)
        {
            var cacheKey = CacheKeys.GetCategoryById(id, tenantId);
            var cachedCategory = await _cacheService.GetAsync<CategoryDto>(cacheKey);
            
            if (cachedCategory != null)
                return cachedCategory;

            var category = await _unitOfWork.Categories.GetByIdAsync(id, tenantId);
            if (category == null)
                return null;

            var categoryDto = _mapper.Map<CategoryDto>(category);
            await _cacheService.SetAsync(cacheKey, categoryDto, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDto;
        }

        public IEnumerable<CategoryDto> GetAll(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllCategories(tenantId);
            var cachedCategories = _cacheService.Get<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = _unitOfWork.Categories.GetByTenant(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            _cacheService.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetAllCategories(tenantId);
            var cachedCategories = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = await _unitOfWork.Categories.GetByTenantAsync(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            await _cacheService.SetAsync(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public IEnumerable<CategoryDto> GetByParentCategory(int? parentCategoryId, int tenantId)
        {
            var categories = _unitOfWork.Categories.GetByParentCategory(parentCategoryId, tenantId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetByParentCategoryAsync(int? parentCategoryId, int tenantId)
        {
            var categories = await _unitOfWork.Categories.GetByParentCategoryAsync(parentCategoryId, tenantId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public IEnumerable<CategoryDto> GetRootCategories(int tenantId)
        {
            var cacheKey = CacheKeys.GetRootCategories(tenantId);
            var cachedCategories = _cacheService.Get<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = _unitOfWork.Categories.GetRootCategories(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            _cacheService.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetRootCategories(tenantId);
            var cachedCategories = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = await _unitOfWork.Categories.GetRootCategoriesAsync(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            await _cacheService.SetAsync(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public IEnumerable<CategoryDto> GetCategoryHierarchy(int tenantId)
        {
            var cacheKey = CacheKeys.GetCategoryHierarchy(tenantId);
            var cachedCategories = _cacheService.Get<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = _unitOfWork.Categories.GetCategoryHierarchy(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            _cacheService.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryHierarchyAsync(int tenantId)
        {
            var cacheKey = CacheKeys.GetCategoryHierarchy(tenantId);
            var cachedCategories = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(cacheKey);
            
            if (cachedCategories != null)
                return cachedCategories;

            var categories = await _unitOfWork.Categories.GetCategoryHierarchyAsync(tenantId);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            await _cacheService.SetAsync(cacheKey, categoryDtos, TimeSpan.FromMinutes(Constants.CATEGORY_CACHE_EXPIRATION));
            
            return categoryDtos;
        }

        public CategoryDto GetCategoryWithProducts(int categoryId, int tenantId)
        {
            var category = _unitOfWork.Categories.GetCategoryWithProducts(categoryId, tenantId);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> GetCategoryWithProductsAsync(int categoryId, int tenantId)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(categoryId, tenantId);
            return _mapper.Map<CategoryDto>(category);
        }

        public CategoryDto Create(CategoryCreateDto categoryCreateDto)
        {
            var category = _mapper.Map<DataAccess.Category>(categoryCreateDto);
            
            _unitOfWork.Categories.Add(category);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateCategoryCache(categoryCreateDto.TenantID);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto categoryCreateDto)
        {
            var category = _mapper.Map<DataAccess.Category>(categoryCreateDto);
            
            _unitOfWork.Categories.Add(category);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateCategoryCacheAsync(categoryCreateDto.TenantID);

            return _mapper.Map<CategoryDto>(category);
        }

        public CategoryDto Update(CategoryUpdateDto categoryUpdateDto, int tenantId)
        {
            var existingCategory = _unitOfWork.Categories.GetById(categoryUpdateDto.ID, tenantId);
            if (existingCategory == null)
                return null;

            _mapper.Map(categoryUpdateDto, existingCategory);
            
            _unitOfWork.Categories.Update(existingCategory);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateCategoryCache(tenantId);

            return _mapper.Map<CategoryDto>(existingCategory);
        }

        public async Task<CategoryDto> UpdateAsync(CategoryUpdateDto categoryUpdateDto, int tenantId)
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(categoryUpdateDto.ID, tenantId);
            if (existingCategory == null)
                return null;

            _mapper.Map(categoryUpdateDto, existingCategory);
            
            _unitOfWork.Categories.Update(existingCategory);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateCategoryCacheAsync(tenantId);

            return _mapper.Map<CategoryDto>(existingCategory);
        }

        public bool Delete(int id, int tenantId)
        {
            var category = _unitOfWork.Categories.GetById(id, tenantId);
            if (category == null)
                return false;

            // Check if category has subcategories or products
            if (HasSubCategories(id, tenantId) || HasProducts(id, tenantId))
                return false;

            category.IsDeleted = true;
            _unitOfWork.Categories.Update(category);
            _unitOfWork.Complete();

            // Cache invalidation
            InvalidateCategoryCache(tenantId);

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, tenantId);
            if (category == null)
                return false;

            // Check if category has subcategories or products
            if (await HasSubCategoriesAsync(id, tenantId) || await HasProductsAsync(id, tenantId))
                return false;

            category.IsDeleted = true;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            // Cache invalidation
            await InvalidateCategoryCacheAsync(tenantId);

            return true;
        }

        public bool HasSubCategories(int categoryId, int tenantId)
        {
            return _unitOfWork.Categories.HasSubCategories(categoryId, tenantId);
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId, int tenantId)
        {
            return await _unitOfWork.Categories.HasSubCategoriesAsync(categoryId, tenantId);
        }

        public bool HasProducts(int categoryId, int tenantId)
        {
            return _unitOfWork.Categories.HasProducts(categoryId, tenantId);
        }

        public async Task<bool> HasProductsAsync(int categoryId, int tenantId)
        {
            return await _unitOfWork.Categories.HasProductsAsync(categoryId, tenantId);
        }

        public IEnumerable<CategoryDto> GetPaged(int pageNumber, int pageSize, int tenantId)
        {
            var categories = _unitOfWork.Categories.GetPaged(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetPagedAsync(int pageNumber, int pageSize, int tenantId)
        {
            var categories = await _unitOfWork.Categories.GetPagedAsync(pageNumber, pageSize, tenantId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public int GetTotalCount(int tenantId)
        {
            return _unitOfWork.Categories.CountByTenant(tenantId);
        }

        public async Task<int> GetTotalCountAsync(int tenantId)
        {
            return await _unitOfWork.Categories.CountByTenantAsync(tenantId);
        }

        private void InvalidateCategoryCache(int tenantId)
        {
            _cacheService.RemoveByPattern(CacheKeys.GetCategoryPattern(tenantId));
        }

        private async Task InvalidateCategoryCacheAsync(int tenantId)
        {
            await _cacheService.RemoveByPatternAsync(CacheKeys.GetCategoryPattern(tenantId));
        }
    }
}