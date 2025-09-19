using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CafeMenu.Infrastructure
{
    /// <summary>
    /// Tenant-aware Authorization Attribute
    /// Kullanıcının hem authenticate hem de doğru tenant'a ait olduğunu kontrol eder
    /// </summary>
    public class TenantAuthorizeAttribute : AuthorizeAttribute
    {
        public bool RequireSameTenant { get; set; } = true;
        public string[] AllowedRoles { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Önce temel authentication kontrol et
            if (!base.AuthorizeCore(httpContext))
                return false;

            // Tenant kontrolü
            if (RequireSameTenant)
            {
                var currentTenantId = TenantResolver.GetCurrentTenantId();
                var userTenantId = GetUserTenantId(httpContext);

                if (userTenantId.HasValue && userTenantId.Value != currentTenantId)
                {
                    return false; // Kullanıcı farklı tenant'tan
                }
            }

            // Role kontrolü
            if (AllowedRoles != null && AllowedRoles.Length > 0)
            {
                var user = httpContext.User;
                foreach (var role in AllowedRoles)
                {
                    if (user.IsInRole(role))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                // Authentication gerekli
                var returnUrl = httpContext.Request.Url.PathAndQuery;
                filterContext.Result = new RedirectResult($"/Auth/Login?returnUrl={HttpUtility.UrlEncode(returnUrl)}");
            }
            else
            {
                // Authenticated ama authorized değil
                filterContext.Result = new RedirectResult("/Auth/AccessDenied");
            }
        }

        private int? GetUserTenantId(HttpContextBase httpContext)
        {
            // Authentication ticket'dan tenant ID'sini al
            var cookie = httpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                try
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket != null && !string.IsNullOrEmpty(ticket.UserData))
                    {
                        var userData = ticket.UserData.Split('|');
                        if (userData.Length >= 2 && int.TryParse(userData[1], out var tenantId))
                        {
                            return tenantId;
                        }
                    }
                }
                catch
                {
                    // Ticket decode edilemezse null döndür
                }
            }

            // Session'dan da kontrol et
            if (httpContext.Session["TenantId"] != null)
            {
                return (int)httpContext.Session["TenantId"];
            }

            return null;
        }
    }

    /// <summary>
    /// Sadece Admin kullanıcılarına izin veren attribute
    /// </summary>
    public class AdminAuthorizeAttribute : TenantAuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            AllowedRoles = new[] { "Admin", "SuperAdmin" };
        }
    }

    /// <summary>
    /// Sadece SuperAdmin kullanıcılarına izin veren attribute
    /// </summary>
    public class SuperAdminAuthorizeAttribute : TenantAuthorizeAttribute
    {
        public SuperAdminAuthorizeAttribute()
        {
            AllowedRoles = new[] { "SuperAdmin" };
            RequireSameTenant = false; // SuperAdmin tüm tenant'lara erişebilir
        }
    }
}