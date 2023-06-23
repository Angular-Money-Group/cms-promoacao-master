using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class ContentController : AdminBaseController
    {
        /// <summary>
        /// Method to show the list of content pages
        /// </summary>
        /// <returns>Returns view for content pages</returns>
        [Route("Conteudo")]
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Locate all related templates
                    var templates = Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "View").ToList();
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
        /// Method to show page content editor
        /// </summary>
        /// <param name="id">Page edit</param>
        /// <returns>Returns edit page</returns>
        [Route("Conteudo/Editar")]
        public async Task<ActionResult> Edit(int? id, int? lang)
        {
            try
            {
                ViewBag.Id = id;
                ViewBag.Lang = lang;
                using (var db = new DatabaseConnection())
                {
                    // Locate page
                    if (id.HasValue)
                        ViewBag.Page = await db.Templates.FindAsync(id);

                    // Load Fields
                    var fields = (await db.Fields.Include(f => f.FieldValues)
                                          .Where(f => f.IdTemplate == id && f.IdParent == null && !f.Resource).ToListAsync())
                                          .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

                    // Return data
                    return View(fields);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), "Content", new { area = "admin" });
            }
        }

        /// <summary>
        /// Method to show text content editor
        /// </summary>
        /// <param name="id">Page edit</param>
        /// <returns>Returns edit page</returns>
        [Route("Conteudo/Texto")]
        public async Task<ActionResult> Text(int? lang)
        {
            try
            {
                ViewBag.Lang = lang;
                using (var db = new DatabaseConnection())
                {
                    // Load Fields
                    var fields = (await db.Fields.Include(f => f.FieldValues)
                                          .Where(f => f.IdTemplate == null && f.IdParent == null && f.Resource).ToListAsync())
                                          .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

                    // Return data
                    return View(fields);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), "Content", new { area = "admin" });
            }
        }

        /// <summary>
        /// Method to show blog posts editor
        /// </summary>
        /// <returns>Returns blog post list</returns>
        [Route("Conteudo/Blog")]
        public ActionResult Blog(string category)
        {
            try
            {
                ViewBag.Category = category;
                using (var db = new DatabaseConnection())
                {
                    // Locate all related blog posts
                    var posts = Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "BlogPost").ToList();

                    // Return posts if not need to filter
                    if (string.IsNullOrWhiteSpace(category))
                        return View(posts);

                    posts = posts.Where(p => p.AsBlogPost().Categories.Any(c => c.Equals(category, StringComparison.CurrentCultureIgnoreCase))).ToList();
                    return View(posts);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction("Index", "Default", new { area = "admin" });
            }
        }

        /// <summary>
        /// Method to create a new Blog Post
        /// </summary>
        /// <returns>Returns blog post list</returns>
        [Route("Conteudo/Criar-Blog-Post")]
        public ActionResult CreateBlogPost(string category)
        {
            try
            {
                var post = Functions.CMS.Blog.CreateBlogPost(this.User, category);
                return RedirectToAction(nameof(Edit), new { id = post.Id });
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction("Index", "Default", new { area = "admin" });
            }
        }

        /// <summary>
        /// Method to create a new Blog Post
        /// </summary>
        /// <returns>Returns blog post list</returns>
        [Route("Conteudo/Remover-Blog-Post")]
        public async Task<ActionResult> RemoveBlogPost(int id)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var blogPost = await db.Templates.Include(t => t.Fields).FirstOrDefaultAsync(t => t.Id == id);
                    db.Fields.RemoveRange(blogPost.Fields);
                    db.Templates.Remove(blogPost);

                    await db.SaveChangesAsync();
                }

                // Clear Cache information
                Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);
                Functions.CMS.ClearCache(typeof(Functions.Internal.Functions).FullName);
                Functions.CMS.ClearRoutes();

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to show page content editor
        /// </summary>
        /// <param name="id">Page edit</param>
        /// <returns>Returns edit page</returns>
        [Route("Conteudo/Carregar")]
        public ActionResult Load(int id, int? lang, int? value)
        {
            try
            {
                ViewBag.Lang = lang;
                using (var db = new DatabaseConnection())
                {
                    var language = Functions.CMS.I18N.AvailableLanguages.FirstOrDefault(l => l.Id == (lang ?? Functions.CMS.I18N.DefaultLanguage.Id));

                    // Locate page
                    var values = db.FieldValues
                                        .Include(f => f.Field)
                                        .Include(f => f.Field.Children)
                                        .Include(f => f.Field.Children.Select(v => v.FieldValues))
                                        .Include(f => f.Field.FieldType)
                                        .Where(f => f.IdField == id &&
                                                    f.IdLanguage == language.Id &&
                                                    (!value.HasValue || f.Id == value.Value))
                                        .ToList();

                    // Return page instance
                    return PartialView("_Field", Tuple.Create(values, language));
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return PartialView("_Field", null);
            }
        }


        /// <summary>
        /// Method to save the nre field on the system
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Conteudo/Adicionar-Campo"), ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewField(Field field)
        {
            var idField = field.IdTemplate;
            try
            {
                await CreateNewField(field);

                //Replica o campo criado para todas as postagens do blogs
                if (Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Id == field.IdTemplate).IsBlogPost())
                {
                    var posts = Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "BlogPost" && t.Id != field.IdTemplate).ToList();
                    foreach (var itens in posts)
                    {
                        field.FieldValues = new List<FieldValue>();
                        field.Id = 0;
                        field.IdTemplate = itens.Id;
                        await CreateNewField(field);
                    }
                }

                //Function Clear Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);

                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Edit), new { id = idField });
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.Message);
                return RedirectToAction(nameof(Edit), new { id = idField });
            }
        }

        /// <summary>
        /// Internal method to add a new field on the system
        /// </summary>
        /// <param name="field">Field object to be added on the system</param>
        /// <returns></returns>
        private static async Task CreateNewField(Field field)
        {
            // Logic to store the new template on the database
            using (var db = new DatabaseConnection())
            {
                var maxOrder = await db.Fields.Where(f => f.Group == field.Group && f.IdParent == field.IdParent).Select(f => (int?)f.Order).DefaultIfEmpty().MaxAsync();
                field.Order = (maxOrder ?? 0) + 1;

                db.Fields.Attach(field);
                db.Entry(field).State = EntityState.Added;

                // Save changes
                await db.SaveChangesAsync();

                // Create or replicate values
                var availableLanguages = Functions.CMS.I18N.AvailableLanguages;

                if (field.IdParent == null)
                    foreach (var language in availableLanguages)
                        field.FieldValues.Add(new FieldValue() { IdLanguage = language.Id });
                else
                {
                    // Create Repeater fields
                    var parentField = await db.Fields.Include(f => f.Children)
                                                     .Include(f => f.FieldValues)
                                                     .Include("Children.FieldValues")
                                                     .FirstOrDefaultAsync(f => f.Id == field.IdParent);
                    var fieldValues = parentField.Children.OrderBy(c => c.Id).FirstOrDefault();
                    if (fieldValues == null || fieldValues.FieldValues.Count == 0)
                        foreach (var language in availableLanguages)
                            field.FieldValues.Add(new FieldValue() { IdLanguage = language.Id });
                    else
                        // Replicate the number of rows available
                        foreach (var value in fieldValues.FieldValues.Where(f => f.IdLanguage == Functions.CMS.I18N.DefaultLanguage.Id))
                            foreach (var language in availableLanguages)
                                field.FieldValues.Add(new FieldValue() { IdLanguage = language.Id, Order = value.Order });
                }

                // Save changes
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Method to create new text on the system
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Conteudo/Adicionar-Text")]
        public async Task<ActionResult> AddNewText(string value)
        {
            try
            {
                var group = "Text";
                var parts = value.Split('-');
                if (parts.Length > 1)
                {
                    group = parts[0];
                    value = string.Join("-", parts.Where(p => p != group).ToArray());
                }

                // Check if the field with same name already exist
                using (var db = new DatabaseConnection())
                    if (await db.Fields.AnyAsync(f => f.Resource && f.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)))
                        throw new Exception(Resources.Strings.Field_TextWithSameNameAlreadyExist);

                // Create new Field object to inser in the database
                var textType = Functions.CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Texto");
                var field = new Field()
                {
                    Group = group,
                    Resource = true,
                    Name = value,
                    IdFieldType = textType.Id,
                    IdParent = null,
                    IdTemplate = null
                };

                // Call field Creation and return Id to caller
                await CreateNewField(field);
                return Json(new { status = field.Id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to create a new record for the 
        /// </summary>
        /// <returns></returns>
        [Route("Conteudo/Adicionar-Registro")]
        public async Task<ActionResult> AddNewRecord(int IdField, int IdLanguage)
        {
            Field field;
            var fieldValues = new List<FieldValue>();
            try
            {
                var availableLanguages = Functions.CMS.I18N.AvailableLanguages;

                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    field = await db.Fields.Include(f => f.FieldValues).Include(f => f.FieldType)
                                    .Include(f => f.Children).Include(f => f.Children.Select(c => c.FieldValues))
                                    .FirstOrDefaultAsync(f => f.Id == IdField);

                    // Validate field type to avoid database inconsistence
                    if (field.FieldType.Name != "Repetidor")
                        throw new Exception("Operação permitida apenas para Repetidores.");

                    // Get MaxOrder
                    var order = field.Children.SelectMany(f => f.FieldValues.Select(v => v.Order)).Max() + 1;

                    // Create new FieldValue
                    foreach (var lang in availableLanguages)
                        foreach (var child in field.Children)
                        {
                            var fieldValue = new FieldValue() { IdLanguage = lang.Id, Order = order };
                            child.FieldValues.Add(fieldValue);

                            // Store Record
                            fieldValues.Add(fieldValue);
                        }

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Default language
                var language = availableLanguages.FirstOrDefault(l => l.Id == IdLanguage);

                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return Json(new { status = "ok", items = fieldValues.Select(f => f.Id).ToArray() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url,
                    Field = IdField,
                    Language = IdLanguage
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to remove a row record from the repeater
        /// </summary>
        /// <returns></returns>
        [Route("Conteudo/Remover-Registro")]
        public async Task<ActionResult> RemoveRow(int id, int row, int? lang)
        {
            try
            {
                lang = lang ?? Functions.CMS.I18N.DefaultLanguage.Id;

                // Logic to store the new template on the database
                using (var db = new DatabaseConnection())
                {
                    var field = await db.Fields.Include(f => f.FieldValues)
                                               .Include(f => f.Children)
                                               .Include("Children.FieldValues")
                                               .Where(f => f.Id == id).FirstOrDefaultAsync();

                    // Remove all the field values from children
                    var fieldValues = field.Children.SelectMany(f => f.FieldValues).Where(f => f.Order == row);
                    db.FieldValues.RemoveRange(fieldValues);

                    // Save changes
                    await db.SaveChangesAsync();

                    // Validate if there is empty fields
                    var emptyFields = (from c in field.Children
                                       join v in db.FieldValues on c.Id equals v.IdField into v1
                                       from v in v1.DefaultIfEmpty()
                                       where v == null
                                       select c).ToList();

                    // Add empty row to this fields
                    foreach (var emptyField in emptyFields)
                    {
                        var fieldValue = new FieldValue() { IdLanguage = lang.Value };
                        emptyField.FieldValues.Add(fieldValue);
                    }

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear system Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);

                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Id = id.ToString(),
                    Row = row.ToString(),
                    Language = lang.ToString()
                };

                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to remove a field and all subvalues from the system
        /// </summary>
        /// <returns></returns>
        [Route("Conteudo/Remover-Campo/{id}")]
        public async Task<JsonResult> RemoveField(int id)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Locate the current field
                    var field = await db.Fields.Include(f => f.Children).FirstOrDefaultAsync(f => f.Id == id);

                    // Armazena o template atual
                    var templateId = field.IdTemplate;

                    // Remove all child elements
                    db.Fields.RemoveRange(field.Children);
                    db.Fields.Remove(field);

                    // Remove os campos iguais dos outros posts do blog
                    if (Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Id == templateId).IsBlogPost())
                    {
                        var posts = Functions.CMS.Functions.Templates.Where(t => t.TemplateType.Name == "BlogPost" && t.Id != templateId).ToList();
                        foreach (var post in posts)
                            foreach (var postField in post.Fields)
                                if (field.Name == postField.Name)
                                {
                                    var deletedField = await db.Fields.Include(f => f.Children).FirstOrDefaultAsync(f => f.Id == postField.Id);
                                    db.Fields.RemoveRange(deletedField.Children);
                                    db.Fields.Remove(deletedField);
                                }
                    }

                    // Save changes
                    await db.SaveChangesAsync();
                }

                // Clear system Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Id = id.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to save the current content of the page
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Conteudo/Salvar"), ValidateInput(false), ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? IdTemplate, int IdLanguage)
        {
            var data = Request.Unvalidated.Form;
            var isGlobal = !IdTemplate.HasValue;

            try
            {
                // Open database to update all the objects
                using (var db = new DatabaseConnection())
                {

                    // Load all the fields available in the database
                    var fields = await db.Fields.Include(f => f.FieldValues).Include(f => f.FieldType).Include(f => f.Template)
                                         .Where(f => f.IdTemplate == IdTemplate).ToListAsync();

                    // Capture current template and values
                    var currentTemplate = Functions.CMS.Functions.Templates.FirstOrDefault(f => f.Id == IdTemplate);
                    if (currentTemplate != null)
                    {
                        var currentValues = Functions.CMS.Global.Values;
                        foreach (var field in currentTemplate.Fields)
                            field.FieldValues = currentValues.Where(v => v.IdField == field.Id).ToList();
                    }

                    // Loop through all the fields comparing values
                    foreach (var field in fields)
                    {
                        // Locate the key and look for the form data to see if there is
                        string subKey = $"{field.FieldType.Name}#{field.Id}";
                        foreach (var key in data.AllKeys.Where(k => k.Contains(subKey)))
                        {
                            if (key.Split('-')[0] != subKey)
                                continue;

                            var newValue = data[key];
                            var fieldId = Convert.ToInt32(key.Split('-')[1]);

                            // Handle newValue to not store empty only null.
                            if (string.IsNullOrWhiteSpace(newValue))
                                newValue = null;

                            // Locate corresponding Field value to form data
                            var fieldValue = field.FieldValues.FirstOrDefault(v => v.Id == fieldId);
                            if (fieldValue != null)
                            {
                                // Only update if it has been changed
                                if (fieldValue.Value != newValue)
                                    fieldValue.Value = newValue;
                            }
                            else
                            {
                                // Insert new field value
                                field.FieldValues.Add(new FieldValue()
                                {
                                    IdField = field.Id,
                                    IdLanguage = IdLanguage,
                                    Value = newValue
                                });
                            }
                        }
                    }

                    // Apply changes to the database
                    await db.SaveChangesAsync();

                    // If page is blog post, make special treatment to update some fields
                    if (IdTemplate.HasValue)
                    {
                        var page = fields.First().Template;
                        var lang = Functions.CMS.I18N.DefaultLanguage.Id;
                        if (page.IsBlogPost())
                        {
                            page.Description = fields.FirstOrDefault(f => f.Name == "Título").FieldValues.FirstOrDefault(f => f.IdLanguage == lang).Value;
                            page.Url = fields.FirstOrDefault(f => f.Name == "Url").FieldValues.FirstOrDefault(f => f.IdLanguage == lang).Value;
                            page.Released = fields.FirstOrDefault(f => f.Name == "Publicado").FieldValues.FirstOrDefault(f => f.IdLanguage == lang).Value.Contains("true");
                            page.Restricted = !fields.FirstOrDefault(f => f.Name == "Público").FieldValues.FirstOrDefault(f => f.IdLanguage == lang).Value.Contains("true");

                            await db.SaveChangesAsync();
                        }
                    }

                    // Clear system Cache
                    Functions.CMS.ClearCache(typeof(Functions.Internal.Global).FullName);

                    var newTemplate = Functions.CMS.Functions.Templates.FirstOrDefault(f => f.Id == IdTemplate);
                    if (newTemplate != null)
                    {
                        var newValues = Functions.CMS.Global.Values;
                        foreach (var field in newTemplate.Fields)
                            field.FieldValues = newValues.Where(v => v.IdField == field.Id).ToList();
                    }

                    // Trigger OnSaveContent event
                    if (!isGlobal)
                        Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnSaveContent, Tuple.Create(currentTemplate, newTemplate));

                    return Json(new { status = Resources.Strings.Data_SuccessfullySaved }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Template = IdTemplate,
                    Language = IdLanguage,
                    Content = data
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to allow the users reorder a repeater content
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Conteudo/Reordenar-Repetidor")]
        public async Task<JsonResult> SortRepeater(int IdLanguage, int repeaterId, Dictionary<int,int> sortOrder)
        {
            try
            {
                // Open database to update all the objects
                using (var db = new DatabaseConnection())
                {
                    // Find the field in the table that should be sorted
                    var fields = db.Fields.Where(f => f.IdParent == repeaterId).ToList();
                    var ids = fields.Select(f => f.Id);

                    // Load values that match the current field ids
                    var values = db.FieldValues.Where(f => ids.Any(a => a == f.IdField)).ToList();

                    // Filter language in case
                    if (!Functions.CMS.Configuration.Get("RepeaterReorderForAllLanguages").Contains("true"))
                        values = values.Where(v => v.IdLanguage == IdLanguage).ToList();

                    // Loop through values, looking for the corresponding sorting config and updating their order field
                    foreach (var value in values)
                        value.Order = sortOrder.FirstOrDefault(s => s.Key == value.Order).Value;

                    // Apply changes in the database
                    await db.SaveChangesAsync();
                }

                return Json(new { message = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    IdLanguage = IdLanguage,
                    IdField = repeaterId,
                    Content = sortOrder
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}