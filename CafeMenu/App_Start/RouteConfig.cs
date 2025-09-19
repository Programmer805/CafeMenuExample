﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CafeMenu
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Setup route - İlk kurulum için
            routes.MapRoute(
                name: "Setup",
                url: "setup/{action}",
                defaults: new { controller = "Setup", action = "Index" }
            );

            // Tenant bazlı menü route'ları - Müşteri paneli
            routes.MapRoute(
                name: "TenantMenu",
                url: "menu/{tenantId}/{action}/{id}",
                defaults: new { controller = "Menu", action = "Index", id = UrlParameter.Optional },
                constraints: new { tenantId = @"\d+" } // Sadece sayılar
            );
            
            // Tenant bazlı kısa menü route'u
            routes.MapRoute(
                name: "TenantMenuShort",
                url: "menu/{tenantId}",
                defaults: new { controller = "Menu", action = "Index" },
                constraints: new { tenantId = @"\d+" }
            );

            // Admin panel route
            routes.MapRoute(
                name: "Admin",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "CafeMenu.Controllers.Admin" }
            );

            // Auth route
            routes.MapRoute(
                name: "Auth",
                url: "Auth/{action}",
                defaults: new { controller = "Auth", action = "Login" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
