using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.install
{
    public class installAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "install";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "install_default",
                "install/{controller}/{action}/{id}",
                new { action = "Index", controller = "Default", id = UrlParameter.Optional }
            );
        }
    }
}