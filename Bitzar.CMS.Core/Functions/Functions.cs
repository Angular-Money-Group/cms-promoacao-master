using Bitzar.CMS.Core.Controllers;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize Functions functions
    /// </summary>
    [Cache(Members.All)]
    public class Functions : Cacheable, IFunctions
    {
        /// <summary>
        /// Method to Load all the available template types
        /// </summary>
        /// <returns>Return a list of Template Types available on the system</returns>
        public List<TemplateType> TemplateTypes
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.TemplateTypes.ToList();
            }
        }

        /// <summary>
        /// Method to Load all the available templates
        /// </summary>
        /// <returns>Return a list of Templates available on the system</returns>
        public List<Template> Templates
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.Templates
                        .Include(t => t.TemplateType)
                        .Include(t => t.Section)
                        .Include(t => t.Fields)
                        .ToList();
            }
        }

        /// <summary>
        /// Method to load the default path for the content directory path
        /// </summary>
        /// <param name="type">Default path of each type</param>
        /// <param name="fileName"></param>
        /// <returns>Returns the relative directory name</returns>
        public string ContentDirectory(string type, string fileName)
        {
            var templateType = TemplateTypes.FirstOrDefault(t => t.Name.Equals(type, StringComparison.CurrentCultureIgnoreCase));
            var template = Templates.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
            return $"{(templateType?.DefaultPath ?? "~").Replace("~", "")}/{fileName}?v={template?.Version}";
        }

        /// <summary>
        /// Method to generate site base Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string BaseUrl(Uri url = null)
        {
            if (url == null)
                url = HttpContext.Current.Request.Url;

            // Create the Url
            var uri = $"{url.Scheme}://{url.Host}";
            if (!url.IsDefaultPort)
                uri = $"{url.Scheme}://{url.Host}:{url.Port}";

            // Return url
            return uri;
        }

        /// <summary>
        /// Method to Create a system URL basead on the current Pages
        /// </summary>
        /// <param name="page">Page to locate the related resource and generate Page Link</param>
        /// <param name="lang">Culture information to generate the page. If not provided will use current Culture</param>
        /// <returns>Returns links to the desired page</returns>
        [NoCache]
        public string Url(string page, string lang = null)
      {
            // Get System Language
            var culture = CMS.I18N.AvailableLanguages.FirstOrDefault(f => f.Culture.Equals(lang, StringComparison.CurrentCultureIgnoreCase)) ?? CMS.I18N.Culture;
            return GetUrl(page, culture);
        }

        /// <summary>
        /// Method to Create a URL for the page basead in their Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Post">Page to locate the related resource and generate Page Link</param>
        /// <param name="lang">Culture information to generate the page. If not provided will use current Culture</param>
        /// <returns>Returns links to the desired page</returns>
        [NoCache]
        public string Url(int id, string lang = null)
        {
            var routes = MvcApplication.AvailableRoutes;
                
            // Get System Language
            var culture = CMS.I18N.AvailableLanguages.FirstOrDefault(f => f.Culture.Equals(lang, StringComparison.CurrentCultureIgnoreCase)) ?? CMS.I18N.Culture;

            // Get post information
            var post = routes.Where(p => p.Page.Id == id && p.Culture.Culture == culture.Culture).FirstOrDefault();
            return post?.Route;
        }

        /// <summary>
        /// Method to Create a URL hosted inside plugins
        /// </summary>
        /// <param name="plugin">Plugin name to locate the Reference Route</param>
        /// <param name="identifier">Identifier to get the right route</param>
        /// <param name="lang">Culture information to generate the page. If not provided will use current Culture</param>
        /// <returns>Returns links to the desired page</returns>
        [NoCache]
        public string PluginUrl(string plugin, string identifier, string lang = null)
        {
            // Get System Language
            var culture = CMS.I18N.AvailableLanguages.FirstOrDefault(f => f.Culture.Equals(lang, StringComparison.CurrentCultureIgnoreCase)) ?? CMS.I18N.Culture;
            var pluginUrl = GetPluginUrl(plugin, identifier, culture);           
            return pluginUrl;
        }

        /// <summary>
        /// Cachable method to Create a system URL basead on the current Pages
        /// </summary>
        /// <param name="page">Page to locate the related resource and generate Page Link</param>
        /// <param name="culture"></param>
        /// <param name="lang">Culture information to generate the page. If not provided will use current Culture</param>
        /// <returns>Returns links to the desired page</returns>
        public string GetUrl(string page, Language culture)
        {
            if (string.IsNullOrWhiteSpace(page))
                page = "#";

            var original = page;
            // Locate templatetype and configure path with extension
            var template = TemplateTypes.FirstOrDefault(f => f.Name == "View");
            if (!page.ToLower().EndsWith(template.DefaultExtension))
              page = $"{page}.{template.DefaultExtension}";

            // Locate the page template
            var view = Templates.FirstOrDefault(f => f.Name.Equals(page, StringComparison.CurrentCultureIgnoreCase));
            if (view != null)
                return $"/{string.Join("/", (new[] { culture.UrlRoute, view.Section?.Url, view.Url }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())}";

            // View was not located, so create link as sent
            return $"/{string.Join("/", (new[] { culture.UrlRoute, null, original }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())}";
        }

        /// <summary>
        /// Method to return an specific URL generated by a plugin.
        /// </summary>
        /// <param name="plugin">Plugin identifier</param>
        /// <param name="identifier">Id of the route to lookup</param>
        /// <param name="culture">Culture identifier to get </param>
        /// <returns></returns>
        public string GetPluginUrl(string plugin, string identifier, Language culture)
        {
            var current = Url(CMS.Page.Current.Id, culture.Culture) + "/#404-Url-Not-Found";

            // Get plugin specific routes
            var routes = MvcApplication.AvailableRoutes.Where(r => r.Plugin?.Equals(plugin, StringComparison.CurrentCultureIgnoreCase) ?? false);
            if (routes.Count() == 0)
                return current;

            // Filter language
            var route = routes.FirstOrDefault(r => r.RouteId.Equals(identifier, StringComparison.CurrentCultureIgnoreCase) && r.Culture.Id == culture.Id);
            if (route == null)
                return current;

            // Return Route Url
            return $"/{string.Join("/", (new[] { culture.UrlRoute, route.Section, route.PageUrl }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())}";
        }

        /// <summary>
        /// Method to get the current RouteParam available for the requested route
        /// </summary>
        /// <param name="urlSource"></param>
        /// <param name="source">Current requested Route</param>
        /// <returns>Returns an instance of the matched route</returns>
        internal RouteParam MatchRoute(Uri urlSource)
        {
            var path = ($"{urlSource.AbsolutePath}/").Replace("//", "/");

            // Locate current Route
            var routes = MvcApplication.AvailableRoutes;
            var route = routes.FirstOrDefault(r => r.Route == path);

            return route;
        }

        /// <summary>
        /// Return to the system the current PluginUrl
        /// </summary>
        /// <returns></returns>
        public string ExecuteUrl
        {
            get => $"/{nameof(MainController.Execute).ToLower()}";
        }

        /// <summary>
        /// Method to return all available routes in the system
        /// </summary>
        [NoCache]
        public HashSet<RouteParam> Routes
        {
            get => MvcApplication.AvailableRoutes;
        }
    }
}