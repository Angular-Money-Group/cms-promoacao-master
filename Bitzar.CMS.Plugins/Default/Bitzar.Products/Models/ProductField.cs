using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Bitzar.Products.Models
{
    public class ProductField
    {
        public int IdField { get; set; }
        public int IdFieldGroup { get; set; }
        public string FieldGroup { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public bool Order { get; set; }


        public IList<Format> FormatObject
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Format))
                    return new List<Format>();

                var json = (JArray)JsonConvert.DeserializeObject(Format);
                return json.Select(x => new Format()
                {
                    Column = x.Value<string>("Column"),
                    Required = x.Value<bool>("Required"),
                    Type = x.Value<string>("Type")
                }).ToList();
            }
        }
    }
}