using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IEmail
    {
        string LoadTemplate(string templateName, dynamic data);

        void SendNotification(string template, string subject, string[] mailTo, string[] mailCc = null, string[] mailBcc = null, string[] mailReply = null, HttpFileCollectionBase attachments = null, UrlFileAttachment[] urlAttachments = null);
    }
}
