using Bitzar.CMS.Data.Model;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core.Helper
{
    [Authorize(Order = 1), SessionExpire(Order = 2), HttpsFilter(Order = 3)]
    public class AdminBaseController : Controller
    {
        public new User User { get => Session.Get<User>(); }

        /// <summary>
        /// Internal method to get the current controller and action data
        /// </summary>
        /// <returns></returns>
        private Tuple<string,string> GetCurrentControllerAndAction()
        {
            var context = HttpContext.Request.RequestContext.RouteData;
            return Tuple.Create(context.GetRequiredString("controller"), context.GetRequiredString("action"));
        }

        /// <summary>
        /// Override the VIEW method that brings out the parameters
        /// </summary>
        /// <param name="viewName">View name that should be returned</param>
        /// <param name="masterName">Master name of the view to rendereize</param>
        /// <param name="model">Model to be attached in the view</param>
        /// <returns></returns>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            // Get current context controller and action
            var context = GetCurrentControllerAndAction();

            // If identify an Admin Component inside the database, return it to the front.
            var template = Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Name == $"_Admin_View_{context.Item1}_{context.Item2}.cshtml");
            if (template != null)
                return base.View(template.NameWithoutExtension(), "~/Areas/admin/Views/Shared/_Layout.cshtml", model);

            // Return the view base result data if not found
            return base.View(viewName, masterName, model);
        }

        /// <summary>
        /// Override the a partial view data to be returned
        /// </summary>
        /// <param name="viewName">View name that should be returned</param>
        /// <param name="masterName">Master name of the view to rendereize</param>
        /// <param name="model">Model to be attached in the view</param>
        /// <returns></returns>
        protected override PartialViewResult PartialView(string viewName, object model)
        {
            // Get current context controller and action
            var context = GetCurrentControllerAndAction();

            // If identify an Admin Component inside the database, return it to the front.
            var template = Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Name == $"_Admin_Partial_{context.Item1}{viewName}.cshtml");
            if (template != null)
                return base.PartialView(template.NameWithoutExtension(), model);

            // Return the view base result data if not found
            return base.PartialView(viewName, model);
        }
    }
}