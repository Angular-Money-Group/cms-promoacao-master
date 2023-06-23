using System;
using Bitzar.CMS.Extension.CMS;

namespace Bitzar.CMS.Extension.Classes
{
    public class CMS : ICMS
    {
        public IDictionaryCache Cache { get; private set; }
        public IBlog Blog { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public IFunctions Functions { get; private set; }
        public ISecurity Security { get; private set; }
        public ILog Log { get; private set; }
        public IGlobal Global { get; private set; }
        public II18N I18N { get; private set; }
        public ILibrary Library { get; private set; }
        public IMembership Membership { get; private set; }
        public IPage Page { get; private set; }
        public IUser User { get; private set; }
        public IPath Path { get; private set; }
        public IEmail Notification { get; private set; }
        public IEvents Events { get; private set; }
        public IPlugins Plugins { get; private set; }
        public Action ClearRoutes { get; private set; }
        public Action<string> ClearCache { get; private set; }

        public CMS(
            IDictionaryCache cache,
            IBlog blog,
            IConfiguration configuration,
            IFunctions functions,
            ISecurity security,
            ILog log,
            IGlobal global,
            II18N i18n,
            ILibrary library,
            IMembership membership,
            IPage page,
            IUser user,
            IPath path,
            IEmail notification,
            IEvents events,
            IPlugins plugins,
            Action clearRoutes,
            Action<string> clearCache)
        {
            Cache = cache;
            Blog = blog;
            Configuration = configuration;
            Functions = functions;
            Security = security;
            Log = log;
            Global = global;
            I18N = i18n;
            Library = library;
            Membership = membership;
            Page = page;
            User = user;
            Path = path;
            Notification = notification;
            ClearRoutes = clearRoutes;
            ClearCache = clearCache;
            Events = events;
            Plugins = plugins;
        }
    }
}
