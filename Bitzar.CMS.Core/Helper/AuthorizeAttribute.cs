using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core.Helper
{
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public string LoginController { get; set; }
        public string LoginAction { get; set; }
        public string LoginArea { get; set; }

        /// <summary>
        /// Método que é acionado toda vez que uma página é requisitada no sistema
        /// </summary>
        /// <param name="filterContext">contexto em que a página está sendo requisitada</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Validate if is an ajax request
            if (filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.HttpContext.Items["AjaxPermissionDenied"] = true;
            else
            {
                // Automatic redirects when detect that the user is unauthorized to access data.
                if (string.IsNullOrEmpty(LoginController))
                    LoginController = "Authentication";
                if (string.IsNullOrEmpty(LoginAction))
                    LoginAction = "Login";
                if (string.IsNullOrWhiteSpace(LoginArea))
                    LoginArea = "admin";

                // Redirect user context to login page
                filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary(new
                                        {
                                            controller = LoginController,
                                            action = LoginAction,
                                            area = LoginArea,
                                            returnUrl = HttpContext.Current.Request.Url
                                        }));

            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}