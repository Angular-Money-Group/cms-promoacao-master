using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class CacheController : AdminBaseController
    {
        [Route("Cache")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(Functions.CMS.Cache.AllKeys);
        }

        [Route("Cache/Clean")]
        [HttpGet]
        public ActionResult CleaningCache(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                Functions.CMS.ClearCache(key);
            else
            {
                foreach (var item in MvcApplication.GlobalCache.AllKeys)
                    Functions.CMS.ClearCache(item);

                Functions.CMS.ClearRoutes();
            }

            this.NotifySuccess(Resources.Strings.Cache_Exclude);
            return RedirectToAction(nameof(Index));
        }

        [Route("Cache/Clean-Routes")]
        [HttpGet]
        public ActionResult CleanRoutes()
        {
            Functions.CMS.ClearRoutes();

            this.NotifySuccess(Resources.Strings.Cache_Exclude);
            return RedirectToAction(nameof(Index));
        }
    }
}