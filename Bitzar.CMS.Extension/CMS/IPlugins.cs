using Bitzar.CMS.Extension.Classes;
using System.Collections.Generic;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IPlugins
    {
        IList<PluginInfo> Available { get; }

        PluginInfo Get(string plugin);
        void RemovePlugin(PluginInfo plugin);
        void UnloadAll();
    }
}