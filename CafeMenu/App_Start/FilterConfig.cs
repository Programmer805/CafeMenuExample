﻿﻿﻿using System.Web;
using System.Web.Mvc;
using CafeMenu.Infrastructure;

namespace CafeMenu
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Turkish character encoding is handled in BaseController
        }
    }
}
