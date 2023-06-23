using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize plugins functions
    /// </summary>
    [Cache(Members.All)]
    public class Plugins : Cacheable, IPlugins
    {
        private static object SyncPlugins = new object();

        /// <summary>
        /// List of Plugins to Handle in the system
        /// </summary>
        private static IList<PluginInfo> plugins;

        /// <summary>
        /// Constanto to define the folder of the plugins
        /// </summary>
        public static string PLUGIN_PATH = "~/content/plugins";
        /// <summary>
        /// Default plugin extension to filter the files
        /// </summary>
        public static string PLUGIN_EXTENSION = "dll";

        /// <summary>
        /// Getter to locate all the plugins available in the system
        /// </summary>
        [NoCache]
        internal List<FileInfo> List
        {
            get
            {
                // Check directory existence
                var dir = HostingEnvironment.MapPath(PLUGIN_PATH);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Local all the dll files with plugin information
                var files = Directory.GetFiles(dir, $"*.{PLUGIN_EXTENSION}", SearchOption.AllDirectories);

                // Return Plugin information
                return files.Select(f => new FileInfo(f)).ToList();
            }
        }

        /// <summary>
        /// Load the plugin list and return each instance to the system
        /// </summary>
        /// <returns>Returns a list filled with instances of given plugins</returns>
        [NoCache]
        private IList<PluginInfo> LoadPlugins()
        {
            var exceptions = new List<Exception>();
            var list = new List<PluginInfo>();
            foreach (var plugin in List)
            {
                // Load assembly from file
                try
                {
                    // Create App Domain
                    var domain = AppDomain.CreateDomain(plugin.Name);
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolve);
                    var assembly = domain.Load(File.ReadAllBytes(plugin.FullName));
                    var type = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IPlugin)));
                    var instance = (IPlugin)Activator.CreateInstance(type);

                    // Only Allow Debug plugin in Debug environment
#if !DEBUG
                    if (!CMS.Configuration.IsDevelopmentMode && (assembly.IsDebug() ?? false))
                        throw new ApplicationException(Resources.Strings.Plugin_DebugLoadError);
#endif

                    // Execute setup
                    instance.Setup(CMS.Instance);

                    // Create plugin info
                    list.Add(new PluginInfo()
                    {
                        Name = plugin.Name,
                        FileInfo = plugin,
                        Plugin = instance,
                        Version = assembly.GetName().Version,
                        Assembly = assembly,
                        AppDomain = domain
                    });
                }
                catch (ReflectionTypeLoadException e)
                {
                    var error = "Plugin: " + plugin.Name + Environment.NewLine;

                    error += e.AllMessages();
                    foreach (var loaderException in e.LoaderExceptions)
                        error += Environment.NewLine + loaderException.AllMessages();

                    var exception = new Exception(error);
                    exceptions.Add(exception);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.AllMessages());
                    exceptions.Add(ex);
                }
            }

            // Store Exception list to be used in the system
            HttpContext.Current.Session["PLUGIN_EXCEPTIONS"] = exceptions;

            return list;
        }

        /// <summary>
        /// Internal method to resolve assembly dependency
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [NoCache]
        private Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // confirm args.Name contains A.dll
            var dll = $"{args.Name.Split(',')[0]}.dll";

            // Bin path 
            var binFolder = System.IO.Path.Combine(HostingEnvironment.MapPath("~/bin"), dll);
            if (File.Exists(binFolder))
                return Assembly.LoadFile(binFolder);

            // Plugins folder
            var pluginsFolder = System.IO.Path.Combine(HostingEnvironment.MapPath(PLUGIN_PATH), dll);
            if (File.Exists(pluginsFolder))
                return Assembly.Load(File.ReadAllBytes(pluginsFolder));

            return null;
        }

        /// <summary>
        /// Method to Get all the plugins that are available in the system
        /// </summary>
        [NoCache]
        public IList<PluginInfo> Available
        {
            get
            {
                if (plugins != null)
                    return plugins;

                lock (SyncPlugins)
                {
                    if (plugins == null)
                        plugins = LoadPlugins();
                }

                return plugins;
            }
        }

        /// <summary>
        /// Method to clear the plugin list and reload all of them
        /// </summary>
        [NoCache]
        public void UnloadAll()
        {
            if (plugins == null)
                return;

            foreach (var plugin in plugins)
                AppDomain.Unload(plugin.AppDomain);

            plugins.Clear();
            plugins = null;
        }

        /// <summary>
        /// Method to uninstall a plugin from the system
        /// </summary>
        [NoCache]
        public void RemovePlugin(PluginInfo plugin)
        {
            // Call uninstall function inside plugin to remove custom data
            if (!plugin.Plugin.Uninstall())
                throw new Exception(Resources.Strings.Plugins_UninstallError);

            // Proceed uninstall
            AppDomain.Unload(plugin.AppDomain);
            plugins.Remove(plugin);
        }

        /// <summary>
        /// Method to get a specific plugin
        /// </summary>
        /// <param name="plugin">Plugin name to locate inside the plugin availability list</param>
        /// <returns>Returns the plugin data instantiated in the list or null if not found</returns>
        public PluginInfo Get(string plugin) => Available.FirstOrDefault(p => p.Name.Equals(plugin, StringComparison.CurrentCultureIgnoreCase));
    }
}