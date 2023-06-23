using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Plugin
    /// </summary>
    public static class PluginHelpers
    {
        private static readonly Bitzar.CMS.Core.Functions.Internal.Plugins plugin = Functions.CMS.Plugins;

        /// <summary>
        /// List available Plugins
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> Availables()
        {
            var response = plugin.Available.Select(x => new
            {
                x.Name,
                x.Version,
                x.Loaded
            });

            return response;
        }

        /// <summary>
        /// Execute Plugin
        /// </summary>
        /// <param name="source"></param>
        /// <param name="function"></param>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static dynamic Execute(string source, string function, string token = null, Dictionary<string, string> parameters = null)
        {
            function = function.Replace("-", "");
            source = $"{source.Replace("-", ".").Replace(".dll", "")}.dll";

            try
            {
                // Validation of the system is find the right plugin to process the request
                var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(p => p.Name.Equals(source, StringComparison.CurrentCultureIgnoreCase));

                if (plugin == null)
                    return Resources.Strings.Plugins_NotFound;

                // Add service parameters to send in the pre validation 
                parameters.Add("source", plugin.Name);
                parameters.Add("function", function);

                // Call pre-validate routines
                Functions.CMS.Events.Trigger(Model.Enumerators.EventType.PreValidateExecute, parameters);

                // Call and return the method to process the request inside the Plugin
                return plugin.Plugin.Execute(function, token, parameters);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}