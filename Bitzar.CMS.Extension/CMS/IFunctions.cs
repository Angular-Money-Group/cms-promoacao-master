using System;
using System.Collections.Generic;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IFunctions
    {
        HashSet<RouteParam> Routes { get; }
        List<Template> Templates { get; }
        List<TemplateType> TemplateTypes { get; }
        string ExecuteUrl { get; }

        string BaseUrl(Uri url = null);
        string ContentDirectory(string type, string fileName);
        string Url(int id, string lang = null);
        string Url(string page, string lang = null);
    }
}