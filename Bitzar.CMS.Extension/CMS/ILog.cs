using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Extension.CMS
{
    public interface ILog
    {
        string LogRequest(params object[] objects);
        string LogRequest(object content = null, string type = "Exception", string source = "Bitzar.CMS.Core", params object[] objects);
        void SaveLogLink(int ReferenceId, string ReferenceType, string Source, string Type, string Url, string Description = null);
    }
}
