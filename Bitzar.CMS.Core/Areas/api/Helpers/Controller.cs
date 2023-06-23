using Bitzar.CMS.Data;
using System.Net;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    public abstract class Controller : System.Web.Mvc.Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Functions.CMS.Configuration.Get("EnsureIsAuthenticated").Contains("true"))
                return;

            if (!User.Identity.IsAuthenticated)
                filterContext.Result = Json(new
                {
                    code = (int)HttpStatusCode.Unauthorized,
                    message = HttpStatusCode.Unauthorized.ToString()
                }, JsonRequestBehavior.AllowGet);
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new CustomJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
    }
}