using Bitzar.CMS.Core.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.update.Controllers
{
    [RouteArea("Update", AreaPrefix = "")]
    public class DefaultController : Controller
    {
        Functions.Internal.Log log = new Functions.Internal.Log();
        /// <summary>
        /// Check for update available
        /// </summary>
        /// <returns></returns>
        [Route("Update/Index")]
        public ActionResult Index()
        {
            try
            {
                ViewBag.Version = Path.GetFileNameWithoutExtension(UpdateHelper.CheckNewVersion().OrderByDescending(x => x).FirstOrDefault());
                return View();
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                log.LogRequest(parameters);
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Apply updates
        /// </summary>
        /// <returns></returns>
        [Route("Update/Apply")]
        public ActionResult Apply()
        {
            try
            {
                UpdateHelper.Update();
                return RedirectToAction("Login", "Authentication", new { area = "admin" });
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                log.LogRequest(parameters);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}