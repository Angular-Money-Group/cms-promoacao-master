using Bitzar.CMS.Core.Models;
using System.Collections.Generic;

namespace Bitzar.Products.Models
{
    public class FilteredResult
    {
        public PaggedResult<Product> Pagged { get; set; }
        public List<Filter> Filters { get; set; }
        public int CountPaggedProducts { get; set; }
    }

    public class Filter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}