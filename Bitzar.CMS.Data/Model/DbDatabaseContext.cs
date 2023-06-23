namespace Bitzar.CMS.Data.Model
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Web.Configuration;

    public class DbDatabaseContext : DbContext
    {
        private const string DefaultSchemaName = "btz";

        // Your context has been configured to use a 'DatabaseConnection' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Bitzar.CMS.Model.DatabaseConnection' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DatabaseConnection' 
        // connection string in the application configuration file.
        public DbDatabaseContext() : 
            base(GetConnection(), false)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public static DbConnection GetConnection()
        {
            // Get in-memory Web.Config
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[Data.Configuration.DATABASE_CONNECTION_ID];

            // Get real file Web.Config
            if (connectionString == null)
                connectionString = WebConfigurationManager.OpenWebConfiguration("~").ConnectionStrings.ConnectionStrings[Data.Configuration.DATABASE_CONNECTION_ID];

            if (connectionString == null)
                throw new Exception("No such connection string found.");

            /* Create the String and return to the System */
            var factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString.ConnectionString;

            return connection;
        }

        /// <summary>
        /// Method to create the specif platform table name
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        protected string GetModelTableName(string modelName)
        {
            return $"{DefaultSchemaName}_{modelName.ToLowerInvariant()}";
        }
    }
}