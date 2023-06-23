using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Model;

namespace Bitzar.CMS.Extension.Classes
{
    public class Route : IRoute
    {
        public string IdRoute { get; set; }
        public Template Page { get; set; }
        public string Url { get; set; }
        public Language Language { get; set; }
        public string Section { get; set; }
        public string[] Parameters { get;set; }
        public string Type { get; set; }
        public SearchResult Search { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
