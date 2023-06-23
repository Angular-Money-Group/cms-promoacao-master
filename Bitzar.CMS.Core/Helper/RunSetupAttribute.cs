using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core.Helper
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class RunSetupAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método acionado toda vez que uma ação é iniciada no sistema
        /// </summary>
        /// <param name="filterContext">contexto onde a ação é requisitada</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (MvcApplication.MustRunSetup)
                filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary(new
                            {
                                controller = "Default",
                                action = "Index",
                                area = "Install"
                            }));

            base.OnActionExecuting(filterContext);
        }
    }
}