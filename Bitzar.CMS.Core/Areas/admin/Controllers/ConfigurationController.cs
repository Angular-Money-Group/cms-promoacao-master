using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class ConfigurationController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Configuracao")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(Functions.CMS.Configuration.All.Where(c => !c.System && c.Plugin == null).ToList());
        }

        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Configuracao/Plugin")]
        public ActionResult ConfigPlugin(string id)
        {
            ViewBag.Plugin = id;
            var model = Functions.CMS.Configuration.All.Where(c => !c.System && c.Plugin == id).ToList();
            return View("Index", model);
        }

        /// <summary>
        /// Action
        /// </summary>
        [Route("Configuracao/Salvar"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Configuration[] config, string plugin)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Set configuration as Modified
                    var configuration = Functions.CMS.Configuration.All;
                    foreach (var entity in config.GroupBy(c => c.Id, c => c))
                    {

                        var record = configuration.FirstOrDefault(c => c.Id == entity.Key);
                        record.Value = string.Join(",", entity.Select(e => e.Value).ToArray());

                        db.Configurations.Attach(record);
                        db.Entry(record).State = EntityState.Modified;
                    }

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear Configuration Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Configuration).FullName);

                //Trigger OnSaveConfiguration
                Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnSaveConfiguration, config);

                // Check Plugin Loaded status on configuration Change
                CheckPluginStatus();

                // Notify Success
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
            }

            if (!string.IsNullOrWhiteSpace(plugin))
                return RedirectToAction(nameof(ConfigPlugin), new { id = plugin });

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Check internal plugin status if the configuration has been changed
        /// </summary>
        private void CheckPluginStatus()
        {
            if (!Functions.CMS.Configuration.IsDevelopmentMode &&
                Functions.CMS.Plugins.Available.Any(p => p.Assembly?.IsDebug() ?? false))
                Functions.CMS.Plugins.UnloadAll();
        }

        /// <summary>
        /// Static method to load the available select sources
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<Tuple<string, string>> SelectSource(string source)
        {
            switch (source.ToLower())
            {
                case "language":
                    return Functions.CMS.I18N.AvailableLanguages.Select(l => Tuple.Create(l.Culture, $"{l.Description} ({l.Culture})")).ToList();
                case "page":
                    return Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "View").Select(t => Tuple.Create(t.Name, t.Name)).ToList();
                case "role":
                    return Functions.CMS.User.MemberRoles.OrderBy(r => r.Name).Select(r => Tuple.Create(r.Id.ToString(), r.Name)).ToList();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Method to generate an site map basead on all URL's in the internal source.
        /// </summary>
        /// <returns></returns>
        [Route("Configuracao/Gerar-SiteMap")]
        [HttpGet]
        public ActionResult GenerateSiteMap()
        {
            try
            {
                Functions.CMS.Configuration.GenerateSiteMap();
                return Json(new { status = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}