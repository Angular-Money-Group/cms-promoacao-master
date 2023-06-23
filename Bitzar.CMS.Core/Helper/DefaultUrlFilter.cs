using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Helper
{
    public class DefaultUrlFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

#if DEBUG
            return;
#endif

            //Variável que armazena o host url atual do site ("www.exemplo.com")
            var urlHost = filterContext.HttpContext.Request.Url.Host;
            //Variável que carrega a url padrão definida no administrativo
            var urlDefault = Functions.CMS.Configuration.DefaultUrl;
            // Variável que armazena a url do site
            var url = filterContext.HttpContext.Request.Url.ToString();
            //Checa se o campo de url padrão não é vazio ou a url padrão é diferente da url atual
            var redirect = false;
            if (!string.IsNullOrWhiteSpace(urlDefault) && urlDefault != urlHost)
            {
                // Atribui o valor da url padrão na url atual se forem diferentes
                url = url.Replace(urlHost, urlDefault);
                redirect = true;
            }
            if (!filterContext.HttpContext.Request.IsSecureConnection && Functions.CMS.Configuration.EnforceSSL)
            {
                // Adiciona o Ssl
                url = url.Replace("http:", "https:");
                redirect = true;
            }
            //Redireciona para a url reformulada
            if (redirect)
                filterContext.Result = new RedirectResult(url);
        }
    }
}