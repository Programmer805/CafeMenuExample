using CafeMenu.Infrastructure;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// Super Admin - Tenant yönetimi Controller'ı
    /// Sadece sistem yöneticileri erişebilir
    /// </summary>
    [TenantAuthorize(RequireSameTenant = false)]
    public class TenantController : BaseController
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public TenantController(ITenantService tenantService, IUserService userService, 
            ICategoryService categoryService, IProductService productService)
        {
            _tenantService = tenantService;
            _userService = userService;
            _categoryService = categoryService;
            _productService = productService;
        }

        // GET: Tenant
        public async Task<ActionResult> Index()
        {
            var tenants = await _tenantService.GetAllAsync();
            return View(tenants);
        }

        // GET: Tenant/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }

            // Tenant istatistiklerini al
            var stats = await GetTenantStatisticsAsync(id);
            ViewBag.Statistics = stats;

            return View(tenant);
        }

        // GET: Tenant/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tenant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TenantCreateDto tenantCreateDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _tenantService.CreateAsync(tenantCreateDto);
                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Tenant '{result.TenantName}' başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Tenant oluşturulurken bir hata oluştu.");
            }

            return View(tenantCreateDto);
        }

        // GET: Tenant/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }

            var updateDto = new TenantUpdateDto
            {
                ID = tenant.ID,
                TenantName = tenant.TenantName,
                IsActive = tenant.IsActive
            };

            return View(updateDto);
        }

        // POST: Tenant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TenantUpdateDto tenantUpdateDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _tenantService.UpdateAsync(tenantUpdateDto);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Tenant başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Tenant güncellenirken bir hata oluştu.");
            }

            return View(tenantUpdateDto);
        }

        // GET: Tenant/Users/5 (Tenant kullanıcıları)
        public async Task<ActionResult> Users(int id)
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }

            var users = await _userService.GetAllAsync(id);
            
            ViewBag.Tenant = tenant;
            return View(users);
        }

        // POST: Tenant/CreateUser (AJAX)
        [HttpPost]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<JsonResult> CreateUser(UserCreateDto userCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new
                    {
                        Success = false,
                        Error = "Geçersiz veri: " + string.Join(", ", errors)
                    }, JsonRequestBehavior.AllowGet);
                }

                // Kullanıcı adının benzersizliğini kontrol et
                var usernameExists = await _userService.IsUsernameExistsAsync(userCreateDto.Username, userCreateDto.TenantID);
                if (usernameExists)
                {
                    return Json(new
                    {
                        Success = false,
                        Error = "Bu kullanıcı adı zaten kullanılıyor."
                    }, JsonRequestBehavior.AllowGet);
                }

                var result = await _userService.CreateAsync(userCreateDto);
                if (result != null)
                {
                    return Json(new 
                    { 
                        Success = true, 
                        Message = "Kullanıcı başarıyla oluşturuldu.",
                        User = result
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    Success = false,
                    Error = "Kullanıcı oluşturulurken bir hata oluştu."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = $"Beklenmeyen hata: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Tenant/UpdateUser (AJAX)
        [HttpPost]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<JsonResult> UpdateUser(UserUpdateDto userUpdateDto, int tenantId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return ErrorJsonResult("Geçersiz veri: " + string.Join(", ", errors));
                }

                var result = await _userService.UpdateAsync(userUpdateDto, tenantId);
                if (result != null)
                {
                    return TenantJsonResult(new 
                    { 
                        Success = true, 
                        Message = "Kullanıcı başarıyla güncellendi.",
                        User = result
                    });
                }

                return ErrorJsonResult("Kullanıcı güncellenirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                return ErrorJsonResult($"Beklenmeyen hata: {ex.Message}");
            }
        }

        // POST: Tenant/DeleteUser (AJAX)
        [HttpPost]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<JsonResult> DeleteUser(int userId, int tenantId)
        {
            try
            {
                var success = await _userService.DeleteAsync(userId, tenantId);
                if (success)
                {
                    return TenantJsonResult(new 
                    { 
                        Success = true, 
                        Message = "Kullanıcı başarıyla silindi."
                    });
                }

                return ErrorJsonResult("Kullanıcı silinirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                return ErrorJsonResult($"Beklenmeyen hata: {ex.Message}");
            }
        }

        // AJAX: Tenant/CheckUsername
        [HttpPost]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<JsonResult> CheckUsername(string username, int tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Json(new { Available = false, Message = "Kullanıcı adı gereklidir." }, JsonRequestBehavior.AllowGet);
                }

                var exists = await _userService.IsUsernameExistsAsync(username, tenantId);
                return Json(new 
                { 
                    Available = !exists, 
                    Message = exists ? "Bu kullanıcı adı zaten kullanılıyor." : "Kullanıcı adı uygun." 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Available = false, Message = $"Kontrol edilemedi: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Tenant/ToggleStatus
        [HttpPost]
        public async Task<JsonResult> ToggleStatus(int id)
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            if (tenant == null)
            {
                return ErrorJsonResult("Tenant bulunamadı.");
            }

            var updateDto = new TenantUpdateDto
            {
                ID = tenant.ID,
                TenantName = tenant.TenantName,
                IsActive = !tenant.IsActive
            };

            var result = await _tenantService.UpdateAsync(updateDto);
            if (result != null)
            {
                var message = result.IsActive ? "Tenant aktifleştirildi." : "Tenant pasifleştirildi.";
                return TenantJsonResult(new { Success = true, Message = message, IsActive = result.IsActive });
            }

            return ErrorJsonResult("Tenant durumu güncellenemedi.");
        }

        // AJAX: Tenant/GetStatistics
        public async Task<JsonResult> GetStatistics(int id)
        {
            var stats = await GetTenantStatisticsAsync(id);
            return TenantJsonResult(stats);
        }

        #region Helper Methods

        private async Task<TenantStatistics> GetTenantStatisticsAsync(int tenantId)
        {
            // Her service'den ilgili istatistikleri al
            var userCount = await _userService.GetTotalCountAsync(tenantId);
            var categoryCount = await _categoryService.GetTotalCountAsync(tenantId);
            var productCount = await _productService.GetTotalCountAsync(tenantId);

            return new TenantStatistics
            {
                TenantId = tenantId,
                UserCount = userCount,
                CategoryCount = categoryCount,
                ProductCount = productCount,
                LastActivityDate = DateTime.UtcNow // Gerçek implementasyonda aktivite tablosundan gelecek
            };
        }

        #endregion
    }

    /// <summary>
    /// Tenant istatistik modeli
    /// </summary>
    public class TenantStatistics
    {
        public int TenantId { get; set; }
        public int UserCount { get; set; }
        public int CategoryCount { get; set; }
        public int ProductCount { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}