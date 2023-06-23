using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Bitzar.CMS.Core.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método acionado toda vez que uma ação é requisitada no sistema
        /// </summary>
        /// <param name="filterContext">contexto onde a ação é requisitada</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = HttpContext.Current;
            var session = (HttpSessionStateBase)new HttpSessionStateWrapper(ctx.Session);

            /// Validate if the session still alive
            if (!session.Get<bool>(nameof(ctx.User.Identity.IsAuthenticated)) || !filterContext.HttpContext.Request.IsAuthenticated)
            {
                // Clear session data and call form logout
                FormsAuthentication.SignOut();
                session.Clear();

                // Return data when dealing with ajax request or just call the logout accounts page.
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                    filterContext.HttpContext.Items["AjaxPermissionDenied"] = true;
                else
                {
                    //filterContext.Controller.TempData[Notification.NOTIFICATION] =
                    //new Notification(String.Empty, "Sessão expirada!", Notification.Type.DANGER, false);

                    filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary(new
                            {
                                controller = "Authentication",
                                action = "Logoff",
                                area = "admin",
                                returnUrl = HttpContext.Current.Request.Url.PathAndQuery
                            }));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}