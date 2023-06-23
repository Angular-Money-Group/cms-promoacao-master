using Newtonsoft.Json;

namespace Bitzar.Products.Helper
{
    public class FilterField
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}