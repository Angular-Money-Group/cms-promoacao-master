using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Helper to process the Social Login requests
    /// </summary>
    public class SocialLoginHelper
    {
        /// <summary>
        /// Ensure that social login user belongs to the accessed access token
        /// </summary>
        /// <param name="type">Type of login social to validate</param>
        /// <param name="userId">User Id for social login identification if need</param>
        /// <param name="user">User parameter to validate the token and login data</param>
        /// <param name="token">Token provided by the user</param>
        /// <returns></returns>
        internal async static Task EnsureAccessTokenBelongsUser(string type, string userId, string user, string token)
        {
            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

                // Set default header data
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Validate the request for each provider
                switch (type.ToLower())
                {
                    case "facebook":
                        // Create service url
                        var url = $"https://graph.facebook.com/{userId}?fields=id,name,email&access_token={token}";
                        var response = await client.GetAsync(url);

                        // Ensure request is ok and get response
                        response.EnsureSuccessStatusCode();
                        var result = await response.Content.ReadAsStringAsync();

                        // Try parse the result
                        var jObject = JObject.Parse(result);

                        // validate if the result belongs to user
                        if (jObject["email"].Value<string>() == user)
                            return;

                        throw new UnauthorizedAccessException(Resources.Strings.Membership_SocialInformationMismatch);

                    // Provider not implemented yet.
                    default:
                        throw new Exception(Resources.Strings.Membership_SocialProviderNotFound);
                }
            }
        }
    }
}