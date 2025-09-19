namespace Domain.DTOs
{
    public class ProductPropertyDto
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int PropertyID { get; set; }
        public int TenantID { get; set; }
        
        // Navigation properties
        public string PropertyKey { get; set; }
        public string PropertyValue { get; set; }
        public string ProductName { get; set; }
    }
    
    public class ProductPropertyCreateDto
    {
        public int ProductID { get; set; }
        public int PropertyID { get; set; }
        public int TenantID { get; set; }
    }
}