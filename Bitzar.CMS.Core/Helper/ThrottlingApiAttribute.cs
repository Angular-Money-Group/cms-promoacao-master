using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Request throttling attribute to prevent DDoS attacks 
	/// or to make sure no one tries to brute-force-use your api.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ThrottlingApiAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Indicates the time unit to be checked
        /// </summary>
        private TimeUnit Unit { get; set; }

        /// <summary>
        /// Default ctor for the time unit
        /// </summary>
        /// <param name="timeUnit"></param>
        public ThrottlingApiAttribute(TimeUnit timeUnit)
        {
            this.Unit = timeUnit;
        }

        /// <summary>
        /// Logic to process the request and throttle data in the service
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(HttpActionContext context)
        {
            try
            {
                // Controller and action name
                var controller = context.ActionDescriptor.ControllerDescriptor.ControllerName;
                var action = context.ActionDescriptor.ActionName;

                // Validate
                ThrottlingHelper.Validate(this.Unit, controller, action);
            }
            catch (AccessViolationException ex)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.Forbidden, ex.Message);
            }
        }
    }
}