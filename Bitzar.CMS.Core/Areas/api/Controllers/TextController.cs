using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Helper;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [ApiAuthorization]
    [RoutePrefix("api/v1")]
    public class TextController : BaseController
    {
        /// <summary>
        /// Endpoint to list field values available
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("i18n-text/{lang?}")]
        public async Task<HttpResponseMessage> Text(string lang = null)
        {
            try
            {
                var response = Helpers.TextHelper.Text(lang);
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}