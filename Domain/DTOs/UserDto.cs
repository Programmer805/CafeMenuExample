using System;

namespace Domain.DTOs
{
    public class UserDto
    {
        public int ID { get; set; }
        public int TenantID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Calculated properties
        public string FullName => $"{Name} {Surname}";
        public string TenantName { get; set; }
    }
    
    public class UserCreateDto
    {
        public int TenantID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class UserUpdateDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
    }
    
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class UserPasswordChangeDto
    {
        public int UserID { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}