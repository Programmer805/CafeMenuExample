using Domain.DTOs;
using AutoMapper;
using DataAccess;


namespace Domain.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateCategoryMappings();
            CreateProductMappings();
            CreateUserMappings();
            CreatePropertyMappings();
            CreateProductPropertyMappings();
            CreateTenantMappings();
        }
        
        private void CreateCategoryMappings()
        {
            // Entity to DTO
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.Category1 != null ? src.Category1.CategoryName : null))
                .ForMember(dest => dest.CreatorUserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.Name} {src.User.Surname}" : null))
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.Categories1))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
                
            // DTO to Entity
            CreateMap<CategoryCreateDto, Category>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => System.DateTime.Now));
                
            CreateMap<CategoryUpdateDto, Category>()
                .ForMember(dest => dest.TenantID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserID, opt => opt.Ignore());
        }
        
        private void CreateProductMappings()
        {
            // Entity to DTO
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.CreatorUserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.Name} {src.User.Surname}" : null))
                .ForMember(dest => dest.ProductProperties, opt => opt.MapFrom(src => src.ProductProperties))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "")) // Şimdilik boş bırak
                .ForMember(dest => dest.PriceWithExchangeRate, opt => opt.MapFrom(src => src.Price)); // Default olarak aynı fiyat
                
            CreateMap<Product, ProductListDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "")) // Şimdilik boş bırak
                .ForMember(dest => dest.PriceWithExchangeRate, opt => opt.MapFrom(src => src.Price)); // Default olarak aynı fiyat
                
            // DTO to Entity
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => System.DateTime.Now));
                
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.TenantID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserID, opt => opt.Ignore());
        }
        
        private void CreateUserMappings()
        {
            // Entity to DTO
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.TenantName : null));
                
            // DTO to Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore()) // Stored procedure'de yapılıyor
                .ForMember(dest => dest.SaltPassword, opt => opt.Ignore()) // Stored procedure'de yapılıyor
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => System.DateTime.Now));
                
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.TenantID, opt => opt.Ignore())
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.SaltPassword, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
        
        private void CreatePropertyMappings()
        {
            // Entity to DTO
            CreateMap<Property, PropertyDto>()
                .ForMember(dest => dest.CreatorUserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.Name} {src.User.Surname}" : null));
                
            // DTO to Entity
            CreateMap<PropertyCreateDto, Property>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => System.DateTime.Now));
                
            CreateMap<PropertyUpdateDto, Property>()
                .ForMember(dest => dest.TenantID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorUserID, opt => opt.Ignore());
        }
        
        private void CreateProductPropertyMappings()
        {
            // Entity to DTO
            CreateMap<ProductProperty, ProductPropertyDto>()
                .ForMember(dest => dest.PropertyKey, opt => opt.MapFrom(src => src.Property != null ? src.Property.Key : null))
                .ForMember(dest => dest.PropertyValue, opt => opt.MapFrom(src => src.Property != null ? src.Property.Value : null))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null));
                
            // DTO to Entity
            CreateMap<ProductPropertyCreateDto, ProductProperty>()
                .ForMember(dest => dest.ID, opt => opt.Ignore());
        }
        
        private void CreateTenantMappings()
        {
            // Entity to DTO
            CreateMap<Tenant, TenantDto>();
                
            // DTO to Entity
            CreateMap<TenantCreateDto, Tenant>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => System.DateTime.Now));
                
            CreateMap<TenantUpdateDto, Tenant>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}