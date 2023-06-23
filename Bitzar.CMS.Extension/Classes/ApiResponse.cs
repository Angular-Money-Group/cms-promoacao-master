using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Classes
{
    /// <summary>
    /// Class to be used to return API data to the system
    /// </summary>
    public class ApiResponse
    {
        private static JsonSerializerSettings Settings => new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, MaxDepth = 3, NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Indicates the current response code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Property to hold any message to the caller
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StatusResponse { get; set; }
        /// <summary>
        /// Property that indicates the response date and time in UTC format
        /// </summary>
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
        /// <summary>
        /// Property to store an exception to be returned to the system
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public dynamic Error { get; set; }
        /// <summary>
        /// Property to return any associated data with the response
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public dynamic Data { get; set; }

        /// <summary>
        /// Method to create and return a new API Response data
        /// </summary>
        /// <param name="code">Http Status code to be returned</param>
        /// <param name="data">Data with any kind of information desired</param>
        /// <param name="message">Any message to be returned to the service</param>
        /// <param name="error">Error information if has anything</param>
        /// <returns></returns>
        public static ApiResponse Create(HttpStatusCode code = HttpStatusCode.OK, dynamic data = null, string message = null, Exception error = null)
        {
            return new ApiResponse()
            {
                StatusCode = code,
                StatusResponse = message,
                Data = data,
                Error = error
            };
        }
    }
}
