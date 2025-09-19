using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CafeMenu.Infrastructure
{
    public class EncodingActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure UTF-8 encoding for Turkish characters
            var response = filterContext.HttpContext.Response;
            if (response != null)
            {
                response.ContentEncoding = Encoding.UTF8;
                response.HeaderEncoding = Encoding.UTF8;
                
                // Set content type with UTF-8 charset
                if (string.IsNullOrEmpty(response.ContentType))
                {
                    response.ContentType = "text/html; charset=utf-8";
                }
                else if (!response.ContentType.Contains("charset"))
                {
                    response.ContentType += "; charset=utf-8";
                }
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}