# CafeMenu Projesi - Teknik Dokümantasyon

## 📋 İçindekiler

1. [Proje Genel Bakışı](#proje-genel-bakışı)
2. [Teknoloji Stack](#teknoloji-stack)
3. [Mimari Yapı](#mimari-yapı)
4. [Veritabanı Tasarımı](#veritabanı-tasarımı)
5. [Proje Katmanları](#proje-katmanları)
6. [Dependency Injection](#dependency-injection)
7. [Döviz Kuru Sistemi](#döviz-kuru-sistemi)
8. [Multi-Tenancy](#multi-tenancy)
9. [Güvenlik](#güvenlik)
10. [Cache Sistemi](#cache-sistemi)
11. [Konfigürasyon](#konfigürasyon)

---

## 🏗️ Proje Genel Bakışı

**CafeMenu**, çok kiracılı (multi-tenant) bir kafe menü yönetim sistemidir. Bu sistem, farklı kiracıların (tenant) kendi menülerini, kategorilerini ve ürünlerini yönetebilmelerini sağlar.

### Temel Özellikler
- **Multi-Tenant Mimari**: Her kiracı kendi verilerine izole erişim
- **Gerçek Zamanlı Döviz Kuru**: ExchangeRate-API entegrasyonu
- **Kategori ve Ürün Yönetimi**: Hiyerarşik kategori yapısı
- **Kullanıcı Yönetimi**: Salt/hash tabanlı güvenli parola sistemi
- **Responsive Tasarım**: Bootstrap 5.2.3 ile mobil uyumlu arayüz

---

## 🔧 Teknoloji Stack ve Kütüphane Kullanımları

### Backend Teknolojileri
- **.NET Framework 4.8**: Ana geliştirme platformu
- **ASP.NET MVC 5.2.9**: Web uygulama framework'ü
- **Entity Framework 6.5.1**: ORM ve veritabanı erişimi (DB-First)
- **SQL Server LocalDB**: Veritabanı sistemi

### Dependency Injection & IoC Container
- **Custom Service Locator Pattern**: Unity yerine özel DI konteyner implementasyonu
- **HttpContext Scoped Lifetime**: Request bazlı yaşam döngüsü yönetimi
- **IDependencyResolver Interface**: MVC ile entegrasyon için
- **Service Lifetime Management**: Singleton, Scoped, Transient pattern'leri

### Object Mapping
- **AutoMapper 8.1.1**: Entity-DTO dönüşümleri için
- **MappingProfile**: Özel mapping konfigürasyonları
- **Conditional Mapping**: Navigation property'ler için koşullu eşlemeler
- **Ignore Mapping**: Güvenlik için bazı property'lerin mapping'den çıkarılması

### JSON Processing & Serialization
- **Newtonsoft.Json 13.0.3**: JSON serialization/deserialization
- **ExchangeRate API Response**: Döviz kuru verilerinin JSON parse edilmesi
- **AJAX Response Handling**: Client-server JSON iletişimi

### Frontend Teknolojileri
- **Bootstrap 5.2.3**: CSS framework ve responsive design
- **jQuery 3.7.0**: DOM manipülasyonu ve AJAX işlemleri
- **jQuery Validation 1.19.5**: Client-side form validasyonu
- **jQuery Unobtrusive Validation 3.2.11**: MVC model validation entegrasyonu
- **Modernizr 2.8.3**: Browser feature detection
- **Razor View Engine**: Server-side rendering

### HTTP Client & External API Integration
- **HttpClient**: Döviz kuru API çağrıları için
- **ExchangeRate-API.com**: Real-time currency conversion
- **Async/Await Pattern**: Non-blocking API calls
- **Timeout & Retry Logic**: API güvenilirliği için

### Caching & Performance
- **HttpContext.Cache**: Server-side memory caching
- **Custom Cache Keys**: Structured cache key management
- **Cache Expiration Policies**: Time-based cache invalidation
- **Performance Monitoring**: Cache hit/miss tracking

### Security & Authentication
- **Forms Authentication**: ASP.NET built-in authentication
- **Custom Authorization Attributes**: Tenant-based access control
- **SQL Server Stored Procedures**: Secure password hashing
- **Salt + Hash Password**: Security best practices

### Data Validation & Constraints
- **Data Annotations**: Model-level validation
- **Custom Validation Attributes**: Business rule validation
- **Client & Server Validation**: Dual-layer validation strategy
- **Unobtrusive Validation**: jQuery integration

### Middleware & Filters (Custom Implementation)
- **EncodingActionFilter**: UTF-8 encoding zorlaması
- **TenantAuthorizeAttribute**: Multi-tenant güvenlik
- **TenantResolver**: URL-based tenant detection
- **Global Exception Handling**: Error management

---

## 🔧 Middleware ve Filter Sistemleri

### Custom Action Filters

#### EncodingActionFilter
Bu filter, uygulamanın tüm HTTP yanıtlarında UTF-8 karakter kodlamasını garantiler. Özellikle Türkçe karakterlerin doğru görüntülenmesi için kritik öneme sahiptir.

**Kullanım Amacı:**
- Türkçe karakter desteği garantisi
- Response encoding standardizasyonu
- Global seviyede encoding kontrolü

#### TenantAuthorizeAttribute
Bu özel yetkilendirme filtresi, multi-tenant sistemde kullanıcıların sadece kendi tenant'larına ait verilere erişebilmesini sağlar. URL'den çıkarılan tenant bilgisi ile kullanıcının session'daki tenant bilgisini karşılaştırır.

**Kullanım Amacı:**
- Cross-tenant veri erişimi engelleme
- URL manipülasyonu koruması
- Tenant bazlı güvenlik kontrolü

### Global Filters

#### FilterConfig.cs
Global filter konfigürasyonu, tüm uygulamaya etki edecek filtrelerin tanımlandığı yerdir. Burada HandleErrorAttribute ile hata yönetimi ve EncodingActionFilter ile encoding kontrolü yapılır.

### Route Constraints

#### TenantRouteConstraint
Bu sınıf, routing seviyesinde tenant doğrulaması yapar. URL'de belirtilen tenant adının geçerli olup olmadığını kontrol eder ve geçersiz tenant'lar için routing'i engeller.

---

## 📊 Kullanılan Design Pattern'ler ve Implementasyonları

### 1. Repository Pattern
**Kullanım Amacı:** Data access katmanında abstraction sağlama
Bu pattern, veritabanı işlemlerini soyutlayarak business logic'ten ayırır. Generic repository interface'i sayesinde tüm entity'ler için standart CRUD operasyonları tanımlanır.

### 2. Unit of Work Pattern
**Kullanım Amacı:** Transaction yönetimi ve repository koordinasyonu
Birden fazla repository'nin aynı transaction içinde çalışmasını sağlar. Veritabanı işlemlerini atomic hale getirir ve tutarlılığı garantiler.

### 3. Service Locator Pattern
**Kullanım Amacı:** Dependency injection container implementasyonu
Unity'nin yerine özel bir DI container implementasyonu. HttpContext bazlı scoped lifetime yönetimi ve singleton pattern'leri destekler.

### 4. DTO Pattern
**Kullanım Amacı:** Katmanlar arası veri transfer objesi
Entity'ler ile view'lar arasında güvenli veri aktarımı sağlar. Navigation property'leri flatten eder ve sadece gerekli verileri taşır.

### 5. Factory Pattern
**Kullanım Amacı:** Service instance creation
Service Locator içinde object creation'ı factory pattern ile yönetilir. Farklı lifetime'lara göre instance oluşturma stratejileri belirlenir.

### 6. Strategy Pattern
**Kullanım Amacı:** Cache implementation switching
ICacheService interface'i sayesinde farklı cache implementasyonları (Memory, Redis vb.) arasında geçiş yapılabilir.

### 7. Observer Pattern
**Kullanım Amacı:** Cache invalidation events
Product güncellemelerinde ilgili cache'lerin otomatik temizlenmesi için event-driven yapı kullanılır.

---

## 📦 Package Detaylı Kullanım Analizleri

### Entity Framework 6.5.1 (DB-First Approach)
**Kullanım Amacı:** Veritabanı erişimi ve ORM
Edmx dosyası ile otomatik entity generation yapılır. Lazy loading, change tracking, stored procedure desteği gibi özellikler kullanılır.

**Özellikleri:**
- Lazy Loading: Navigation property'ler için
- Change Tracking: Entity state management
- Stored Procedure Support: sp_CreateUserWithHashedPassword
- Connection Pooling: Performance optimization

### AutoMapper 8.1.1 (Object-Object Mapping)
**Kullanım Amacı:** Entity ↔ DTO dönüşümleri
Conditional mapping ile navigation property'lerin null kontrolü yapılır. Custom value resolver'lar ile karmaşık mapping işlemleri gerçekleştirilir.

**Avantajları:**
- Convention-based mapping
- Custom value resolvers
- Conditional mapping
- Performance optimization

### Newtonsoft.Json 13.0.3 (JSON Processing)
**Kullanım Amacı:** API response handling ve serialization
ExchangeRate API'den gelen JSON response'ları deserialize edilir. Custom serialization settings ile özel format tanımlamaları yapılır.

### Bootstrap 5.2.3 (CSS Framework)
**Kullanım Amacı:** Responsive UI design
Grid system ile responsive layout, card component'leri ile product görüntüleme, badge'ler ile price display yapılır.

### jQuery 3.7.0 + Validation (Client-side Processing)
**Kullanım Amacı:** DOM manipulation ve form validation
AJAX ile real-time price conversion, form validation kuralları, dynamic content updates gerçekleştirilir.

### Microsoft.CodeDom.Providers.DotNetCompilerPlatform 2.0.1
**Kullanım Amacı:** Runtime compilation optimization
Web.config'de compiler settings ile C# kod derlemesi optimize edilir. Warning level ve compiler options belirlenir.

### ASP.NET Web Optimization Framework 1.1.3
**Kullanım Amacı:** CSS/JS bundling ve minification
BundleConfig ile script ve style dosyaları gruplandırılır ve sıkıştırılır. Şu an development aşamasında disabled durumda.

---

## 🔍 HTTP Client Implementation Detayları

### ExchangeRate API Integration
ExchangeRateHelper sınıfında singleton HttpClient instance kullanılır. Bu pattern, connection pooling ve resource management açısından best practice'dir.

**API Endpoints:**
- ExchangeRate-API.com servisinden real-time kurlar çekilir
- Fallback rates için static dictionary tutulur
- 15 dakikalık cache mechanism ile performance optimize edilir

### Error Handling Strategy
Network issues, timeout problems gibi durumlar için comprehensive error handling yapılır. API başarısız olduğunda fallback rate sistemi devreye girer.

```
