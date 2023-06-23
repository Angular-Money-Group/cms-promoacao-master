using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class LanguageController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Idiomas")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(Functions.CMS.I18N.AvailableLanguages);
        }

        /// <summary>
        /// Action to show the edit or new entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Idiomas/Detalhe")]
        [HttpGet]
        public ActionResult Detail(int? id, object transfer = null)
        {
            try
            {
                // Handle create new user data 
                if (!id.HasValue)
                    return View(new Language());

                // Handle if the has any problem on save
                if (!id.HasValue && transfer != null)
                    return View(transfer);

                // Locate user data to allow edit
                var lang = Functions.CMS.I18N.AvailableLanguages.FirstOrDefault(f => f.Id == id.Value);
                return View(lang);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Action to allow save an entity on the database
        /// </summary>
        [Route("Idiomas/Salvar"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Language entity)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Attach database item
                    db.Languages.Attach(entity);

                    // Set entity as New or Modified
                    db.Entry(entity).State = (entity.Id == 0 ? EntityState.Added : EntityState.Modified);

                    // Save changes
                    await db.SaveChangesAsync();

                    // Replicate the field values to the new language
                    var language = Functions.CMS.I18N.AvailableLanguages.FirstOrDefault(l => l.Id == Functions.CMS.I18N.DefaultLanguage.Id);
                    var fieldValues = await db.FieldValues.Where(v => v.IdLanguage == language.Id).ToListAsync();
                    var fields = await db.FieldValues.Where(v => v.IdLanguage == entity.Id).ToListAsync();

                    // Get only the fields that are missing to replicate
                    foreach (var item in fieldValues.Where(f => !fields.Any(x => x.IdField == f.IdField)))
                        db.FieldValues.Add(new FieldValue()
                        {
                            IdField = item.IdField,
                            IdLanguage = entity.Id,
                            Order = item.Order,
                            Value = null
                        });

                    // Apply new items
                    await db.SaveChangesAsync();
                }

                // Clear cache data
                Functions.CMS.ClearCache(typeof(Functions.Internal.I18N).FullName);
                Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);

                Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnSaveLanguage, entity);

                // Notify Success
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Replicar-Idiomas")]
        public async Task<ActionResult> Replicate()
        {
            try
            {
                // Call method to replicate
                await Functions.CMS.I18N.ReplicateValues();
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
            }

            // Redirect result
            return RedirectToAction(nameof(Index));
        }
    }
}