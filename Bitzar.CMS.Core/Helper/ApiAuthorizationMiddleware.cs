using Bitzar.CMS.Core.Configurations;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Middleware responsible to intercept any context execution inside OWIN
    /// and format the specific result to the caller.
    /// </summary>
    public class ApiAuthorizationMiddleware : OwinMiddleware
    {
        public ApiAuthorizationMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            // If not status code from OWIN (400) and does not have the challenge key, ignore
            if (context.Response.StatusCode != 400 || !context.Response.Headers.ContainsKey(ApiAuthorizationServerProvider.OwinChallenge))
                return;

            // Format the result with the challenge provided status code
            var values = context.Response.Headers.GetValues(ApiAuthorizationServerProvider.OwinChallenge);
            context.Response.Headers.Remove(ApiAuthorizationServerProvider.OwinChallenge);

            context.Response.StatusCode = int.Parse(values.FirstOrDefault() ?? ((int)HttpStatusCode.BadRequest).ToString());
        }
    }
}