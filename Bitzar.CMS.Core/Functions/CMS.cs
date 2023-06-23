using Bitzar.CMS.Core.Functions.Internal;
using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using System;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Functions
{
    public class CMS
    {
        public static DictionaryCache Cache => MvcApplication.GlobalCache;
        public static Configuration Configuration => new Configuration();
        public static Blog Blog => new Blog();
        public static Internal.Functions Functions => new Internal.Functions();
        public static Global Global => new Global();
        public static I18N I18N => new I18N();
        public static Library Library => new Library();
        public static Membership Membership => new Membership();
        public static Page Page => new Page();
        public static User User => new User();
        public static Plugins Plugins => new Plugins();
        public static Path Path => new Path();
        public static Internal.Notification Notification => new Internal.Notification();
        public static Internal.Security Security => new Internal.Security();
        public static Log Log => new Log();
        public static Events Events => new Events();

        /// <summary>
        /// Method to allow the cache service be cleared
        /// </summary>
        [Obsolete("Não utilizar a função de Limpeza de Cache global.", true)]
        public static void ClearCache()
        {
            MvcApplication.GlobalCache.Clear();
            MvcApplication.AvailableRoutes = null;
            Plugins.UnloadAll();

            // Clear stats cache
            if ((HttpContext.Current?.Cache["CMS.TEMP.STATS"] ?? null) != null)
                HttpContext.Current.Cache.Remove("CMS.TEMP.STATS");
        }

        /// <summary>
        /// Clear cache for specific group of data
        /// </summary>
        /// <param name="key">Key part of the name to be removed. All the keys that contains the given Key, 
        /// will be removed from the cache system</param>
        public static void ClearCache(string key)
        {
            var keys = MvcApplication.GlobalCache.AllKeys.Where(k => k.Contains(key));
            foreach (var item in keys)
                MvcApplication.GlobalCache.Remove(item);

            // Generate SiteMap if is configured to work in this way
            if (CMS.Configuration.Get("GenerateSiteMapOnCacheClear").Contains("true"))
                CMS.Configuration.GenerateSiteMap();
        }

        /// <summary>
        /// Function to force the service to clear the routes and rebuild it again
        /// </summary>
        public static void ClearRoutes()
        {
            // Force reload all the Routes
            MvcApplication.AvailableRoutes = null;
            CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Functions.MatchRoute");            
        }

        /// <summary>
        ///  Return to the system a Global Instance to be shared with Plugins
        /// </summary>
        public static ICMS Instance => 
            new Extension.Classes.CMS(MvcApplication.GlobalCache, Blog, Configuration, Functions, Security,
                                      Log, Global, I18N, Library, Membership, Page, User, Path, Notification,
                                      Events, Plugins, new Action(()=> ClearRoutes()), new Action<string>((s) => ClearCache(s)));
    }
}