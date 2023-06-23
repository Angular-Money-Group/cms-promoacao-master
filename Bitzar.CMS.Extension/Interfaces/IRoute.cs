using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Interfaces
{
    public interface IRoute
    {
        string IdRoute { get; set; }
        Template Page { get; set; }
        string Url { get; set; }
        Language Language { get; set; }
        string Section { get; set; }
        string[] Parameters { get; set; }
        string Type { get; set; }
        SearchResult Search { get; set; }
        DateTime? LastModified { get; set; }
    }
}
