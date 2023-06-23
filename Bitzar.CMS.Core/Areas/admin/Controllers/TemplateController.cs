using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using NUglify;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class TemplateController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site with editor page file
        /// </summary>
        /// <returns></returns>
        [Route("Modelos/{type}")]
        [HttpGet]
        public async Task<ActionResult> Index(string type)
        {
            try
            {
                ViewBag.Type = type;
                using (var db = new DatabaseConnection())
                {
                    var searchType = TranslateTemplateType(type);

                    // Locate the correponsding type for the appropriate type
                    var templateType = await db.TemplateTypes.FirstOrDefaultAsync(t => t.Name.Equals(searchType, StringComparison.CurrentCultureIgnoreCase));
                    ViewBag.TemplateType = templateType ?? throw new InvalidOperationException(Resources.Strings.Template_FileTypeNotAllowed);

                    // Locate all related templates
                    var templates = await (from t in db.Templates.Include(t => t.TemplateType).Include(t => t.Section)
                                           where t.IdTemplateType == templateType.Id
                                           select t).ToListAsync();

                    return View(templates);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction("Index", "Default", new { area = "admin" });
            }
        }

        /// <summary>
        /// Default method to show site with editor page file
        /// </summary>
        /// <returns></returns>
        [Route("Modelos/{type}/Editor")]
        [HttpGet]
        public async Task<ActionResult> Editor(string type, int? id)
        {
            try
            {
                ViewBag.Type = type;
                ViewBag.Sections = Functions.CMS.Page.Sections;

                using (var db = new DatabaseConnection())
                {
                    var searchType = TranslateTemplateType(type);

                    // Locate the correponsding type for the appropriate type
                    var templateType = await db.TemplateTypes.FirstOrDefaultAsync(t => t.Name.Equals(searchType, StringComparison.CurrentCultureIgnoreCase));
                    if (templateType == null)
                        throw new InvalidOperationException(Resources.Strings.Template_FileTypeNotAllowed);

                    // Locate all related templates
                    var templates = await (from t in db.Templates.Include(t => t.TemplateType).Include(t => t.Section)
                                           where t.IdTemplateType == templateType.Id
                                           select t).ToListAsync();

                    // Locate the file if an id was provided
                    Template file = (Template)TempData["Template"];
                    if (id.HasValue)
                        file = await db.Templates.Include(t => t.TemplateType).FirstOrDefaultAsync(t => t.Id == id.Value);

                    // Check if file still null
                    if (file == null)
                        file = new Template()
                        {
                            Name = $"novo.{templateType.DefaultExtension}",
                            IdTemplateType = templateType.Id,
                            TemplateType = templateType,
                            Extension = templateType.DefaultExtension
                        };

                    return View(Tuple.Create(file, templates));
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction("Index", "Default", new { area = "admin" });
            }
        }

        /// <summary>
        /// Method to save a new file on the system
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Modelos/{type}/salvar"), ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Save(string type, Template template, string section, string[] roles)
        {
            try
            {
                var searchType = TranslateTemplateType(type);

                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate the correponsding type for the appropriate type
                    var templateType = await db.TemplateTypes.FirstOrDefaultAsync(t => t.Name.Equals(searchType, StringComparison.CurrentCultureIgnoreCase));

                    // Handle section
                    int? IdSection = null;
                    if (!string.IsNullOrWhiteSpace(section))
                    {
                        var dbSection = Functions.CMS.Page.Sections.FirstOrDefault(s => s.Name == section);
                        if (dbSection == null)
                        {
                            // Insert section if not exists
                            dbSection = new Section() { Name = section, Url = section.ToLower().AsUrl() };
                            db.Sections.Add(dbSection);
                        }

                        // Get Section ID
                        IdSection = dbSection.Id;
                    }

                    // Check if its a new template to be saved
                    var roleRestriction = (roles != null && roles.Length > 0 ? string.Join(",", roles) : null);
                    if (template.Id == 0)
                    {
                        db.Templates.Attach(template);
                        template.CreatedAt = DateTime.Now;
                        template.UpdatedAt = DateTime.Now;
                        template.Extension = templateType.DefaultExtension;
                        template.Path = templateType.DefaultPath;
                        template.Released = false;
                        template.User = this.User.UserName;
                        template.IdSection = IdSection;
                        template.RoleRestriction = roleRestriction;
                        template.Mapped = true;

                        db.Entry(template).State = EntityState.Added;
                    }
                    else
                    {
                        // Locate database record
                        var entity = await db.Templates.FindAsync(template.Id);

                        // Update only the defined options of the template
                        entity.Released = false;
                        entity.User = this.User.UserName;
                        entity.UpdatedAt = DateTime.Now;
                        entity.Description = template.Description;
                        entity.Content = template.Content;
                        entity.Url = template.Url;
                        entity.IdSection = IdSection;
                        entity.Restricted = template.Restricted;
                        entity.RoleRestriction = roleRestriction;
                        entity.Mapped = template.Mapped;
                    }

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Refresh site map if allowed
                if (Functions.CMS.Configuration.Get("GenerateSiteMapOnSave").Contains("true"))
                    Functions.CMS.Configuration.GenerateSiteMap();

                Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnSaveTemplate, template);

                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Editor), new { type, id = template.Id });
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Type = type,
                    Template = template,
                    Section = section,
                    Roles = roles
                };
                Functions.CMS.Log.LogRequest(parameters);

                this.NotifyError(ex, ex.Message);
                TempData["Template"] = template;
                return RedirectToAction(nameof(Editor), new { type, id = template.Id });
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Modelos/{type}/salvar-ajax"), ValidateInput(false)]
        public async Task<JsonResult> QuickSave(int id, string type, string value, bool draft = false)
        {
            try
            {
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var entity = await db.Templates.FindAsync(id);

                    // Update only the defined options of the template
                    entity.Released = false;
                    entity.User = this.User.UserName;
                    entity.UpdatedAt = DateTime.Now;
                    entity.Content = value;
                    entity.Version = entity.Version + 1;

                    // Save changes
                    await db.SaveChangesAsync();

                    // Release file if its not draft
                    if (!draft)
                        await ReleaseMethod(id);
                }

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
                    Type = type,
                    Id = id,
                    Value = value,
                    Draft = draft
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [Route("Modelos/{type}/Excluir"), ValidateInput(false)]
        public async Task<JsonResult> RemoveFile(int id, string type)
        {
            try
            {
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var template = await db.Templates.FindAsync(id);

                    // Remove file
                    var filePath = HostingEnvironment.MapPath(Path.Combine(template.Path, template.Name));
                    System.IO.File.Delete(filePath);

                    // Remove template
                    db.Entry(template).State = EntityState.Deleted;

                    // Save changes
                    await db.SaveChangesAsync();
                }

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
                    Type = type,
                    Id = id
                };
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to quick save the file on the system
        /// </summary>
        /// <returns></returns>
        [Route("Modelos/{type}/Renomear"), ValidateInput(false)]
        public async Task<JsonResult> RenameFile(int id, string type, string value)
        {
            try
            {
                var name = string.Empty;
                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    // Locate database record
                    var template = await db.Templates.Include(t => t.TemplateType).FirstOrDefaultAsync(t => t.Id == id);

                    // var check extension
                    name = Path.GetFileNameWithoutExtension(value);
                    name = $"{name}.{template.Extension}";

                    // Remove file
                    var filePath = HostingEnvironment.MapPath(Path.Combine(template.Path, template.Name));
                    var newFile = HostingEnvironment.MapPath(Path.Combine(template.Path, name));
                    System.IO.File.Move(filePath, newFile);

                    // Update template
                    template.Name = name;
                    db.Entry(template).State = EntityState.Modified;

                    // Save changes
                    await db.SaveChangesAsync();
                }

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
                    Type = type,
                    Id = id,
                    Value = value
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to allow the system to publish an specific file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Route("Modelo/{type}/Publicar")]
        public async Task<ActionResult> ReleaseFile(int? id, string type)
        {
            try
            {
                await ReleaseMethod(id);
                this.NotifySuccess(Resources.Strings.Template_AllFilesPublished);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.Message);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Internal method to release the file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task ReleaseMethod(int? id, bool forceRelease = false)
        {
            using (var db = new DatabaseConnection())
            {
                var query = db.Templates.AsQueryable();
                if (id.HasValue)
                    query = db.Templates.Where(t => t.Id == id.Value);

                // perform query execution and loop through data to save in database file
                foreach (var item in await query.Include(t => t.TemplateType).ToListAsync())
                {
                    // if item is already released go next
                    if (item.Released && !forceRelease)
                        continue;

                    // Update entity with new values
                    item.Released = true;

                    // Save file content
                    var fileName = HostingEnvironment.MapPath($"{item.Path}/{item.Name}");
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                    // Check the type 
                    var minify = Functions.CMS.Configuration.Get("MinifyCssAndJs").Contains("true");
                    var content = item.Content;
                    if (content == null)
                        return;

                    // Minify files
                    if (minify && (item.TemplateType.Name == "StyleSheet" || item.TemplateType.Name == "Javascript"))
                    {
                        var status = (item.TemplateType.Name == "StyleSheet" ? Uglify.Css(content) : Uglify.Js(content));
                        if (!status.HasErrors)
                            content = status.Code;
                    }

                    System.IO.File.WriteAllText(fileName, content, Encoding.UTF8);
                }

                // Apply db changes
                await db.SaveChangesAsync();

                // Clear cache data
                Functions.CMS.ClearCache(typeof(Functions.Internal.Functions).FullName);
            }
        }

        /// <summary>
        /// Method to allow user to upload files and Store it on the database and Local Path
        /// </summary>
        /// <param name="type">Type of file to be uploaded</param>
        /// <returns></returns>
        [HttpPost, Route("Modelo/{type}/Upload")]
        public async Task<ActionResult> UploadFile(string type)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Check if there is any file to send
                    if (Request.Files.Count == 0)
                        throw new InvalidOperationException(Resources.Strings.Template_MustSendFile);

                    // Get templatetype
                    var searchType = TranslateTemplateType(type);
                    var templateType = await db.TemplateTypes.FirstOrDefaultAsync(t => t.Name == searchType);

                    // Check extension to see if there is any problem
                    var extension = templateType.DefaultExtension;
                    if (extension != "*")
                        if (Request.Files.GetMultiple("FileUpload").Any(t => Path.GetExtension(t.FileName) != $".{extension}"))
                            throw new InvalidOperationException(Resources.Strings.Template_ExtensionNotAllowed);

                    // All Files are allowed, so keep going and save each of them
                    foreach (HttpPostedFileBase file in Request.Files.GetMultiple("FileUpload"))
                    {
                        var data = new byte[file.ContentLength];
                        await file.InputStream.ReadAsync(data, 0, data.Length);
                        var content = Encoding.UTF8.GetString(data);

                        // Transform content to same pattern
                        if (extension != "*")
                        {
                            content = content.Replace("\r\n", "\n");
                            content = content.Replace("\n", "\r\n");
                        }

                        // Set File Name if already exists
                        var fileName = file.FileName;
                        if (await db.Templates.AnyAsync(t => t.Name == fileName))
                            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.ToString("mmssfff")}{Path.GetExtension(fileName)}";

                        var template = new Template()
                        {
                            Name = fileName,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            Extension = Path.GetExtension(fileName).Trim('.'),
                            Path = templateType.DefaultPath,
                            IdTemplateType = templateType.Id,
                            Content = content,
                            Released = true,
                            User = this.User.UserName
                        };

                        // Set default URL if it's page
                        if (templateType.Name == "View")
                            template.Url = Path.GetFileNameWithoutExtension(fileName).AsUrl();

                        // Add the new template in the databse
                        db.Templates.Add(template);

                        // Save the new File on Disk
                        var fullName = Path.Combine(HostingEnvironment.MapPath(templateType.DefaultPath), fileName);
                        var folder = Path.GetDirectoryName(fullName);
                        Directory.CreateDirectory(folder);
                        file.SaveAs(fullName);
                    }

                    // Apply changes on the database
                    await db.SaveChangesAsync();
                }

                this.NotifySuccess(Resources.Strings.Template_AllFilesPublished);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Type = type
                };
                Functions.CMS.Log.LogRequest(parameters);

                this.NotifyError(ex, ex.Message);
            }

            return RedirectToAction(nameof(Index), new { type });
        }
        #region Functions
        /// <summary>
        /// Internal method that can translate the internal type and return
        /// the corresponding database identification name
        /// </summary>
        /// <param name="type">Url Route parameter</param>
        /// <returns>Returns database identification Type</returns>
        private static string TranslateTemplateType(string type)
        {
            switch (type)
            {
                case "layouts":
                    return "Layout";
                case "componentes":
                    return "Partial";
                case "estilos":
                    return "StyleSheet";
                case "scripts":
                    return "Javascript";
                case "paginas":
                    return "View";
                case "outros":
                    return "Other";
                case "modelos":
                    return "Template";
                default:
                    return string.Empty;
            }
        }
        #endregion
    }
}