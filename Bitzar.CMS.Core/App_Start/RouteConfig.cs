using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
    {
            // Configure Routing System
            routes.MapMvcAttributeRoutes();
            routes.LowercaseUrls = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Configure default Processor Route
            #region Main Route configuration
            routes.MapRoute(
               name: "DefaultApi",
               url: "api/{controller}/{id}",
               defaults: new { id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Ajax",
                url: "ajax",
                defaults: new { controller = "Main", action = "Ajax" }
            );
            routes.MapRoute(
                name: "Execute",
                url: "execute",
                defaults: new { controller = "Main", action = "Execute" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{id}",
                defaults: new { controller = "Main", action = "PageRenderer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Section",
                url: "{section}/{id}",
                defaults: new { controller = "Main", action = "PageRenderer", id = UrlParameter.Optional, section = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Lang",
                url: "{lang}/{id}",
                defaults: new { controller = "Main", action = "PageRenderer", id = UrlParameter.Optional, lang = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LangSection",
                url: "{lang}/{section}/{id}",
                defaults: new { controller = "Main", action = "PageRenderer", id = UrlParameter.Optional, lang = UrlParameter.Optional, section = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MaxRoute",
                url: "{p1}/{p2}/{p3}/{p4}/{p5}/{p6}/{p7}/{p8}/{p9}/{p10}/{p11}/{p12}",
                defaults: new { controller = "Main", action = "PageRenderer", p1 = UrlParameter.Optional, p2 = UrlParameter.Optional, p3 = UrlParameter.Optional, p4 = UrlParameter.Optional, p5 = UrlParameter.Optional, p6 = UrlParameter.Optional, p7 = UrlParameter.Optional, p8 = UrlParameter.Optional, p9 = UrlParameter.Optional, p10 = UrlParameter.Optional, p11 = UrlParameter.Optional, p12 = UrlParameter.Optional }
            );
            #endregion

            // Area registration Code
            AreaRegistration.RegisterAllAreas();
        }
    }
}
