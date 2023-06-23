using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Resources;
using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Http.Controllers;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Custom Api Validation Logic to handle logged in users or not for every request
    /// </summary>
    public sealed class ApiAdminAuthorizationAttribute : System.Web.Http.AuthorizeAttribute
    {
        /// <summary>
        /// Implementation of custom validation
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                /* Check if the user is authenticated */
                var user = AuthenticationHelper.GetUser();

                /* Check if the variable still authenticated */
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                    throw new UnauthorizedAccessException(Strings.Membership_UserNotLogged);

                /* Check if the variable still authenticated */
                if (!user.AdminAccess || user.IdRole == 3)
                    throw new UnauthorizedAccessException(Strings.Membership_ActionNotAllowed);
            }
            catch (SecurityException scException)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.UpgradeRequired, new { error = scException.Message });
            }
            catch (UnauthorizedAccessException uaException)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, new { error = uaException.Message });
            }
        }
    }
}