using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Resources;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [ApiAuthorization]
    [RoutePrefix("api/v1")]
    public class TemplateController : BaseController
    {
        /// <summary>
        /// Endpoint responsible for listing the Templates
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("templates")]
        public async Task<HttpResponseMessage> Templates()
        {
            try
            {
                var response = FunctionHelper.ListTemplates();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for listing the Template Fields
        /// </summary>
        /// <param name="idTemplate"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("templates/fields/{idTemplate:int:min(1)}/{lang?}")]
        public async Task<HttpResponseMessage> Fields(int idTemplate, string lang = null)
        {
            try
            {
                var userRoleId = AuthenticationHelper.GetClaim(Data.ClaimType.RoleId);
                var transactionCode = FunctionHelper.GetTemplateTransactionCode(idTemplate, User.Identity.IsAuthenticated, userRoleId);

                if (transactionCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(Strings.Membership_UserNotLogged);

                var response = GlobalHelper.ListValues(lang, idTemplate);
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}