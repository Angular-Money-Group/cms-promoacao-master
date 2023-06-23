using Bitzar.CMS.Core.Areas.admin.Controllers;
using Bitzar.CMS.Core.Helper.ViewFromAssembly;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Model;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core
{
    public class MvcApplication : HttpApplication
    {
        private static object SyncRoutes = new object();

        /// <summary>
        /// Control to determine if the system should restart
        /// </summary>
        internal static bool MustRunSetup { get; set; } = false;

        /// <summary>
        /// Global Cache class used to store System Cache
        /// </summary>
        public static DictionaryCache GlobalCache { get; set; }

        /// <summary>
        /// Property to hold available routes to the system
        /// </summary>
        private static HashSet<RouteParam> availableRoutes;
        public static HashSet<RouteParam> AvailableRoutes
        {
            get
            {
                if (availableRoutes != null)
                    return availableRoutes;

                lock (SyncRoutes)
                {
                    if (availableRoutes == null)
                        availableRoutes = RebuildRoutes();
                }

                return availableRoutes;
            }
            set => availableRoutes = value;
        }

        /// <summary>
        /// Main method executed when the application Start
        /// </summary>
        protected void Application_Start()
        {
            // Improve App StartUp Optimization
            ProfileOptimization.SetProfileRoot(HostingEnvironment.MapPath("~/bin"));
            ProfileOptimization.StartProfile("Startup.Profile");

            // Register Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Clear View Engine Compilation
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            HostingEnvironment.RegisterVirtualPathProvider(new CmsVirtualPathProvider());

            // Set default serialization configuration
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, MaxDepth =64 };
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            // Initialize custom libs
            Security.Cryptography.Setup();

            // Setup Global Cache
            GlobalCache = new DictionaryCache();

            // Database Setup only if database exists. if not set var to force redirect to Login
            var connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"];
            if (connectionString != null && !string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                AppStart();
            else
                MustRunSetup = true;

            /* WARNING - DO NOT ADD ANY CODE BELOW HERE THAT ACCESS DATABASE IF IT's HAS NOT BEEN INITIALIZED YET */
        }

        private void AppStart()
        {
            // Apply database migration start and Seed database
            Data.Configuration.Migrate<DatabaseConnection>();
            Data.Database.Seed();

            // Configura RazorEngine service
            var config = new TemplateServiceConfiguration();
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;

            // Check if there is any view that is not published
            Task.Run(async () =>
            {
                foreach (var template in Functions.CMS.Functions.Templates)
                    if (!System.IO.File.Exists(HostingEnvironment.MapPath($"{template.Path}/{template.Name}")))
                        await TemplateController.ReleaseMethod(template.Id, true);
            }).Wait();

        }

        /// <summary>
        /// Used to validate end request and throw ajax redirection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            if (Context.Items["AjaxPermissionDenied"] is bool)
            {
                Context.Response.StatusCode = 401;
                Context.Response.End();
            }
        }

        /// <summary>
        /// Method to return the current client IP address
        /// </summary>
        /// <returns></returns>
        public static string GetClientIp()
        {
            // Look for a proxy address first
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            // If there is no proxy, get the standard remote address
            if (string.IsNullOrWhiteSpace(ip) || ip.ToLower() == "unknown")
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            return ip;
        }

        /// <summary>
        /// Method that clear and cache all the available 
        /// </summary>
        private static HashSet<RouteParam> RebuildRoutes()
        {
            var routes = new HashSet<RouteParam>();

            // Load languages
            var languages = Functions.CMS.I18N.AvailableLanguages;

            // Load Sections
            var pages = Functions.CMS.Functions.Templates;

            // Get blog page holder
            var blogPage = pages.FirstOrDefault(f => f.Name == Functions.CMS.Configuration.Get("BlogPostPage"));

            // create routes for pages and posts
            foreach (var page in pages)
                foreach (var lang in languages)
                {
                    var isBlogPost = (page?.IsBlogPost() ?? false);
                    if (isBlogPost && !page.Released)
                        continue;

                    if (page.TemplateType.Name != "View" && page.TemplateType.Name != "BlogPost")
                        continue;

                    // Configure page section
                    var section = page.Section?.Url;
                    if (isBlogPost && blogPage != null)
                        section = blogPage.Url;

                    // Store post data if available
                    var post = !isBlogPost ? null : page.AsBlogPost();
                    var route = new RouteParam()
                    {
                        Page = page,
                        PageUrl = page.Url,
                        Culture = lang,
                        Language = lang.UrlRoute,
                        Section = section,
                        BlogPage = (isBlogPost ? blogPage : null),
                        Parameters = (isBlogPost ? new[] { page.Url } : null),
                        RouteType = (isBlogPost ? "Post" : "Page"),
                        LastModified = page.UpdatedAt,
                        Search = new SearchResult()
                        {
                            Category = (isBlogPost ? post.Categories.FirstOrDefault() ?? "Blog" : "Page"),
                            Title = (isBlogPost ? post.Title : page.Description ?? page.Name),
                            Image = (isBlogPost ? post.Image?.ToString() : null)
                        }
                    };

                    // Build search Route
                    if (route.Search != null)
                        route.Search.Url = route.ToString();
                    routes.Add(route);
                }

            // Lookup in plugins for available Routes
            foreach (var plugin in Functions.CMS.Plugins.Available)
            {
                foreach (var route in plugin.Plugin?.Routes())
                {
                    var routeItem = new RouteParam()
                    {
                        Page = route.Page,
                        PageUrl = route.Url,
                        Culture = route.Language,
                        Language = route.Language.UrlRoute,
                        Section = route.Section,
                        Plugin = plugin.Name,
                        Parameters = route.Parameters,
                        RouteId = route.IdRoute,
                        RouteType = route.Type,
                        Search = route.Search,
                        LastModified = route.LastModified
                    };

                    // Build search Route
                    if (routeItem.Search != null)
                        routeItem.Search.Url = routeItem.ToString();
                    routes.Add(routeItem);
                }
            }

            // Clear and set available Routes
            return routes;
        }
    }
}
