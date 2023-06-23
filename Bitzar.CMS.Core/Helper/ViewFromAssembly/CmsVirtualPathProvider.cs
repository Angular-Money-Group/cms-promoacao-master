using Bitzar.CMS.Extension.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace Bitzar.CMS.Core.Helper.ViewFromAssembly
{
    public class CmsVirtualPathProvider : VirtualPathProvider
    {
        public override bool FileExists(string virtualPath)
        {
            if (!virtualPath.Contains(".dll/"))
                return base.FileExists(virtualPath);

            var parts = virtualPath.Split('/');
            var pluginName = parts[parts.Length - 2];
            var viewName = parts[parts.Length - 1];

            // Check if plugin exists. If not, go ahead
            var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(p => p.Name.Equals(pluginName, StringComparison.CurrentCultureIgnoreCase));
            if (plugin == null)
                return base.FileExists(virtualPath);

            var viewData = GetViewFromAssembly(plugin, viewName);
            if (viewData == null)
                return base.FileExists(virtualPath);

            return true;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            // First check if it's from plugin. Plugin will be in this format "Plugin.dll/Index"
            if (!virtualPath.Contains(".dll/"))
                return base.GetFile(virtualPath);

            var parts = virtualPath.Split('/');
            var pluginName = parts[parts.Length - 2];
            var viewName = parts[parts.Length - 1];

            // Check if plugin exists. If not, go ahead
            var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(p => p.Name.Equals(pluginName, StringComparison.CurrentCultureIgnoreCase));
            if (plugin == null)
                return base.GetFile(virtualPath);

            // Plugin was found, so look for the resource
            var content = GetViewFromAssembly(plugin, viewName);
            if (content == null || content.Length == 0)
                return base.GetFile(virtualPath);

            return new CmsVirtualFile(virtualPath, content);
        }

        /// <summary>
        /// Handle view cache to allow system speed up the process
        /// </summary>
        /// <param name="virtualPath">Virtual path to check the view</param>
        /// <param name="virtualPathDependencies">Dependencies</param>
        /// <param name="utcStart">Start time</param>
        /// <returns></returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (!virtualPath.Contains(".dll/"))
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

            var parts = virtualPath.Split('/');
            var pluginName = parts[parts.Length - 2];
            var viewName = parts[parts.Length - 1];

            // Check if plugin exists. If not, go ahead
            var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(p => p.Name.Equals(pluginName, StringComparison.CurrentCultureIgnoreCase));
            if (plugin == null)
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

            var viewData = GetViewFromAssembly(plugin, viewName);
            if (viewData == null)
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

            return null;
        }

        /// <summary>
        /// Load the view data from the Resource in the assembly
        /// </summary>
        /// <param name="view">View name to look for</param>
        /// <returns></returns>
        private byte[] GetViewFromAssembly(PluginInfo plugin, string view)
        {
            // Locate in the Assembly plugin the object instance to show the page
            var resourceName = plugin.Assembly.GetManifestResourceNames().FirstOrDefault(r => r.ContainsIgnoreCase(view));
            if (string.IsNullOrWhiteSpace(resourceName))
                return null;

            // Get the Stream and set to the View
            using (var stream = plugin.Assembly.GetManifestResourceStream(resourceName))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}