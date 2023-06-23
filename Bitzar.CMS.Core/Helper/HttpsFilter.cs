using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Helper
{
    public class HttpsFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if DEBUG
            return;
#endif
            if (!filterContext.HttpContext.Request.IsSecureConnection && Functions.CMS.Configuration.EnforceSSL)
            {
                var url = filterContext.HttpContext.Request.Url.ToString().Replace("http:", "https:");
                filterContext.Result = new RedirectResult(url);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}