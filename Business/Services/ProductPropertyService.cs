using AutoMapper;
using Domain.DTOs;
using DataAccess.Interfaces;
using Domain.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ProductPropertyService : IProductPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductPropertyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public ProductPropertyDto GetById(int id, int tenantId)
        {
            var productProperty = _unitOfWork.ProductProperties.GetById(id, tenantId);
            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public async Task<ProductPropertyDto> GetByIdAsync(int id, int tenantId)
        {
            var productProperty = await _unitOfWork.ProductProperties.GetByIdAsync(id, tenantId);
            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public IEnumerable<ProductPropertyDto> GetByProduct(int productId, int tenantId)
        {
            var productProperties = _unitOfWork.ProductProperties.GetByProduct(productId, tenantId);
            return _mapper.Map<IEnumerable<ProductPropertyDto>>(productProperties);
        }

        public async Task<IEnumerable<ProductPropertyDto>> GetByProductAsync(int productId, int tenantId)
        {
            var productProperties = await _unitOfWork.ProductProperties.GetByProductAsync(productId, tenantId);
            return _mapper.Map<IEnumerable<ProductPropertyDto>>(productProperties);
        }

        public IEnumerable<ProductPropertyDto> GetByProperty(int propertyId, int tenantId)
        {
            var productProperties = _unitOfWork.ProductProperties.GetByProperty(propertyId, tenantId);
            return _mapper.Map<IEnumerable<ProductPropertyDto>>(productProperties);
        }

        public async Task<IEnumerable<ProductPropertyDto>> GetByPropertyAsync(int propertyId, int tenantId)
        {
            var productProperties = await _unitOfWork.ProductProperties.GetByPropertyAsync(propertyId, tenantId);
            return _mapper.Map<IEnumerable<ProductPropertyDto>>(productProperties);
        }

        public ProductPropertyDto GetByProductAndProperty(int productId, int propertyId, int tenantId)
        {
            var productProperty = _unitOfWork.ProductProperties.GetByProductAndProperty(productId, propertyId, tenantId);
            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public async Task<ProductPropertyDto> GetByProductAndPropertyAsync(int productId, int propertyId, int tenantId)
        {
            var productProperty = await _unitOfWork.ProductProperties.GetByProductAndPropertyAsync(productId, propertyId, tenantId);
            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public ProductPropertyDto Create(ProductPropertyCreateDto productPropertyCreateDto)
        {
            // Check if this combination already exists
            if (IsProductPropertyExists(productPropertyCreateDto.ProductID, productPropertyCreateDto.PropertyID, productPropertyCreateDto.TenantID))
                return null;

            var productProperty = _mapper.Map<DataAccess.ProductProperty>(productPropertyCreateDto);
            
            _unitOfWork.ProductProperties.Add(productProperty);
            _unitOfWork.Complete();

            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public async Task<ProductPropertyDto> CreateAsync(ProductPropertyCreateDto productPropertyCreateDto)
        {
            // Check if this combination already exists
            if (await IsProductPropertyExistsAsync(productPropertyCreateDto.ProductID, productPropertyCreateDto.PropertyID, productPropertyCreateDto.TenantID))
                return null;

            var productProperty = _mapper.Map<DataAccess.ProductProperty>(productPropertyCreateDto);
            
            _unitOfWork.ProductProperties.Add(productProperty);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProductPropertyDto>(productProperty);
        }

        public bool Delete(int id, int tenantId)
        {
            var productProperty = _unitOfWork.ProductProperties.GetById(id, tenantId);
            if (productProperty == null)
                return false;

            _unitOfWork.ProductProperties.Remove(productProperty);
            _unitOfWork.Complete();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int tenantId)
        {
            var productProperty = await _unitOfWork.ProductProperties.GetByIdAsync(id, tenantId);
            if (productProperty == null)
                return false;

            _unitOfWork.ProductProperties.Remove(productProperty);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public bool DeleteByProductAndProperty(int productId, int propertyId, int tenantId)
        {
            var productProperty = _unitOfWork.ProductProperties.GetByProductAndProperty(productId, propertyId, tenantId);
            if (productProperty == null)
                return false;

            _unitOfWork.ProductProperties.Remove(productProperty);
            _unitOfWork.Complete();

            return true;
        }

        public async Task<bool> DeleteByProductAndPropertyAsync(int productId, int propertyId, int tenantId)
        {
            var productProperty = await _unitOfWork.ProductProperties.GetByProductAndPropertyAsync(productId, propertyId, tenantId);
            if (productProperty == null)
                return false;

            _unitOfWork.ProductProperties.Remove(productProperty);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public void DeleteByProduct(int productId, int tenantId)
        {
            _unitOfWork.ProductProperties.RemoveByProduct(productId, tenantId);
            _unitOfWork.Complete();
        }

        public async Task DeleteByProductAsync(int productId, int tenantId)
        {
            await _unitOfWork.ProductProperties.RemoveByProductAsync(productId, tenantId);
            await _unitOfWork.CompleteAsync();
        }

        public void DeleteByProperty(int propertyId, int tenantId)
        {
            _unitOfWork.ProductProperties.RemoveByProperty(propertyId, tenantId);
            _unitOfWork.Complete();
        }

        public async Task DeleteByPropertyAsync(int propertyId, int tenantId)
        {
            await _unitOfWork.ProductProperties.RemoveByPropertyAsync(propertyId, tenantId);
            await _unitOfWork.CompleteAsync();
        }

        public bool CreateMultiple(IEnumerable<ProductPropertyCreateDto> productPropertyCreateDtos)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                foreach (var dto in productPropertyCreateDtos)
                {
                    // Skip if already exists
                    if (IsProductPropertyExists(dto.ProductID, dto.PropertyID, dto.TenantID))
                        continue;

                    var productProperty = _mapper.Map<DataAccess.ProductProperty>(dto);
                    _unitOfWork.ProductProperties.Add(productProperty);
                }

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public async Task<bool> CreateMultipleAsync(IEnumerable<ProductPropertyCreateDto> productPropertyCreateDtos)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var dto in productPropertyCreateDtos)
                {
                    // Skip if already exists
                    if (await IsProductPropertyExistsAsync(dto.ProductID, dto.PropertyID, dto.TenantID))
                        continue;

                    var productProperty = _mapper.Map<DataAccess.ProductProperty>(dto);
                    _unitOfWork.ProductProperties.Add(productProperty);
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return false;
            }
        }

        public bool IsProductPropertyExists(int productId, int propertyId, int tenantId)
        {
            return _unitOfWork.ProductProperties.IsProductPropertyExists(productId, propertyId, tenantId);
        }

        public async Task<bool> IsProductPropertyExistsAsync(int productId, int propertyId, int tenantId)
        {
            return await _unitOfWork.ProductProperties.IsProductPropertyExistsAsync(productId, propertyId, tenantId);
        }
    }
}