using CafeMenu.Infrastructure;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// İlk kurulum için Setup Controller
    /// Sistemde tenant yoksa kurulum sayfası gösterir
    /// </summary>
    [AllowAnonymous]
    public class SetupController : BaseController
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;

        public SetupController(ITenantService tenantService, IUserService userService)
        {
            _tenantService = tenantService;
            _userService = userService;
        }

        // GET: Setup
        public async Task<ActionResult> Index()
        {
            // Sistemde tenant var mı kontrol et
            var tenants = await _tenantService.GetAllAsync();
            if (tenants != null && tenants.Any())
            {
                // Tenant varsa ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }

            // İlk kurulum sayfasını göster
            return View();
        }

        // POST: Setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(SetupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Sistemde zaten tenant var mı kontrol et
                    var existingTenants = await _tenantService.GetAllAsync();
                    if (existingTenants != null && existingTenants.Any())
                    {
                        TempData["ErrorMessage"] = "Sistem zaten kurulmuş. Ana sayfaya yönlendiriliyorsunuz.";
                        return RedirectToAction("Index", "Home");
                    }

                    // İlk tenant'ı oluştur
                    var tenantCreateDto = new TenantCreateDto
                    {
                        TenantName = model.TenantName,
                        IsActive = true
                    };

                    var tenant = await _tenantService.CreateAsync(tenantCreateDto);
                    if (tenant == null)
                    {
                        ModelState.AddModelError("", "Tenant oluşturulurken bir hata oluştu.");
                        return View(model);
                    }

                    // İlk admin kullanıcısını oluştur
                    var userCreateDto = new UserCreateDto
                    {
                        TenantID = tenant.ID,
                        Name = model.FirstName,
                        Surname = model.LastName,
                        Username = model.Username,
                        Password = model.Password
                    };

                    var user = await _userService.CreateAsync(userCreateDto);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Kullanıcı oluşturulurken bir hata oluştu.");
                        return View(model);
                    }

                    // Başarılı kurulum mesajı
                    TempData["SuccessMessage"] = $"Hoş geldiniz! '{tenant.TenantName}' sistemi başarıyla kuruldu.";
                    
                    // Kullanıcıyı otomatik login yap ve dashboard'a yönlendir
                    Session["UserId"] = user.ID;
                    Session["UserName"] = user.FullName;
                    Session["TenantId"] = tenant.ID;
                    
                    // Forms Authentication ticket oluştur
                    var authTicket = new System.Web.Security.FormsAuthenticationTicket(
                        1, // version
                        user.Username, // username
                        System.DateTime.Now, // issue time
                        System.DateTime.Now.AddHours(8), // expiration
                        false, // persistent
                        $"{user.ID}|{tenant.ID}" // user data
                    );
                    
                    var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(authCookie);
                    
                    return RedirectToAction("Index", "Admin");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", $"Kurulum sırasında bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Setup/Check - AJAX için tenant kontrol endpoint'i
        public async Task<JsonResult> CheckSetupRequired()
        {
            var tenants = await _tenantService.GetAllAsync();
            var setupRequired = tenants == null || !tenants.Any();
            
            return Json(new { SetupRequired = setupRequired }, JsonRequestBehavior.AllowGet);
        }
    }

    /// <summary>
    /// İlk kurulum için ViewModel
    /// </summary>
    public class SetupViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tenant adı gereklidir.")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2, ErrorMessage = "Tenant adı 2-100 karakter arasında olmalıdır.")]
        public string TenantName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ad gereklidir.")]
        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
        public string FirstName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Soyad gereklidir.")]
        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
        public string LastName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
        public string Username { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre gereklidir.")]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}