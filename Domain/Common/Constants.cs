namespace Domain.Common
{
    public static class Constants
    {
        // Pagination
        public const int DEFAULT_PAGE_SIZE = 20;
        public const int MAX_PAGE_SIZE = 100;
        
        // Cache expiration times (in minutes)
        public const int PRODUCT_CACHE_EXPIRATION = 60; // 1 hour
        public const int CATEGORY_CACHE_EXPIRATION = 120; // 2 hours
        public const int USER_CACHE_EXPIRATION = 30; // 30 minutes
        public const int PROPERTY_CACHE_EXPIRATION = 240; // 4 hours
        public const int TENANT_CACHE_EXPIRATION = 1440; // 24 hours
        
        // Search
        public const int MIN_SEARCH_LENGTH = 3;
        public const int MAX_SEARCH_RESULTS = 50;
        
        // File uploads
        public const int MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
        public const string ALLOWED_IMAGE_EXTENSIONS = ".jpg,.jpeg,.png,.gif,.bmp";
        public const string UPLOAD_PATH = "~/Content/Images/Products/";
        
        // Exchange rate
        public const decimal DEFAULT_EXCHANGE_RATE = 1.0m;
        
        // Business rules
        public const int MAX_CATEGORY_DEPTH = 5;
        public const int MAX_PROPERTIES_PER_PRODUCT = 20;
        
        // Authentication
        public const int PASSWORD_MIN_LENGTH = 6;
        public const int USERNAME_MIN_LENGTH = 3;
        public const int SESSION_TIMEOUT_MINUTES = 60;
        
        // Messages
        public static class Messages
        {
            public const string RECORD_NOT_FOUND = "Kayıt bulunamadı.";
            public const string ACCESS_DENIED = "Bu işlem için yetkiniz bulunmuyor.";
            public const string INVALID_OPERATION = "Geçersiz işlem.";
            public const string SUCCESS = "İşlem başarılı.";
            public const string ERROR = "Bir hata oluştu.";
            
            // Validation messages
            public const string REQUIRED_FIELD = "Bu alan zorunludur.";
            public const string INVALID_FORMAT = "Geçersiz format.";
            public const string DUPLICATE_RECORD = "Bu kayıt zaten mevcut.";
            public const string CANNOT_DELETE_HAS_RELATIONS = "Bu kayıt silinemez, ilişkili kayıtlar mevcut.";
            
            // User messages
            public const string USERNAME_EXISTS = "Bu kullanıcı adı zaten kullanılıyor.";
            public const string INVALID_LOGIN = "Kullanıcı adı veya şifre hatalı.";
            public const string PASSWORD_CHANGED = "Şifre başarıyla değiştirildi.";
            
            // Category messages
            public const string CATEGORY_HAS_SUBCATEGORIES = "Bu kategorinin alt kategorileri mevcut, silinemez.";
            public const string CATEGORY_HAS_PRODUCTS = "Bu kategoride ürünler mevcut, silinemez.";
            
            // Product messages
            public const string PRODUCT_NAME_EXISTS = "Bu ürün adı zaten kullanılıyor.";
        }
    }
}