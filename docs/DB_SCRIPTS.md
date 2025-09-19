-- =================================================================
-- TENANTS TABLE
-- Her bir kiracıyı (kafe, AVM vb.) temsil eder.
-- Tüm diğer tablolar bu tabloya bağlanarak multi-tenant yapısı sağlanır.
-- =================================================================
CREATE TABLE Tenants (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenantName NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =================================================================
-- USERS TABLE
-- Sisteme giriş yapacak kullanıcıları tutar.
-- Her kullanıcı bir Tenant'a bağlıdır.
-- =================================================================
CREATE TABLE Users (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenantID INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Surname NVARCHAR(100) NOT NULL,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    HashPassword VARBINARY(MAX) NOT NULL,
    SaltPassword VARBINARY(MAX) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantID) REFERENCES Tenants(ID)
);
GO

-- =================================================================
-- CATEGORIES TABLE
-- Ürün kategorilerini hiyerarşik bir yapıda tutar (ParentCategoryID).
-- Her kategori bir Tenant'a aittir.
-- =================================================================
CREATE TABLE Categories (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenantID INT NOT NULL,
    CategoryName NVARCHAR(255) NOT NULL,
    ParentCategoryID INT NULL, -- Ana kategorisi olmayanlar için NULL olacak
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatorUserID INT NOT NULL,

    CONSTRAINT FK_Categories_Tenants FOREIGN KEY (TenantID) REFERENCES Tenants(ID),
    CONSTRAINT FK_Categories_Users FOREIGN KEY (CreatorUserID) REFERENCES Users(ID),
    CONSTRAINT FK_Categories_ParentCategory FOREIGN KEY (ParentCategoryID) REFERENCES Categories(ID)
);
GO

-- =================================================================
-- PRODUCTS TABLE
-- Satılacak ürünlerin bilgilerini tutar.
-- Her ürün bir Tenant'a ve bir kategoriye bağlıdır.
-- =================================================================
CREATE TABLE Products (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenantID INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    CategoryID INT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    ImagePath NVARCHAR(MAX) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatorUserID INT NOT NULL,

    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantID) REFERENCES Tenants(ID),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(ID),
    CONSTRAINT FK_Products_Users FOREIGN KEY (CreatorUserID) REFERENCES Users(ID)
);
GO

-- =================================================================
-- PROPERTIES TABLE
-- Ürünlere eklenebilecek özellikleri (örn: "Renk", "Boyut") tanımlar.
-- Özellikler her Tenant için ayrı yönetilir.
-- =================================================================
CREATE TABLE Properties (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenantID INT NOT NULL,
    [Key] NVARCHAR(100) NOT NULL, -- "Key" SQL'de rezerve bir kelime olduğu için köşeli parantez kullanıldı.
    [Value] NVARCHAR(255) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatorUserID INT NOT NULL,

    CONSTRAINT FK_Properties_Tenants FOREIGN KEY (TenantID) REFERENCES Tenants(ID),
    CONSTRAINT FK_Properties_Users FOREIGN KEY (CreatorUserID) REFERENCES Users(ID)
);
GO

-- =================================================================
-- PRODUCT_PROPERTIES TABLE (İlişki Tablosu)
-- Ürünler ve Özellikler arasında çok-a-çok ilişkiyi kurar.
-- Hangi ürünün hangi özelliğe sahip olduğunu belirtir.
-- =================================================================
CREATE TABLE ProductProperties (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    PropertyID INT NOT NULL,
    TenantID INT NOT NULL,

    CONSTRAINT FK_ProductProperties_Products FOREIGN KEY (ProductID) REFERENCES Products(ID),
    CONSTRAINT FK_ProductProperties_Properties FOREIGN KEY (PropertyID) REFERENCES Properties(ID),
    CONSTRAINT FK_ProductProperties_Tenants FOREIGN KEY (TenantID) REFERENCES Tenants(ID)
);
GO

-- =================================================================
-- STORED PROCEDURES
-- =================================================================

-- =================================================================
-- sp_CreateUserWithHashedPassword
-- Yeni bir kullanıcı oluşturur. Verilen şifreyi (Password)
-- Salt ile birleştirerek SHA2_512 algoritması ile hash'ler ve
-- Users tablosuna kaydeder.
-- =================================================================
CREATE PROCEDURE sp_CreateUserWithHashedPassword
    @TenantID INT,
    @Name NVARCHAR(100),
    @Surname NVARCHAR(100),
    @Username NVARCHAR(100),
    @Password NVARCHAR(255),
    @IsSuccess BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Salt VARBINARY(MAX);
    DECLARE @HashedPassword VARBINARY(MAX);

    BEGIN TRY
        -- Benzersiz bir Salt oluştur
        SET @Salt = CRYPT_GEN_RANDOM(16); -- 16 byte'lık rastgele bir salt

        -- Şifreyi ve Salt'ı birleştirip hash'le
        SET @HashedPassword = HASHBYTES('SHA2_512', CAST(@Password AS VARBINARY(MAX)) + @Salt);

        -- Yeni kullanıcıyı tabloya ekle
        INSERT INTO Users (TenantID, Name, Surname, Username, HashPassword, SaltPassword)
        VALUES (@TenantID, @Name, @Surname, @Username, @HashedPassword, @Salt);

        -- Başarılı olursa
        SET @IsSuccess = 1;
    END TRY
    BEGIN CATCH
        -- Hata alırsa
        SET @IsSuccess = 0;
    END CATCH
END
GO

-- =================================================================
-- sp_VerifyUserPassword
-- Kullanıcı adı ve şifre ile kullanıcı doğrulaması yapar.
-- Gelen şifreyi, veritabanındaki Salt ile hash'ler ve 
-- veritabanındaki Hash ile karşılaştırır.
-- Sonuç: 1 = Başarılı, 0 = Başarısız.
-- =================================================================
CREATE PROCEDURE sp_VerifyUserPassword
    @Username NVARCHAR(100),
    @Password NVARCHAR(255),
    @IsVerified BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StoredSalt VARBINARY(MAX);
    DECLARE @StoredHash VARBINARY(MAX);
    DECLARE @ComputedHash VARBINARY(MAX);

    -- Kullanıcının Salt ve Hash bilgilerini al
    SELECT 
        @StoredSalt = SaltPassword,
        @StoredHash = HashPassword
    FROM 
        Users
    WHERE 
        Username = @Username AND IsDeleted = 0;

    -- Eğer kullanıcı bulunamazsa, doğrulama başarısız
    IF @StoredSalt IS NULL
    BEGIN
        SET @IsVerified = 0;
        RETURN;
    END

    -- Gelen şifreyi, veritabanındaki Salt ile hash'le
    SET @ComputedHash = HASHBYTES('SHA2_512', CAST(@Password AS VARBINARY(MAX)) + @StoredSalt);

    -- Hesaplanan Hash ile veritabanındaki Hash'i karşılaştır
    IF @ComputedHash = @StoredHash
    BEGIN
        SET @IsVerified = 1; -- Başarılı
    END
    ELSE
    BEGIN
        SET @IsVerified = 0; -- Başarısız
    END
END
GO