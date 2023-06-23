using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize Functions functions
    /// </summary>
    [Cache(Members.All)]
    public class Configuration : Cacheable, IConfiguration
    {
        /// <summary>
        /// Internal static handler to check if the sitemap is in Loop generation
        /// </summary>
        private static DateTime? LastSiteMapGenerated { get; set; } = null;

        /// <summary>
        /// Property to Load the default configuration 
        /// </summary>
        /// <returns></returns>
        public List<Data.Model.Configuration> All
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.Configurations.ToList();
            }
        }

        /// <summary>
        /// Method to Return the SiteName configuration
        /// </summary>
        public string SiteName => All.FirstOrDefault(c => c.Id == "SiteName").Value;

        /// <summary>
        /// Method to Return the DefaultUrl configuration
        /// </summary>
        public string DefaultUrl => All.FirstOrDefault(c => c.Id == "DefaultUrl").Value;

        /// <summary>
        /// Method to Return the DefaultLanguage configuration
        /// </summary>
        public string DefaultLanguage => All.FirstOrDefault(c => c.Id == "DefaultLanguage").Value;

        /// <summary>
        /// Method to Return any desired configuration passed through parameter
        /// </summary>
        public string Get(string key, string plugin = null)
        {
            return All.FirstOrDefault(c => c.Id == key && c.Plugin == plugin)?.Value;
        }

        /// <summary>
        /// Method to Return if desired key exists in the configuration
        /// </summary>
        public bool ContainsKey(string key, string plugin = null)
        {
            return All.Any(c => c.Id == key && c.Plugin == plugin);
        }

        /// <summary>
        /// Method to refresh the configuration List
        /// </summary>
        public void Refresh()
        {
            Cache.Remove("Bitzar.CMS.Core.Functions.Internal.Configuration.All");
        }

        /// <summary>
        /// Method to indicate if the membership controller is activated
        /// </summary>
        public bool MembershipEnabled => Get("MembershipEnabled") != "false";

        /// <summary>
        /// Method to indicate if the system should redirect to Https
        /// </summary>
        public bool EnforceSSL => Get("EnforceSSL") != "false";

        /// <summary>
        /// Method to indicate the system to use Captcha control
        /// </summary>
        public bool EnforceCaptcha => Get("EnforceCaptcha") != "false";

        /// <summary>
        /// Method to indicate if the membership is enabled to all non-admin users
        /// </summary>
        public bool AllowMembershipManagement => Get("AllowMembershipManagement") != "false";

        /// <summary>
        /// Method to return to the system the current Service Token Available
        /// </summary>
        public string Token => Get("ApiToken");

        /// <summary>
        /// Method to return if the site is in development method or not
        /// </summary>
        public bool IsDevelopmentMode => Get("DevelopmentMode") != "false";

        /// <summary>
        /// Default throttling parameter to avoid API too many requests
        /// </summary>
        internal int ThrottlingInstant => int.Parse(Get("ThrottlingInstant") ?? "5");
        internal int ThrottlingMinute => int.Parse(Get("ThrottlingMinute") ?? "20");
        internal int ThrottlingHour => int.Parse(Get("ThrottlingHour") ?? "200");
        internal int ThrottlingDay => int.Parse(Get("ThrottlingDay") ?? "1000");
        internal int ThrottlingMultiplier => int.Parse(Get("ThrottlingAuthUsersMultiplier") ?? "2");

        /// <summary>
        /// Function to generate SiteMap
        /// </summary>
        public void GenerateSiteMap()
        {
            if (HttpContext.Current == null)
                return;

            // Validate if the sitemap is in loop generation
            if (LastSiteMapGenerated.HasValue && (DateTime.Now - LastSiteMapGenerated.Value) < TimeSpan.FromSeconds(5))
                return;

            LastSiteMapGenerated = DateTime.Now;

            // Define the base urls and internal urls to avoid
            var baseUrl = CMS.Functions.BaseUrl().TrimEnd('/');
            var internalUrls = Enum.GetValues(typeof(System.Net.HttpStatusCode)).Cast<int>().Select(i => i.ToString());

            // Get all available routes in the WebSite
            var urls = CMS.Functions.Routes
                          .Where(r => !r.Page.Restricted)
                          .Where(r => !internalUrls.Any(i => i == r.Page.Url))
                          .Where(r => r.Page.Released)
                          .Where(r => r.Page.Mapped)
                          .Select(r => new Helper.SiteMap()
                          {
                              Url = $"{baseUrl}/{r.Route.Trim('/')}",
                              LastModified = r.LastModified
                          });

            // Default sitemap template
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builder.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Loop through each URL to generate sitemap
            foreach (var url in urls)
            {
                builder.AppendLine("<url>");
                builder.AppendLine($"<loc>{url.Url}</loc>");
                if (url.LastModified.HasValue)
                    builder.AppendLine($"<lastmod>{url.LastModified.Value:s}</lastmod>");
                builder.AppendLine("</url>");
            }

            builder.AppendLine("</urlset>");

            try
            {
                // Export to the site root
                File.WriteAllText(HttpContext.Current.Server.MapPath("~/sitemap.xml"), builder.ToString());
            }
            catch (Exception ex)
            {
                // Ignore erro
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Function to check if it should auto refresh or not the sitemap
        /// </summary>
        public void AutoRefreshSiteMap()
        {
            // Refresh site map if parameter is enabled
            if (CMS.Configuration.Get("GenerateSiteMapOnSave").Contains("true"))
                CMS.Configuration.GenerateSiteMap();
        }
    }
}