using Bitzar.CMS.Core.Helper;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Base Controller for Web API
    /// </summary>
    [Statistic]
    [ThrottlingApi(TimeUnit.Instantly), ThrottlingApi(TimeUnit.Minutely), ThrottlingApi(TimeUnit.Hourly), ThrottlingApi(TimeUnit.Daily)]
    public class BaseController : ApiController
    {
        /// <summary>
        /// Returns default OK (200) object to the service
        /// </summary>
        /// <typeparam name="T">Type of resulting object</typeparam>
        /// <param name="result">Result to be added as the output of the service</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> CreateResponse(object result = default)
        {
            if (result is string)
                return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, new { message = result }));

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, result));
        }

        /// <summary>
        /// Return an error incapsulated response providing the necessary status code to the service
        /// </summary>
        /// <param name="code">Code to be associated with the result</param>
        /// <param name="error">Error message to be returned</param>
        /// <param name="result">Any object desired to be added in the result</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> CreateErrorResponse(HttpStatusCode code, string error, object result = null)
        {
            if (result == null)
                return Task.FromResult(Request.CreateResponse(code, new { error }));

            return Task.FromResult(Request.CreateResponse(code, new { error, @object = result }));
        }
    }
}