using CafeMenu.Infrastructure;
using System.Text;
using System.Web.Mvc;

namespace CafeMenu.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// Mevcut tenant ID'sini al
        /// </summary>
        protected int CurrentTenantId
        {
            get { return TenantResolver.GetCurrentTenantId(); }
        }
        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure UTF-8 encoding for Turkish characters
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
            
            // Set proper content type with UTF-8 charset
            if (Request.AcceptTypes != null)
            {
                foreach (var acceptType in Request.AcceptTypes)
                {
                    if (acceptType.Contains("text/html"))
                    {
                        Response.ContentType = "text/html; charset=utf-8";
                        break;
                    }
                }
            }
            
            base.OnActionExecuting(filterContext);
        }
        
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            // Ensure JSON responses also use UTF-8
            return base.Json(data, contentType ?? "application/json; charset=utf-8", Encoding.UTF8, behavior);
        }
        
        /// <summary>
        /// Tenant-aware JSON result döndürür
        /// </summary>
        protected JsonResult TenantJsonResult(object data)
        {
            return Json(new
            {
                TenantId = CurrentTenantId,
                Data = data,
                Success = true
            }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// Hata durumunda JSON result döndürür
        /// </summary>
        protected JsonResult ErrorJsonResult(string message, object data = null)
        {
            return Json(new
            {
                TenantId = CurrentTenantId,
                Data = data,
                Success = false,
                Error = message
            }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// Mevcut kullanıcı ID'sini al (Session'dan)
        /// </summary>
        protected int GetCurrentUserId()
        {
            // Session'dan kullanıcı ID'sini al
            if (Session["UserId"] != null)
            {
                return (int)Session["UserId"];
            }
            return 1; // Varsayılan admin user
        }
    }
}