using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    public class JsonResponse
    {
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public dynamic Result { get; set; }
    }
}