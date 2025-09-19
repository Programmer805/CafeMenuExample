using System;
using System.Collections.Generic;

namespace Domain.DTOs
{
    public class ProductDto
    {
        public int ID { get; set; }
        public int TenantID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatorUserID { get; set; }
        
        // Navigation properties
        public string CategoryName { get; set; }
        public string CreatorUserName { get; set; }
        public List<ProductPropertyDto> ProductProperties { get; set; }
        
        // Calculated properties for customer panel
        public decimal PriceWithExchangeRate { get; set; }
        
        public ProductDto()
        {
            ProductProperties = new List<ProductPropertyDto>();
        }
    }
    
    public class ProductCreateDto
    {
        public int TenantID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public int CreatorUserID { get; set; }
    }
    
    public class ProductUpdateDto
    {
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }
    
    public class ProductListDto
    {
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal PriceWithExchangeRate { get; set; }
    }
}