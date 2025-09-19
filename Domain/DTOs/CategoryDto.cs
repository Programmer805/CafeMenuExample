using System;
using System.Collections.Generic;

namespace Domain.DTOs
{
    public class CategoryDto
    {
        public int ID { get; set; }
        public int TenantID { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatorUserID { get; set; }
        
        // Navigation properties
        public string ParentCategoryName { get; set; }
        public string CreatorUserName { get; set; }
        public List<CategoryDto> SubCategories { get; set; }
        public List<ProductDto> Products { get; set; }
        
        public CategoryDto()
        {
            SubCategories = new List<CategoryDto>();
            Products = new List<ProductDto>();
        }
    }
    
    public class CategoryCreateDto
    {
        public int TenantID { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryID { get; set; }
        public int CreatorUserID { get; set; }
    }
    
    public class CategoryUpdateDto
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryID { get; set; }
    }
}