using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class CategoryController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Categorias")]
        [HttpGet]
        public ActionResult Index()
        {
            return View(Functions.CMS.Page.Sections);
        }

        /// <summary>
        /// Action to show the edit or new entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Categorias/Detalhe")]
        public async Task<ActionResult> Detail(int? id, object transfer = null)
        {
            try
            {
                // Handle create new user data 
                if (!id.HasValue)
                    return View(new Section());

                // Handle if the has any problem on save
                if (!id.HasValue && transfer != null)
                    return View(transfer);

                // Locate user data to allow edit
                using (var db = new DatabaseConnection())
                {
                    var section = await db.Sections.FindAsync(id.Value);
                    return View(section);
                }
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
        [Route("Categorias/Salvar"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Section entity)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Attach database item
                    db.Sections.Attach(entity);

                    // Set entity as New or Modified
                    db.Entry(entity).State = (entity.Id == 0 ? EntityState.Added : EntityState.Modified);

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear cache data
                Functions.CMS.ClearCache(typeof(Functions.Internal.Page).FullName);

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
    }
}