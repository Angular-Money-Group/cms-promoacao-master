namespace Bitzar.CMS.Data.Model
{
    using System.Data.Entity;

    public class DatabaseConnection : DbDatabaseContext
    {
        public DatabaseConnection()
        {
            // Sets the command timeout for all the commands
            this.Database.CommandTimeout = 1200;
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RolePermission> RolesPermissions{get; set;}
        public virtual DbSet<UserField> UserFields { get; set; }
        public virtual DbSet<UserSocial> UserSocial { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LogLink> LogLinks { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TemplateType> TemplateTypes { get; set; }
        public virtual DbSet<LibraryType> LibraryTypes { get; set; }
        public virtual DbSet<Library> Library { get; set; }
        public virtual DbSet<FieldType> FieldTypes { get; set; }
        public virtual DbSet<Field> Fields { get; set; }
        public virtual DbSet<FieldValue> FieldValues { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Stats> Stats { get; set; }

        /// <summary>
        /// Mapping of each relashionship of the Entities
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Table Name definitions
            modelBuilder.Entity<User>().ToTable(GetModelTableName(nameof(User)));
            modelBuilder.Entity<Role>().ToTable(GetModelTableName(nameof(Role)));
            modelBuilder.Entity<RolePermission>().ToTable(GetModelTableName(nameof(RolePermission)));
            modelBuilder.Entity<UserField>().ToTable(GetModelTableName(nameof(UserField)));
            modelBuilder.Entity<UserSocial>().ToTable(GetModelTableName(nameof(Model.UserSocial)));
            modelBuilder.Entity<Configuration>().ToTable(GetModelTableName(nameof(Model.Configuration)));
            modelBuilder.Entity<Language>().ToTable(GetModelTableName(nameof(Language)));
            modelBuilder.Entity<LogLink>().ToTable(GetModelTableName(nameof(LogLink)));
            modelBuilder.Entity<TemplateType>().ToTable(GetModelTableName(nameof(TemplateType)));
            modelBuilder.Entity<Template>().ToTable(GetModelTableName(nameof(Template)));
            modelBuilder.Entity<LibraryType>().ToTable(GetModelTableName(nameof(LibraryType)));
            modelBuilder.Entity<Library>().ToTable(GetModelTableName(nameof(Library)));
            modelBuilder.Entity<FieldType>().ToTable(GetModelTableName(nameof(FieldType)));
            modelBuilder.Entity<Field>().ToTable(GetModelTableName(nameof(Field)));
            modelBuilder.Entity<FieldValue>().ToTable(GetModelTableName(nameof(FieldValue)));
            modelBuilder.Entity<Section>().ToTable(GetModelTableName(nameof(Section)));
            modelBuilder.Entity<Stats>().ToTable(GetModelTableName(nameof(Model.Stats)));

            // Set Coumn TEXT to create max allowed data
            modelBuilder.Entity<Template>().Property(p => p.Content).IsMaxLength();
            modelBuilder.Entity<Library>().Property(p => p.Attributes).IsMaxLength();
            modelBuilder.Entity<FieldValue>().Property(p => p.Value).IsMaxLength();
            modelBuilder.Entity<UserField>().Property(p => p.Value).IsMaxLength();
            modelBuilder.Entity<User>().Property(p => p.RestrictedOptions).IsMaxLength();
            modelBuilder.Entity<Field>().Property(p => p.SelectData).IsMaxLength();
            modelBuilder.Entity<UserSocial>().Property(p => p.AccessToken).IsMaxLength();
            modelBuilder.Entity<UserSocial>().Property(p => p.Data).IsMaxLength();

            // Navigation property for User and Roles
            modelBuilder.Entity<User>().HasRequired(e => e.Role).WithMany(e => e.Users).HasForeignKey(e => e.IdRole);
            modelBuilder.Entity<Template>().HasRequired(e => e.TemplateType).WithMany(e => e.Templates).HasForeignKey(e => e.IdTemplateType);
            modelBuilder.Entity<Library>().HasRequired(e => e.LibraryType).WithMany(e => e.Libraries).HasForeignKey(e => e.IdLibraryType);
            modelBuilder.Entity<Field>().HasRequired(e => e.FieldType).WithMany(e => e.Fields).HasForeignKey(e => e.IdFieldType);
            modelBuilder.Entity<Field>().HasOptional(e => e.Template).WithMany(e => e.Fields).HasForeignKey(e => e.IdTemplate);
            modelBuilder.Entity<FieldValue>().HasRequired(e => e.Field).WithMany(e => e.FieldValues).HasForeignKey(e => e.IdField);
            modelBuilder.Entity<FieldValue>().HasRequired(e => e.Language).WithMany(e => e.FieldValues).HasForeignKey(e => e.IdLanguage);
            modelBuilder.Entity<UserField>().HasRequired(e => e.User).WithMany(e => e.UserFields).HasForeignKey(e => e.IdUser);
            modelBuilder.Entity<Template>().HasOptional(e => e.Section).WithMany(e => e.Templates).HasForeignKey(e => e.IdSection).WillCascadeOnDelete(false);

            if (Data.Configuration.Provider == DatabaseProvider.MySql)
                modelBuilder.Entity<Field>().HasOptional(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.IdParent).WillCascadeOnDelete(true);
            else if (Data.Configuration.Provider == DatabaseProvider.SqlServer)
                modelBuilder.Entity<Field>().HasOptional(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.IdParent);
        }
    }
}