using System;

namespace Bitzar.CMS.Extension.CMS
{
    public interface ICMS
    {
        IDictionaryCache Cache { get; }
        IBlog Blog { get; }
        IConfiguration Configuration { get; }
        ISecurity Security { get; }
        ILog Log { get; }
        IFunctions Functions { get; }
        IGlobal Global { get; }
        II18N I18N { get; }
        ILibrary Library { get; }
        IMembership Membership { get; }
        IPage Page { get; }
        IUser User { get; }
        IPath Path { get; }
        IEmail Notification { get; }
        IEvents Events { get; }
        IPlugins Plugins { get; }
        Action ClearRoutes { get; }
        Action<string> ClearCache { get; }
    }
}
