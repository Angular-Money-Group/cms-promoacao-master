using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class LibraryController : AdminBaseController
    {
        /// <summary>
        /// Method to show the library main page
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca")]
        public ActionResult Index(string tipo = null)
        {
            ViewBag.ValidateFile = Functions.CMS.Library.Objects().Any(item => System.IO.File.Exists(Path.Combine(HostingEnvironment.MapPath(item.LibraryType.DefaultPath), item.Name)));

            return View();
        }

        /// <summary>
        /// Method to show the library path of image
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca/Image/{id}")]
        [AllowAnonymous]
        public ActionResult ImagePath(string id = null)
        {
            var DefaultImage = "/Areas/admin/Content/basic/imgs/img-placeholder.png";

            var img = (id == null ? DefaultImage : Functions.CMS.Library.Objects().FirstOrDefault(x => x.Id == Convert.ToInt32(id))?.FullPath ?? DefaultImage);

            return Content(img);
        }

        /// <summary>
        /// Method to load the library on the system
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca/Listar-Midias/{tipo?}")]
        public ActionResult List(string source = "midia", int pagina = 1, int tamanho = 18, string tipo = null, string pesquisa = null)
        {
            try
            {
                ViewBag.Source = source;
                ViewBag.LibraryTypes = Functions.CMS.Library.Types();
                ViewBag.AllowedMimeTypes = Functions.CMS.Library.AllowedMimeTypes;
                ViewBag.Type = tipo;
                ViewBag.Search = pesquisa;

                // Set Pagination Objects
                var count = Functions.CMS.Library.MediaCount(tipo, pesquisa);
                ViewBag.Pagination = new Pagination()
                {
                    CurrentPage = pagina,
                    Size = tamanho,
                    TotalPages = (int)Math.Ceiling((decimal)count / tamanho),
                    MaxPageItems = 3
                };

                // Get Library data from server
                var library = Functions.CMS.Library.Media(pagina, tamanho, tipo, pesquisa);
                return PartialView("_Library", library);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return PartialView("_Library", null);
            }
        }

        /// <summary>
        /// Method to allow user to upload files and Store it on the database and Local Path
        /// </summary>
        /// <param name="type">Type of file to be uploaded</param>
        /// <returns></returns>
        [HttpPost, Route("Biblioteca/Upload")]
        public ActionResult UploadFile()
        {
            try
            {
                var files = Functions.CMS.Library.UploadFiles(Request.Files);
                return Json(new { status = Resources.Strings.Template_AllFilesPublished }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Functions.CMS.Log.LogRequest(ex);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca/Excluir")]
        public async Task<JsonResult> RemoveFile(int id)
        {
            try
            {
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var library = await db.Library.Include(t => t.LibraryType).FirstOrDefaultAsync(t => t.Id == id);

                    if (library != null)
                    {
                        // Remove file
                        if (Functions.CMS.Configuration.Get("AzureStorage").Contains("true"))
                        {
                            var blobName = $"{library.LibraryType.DefaultPath}/{library.Name}".Replace("~/", "");
                            Functions.CMS.Library.DeleteAzureStorage(blobName);
                        }
                        else
                        {
                            var filePath = HostingEnvironment.MapPath(Path.Combine(library.Path, library.Name));
                            System.IO.File.Delete(filePath);
                        }

                        // Remove template
                        db.Entry(library).State = EntityState.Deleted;

                        // Save changes
                        await db.SaveChangesAsync();
                    }
                }

                // Clear Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Library).FullName);

                return Json(new { status = Resources.Strings.Data_SuccessfullySaved }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Id = id
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca/Renomear")]
        public async Task<JsonResult> RenameFile(int id, string value)
        {
            try
            {
                var name = string.Empty;
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var library = await db.Library.Include(t => t.LibraryType).FirstOrDefaultAsync(t => t.Id == id);

                    // var check extension
                    name = Path.GetFileNameWithoutExtension(value);
                    name = $"{name}{library.Extension}";



                    // Rename file
                    if (Functions.CMS.Configuration.Get("AzureStorage").Contains("true"))
                    {
                        var blobName = $"{library.LibraryType.DefaultPath}/{library.Name}".Replace("~/", "");
                        var newName = $"{library.LibraryType.DefaultPath}/{name}".Replace("~/", "");
                        Functions.CMS.Library.RenameAzureStorage(blobName, newName);
                    }
                    else
                    {
                        var filePath = HostingEnvironment.MapPath(Path.Combine(library.Path, library.Name));
                        var newFile = HostingEnvironment.MapPath(Path.Combine(library.Path, name));
                        if (System.IO.File.Exists(newFile))
                            throw new Exception(Resources.Strings.Library_FileWithSameNameAlreadyExists);

                        // Effectvelly move objects betwwenservers
                        System.IO.File.Move(filePath, newFile);
                    }

                    // Update template
                    library.Name = name;
                    db.Entry(library).State = EntityState.Modified;

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Library).FullName);

                return Json(new { status = name }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Id = id,
                    Value = value
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [Route("Biblioteca/Descricao")]
        public async Task<JsonResult> SetDescription(int id, string value)
        {
            try
            {
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var library = await db.Library.Include(t => t.LibraryType).FirstOrDefaultAsync(t => t.Id == id);
                    library.Description = value;
                    db.Entry(library).State = EntityState.Modified;

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Library).FullName);

                return Json(new { status = value }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Id = id,
                    Value = value
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Método de migrar para o azure storage
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Biblioteca/Migrar-Azure-Storage")]
        public JsonResult MigrateToAzureStorage()
        {
            try
            {
                Functions.CMS.Library.MigrateLybraryToAzureStorage();

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { status = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}