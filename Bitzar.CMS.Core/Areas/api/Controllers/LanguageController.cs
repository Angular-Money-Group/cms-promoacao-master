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
    public class LanguageController : BaseController
    {
        /// <summary>
        /// Endpoint responsible for listing the available languages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("languages")]
        public async Task<HttpResponseMessage> Available()
        {
            try
            {
                var response = LanguageHelper.Available();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Enpoint responsible for obtaining the default language
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("languages/default")]
        public async Task<HttpResponseMessage> Default()
        {
            try
            {
                var response = LanguageHelper.Default();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}