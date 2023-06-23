using Bitzar.CMS.Core.Functions.Internal;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class PluginController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site plugin page
        /// </summary>
        /// <returns></returns>
        [Route("Plugins")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(LoadPlugins());
        }

        private IList<PluginInfo> LoadPlugins()
        {
            var plugins = Functions.CMS.Plugins.Available.ToList();

            // Load all the plugins that are not loaded in the path
            var files = Functions.CMS.Plugins.List;
            files = files.Where(f => !plugins.Any(p => p.FileInfo.FullName == f.FullName)).ToList();

            // Add in the plugin list as not loaded
            plugins.AddRange(files.Select(f => new PluginInfo
            {
                Name = f.Name,
                FileInfo = f,
                Plugin = null,
                Version = null,
                Assembly = null,
                AppDomain = null,
                Loaded = false
            }));

            return plugins;
        }

        /// <summary>
        /// Default method to show site plugin page
        /// </summary>
        /// <returns></returns>
        [Route("Plugins/Remove")]
        [HttpGet]
        public ActionResult Remove(string plugin)
        {
            var pluginInfo = this.LoadPlugins().FirstOrDefault(p => p.Name.Equals(plugin, StringComparison.CurrentCultureIgnoreCase));
            if (pluginInfo != null)
            {
                // Deallocate Plugin
                if (pluginInfo.Loaded)
                    Functions.CMS.Plugins.RemovePlugin(pluginInfo);

                // Remove plugin from file
                System.IO.File.Delete(pluginInfo.FileInfo.FullName);

                Session["PLUGIN_EXCEPTIONS"] = null;

                Functions.CMS.ClearCache(plugin);
                Functions.CMS.Plugins.UnloadAll();
                Functions.CMS.ClearRoutes();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Method to allow user to upload plugins to the system
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Plugins/Upload")]
        public async Task<ActionResult> Upload()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Check if there is any file to send
                    if (Request.Files.Count == 0)
                        throw new InvalidOperationException(Resources.Strings.Template_MustSendFile);

                    // Check extension to see if there is any problem
                    if (Request.Files.GetMultiple("FileUpload").Any(t => Path.GetExtension(t.FileName) != $".{Plugins.PLUGIN_EXTENSION}"))
                        throw new InvalidOperationException(Resources.Strings.Template_ExtensionNotAllowed);

                    // All Files are allowed, so keep going and save each of them
                    foreach (HttpPostedFileBase file in Request.Files.GetMultiple("FileUpload"))
                    {
                        var data = new byte[file.ContentLength];
                        await file.InputStream.ReadAsync(data, 0, data.Length);

                        // Set File Name if already exists
                        var fileName = file.FileName;
                        if (Functions.CMS.Plugins.Available.Any(p => p.Name.Equals(file.FileName, StringComparison.CurrentCultureIgnoreCase)))
                            throw new Exception(Resources.Strings.Plugins_AlreadyExists);

                        // Check if the plugin implements IPlugin interface
                        var assembly = Assembly.Load(data);
                        if (!assembly.GetType().GetInterfaces().Any(i => i.GetType() != typeof(IPlugin)))
                            throw new Exception(Resources.Strings.Plugins_NotAllowed);

                        // Save the new File on Disk
                        var directory = HostingEnvironment.MapPath(Plugins.PLUGIN_PATH);
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        file.SaveAs(Path.Combine(directory, fileName));

                        Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnUploadPlugin, fileName);
                    }

                    // Apply changes on the database
                    await db.SaveChangesAsync();

                    // Clear cache
                    Functions.CMS.Plugins.UnloadAll();
                    Functions.CMS.ClearRoutes();
                }

                this.NotifySuccess(Resources.Strings.Plugin_SuccessfullyUploaded);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Default method to render a view from plugin controller
        /// </summary>
        /// <returns>Returns a View from the Plugin assembly</returns>
        [Route("Plugins/Action")]
        [HttpGet]
        public ActionResult Action(string source, string function, string[] parameters, string[] values)
        {
            // Check and load the plugin instance to process the request
            var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(f => f.Name.Equals(source, StringComparison.CurrentCultureIgnoreCase));
            if (plugin == null)
                return View();

            // Remove ending of the file
            if (function.EndsWith(".cshtml"))
                function = function.Replace(".cshtml", "");

            var paramList = new Dictionary<string, string>();
            for (var i = 0; i < (parameters?.Length ?? 0); i++)
                paramList.Add(parameters[i], values[i]);

            // add in paramList the other parameters that came in Request.QueryString
            foreach (var key in Request.QueryString.AllKeys)
                if (!paramList.ContainsKey(key))
                    paramList.Add(key, Request.QueryString[key]);

            ViewBag.Parameters = paramList;

            // Check if there is a custom page on CMS
            var view = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "Partial");
            if (Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Name.Equals($"{function}.{view.DefaultExtension}", StringComparison.CurrentCultureIgnoreCase))?.IdTemplateType == view.Id)
                return View(function, "~/Areas/admin/Views/Shared/_Layout.cshtml");

            // Use the CmsPathProvider to look for the View in the specified assembly
            return View($"{plugin.Version}/{source}/{function}");
        }
    }
}