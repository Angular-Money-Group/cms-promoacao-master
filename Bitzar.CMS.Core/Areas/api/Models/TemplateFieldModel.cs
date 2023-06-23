using System.Collections.Generic;
using System.Net;

namespace Bitzar.CMS.Core.Areas.api.Models
{
    public class TemplateFieldModel
    {
        public int Id { get; set; }
        public bool Restricted { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> RoleRestriction { get; set; }
    }
}