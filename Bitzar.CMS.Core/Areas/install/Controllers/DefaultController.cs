using Bitzar.CMS.Core.Areas.admin.Controllers;
using Bitzar.CMS.Data.Model;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.install.Controllers
{
    [RouteArea("Install", AreaPrefix = "")]
    public class DefaultController : Controller
    {
        [Route("Install")]
        public ActionResult Index()

        {
            // Check if the connection string Already Exists on the database
            var connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"];
            if (connectionString != null && !string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                return Redirect("~/");

            return View();
        }

        [HttpPost, Route("Install/Testar-Conexao")]
        public JsonResult TestConnection(string dbtype, string server, string database, string user, string password, string connectionString, int? port, bool ssl)
        {
            try
            {
                var connString = TestDatabaseConnection(dbtype, server, database, user, password, connectionString, port, ssl);

                return Json(new { status = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Route("Install/Aplicar-Configuracao")]
        public JsonResult ApplyConfiguration(string dbtype, string server, string database, string user, string password, string connectionString, int? port, bool ssl, string userAdmin, string passwordAdmin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userAdmin) || string.IsNullOrWhiteSpace(passwordAdmin))
                    throw new Exception("Usuário e Senha do Administrador devem ser especificados.");

                // Check database connection Again
                var connString = TestDatabaseConnection(dbtype, server, database, user, password, connectionString, port, ssl);

                // Update web.config with new database provided
                var configuration = WebConfigurationManager.OpenWebConfiguration("~");
                configuration.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("DatabaseConnection", connString, GetProvider(dbtype)));
                configuration.Save(ConfigurationSaveMode.Modified);

                // Apply database migration start and Seed database
                Data.Configuration.Migrate<DatabaseConnection>();
                Data.Database.Seed();
                
                using (var db = new DatabaseConnection())
                {
                    // Insert the Admin User in the Database
                    if (!db.Users.Any())
                    {
                        db.Users.Add(new User()
                        {
                            AdminAccess = true,
                            ChangePassword = false,
                            Completed = true,
                            Disabled = false,
                            FirstName = "System",
                            LastName = "Administrator",
                            IdRole = 1,
                            UserName = userAdmin,
                            Password = Security.Cryptography.Encrypt(passwordAdmin)
                        });

                        db.SaveChanges();
                    }

                    // Insert templates in the Database 
                    if (!db.Templates.Any())
                    {
                        // Load type for pages to configure the basic theming
                        var layoutType = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "Layout");
                        var pageType = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "View");
                        var jsType = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "Javascript");
                        var cssType = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "StyleSheet");

                        // Method to Setup default objects of the CMS for Basic start
                        db.Templates.Add(new Template() { Name = "_Layout.cshtml", Path = layoutType.DefaultPath, Extension = layoutType.DefaultExtension, Description = "Layout principal do Site", IdTemplateType = layoutType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.Layout, Restricted = false });
                        db.Templates.Add(new Template() { Name = "404.cshtml", Url = "404", Path = pageType.DefaultPath, Extension = pageType.DefaultExtension, Description = "Página padrão para erro 404 - Not Found", IdTemplateType = pageType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.PageNotFound, Restricted = false });
                        db.Templates.Add(new Template() { Name = "Home.cshtml", Path = pageType.DefaultPath, Extension = pageType.DefaultExtension, Description = "Página padrão de início", IdTemplateType = pageType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.PageHome, Restricted = false });

                        db.Templates.Add(new Template() { Name = "jquery.cycle2.min.js", Path = jsType.DefaultPath, Extension = jsType.DefaultExtension, Description = "Plugin Javascript de Slides", IdTemplateType = jsType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.JsCycle2, Restricted = false });
                        db.Templates.Add(new Template() { Name = "jquery.cycle2.carousel.min.js", Path = jsType.DefaultPath, Extension = jsType.DefaultExtension, Description = "Plugin Javascript de Slides no formato Carousel", IdTemplateType = jsType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.JsCycle2Carousel, Restricted = false });
                        db.Templates.Add(new Template() { Name = "site.js", Path = jsType.DefaultPath, Extension = jsType.DefaultExtension, Description = "Página de Javascript padrão do CMS", IdTemplateType = jsType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = "", Restricted = false });

                        db.Templates.Add(new Template() { Name = "style.css", Path = cssType.DefaultPath, Extension = cssType.DefaultExtension, Description = "Estilo principal do Site", IdTemplateType = cssType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.CssStyle, Restricted = false });
                        db.Templates.Add(new Template() { Name = "shorthands.min.css", Path = cssType.DefaultPath, Extension = cssType.DefaultExtension, Description = "Arquivo de estilos de Apoio", IdTemplateType = cssType.Id, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Released = false, User = userAdmin, Content = Resources.Setup.CssShorthands, Restricted = false });

                        // Apply data
                        db.SaveChanges();
                    }

                    // Force publish files
                    Task.Run(async () => { await TemplateController.ReleaseMethod(null); }).Wait();
                }

                return Json(new { status = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        private static string TestDatabaseConnection(string dbtype, string server, string database, string user, string password, string connectionString, int? port, bool ssl)
        {
            var connString = connectionString;

            try
            {
                // Check database
                switch (dbtype)
                {
                    case "MYSQL":
                        {
                            if (string.IsNullOrWhiteSpace(connectionString))
                            {
                                var builder = new MySqlConnectionStringBuilder
                                {
                                    Server = server,
                                    Database = database,
                                    Port = ((uint?)port) ?? 3306,
                                    PersistSecurityInfo = true,
                                    SslMode = ssl ? MySqlSslMode.Required : MySqlSslMode.None,
                                    UserID = user,
                                    Password = password
                                };

                                connString = builder.ConnectionString;
                            }

                            using (var mySqlConnection = new MySqlConnection(connString))
                            {
                                mySqlConnection.Open();
                                mySqlConnection.Close();
                            }
                        }
                        break;
                    case "SQLSERVER":
                        {
                            if (string.IsNullOrWhiteSpace(connectionString))
                            {
                                var builder = new SqlConnectionStringBuilder
                                {
                                    DataSource = $"{server},{port ?? 1433}",
                                    InitialCatalog = database,
                                    MultipleActiveResultSets = true,
                                    UserID = user,
                                    Password = password,
                                    Encrypt = ssl,
                                    ApplicationName = "Bitzar.CMS"
                                };

                                connString = builder.ConnectionString;
                            }

                            using (var sqlConnection = new SqlConnection(connString))
                            {
                                sqlConnection.Open();
                                sqlConnection.Close();
                            }
                        }
                        break;
                    case null:
                    default:
                        throw new Exception("Provedor de dados não Disponível.");

                }
            }
            finally
            {
                // Output debug
                Debug.WriteLine(connString);
            }

            return connString;
        }
        private static string GetProvider(string dbtype)
        {
            if (dbtype == "MYSQL")
                return "MySql.Data.MySqlClient";

            if (dbtype == "SQLSERVER")
                return "System.Data.SqlClient";

            return dbtype;
        }
    }
}