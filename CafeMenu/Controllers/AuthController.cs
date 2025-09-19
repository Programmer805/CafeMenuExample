using CafeMenu.Infrastructure;
using Domain.DTOs;
using Domain.Interfaces.Services;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace CafeMenu.Controllers
{
    /// <summary>
    /// Authentication Controller - Kullanıcı girişi ve çıkışı
    /// </summary>
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;

        public AuthController(IUserService userService, ITenantService tenantService)
        {
            _userService = userService;
            _tenantService = tenantService;
        }

        // GET: Auth/Login
        public ActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            
            // Eğer zaten giriş yapmışsa admin paneline yönlendir
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin", new { area = "" });
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLoginDto loginDto, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.LoginAsync(loginDto);
                if (user != null)
                {
                    // Tenant bilgisini session'a kaydet
                    TenantResolver.SetTenantIdToSession(user.TenantID);
                    Session["UserId"] = user.ID;
                    Session["UserName"] = user.FullName;
                    Session["TenantName"] = user.TenantName;

                    // Forms Authentication cookie oluştur
                    var userData = $"{user.ID}|{user.TenantID}|{user.Username}";
                    var ticket = new FormsAuthenticationTicket(
                        1,
                        user.Username,
                        System.DateTime.Now,
                        System.DateTime.Now.AddHours(8), // 8 saat geçerli
                        false, // Remember me (şimdilik false)
                        userData,
                        FormsAuthentication.FormsCookiePath
                    );

                    var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(cookie);

                    TempData["SuccessMessage"] = $"Hoşgeldiniz, {user.FullName}!";

                    // Return URL varsa oraya, yoksa dashboard'a yönlendir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Admin", new { area = "" });
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(loginDto);
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            // Session temizle
            Session.Clear();
            Session.Abandon();
            
            // Tenant bilgisini temizle
            TenantResolver.ClearTenantFromSession();
            
            // Forms Authentication cookie'sini temizle
            FormsAuthentication.SignOut();

            TempData["InfoMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
        }

        // GET: Auth/Register (Yeni kullanıcı kaydı)
        public async Task<ActionResult> Register()
        {
            // Aktif tenant'ları al (kullanıcı hangi tenant'a kayıt olacağını seçecek)
            var tenants = await _tenantService.GetActiveTenantsAsync();
            ViewBag.Tenants = new SelectList(tenants, "ID", "TenantName");
            
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserCreateDto registerDto)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı kontrol et
                if (await _userService.IsUsernameExistsAsync(registerDto.Username, registerDto.TenantID))
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılıyor.");
                }
                else
                {
                    var user = await _userService.CreateAsync(registerDto);
                    if (user != null)
                    {
                        TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                        return RedirectToAction("Login");
                    }

                    ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu.");
                }
            }

            // Hata durumunda tenant listesini tekrar yükle
            var tenants = await _tenantService.GetActiveTenantsAsync();
            ViewBag.Tenants = new SelectList(tenants, "ID", "TenantName");
            
            return View(registerDto);
        }

        // GET: Auth/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("", "Kullanıcı adı gereklidir.");
                return View();
            }

            // Gerçek implementasyonda burada:
            // 1. Kullanıcıyı bul
            // 2. Password reset token oluştur
            // 3. Email gönder
            
            TempData["InfoMessage"] = "Eğer bu kullanıcı adı sistemde kayıtlıysa, şifre sıfırlama bağlantısı email adresinize gönderilmiştir.";
            return RedirectToAction("Login");
        }

        // GET: Auth/AccessDenied
        public ActionResult AccessDenied()
        {
            return View();
        }

        // GET: Auth/TenantNotFound
        public ActionResult TenantNotFound()
        {
            return View();
        }

        // AJAX: Auth/CheckUsername
        public async Task<JsonResult> CheckUsername(string username, int tenantId)
        {
            if (string.IsNullOrEmpty(username))
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

        // AJAX: Auth/GetTenantInfo
        public async Task<JsonResult> GetTenantInfo(int tenantId)
        {
            var tenant = await _tenantService.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return Json(new { Success = false, Message = "Tenant bulunamadı." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new 
            { 
                Success = true, 
                TenantName = tenant.TenantName,
                IsActive = tenant.IsActive
            }, JsonRequestBehavior.AllowGet);
        }
    }
}