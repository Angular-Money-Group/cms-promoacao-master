using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [ApiAuthorization]
    [RoutePrefix("api/v1")]
    public class PluginController : BaseController
    {
        /// <summary>
        /// Enpoint responsible to list the availables Plugins
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("plugins")]
        public async Task<HttpResponseMessage> Available()
        {
            try
            {
                var response = PluginHelpers.Availables();
                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint resposible for execute plugin
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("plugins/{source}/{function}")]
        public async Task<HttpResponseMessage> Execute([FromUri(Name = "source")] string source, [FromUri(Name = "function")] string function, [FromBody] dynamic body)
        {
            try
            {
                // Create the parameter list to be stored in the ViewBag
                var parameters = (Dictionary<string, string>)(body?.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>());

                /*
                 * If the user is authenticated generate a new RequestToken to be inserted in the plugin execution
                 * If not thus the token will be null and the security check will be handled inside the plugin
                 */
                string token = null;
                if (AuthenticationHelper.CurrentUserIsAuthenticated())
                {
                    // Set default request token
                    token = Functions.CMS.Security.RequestToken;

                    // Set the user data
                    var user = AuthenticationHelper.GetUser();
                    if (!user.AdminAccess)
                        HttpContext.Current.Session.Set(new MembershipAuthentication(user));
                    else
                        HttpContext.Current.Session.Set(user);
                }
                else
                {
                    token = Functions.CMS.Configuration.Token;
                    var auth = HttpContext.Current.Request.Headers["Authorization"]?.ToString();

                    if (string.IsNullOrEmpty(auth))
                    {
                        throw new Exception("Token não informado!");
                    }

                    var decAuth = Functions.CMS.Security.Decrypt(auth.Replace("Bearer ", ""));

                    if (decAuth != token)
                    {
                        throw new Exception("Token Expirado!");
                    }
                }

                // Return the output result to caller
                return await CreateResponse(PluginHelpers.Execute(source, function, token, parameters));
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
            finally
            {
                HttpContext.Current.Session[Extensions.NameOfType<MembershipAuthentication>()] = null;
                HttpContext.Current.Session[Extensions.NameOfType<Data.Model.User>()] = null;
            }
        }
    }
}