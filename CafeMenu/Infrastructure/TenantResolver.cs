using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;

namespace CafeMenu.Infrastructure
{
    /// <summary>
    /// Multi-tenancy için tenant bilgisini çözümleyen sınıf.
    /// Subdomain, header veya query string'den tenant ID'sini belirler.
    /// </summary>
    public class TenantResolver
    {
        private const string TENANT_HEADER_KEY = "X-Tenant-ID";
        private const string TENANT_QUERY_KEY = "tenantId";
        private const int DEFAULT_TENANT_ID = 1;

        /// <summary>
        /// Mevcut HTTP request'inden tenant ID'sini çözümler
        /// </summary>
        public static int GetCurrentTenantId()
        {
            if (HttpContext.Current?.Request == null)
                return DEFAULT_TENANT_ID;

            var request = HttpContext.Current.Request;

            // 1. Önce URL Route parametresinden kontrol et (menü için)
            var tenantIdFromRoute = GetTenantIdFromRoute();
            if (tenantIdFromRoute.HasValue)
                return tenantIdFromRoute.Value;

            // 2. HTTP Header'dan kontrol et
            var tenantIdFromHeader = GetTenantIdFromHeader(request);
            if (tenantIdFromHeader.HasValue)
                return tenantIdFromHeader.Value;

            // 3. Query string'den kontrol et
            var tenantIdFromQuery = GetTenantIdFromQuery(request);
            if (tenantIdFromQuery.HasValue)
                return tenantIdFromQuery.Value;

            // 4. Subdomain'den çözümle
            var tenantIdFromSubdomain = GetTenantIdFromSubdomain(request);
            if (tenantIdFromSubdomain.HasValue)
                return tenantIdFromSubdomain.Value;

            // 5. Session'dan kontrol et (login sonrası saklanan)
            var tenantIdFromSession = GetTenantIdFromSession();
            if (tenantIdFromSession.HasValue)
                return tenantIdFromSession.Value;

            // Hiçbiri yoksa varsayılan tenant
            return DEFAULT_TENANT_ID;
        }

        /// <summary>
        /// URL Route parametresinden tenant ID'sini alır
        /// Örnek: /menu/123 -> tenant ID: 123
        /// </summary>
        private static int? GetTenantIdFromRoute()
        {
            try
            {
                if (HttpContext.Current?.Request.RequestContext?.RouteData?.Values != null)
                {
                    var routeData = HttpContext.Current.Request.RequestContext.RouteData.Values;
                    
                    // Route'da tenantId parametresi var mı?
                    if (routeData.ContainsKey("tenantId") && 
                        int.TryParse(routeData["tenantId"]?.ToString(), out var tenantId))
                    {
                        return tenantId;
                    }
                }
            }
            catch
            {
                // Route parse edilemezse null döndür
            }
            
            return null;
        }

        /// <summary>
        /// HTTP Header'dan tenant ID'sini alır
        /// </summary>
        private static int? GetTenantIdFromHeader(HttpRequest request)
        {
            var tenantHeader = request.Headers[TENANT_HEADER_KEY];
            if (!string.IsNullOrEmpty(tenantHeader) && int.TryParse(tenantHeader, out var tenantId))
            {
                return tenantId;
            }
            return null;
        }

        /// <summary>
        /// Query string'den tenant ID'sini alır
        /// </summary>
        private static int? GetTenantIdFromQuery(HttpRequest request)
        {
            var tenantQuery = request.QueryString[TENANT_QUERY_KEY];
            if (!string.IsNullOrEmpty(tenantQuery) && int.TryParse(tenantQuery, out var tenantId))
            {
                return tenantId;
            }
            return null;
        }

        /// <summary>
        /// Subdomain'den tenant ID'sini çözümler
        /// Örnek: tenant1.cafemenu.com -> tenant ID: 1
        /// </summary>
        private static int? GetTenantIdFromSubdomain(HttpRequest request)
        {
            try
            {
                var host = request.Url.Host.ToLower();
                var hostParts = host.Split('.');

                if (hostParts.Length >= 3) // subdomain.domain.com format
                {
                    var subdomain = hostParts[0];
                    
                    // Subdomain'den sayısal kısmı çıkar
                    // "tenant1" -> 1, "cafe2" -> 2 gibi
                    var numberPart = new string(subdomain.Where(char.IsDigit).ToArray());
                    
                    if (!string.IsNullOrEmpty(numberPart) && int.TryParse(numberPart, out var tenantId))
                    {
                        return tenantId;
                    }
                    
                    // Eğer subdomain mapping'i varsa o tablodan çek
                    // Bu kısım veritabanından subdomain -> tenant mapping'ini alabilir
                    return GetTenantIdBySubdomainMapping(subdomain);
                }
            }
            catch
            {
                // Host parse edilemezse null döndür
            }
            
            return null;
        }

        /// <summary>
        /// Session'dan tenant ID'sini alır
        /// </summary>
        private static int? GetTenantIdFromSession()
        {
            if (HttpContext.Current?.Session != null && 
                HttpContext.Current.Session["TenantId"] != null)
            {
                return (int)HttpContext.Current.Session["TenantId"];
            }
            return null;
        }

        /// <summary>
        /// Subdomain mapping'den tenant ID'sini alır
        /// Gerçek implementasyonda bu bilgi veritabanından gelecek
        /// </summary>
        private static int? GetTenantIdBySubdomainMapping(string subdomain)
        {
            // Basit mapping örneği (gerçek implementasyonda veritabanından gelecek)
            var subdomainMappings = new Dictionary<string, int>
            {
                ["demo"] = 1,
                ["test"] = 2,
                ["cafe1"] = 3,
                ["restaurant1"] = 4
            };

            if (subdomainMappings.ContainsKey(subdomain))
                return subdomainMappings[subdomain];
            else
                return null;
        }

        /// <summary>
        /// Session'a tenant ID'sini kaydeder (login sonrası)
        /// </summary>
        public static void SetTenantIdToSession(int tenantId)
        {
            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session["TenantId"] = tenantId;
            }
        }

        /// <summary>
        /// Session'dan tenant bilgisini temizler (logout sonrası)
        /// </summary>
        public static void ClearTenantFromSession()
        {
            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session.Remove("TenantId");
            }
        }

        /// <summary>
        /// Geçerli tenant bilgisini kontrol eder
        /// </summary>
        public static bool IsValidTenant(int tenantId)
        {
            // Gerçek implementasyonda bu bilgi veritabanından kontrol edilecek
            return tenantId > 0 && tenantId <= 1000; // Basit validation
        }

        /// <summary>
        /// Debug amaçlı tenant bilgilerini döndürür
        /// </summary>
        public static object GetTenantDebugInfo()
        {
            if (HttpContext.Current?.Request == null)
                return null;

            var request = HttpContext.Current.Request;
            
            return new
            {
                CurrentTenantId = GetCurrentTenantId(),
                FromRoute = GetTenantIdFromRoute(),
                FromHeader = GetTenantIdFromHeader(request),
                FromQuery = GetTenantIdFromQuery(request),
                FromSubdomain = GetTenantIdFromSubdomain(request),
                FromSession = GetTenantIdFromSession(),
                Host = request.Url.Host,
                RequestUrl = request.Url.ToString()
            };
        }
    }
}