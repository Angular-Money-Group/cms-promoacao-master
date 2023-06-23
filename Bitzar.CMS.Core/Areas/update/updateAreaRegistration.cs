using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.update
{
    public class updateAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "update";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "update_default",
                "update/{controller}/{action}/{id}",
                new { action = "Index", controller = "Default", id = UrlParameter.Optional }
            );
        }
    }
}