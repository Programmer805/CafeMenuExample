using System;

namespace Domain.DTOs
{
    public class PropertyDto
    {
        public int ID { get; set; }
        public int TenantID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatorUserID { get; set; }
        
        // Navigation properties
        public string CreatorUserName { get; set; }
    }
    
    public class PropertyCreateDto
    {
        public int TenantID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int CreatorUserID { get; set; }
    }
    
    public class PropertyUpdateDto
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}