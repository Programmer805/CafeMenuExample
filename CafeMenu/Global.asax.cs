﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Text;
using CafeMenu.App_Start;

namespace CafeMenu
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Dependency Injection konfigürasyonunu başlat
            UnityConfig.RegisterComponents();
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            // Geçici olarak bundling'i kapatıyoruz - JavaScript parser hatası nedeniyle
            // BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Bundling'i tamamen devre dışı bırak
            BundleTable.EnableOptimizations = false;
        }
        
        protected void Application_BeginRequest()
        {
            // Ensure UTF-8 encoding for all requests
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
        }
    }
}
