using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.Products.Models
{
    public class GroupedSubProduct
    {
        public string SKU { get; set; }
        public string Description { get; set; }
        public string RouteUrl { get; set; }
        public bool Disabled { get; set; }
        public int IdType { get; set; }
        public List<int> Ids { get; set; }
    }
}