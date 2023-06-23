using Newtonsoft.Json;

namespace Bitzar.CMS.Core.Areas.api.Models
{
    public class UserFieldModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}