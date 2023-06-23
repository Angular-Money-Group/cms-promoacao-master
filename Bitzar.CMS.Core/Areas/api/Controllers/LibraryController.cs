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
    public class LibraryController : BaseController
    {
        /// <summary>
        /// Enpoint responsible to list the types of libraries
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("library/types")]
        public async Task<HttpResponseMessage> Types()
        {
            try
            {
                var response = LibraryHelper.Types();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Enpoint responsible to list the objects of libraries
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("library/objects")]
        public async Task<HttpResponseMessage> Objects()
        {
            try
            {
                var response = LibraryHelper.Objects();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}