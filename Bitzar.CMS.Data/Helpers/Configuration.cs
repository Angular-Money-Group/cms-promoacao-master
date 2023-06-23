using Bitzar.CMS.Data.Migrations;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Web.Configuration;
using System.Linq;

namespace Bitzar.CMS.Data
{
    /// <summary>
    /// Class to hold main method of Data setup
    /// This will execute all database migrations and custom setup
    /// </summary>
    public class Configuration
    {
        internal const string DATABASE_CONNECTION_ID = "DatabaseConnection";
        internal const string MYSQL_DATABASE_PROVIDER = "MySql.Data.MySqlClient";
        internal const string SQLSERVER_DATABSE_PROVIDER = "System.Data.SqlClient";


        /// <summary>
        /// Static class to hold whats provider is currently selected
        /// </summary>
        public static DatabaseProvider? Provider { get; private set; }

        /// <summary>
        /// Main method to perform setup operation
        /// </summary>
        public static void Migrate<T>() where T : DbContext
        {
            Provider = GetProvider();

            // Perform system Migrations
            var configuration = new Configuration<T>() { ContextType = typeof(T) };
            var migrator = new DbMigrator(configuration);

            //This will get the SQL script which will update the DB and write it to debug
            var scriptor = new MigratorScriptingDecorator(migrator);
            string script = scriptor.ScriptUpdate(null, null).ToString();

            //Debug.WriteLine(script);

            //This will run the migration update script and will run Seed() method
            if (!string.IsNullOrWhiteSpace(script))
                ExecuteDatabaseScript(script);
        }

        /// <summary>
        /// Method to get the Current connection  provider
        /// </summary>
        /// <returns>Returns an instance of DatabaseProvider class</returns>
        internal static DatabaseProvider GetProvider()
        {
            var config = ConfigurationManager.ConnectionStrings[DATABASE_CONNECTION_ID];
            if (config == null)
                config = WebConfigurationManager.OpenWebConfiguration("~").ConnectionStrings.ConnectionStrings[DATABASE_CONNECTION_ID];

            switch (config.ProviderName)
            {
                case SQLSERVER_DATABSE_PROVIDER:
                    return DatabaseProvider.SqlServer;
                case MYSQL_DATABASE_PROVIDER:
                    return DatabaseProvider.MySql;
                default:
                    return DatabaseProvider.SqlServer;
            }
        }

        /// <summary>
        /// Main method used to execute system scripts on the database
        /// </summary>
        /// <param name="script">Script to be executed</param>
        private static void ExecuteDatabaseScript(string script)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[DATABASE_CONNECTION_ID];
            if (connectionString == null)
                connectionString = WebConfigurationManager.OpenWebConfiguration("~").ConnectionStrings.ConnectionStrings[DATABASE_CONNECTION_ID];

            switch (Provider)
            {
                case DatabaseProvider.SqlServer:
                    {
                        // Adjust script breaker
                        script = AdjustSqlServerScript(script);

                        using (var connection = new SqlConnection(connectionString.ConnectionString))
                        {
                            connection.Open();
                            var command = new SqlCommand(script, connection);
                            command.ExecuteNonQuery();
                        }
                    }
                    break;
                case DatabaseProvider.MySql:
                    {
                        // Adjust script breaker
                        script = AdjustMySqlScript(script);
                        using (var connection = new MySqlConnection(connectionString.ConnectionString))
                        {
                            connection.Open();

                            try
                            {
                                var command = new MySqlCommand(script, connection);
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);

                                // if any error happen, execute the script in command batchs
                                foreach (var sql in script.Split(';'))
                                {
                                    try
                                    {
                                        var command = new MySqlCommand(sql, connection);
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine(e.Message);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Method adjustments for MySQL EF Compatibility
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string AdjustMySqlScript(string script)
        {
            script = "ALTER TABLE `btz_configuration` CHANGE COLUMN `Id` `Id` VARCHAR(50) NOT NULL " + script;

            // Handle keywords to put command separator ";"
            script = script.Replace("CREATE", ";CREATE")
                           .Replace("create", ";create")
                           .Replace("ALTER", ";ALTER")
                           .Replace("alter", ";alter")
                           .Replace("UPDATE", ";UPDATE")
                           .Replace("update", ";update")
                           .Replace("INSERT", ";INSERT")
                           .Replace("insert", ";insert")
                           .Replace("DELETE", ";DELETE")
                           .Replace("delete", ";delete")
                           .Replace("rename", ";rename")
                           .Replace("RENAME", ";RENAME")
                           .Replace("drop table", ";drop table")
                           .Replace("DROP TABLE", ";DROP TABLE");

            // Handle on update cascade and on delete cascade
            script = script.Replace("ON ;UPDATE", "ON UPDATE")
                           .Replace("on ;update", "on update")
                           .Replace("ON ;DELETE", "ON DELETE")
                           .Replace("on ;delete", "on delete");

            // Fix dbo. schema that leaked from main script
            script = script.Replace("dbo.", "");

            // Remove first command separator
            if (script.StartsWith(";"))
                script = script.Substring(1);

            return script;
        }

        /// <summary>
        /// Method adjustments for MySQL EF Compatibility
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string AdjustSqlServerScript(string script)
        {
            // Fix dbo. schema that leaked from main script
            script = script.Replace("dbo.", "");
            return script;
        }

        /// <summary>
        /// Method to create and return a token to the system
        /// </summary>
        /// <returns></returns>
        public static string CreateToken()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString())).Trim('=');
        }
    }
}
