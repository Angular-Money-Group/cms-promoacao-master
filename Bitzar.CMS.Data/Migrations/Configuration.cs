namespace Bitzar.CMS.Data.Migrations
{
    using Bitzar.CMS.Data.Model;
    using MySql.Data.EntityFramework;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration<T> : DbMigrationsConfiguration<DbContext>
    {
        public Configuration()
        {
            ContextKey = "Bitzar.CMS.Data.Migrations.Configuration";
            if (typeof(T) != typeof(DatabaseConnection))
                ContextKey = $"Migrations[{typeof(T).FullName}]";

            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            if (Data.Configuration.Provider == DatabaseProvider.MySql)
            {
                SetSqlGenerator(Data.Configuration.MYSQL_DATABASE_PROVIDER, new MySqlMigrationSqlGenerator());
                CodeGenerator = new MySqlMigrationCodeGenerator();
            }
        }
    }
}
