using System;

namespace Domain.DTOs
{
    public class TenantDto
    {
        public int ID { get; set; }
        public string TenantName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    
    public class TenantCreateDto
    {
        public string TenantName { get; set; }
        public bool IsActive { get; set; }
        
        public TenantCreateDto()
        {
            IsActive = true; // Varsayılan olarak aktif
        }
    }
    
    public class TenantUpdateDto
    {
        public int ID { get; set; }
        public string TenantName { get; set; }
        public bool IsActive { get; set; }
    }
}