using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class UserController : AdminBaseController
    {
        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Usuarios/{type}")]
        [HttpGet]
        public ActionResult Index(string type = "administrativo")
        {
            var admin = (type == "administrativo");
            ViewBag.Admin = admin;
            ViewBag.Type = type;

            return View();
        }

        /// <summary>
        /// Default method to show users filter by role
        /// </summary>
        /// <returns></returns>
        [Route("Usuarios/Role/{id}")]
        [HttpGet]
        public async Task<JsonResult> GetUserByRole(int id)
        {
            try
            {
                var members = Functions.CMS.User.Users().FindAll(x => x.IdRole == id);

                return Json(new { status = "ok", data = JsonConvert.SerializeObject(members.ToList()) }, JsonRequestBehavior.AllowGet);
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
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { status = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to load the users on the system
        /// </summary>
        /// <returns></returns>
        [Route("Usuarios/Listar/{type}/{pagina?}/{tamanho?}")]
        [HttpGet]
        public ActionResult List(string type = "administrativo", bool deepSearch = true, int pagina = 1, int tamanho = 25, string pesquisa = null)
        {
            try
            {
                var admin = (type == "administrativo");
                ViewBag.Admin = admin;
                ViewBag.Type = type;
                ViewBag.Search = pesquisa;

                // Validation for using the relationship filter in list function
                var idRelated = this.User.Id;
                    idRelated = this.User.IdRole > 3 ? Convert.ToInt32(idRelated) : 0;


                // Set Pagination Objects
                var count = Functions.CMS.User.Count(admin, pesquisa, deepSearch, (int)idRelated);
                ViewBag.Pagination = new Pagination()
                {
                    CurrentPage = pagina,
                    Size = tamanho,
                    TotalPages = (int)Math.Ceiling((decimal)count / tamanho),
                    MaxPageItems = 3
                };

                // Get Library data from server
                var data = Functions.CMS.User.List(pagina, tamanho, admin, pesquisa, deepSearch, (int)idRelated);

                // Filter user for non - admin access
                if (admin && this.User.Role.Name != "Administrador")
                        data = data.Where(u => u.Id == this.User.Id).ToList();

                return PartialView("_User", data);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return PartialView("_User", null);
            }
        }


        /// <summary>
        /// Action to show the edit or new entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Usuarios/Detalhe/{type}/{id?}")]
        [HttpGet]
        public ActionResult Detail(int? id, object transfer = null, string type = "administrativo")
        {
            try
            {
                var admin = (type == "administrativo");
                ViewBag.Admin = admin;
                ViewBag.Type = type;

                // Handle create new user data 
                if (!id.HasValue)
                    return View(new User());

                // Handle if the has any problem on save
                if (!id.HasValue && transfer != null)
                    return View(transfer);

                // Locate user data to allow edit
                var user = Functions.CMS.User.Users(true).FirstOrDefault(u => u.AdminAccess == admin && u.Id == id);
                return View(user);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), new { type });
            }
        }

        /// <summary>
        /// Action to show the edit or new entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Usuarios/Complemento/{type}")]
        [HttpGet]
        public ActionResult Additional(int id, string type = "administrativo")
        {
            try
            {
                // Locate user data to allow edit
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.Id == id);
                return View(user);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), new { type });
            }
        }

        /// <summary>
        /// Action add a new field to the member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Usuarios/Adicionar-Campo")]
        [HttpGet]
        public async Task<JsonResult> AddAdditionalField(int id, string value)
        {
            try
            {
                // Locate user data to allow edit
                var user = Functions.CMS.User.Users().FirstOrDefault(u => !u.AdminAccess && u.Id == id);
                using (var db = new DatabaseConnection())
                {
                    var field = new UserField()
                    {
                        IdUser = id,
                        Name = value
                    };

                    db.UserFields.Add(field);

                    // Save changes
                    await db.SaveChangesAsync();

                    //Replica os campos des usuários
                    Functions.CMS.User.ReplicateFieldsUsers();

                    // Clear cache to allow the field be retrieved
                    Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);

                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
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
                    Id = id,
                    Value = value
                };
                Functions.CMS.Log.LogRequest(parameters);
                return Json(new { status = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Action to allow save an entity on the database
        /// </summary>
        [Route("Usuarios/Salvar/{type}"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(User entity, string type = "administrativo")
        {
            try
            {
                var admin = (type == "administrativo");
                entity.AdminAccess = admin;

                if(this.User.IdRole > 3)
                    entity.IdParent = this.User.Id;

                // Get original Password
                string originalPassword = null;
                if (entity.Id != 0)
                    using (var db = new DatabaseConnection())
                        originalPassword = db.Users.Find(entity.Id).Password;

                // Get user fields
                var fields = entity.UserFields;
                entity.UserFields = new List<UserField>();

                using (var db = new DatabaseConnection())
                {
                    // Attach database item
                    db.Users.Attach(entity);

                    // Update password to reflect policy
                    entity.Password = !string.IsNullOrWhiteSpace(entity.Password) ? Security.Cryptography.Encrypt(entity.Password) : originalPassword;

                    // Check if the user is completed
                    if (entity.Completed)
                        if (entity.CompletedAt == null)
                            entity.CompletedAt = DateTime.Now;

                    // Set entity as New or Modified
                    db.Entry(entity).State = (entity.Id == 0 ? EntityState.Added : EntityState.Modified);

                    // Save changes
                    await db.SaveChangesAsync();

                    // Save user fields
                    if (fields.Any())
                    {
                        // Bind userfield with user
                        foreach (var item in fields)
                            item.IdUser = entity.Id;

                        // Identify if the user field already exist for the user
                        var userFields = db.UserFields.Where(u => u.IdUser == entity.Id).ToList();
                        var matchFields = userFields.Where(f => fields.Any(u => f.Name == u.Name)).ToList();
                        var nonMatch = fields.Where(f => !matchFields.Any(u => f.Name == u.Name)).ToList();

                        // Check if match exists
                        foreach (var match in matchFields)
                            match.Value = fields.FirstOrDefault(f => f.Name == match.Name).Value;

                        // Add non match fields
                        foreach (var newItem in nonMatch)
                            db.UserFields.Add(newItem);

                        await db.SaveChangesAsync();
                    }

                    //Replica os campos des usuários
                    Functions.CMS.User.ReplicateFieldsUsers();
                }

                // Clear user cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);

                Functions.CMS.Events.Trigger(CMS.Model.Enumerators.EventType.OnSaveUser, entity);

                // Notify Success
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Index), new { type });
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), new { type });
            }
        }

        /// <summary>
        /// Action to allow save an entity on the database
        /// </summary>
        [Route("Usuarios/Salvar-Adicionais/{type}"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAdditional(int IdUser, string type = "administrativo")
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    foreach (var field in this.Request.Form.AllKeys)
                    {
                        if (!field.Contains("#"))
                            continue;

                        // split parameters
                        var id = Convert.ToInt32(field.Split('#')[1]);
                        var value = this.Request.Form[field];
                        if (string.IsNullOrWhiteSpace(value))
                            value = null;

                        // Locate db object to update
                        var entity = await db.UserFields.FindAsync(id);
                        entity.Value = value;
                    }
                    // Save changes
                    await db.SaveChangesAsync();
                }
                Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);
                // Notify Success
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Index), new { type });
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Index), new { type });
            }
        }

        /// <summary>
        /// Method to allow user to upload files and Store it on the database and Local Path
        /// </summary>
        /// <param name="type">Type of file to be uploaded</param>
        /// <returns></returns>
        [HttpPost, Route("Usuarios/Importar")]
        public async Task<ActionResult> Import()
        {
            try
            {
                // Check if there is any file to send
                if (Request.Files.Count != 1)
                    throw new InvalidOperationException(Resources.Strings.Template_MustSendFile);

                using (var db = new DatabaseConnection())
                {
                    // Check extension to see if there is any problem
                    var uploadedFile = Request.Files.Get("FileUpload");
                    if (Path.GetExtension(uploadedFile.FileName) != ".csv")
                        throw new InvalidOperationException(Resources.Strings.Library_ExtensionNotAllowed);

                    // Read buffer content
                    var buffer = new byte[uploadedFile.ContentLength];
                    await uploadedFile.InputStream.ReadAsync(buffer, 0, buffer.Length);
                    var content = Encoding.UTF8.GetString(buffer);

                    // Create command
                    var sql = new StringBuilder();
                    sql.AppendLine("INSERT INTO btz_user (FirstName,LastName,Email,UserName,Password,IdRole,ChangePassword,AdminAccess,Disabled,Completed)");
                    sql.AppendLine("VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7, @p8,0)");

                    var sqlFields = "INSERT INTO btz_userfield (IdUser, Name, Value) VALUES (@p0, @p1, @p2)";

                    // Loop through each line
                    var customFields = new List<String>();
                    var Roles = Functions.CMS.User.MemberRoles;

                    // Get default role
                    var defaultRoleId = Functions.CMS.Configuration.Get("DefaultMemberRole");
                    var defaultRole = Roles.FirstOrDefault(r => r.Id.ToString() == defaultRoleId);

                    foreach (var line in content.Split(Environment.NewLine.ToCharArray()))
                    {
                        if (line.StartsWith("Nome;Sobrenome;Email;Usuario;Senha;Perfil", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var header = line.Split(';');
                            // Check if has custom fields
                            if (header.Length == 6)
                                continue;

                            // Get custom fields
                            for (var i = 6; i < header.Length; i++)
                                customFields.Add(header[i]);

                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(line.Trim()))
                            continue;

                        // Separate fields to perform command
                        var fields = line.Trim().Split(';');
                        var userName = fields[3];
                        var roleName = fields[5];
                        
                        // Check if the user already exists
                        if (await db.Users.FirstOrDefaultAsync(m => m.UserName == userName) != null)
                            continue;

                        // get role provided in the file
                        var role = Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
                        if (role == null)
                            // Check if there is a role provided, if not, get the default role
                            if (string.IsNullOrWhiteSpace(roleName))
                                role = defaultRole;
                            else
                            {
                                role = new Role() { Name = fields[5], Description = fields[5], AdminRole = false };
                                db.Roles.Add(role);
                                await db.SaveChangesAsync();

                                // Update Roles
                                Roles.Add(role);
                            }

                        // If there is no RoleName and not default Role, get the first
                        if (role == null)
                            role = Roles.FirstOrDefault();

                        // Insert data on the table
                        var password = string.IsNullOrWhiteSpace(fields[4]) ? null : Security.Cryptography.Encrypt(fields[4]);
                        await db.Database.ExecuteSqlCommandAsync(sql.ToString(), fields[0], fields[1], fields[2], fields[3], password, role.Id, true, false, false);

                        // Set custom fields
                        if (customFields.Count > 0)
                        {
                            var userId = await db.Users.FirstOrDefaultAsync(f => f.UserName == userName);
                            for (var i = 0; i < customFields.Count; i++)
                                await db.Database.ExecuteSqlCommandAsync(sqlFields, userId.Id, customFields[i], fields[6 + i]);
                        }
                    }
                    Functions.CMS.User.ReplicateFieldsUsers();
                }

                this.NotifySuccess(Resources.Strings.Import_AllFilesImported);

                // Clear Cache
                Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
            }

            return RedirectToAction(nameof(Index), new { type = "membros" });
        }

        /// <summary>
        /// Method to import users to disable or deleted
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Usuarios/Desativar")]
        public async Task<ActionResult> DisableUsers(bool Disabled)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Check extension to see if there is any problem
                    var uploadedFile = Request.Files.Get("FileUpload");
                    if (Path.GetExtension(uploadedFile.FileName) != ".csv")
                        throw new InvalidOperationException(Resources.Strings.Library_ExtensionNotAllowed);

                    // Read buffer content
                    var buffer = new byte[uploadedFile.ContentLength];
                    await uploadedFile.InputStream.ReadAsync(buffer, 0, buffer.Length);
                    var content = Encoding.UTF8.GetString(buffer).Split(Environment.NewLine.ToCharArray());

                    //Query de update no banco para o campo de desabilitado e deletado
                    var sql = $"UPDATE btz_user SET {(Disabled ? "Disabled" : "Deleted")} = 1 WHERE UserName = @p0 AND AdminAccess = 0";

                    //Loop que percorre todas as linhas verificando o campo userName e executando a query
                    foreach (var line in content)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var fields = line.Trim().Split(';');
                        var userName = fields.Length == 1 ? fields[0] : (int.TryParse(fields[0], out int x) ? fields[4] : fields[3]);

                        await db.Database.ExecuteSqlCommandAsync(sql, userName);
                    } 
                }
                //Limpar cache de usuário
                Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);

                if (Disabled)
                    this.NotifySuccess(Resources.Strings.Import_FilesDisabled);
                else
                    this.NotifySuccess(Resources.Strings.Import_FilesDeleted);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
            }
            return RedirectToAction(nameof(Index), new { type = "membros" });
        }

        /// <summary>
        /// Action to download the import model
        /// </summary>
        [Route("Usuarios/Download-Modelo")]
        [HttpGet]
        public ActionResult Model(bool complex = false)
        {
            // Send it to the response
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=modelo.csv");
            Response.ContentType = "text/csv";
            if (complex)
                Response.Write("Nome;Sobrenome;Email;Usuario;Senha;Perfil;Nome Campo 1;Nome Campo 2;Nome Campo 3;Nome Campo N");
            else
                Response.Write("Nome;Sobrenome;Email;Usuario;Senha;Perfil");
            Response.End();
            HttpContext.ApplicationInstance.CompleteRequest();

            return View();
        }


        /// <summary>
        /// Method to request the user to validate the e-mail
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Usuarios/Requisitar-Validacao")]
        public async Task<JsonResult> RequestMailValidation(int id)
        {
            try
            {
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.Id == id);
                if ((user.Validated.HasValue))
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);

                // Set token to the user
                if (string.IsNullOrWhiteSpace(user.Token))
                    using (var db = new DatabaseConnection())
                    {
                        var token = Guid.NewGuid().ToString();
                        user.Token = token;
                        db.Database.ExecuteSqlCommand("UPDATE btz_user SET Token = @p1 WHERE Id = @p0", user.Id, token);
                    }

                // Set selected user
                Functions.CMS.Membership.SelectedUser = user;
                TempData["User"] = user;

                // Trigger e-mail to validate user
                await this.TriggerNotification("ValidarEmail", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ActivateEmailSubject}", new[] { user.Email });
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to export members to excel file
        /// </summary>
        /// <returns></returns>
        [Route("Usuarios/Exportar")]
        [HttpGet]
        public ActionResult ExportMembers()
        {
            try
            {
                Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);

                // Get Users and fields 
                var users = Functions.CMS.User.Users(true).ToList();
                var fields = users.SelectMany(u => u.UserFields).Select(f => f.Name).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(f => f).ToList();

                // Create Datatable to export
                var dataTable = new DataTable("Usuários");

                // Create table columns
                dataTable.Columns.Add("Id");
                dataTable.Columns.Add("Nome");
                dataTable.Columns.Add("Sobrenome");
                dataTable.Columns.Add("E-mail");
                dataTable.Columns.Add("Usuário");
                dataTable.Columns.Add("Tipo");
                dataTable.Columns.Add("Ultimo Login");
                dataTable.Columns.Add("Inativo");
                dataTable.Columns.Add("Administrativo?");
                dataTable.Columns.Add("Alterar Senha?");
                dataTable.Columns.Add("Cadastro Completo?");
                dataTable.Columns.Add("Completado Em");
                dataTable.Columns.Add("Validação E-mail");
                foreach (var field in fields)
                    dataTable.Columns.Add(field);

                // Fill the datatable with the value data
                foreach (var user in users)
                {
                    var row = dataTable.NewRow();

                    row[0] = user.Id;
                    row[1] = user.FirstName;
                    row[2] = user.LastName;
                    row[3] = user.Email;
                    row[4] = user.UserName;
                    row[5] = user.Role.Name;
                    row[6] = user.LastLogin;
                    row[7] = user.Disabled ? "Sim" : "Não";
                    row[8] = user.AdminAccess ? "Sim" : "Não";
                    row[9] = user.ChangePassword ? "Sim" : "Não";
                    row[10] = user.Completed ? "Sim" : "Não";
                    row[11] = user.CompletedAt;
                    row[12] = user.Validated;
                    foreach (var field in fields)
                        row[dataTable.Columns[field]] = user.UserFields.FirstOrDefault(f => f.Name == field)?.Value;

                    dataTable.Rows.Add(row);
                }

                // Create the excel file and include data inside it
                var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Usuários");

                sheet.Cell(1, 1).InsertTable(dataTable, true);
                sheet.Columns().AdjustToContents();

                // Generate the stream to save the excel file
                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;

                // Export and return data
                var cd = new System.Net.Mime.ContentDisposition { FileName = "Usuarios.xlsx" };
                Response.AppendHeader("Content-Disposition", cd.ToString());
                return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception e)
            {
                this.NotifyError(e, e.Message);
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        #region MemberRoles functions
        /// <summary>
        /// Default method to show membership provider page
        /// </summary>
        /// <returns></returns>
        [Route("Perfil-Acesso")]
        public async Task<ActionResult> Roles()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var roles = await db.Roles.Where(r => !r.AdminRole).ToListAsync();
                    return View(roles);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return View();
            }
        }

        /// <summary>
        /// Action to show the edit or new entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Perfil-Acesso/Detalhe")]
        public async Task<ActionResult> Role_Detail(int? id, object transfer = null)
        {
            try
            {
                // Handle create new member data 
                if (!id.HasValue)
                    return View(new Role());

                // Handle if the has any problem on save
                if (!id.HasValue && transfer != null)
                    return View(transfer);

                // Locate user data to allow edit
                using (var db = new DatabaseConnection())
                {
                    var role = await db.Roles.FirstOrDefaultAsync(r => !r.AdminRole && r.Id == id.Value);
                    return View(role);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Roles));
            }
        }

        /// <summary>
        /// Action to allow save an entity on the database
        /// </summary>
        [Route("Perfil-Acesso/Salvar"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Role_Save(Role entity)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    entity.AdminRole = false;

                    // Attach database item
                    db.Roles.Attach(entity);
                    db.Entry(entity).State = (entity.Id == 0 ? EntityState.Added : EntityState.Modified);

                    // Save changes
                    await db.SaveChangesAsync();
                }

                Functions.CMS.ClearCache(nameof(Functions.CMS.User.MemberRoles));

                // Notify Success
                this.NotifySuccess(Resources.Strings.Data_SuccessfullySaved);
                return RedirectToAction(nameof(Roles));
            }
            catch (Exception ex)
            {
                // Notify Error
                this.NotifyError(ex, ex.AllMessages());
                return RedirectToAction(nameof(Roles));
            }
        }
        #endregion
    }
}