using Bitzar.CMS.Core.Controllers;
using Bitzar.CMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthenticateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método acionado toda vez que uma ação é requisitada no sistema
        /// </summary>
        /// <param name="filterContext">contexto onde a ação é requisitada</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // If membership is not enabled, ignore validations and go ahead
                if (!Functions.CMS.Configuration.MembershipEnabled)
                    return;

                var route = Functions.CMS.Functions.MatchRoute(filterContext.HttpContext.Request.Url);
                if (route == null)
                    return;

                // Check if the page exists and has access restriction
                var lang = route.Language;
                var id = route.PageUrl;

                var templates = Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "View");
                var page = route.Page;
                if (page == null || !page.Restricted)
                    return;

                // Create login Page Configuration
                var time = Convert.ToInt32(Functions.CMS.Configuration.Get("ExpirationTime") ?? "60");
                var renew = (Functions.CMS.Configuration.Get("UseSlidingExpiration") == "true");
                var loginPage = templates.FirstOrDefault(p => p.Name == Functions.CMS.Configuration.Get("LoginPage"));
                if (loginPage == null)
                    return;

                // Get Authorization
                var authorization = Functions.CMS.Membership.Authorization;
                var date = DateTimeOffset.Now;

                // if there is no authorization, redirect to login
                if (authorization == null || ((date - authorization.LastRequest).TotalMinutes > time))
                {
                    var section = loginPage.Section?.Url;
                    var routeName = GetRouteName(lang, section);

                    var baseUrl = HttpContext.Current.Request.Url.ToString();
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(baseUrl);
                    var returnUrl = System.Convert.ToBase64String(plainTextBytes);

                    filterContext.Result = new RedirectToRouteResult(routeName,
                        new RouteValueDictionary(new
                        {
                            controller = "Main",
                            action = "PageRenderer",
                            id = loginPage.Url,
                            section = section,
                            lang = lang,
                            returnUrl = returnUrl
                        }));
                    return;
                }

                // The user Credentials are still valid, so update the last Request if is still valid
                if (renew)
                    authorization.LastRequest = date;

                // Check if the member has to change password
                if (authorization.User.ChangePassword)
                {
                    var alterPasswordName = Functions.CMS.Configuration.Get("AlterPasswordPage");
                    var changePasswordPage = templates.FirstOrDefault(t => t.Name == alterPasswordName);
                    if (changePasswordPage != null)
                    {
                        if (changePasswordPage == page)
                            return;

                        var section = changePasswordPage.Section?.Url;
                        var routeName = GetRouteName(lang, section);

                        filterContext.Result = new RedirectToRouteResult(routeName,
                            new RouteValueDictionary(new
                            {
                                controller = "Main",
                                action = "PageRenderer",
                                id = changePasswordPage.Url,
                                section = section,
                                lang = lang,
                                returnUrl = HttpContext.Current.Request.Url
                            }));
                        return;
                    }
                }

                // Check if the member has to complete their own profile data
                if (!authorization.User.Completed)
                {
                    var profilePageName = Functions.CMS.Configuration.Get("ProfilePage");
                    var profilePage = templates.FirstOrDefault(t => t.Name == profilePageName);
                    if (profilePage != null)
                    {
                        if (profilePage == page)
                            return;

                        var section = profilePage.Section?.Url;
                        var routeName = GetRouteName(lang, section);

                        filterContext.Result = new RedirectToRouteResult(routeName,
                            new RouteValueDictionary(new
                            {
                                controller = "Main",
                                action = "PageRenderer",
                                id = profilePage.Url,
                                section = section,
                                lang = lang,
                                returnUrl = HttpContext.Current.Request.Url
                            }));
                        return;
                    }
                }

                // Validate if the page is restricted but is available to all logged users
                if (string.IsNullOrWhiteSpace(page.RoleRestriction))
                    return;

                // Validate if the user can access the page requested
                var roles = page.RoleRestriction.Split(',').Select(i => Convert.ToInt32(i)).ToArray();
                if (!roles.Contains(authorization.User.Role.Id))
                {
                    var unauthorizedPage = templates.FirstOrDefault(t => t.Name == "401.cshtml") ?? templates.FirstOrDefault(t => t.Url == null);

                    var section = unauthorizedPage.Section?.Url;
                    var routeName = GetRouteName(lang, section);

                    filterContext.Result = new RedirectToRouteResult(routeName,
                        new RouteValueDictionary(new
                        {
                            controller = "Main",
                            action = "PageRenderer",
                            id = unauthorizedPage.Url,
                            section = section,
                            lang = lang
                        }));
                }
            }
            finally
            {
                base.OnActionExecuting(filterContext);
            }
        }

        private string GetRouteName(object lang, string section)
        {
            if (lang != null && section != null)
                return "LangSection";

            if (lang != null)
                return "Lang";

            if (section != null)
                return "Section";

            return "Default";
        }
    }
}