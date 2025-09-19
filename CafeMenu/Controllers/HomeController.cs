﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CafeMenu.Infrastructure;
using Domain.Interfaces.Services;
using System.Threading.Tasks;

namespace CafeMenu.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ITenantService _tenantService;

        public HomeController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        public async Task<ActionResult> Index()
        {
            // Önce sistemde tenant var mı kontrol et - setup gerekli mi?
            var tenants = await _tenantService.GetAllAsync();
            if (tenants == null || !tenants.Any())
            {
                // Hiç tenant yoksa kurulum sayfasına yönlendir
                return RedirectToAction("Index", "Setup");
            }

            // Varsayılan tenant ID'si ile menü sayfasına yönlendir
            var tenantId = TenantResolver.GetCurrentTenantId();
            
            // Eğer tenant ID yoksa varsayılan olarak 1 kullan
            if (tenantId <= 0)
            {
                tenantId = 1;
            }
            
            // Menü sayfasına tenant ID ile yönlendir
            return RedirectToAction("Index", "Menu", new { tenantId = tenantId });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}