using Bitzar.CMS.Core.Controllers;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Model;
using MethodCache.Attributes;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize notification functions
    /// </summary>
    // [Cache(Members.All)]
    public class Notification : Cacheable, IEmail
    {
        /// <summary>
        /// Método que carrega o template e o converte em uma string para ser enviada por e-mail
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string LoadTemplate(string templateName, dynamic data)
        {
            //Lê o template padrão
            var template = CMS.Functions.Templates.FirstOrDefault(t => t.Name == templateName);
            var templateData = System.IO.File.ReadAllText(HostingEnvironment.MapPath($"{template.Path}/{templateName}"));

            // Configura RazorEngine service
            var config = new TemplateServiceConfiguration();
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;

            //Convert o template em uma string
            var content = Engine.Razor.RunCompile(templateData, templateName, null, (object)data);
            return content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="mailTo"></param>
        /// <param name="mailCc"></param>
        /// <param name="mailBcc"></param>
        /// <param name="mailReply"></param>
        /// <param name="attachments"></param>
        public void SendNotification(string template, string subject, string[] mailTo, string[] mailCc = null, string[] mailBcc = null, string[] mailReply = null, HttpFileCollectionBase attachments = null, UrlFileAttachment[] urlAttachements = null)
        {
            Task.Run(async() =>
            {
                await Mail.SendNotification(mailTo, mailCc, mailBcc, mailReply, subject, template, true, attachments,urlAttachements);
            });
        }

    }
}