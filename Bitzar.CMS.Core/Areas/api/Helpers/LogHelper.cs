using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Http.Controllers;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Log
    /// </summary>
    public static class LogHelper
    {
        private static readonly Functions.Internal.Log log = new Functions.Internal.Log();

        /// <summary>
        /// Method responsible for logging web api
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="controllerContext"></param>
        /// <param name="methodBase"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void Register(Exception exception, HttpControllerContext controllerContext, string methodBase, object data = null)
        {
            try
            {
                var parameters = new
                {
                    Exception = exception.AllMessages(),
                    Controller = controllerContext.ControllerDescriptor.ControllerName,
                    Action = methodBase,
                    Url = controllerContext.Request.RequestUri.AbsoluteUri,
                    Data = data
                };

                log.LogRequest(parameters, source: "Bitzar.CMS.Api");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}