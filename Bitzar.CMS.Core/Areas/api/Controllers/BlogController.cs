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
    public class BlogController : BaseController
    {
        /// <summary>
        /// Endpoint responsible for listing the Posts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("blog/posts")]
        public async Task<HttpResponseMessage> Posts(int page = 1, int size = 10, string category = null)
        {
            try
            {
                var response = BlogHelper.Posts(page, size, category);
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for listing the Categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("blog/categories")]
        public async Task<HttpResponseMessage> Categories()
        {
            try
            {
                var response = BlogHelper.Categories();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}