using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Areas.api.Models;
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
    public class GlobalController : BaseController
    {
        /// <summary>
        /// Endpoint responsible for listing the Field Values
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("global/{lang?}")]
        public async Task<HttpResponseMessage> FieldValues(string lang = null)
        {
            try
            {
                var response = GlobalHelper.ListValues(lang);
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for listing the Field Types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("global/field-types")]
        public async Task<HttpResponseMessage> FieldTypes()
        {
            try
            {
                var response = GlobalHelper.ListTypes();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for send email for passengers not registered
        /// </summary>
        /// <returns></returns>
        [Route("global/emailPassengerUnregistered")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> PassengerUnregistered()
        {
            try
            {
                dynamic method = "GET";
                
                Functions.CMS.Events.Trigger("emailPassengerUnregistered", method);

                return await CreateResponse("OK");
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}