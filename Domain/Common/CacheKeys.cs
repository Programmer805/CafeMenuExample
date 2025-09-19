namespace Domain.Common
{
    public static class CacheKeys
    {
        // Cache key prefixes
        public const string TENANT_PREFIX = "tenant:";
        public const string CATEGORY_PREFIX = "category:";
        public const string PRODUCT_PREFIX = "product:";
        public const string USER_PREFIX = "user:";
        public const string PROPERTY_PREFIX = "property:";
        
        // Specific cache keys
        public static string GetAllProducts(int tenantId) => $"{PRODUCT_PREFIX}all:{tenantId}";
        public static string GetProductById(int productId, int tenantId) => $"{PRODUCT_PREFIX}{productId}:{tenantId}";
        public static string GetProductsByCategory(int categoryId, int tenantId) => $"{PRODUCT_PREFIX}category:{categoryId}:{tenantId}";
        public static string GetProductsWithProperties(int tenantId) => $"{PRODUCT_PREFIX}withproperties:{tenantId}";
        
        public static string GetAllCategories(int tenantId) => $"{CATEGORY_PREFIX}all:{tenantId}";
        public static string GetCategoryById(int categoryId, int tenantId) => $"{CATEGORY_PREFIX}{categoryId}:{tenantId}";
        public static string GetRootCategories(int tenantId) => $"{CATEGORY_PREFIX}root:{tenantId}";
        public static string GetCategoryHierarchy(int tenantId) => $"{CATEGORY_PREFIX}hierarchy:{tenantId}";
        
        public static string GetAllUsers(int tenantId) => $"{USER_PREFIX}all:{tenantId}";
        public static string GetUserById(int userId, int tenantId) => $"{USER_PREFIX}{userId}:{tenantId}";
        public static string GetUserByUsername(string username, int tenantId) => $"{USER_PREFIX}username:{username}:{tenantId}";
        
        public static string GetAllProperties(int tenantId) => $"{PROPERTY_PREFIX}all:{tenantId}";
        public static string GetPropertyById(int propertyId, int tenantId) => $"{PROPERTY_PREFIX}{propertyId}:{tenantId}";
        
        public static string GetActiveTenants() => $"{TENANT_PREFIX}active";
        public static string GetTenantById(int tenantId) => $"{TENANT_PREFIX}{tenantId}";
        
        // Pattern keys for bulk operations
        public static string GetTenantPattern(int tenantId) => $"*:{tenantId}";
        public static string GetProductPattern(int tenantId) => $"{PRODUCT_PREFIX}*:{tenantId}";
        public static string GetCategoryPattern(int tenantId) => $"{CATEGORY_PREFIX}*:{tenantId}";
        public static string GetUserPattern(int tenantId) => $"{USER_PREFIX}*:{tenantId}";
        public static string GetPropertyPattern(int tenantId) => $"{PROPERTY_PREFIX}*:{tenantId}";
    }
}