using Bitzar.CMS.Data.Model;
using Bitzar.Products.Helper;
using System;
using System.Linq;

namespace Bitzar.Products.Helpers
{
    public class Database
    {
        public const string NameSku = "NameSku";
        public const string DescProduto = "DescProduto";
        public const string DescImagem = "DescImagem";
        public const string NameProduct = "NameProduct";
        public const string NameCombo = "NameCombo";
        public const string NameSubProduct = "NameSubProduct";
        public const string NameCadastros = "NameCadastros";
        public const string IsUserOwned = "IsUserOwned";
        public const string IsProdDisabByAdmin = "IsProdDisabByAdmin";
        public const string SelectOwnerText = "SelectOwnerText";
        public const string OwnerType = "OwnerType";
        public const string ClearCache = "ClearCache";
        public const string CategoryParent = "CategoryParent";
        public const string UserRolesToList = "UserRolesToList";
        public const string UserRolesToEditAll = "UserRolesToEditAll";
        public const string AutomaticallyActivateNewProducts = "AutomaticallyActivateNewProducts";
        public const string ProductsFilterByIdParent = "ProductsFilterByIdParent";
        public const string GroupedSubProducts = "GroupedSubProducts";

        /// <summary>
        /// Method to setup database objects
        /// </summary>
        internal static void Setup()
        {
            using (var db = new DatabaseConnection())
            {
                // Start database transaction
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                db.Database.CommandTimeout = 60;

                try
                {
                    // Create all tables
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductType);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_Product);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductFieldGroup);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductField);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductFieldType);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductFieldValue);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_Category);

                    var fieldsCategory = db.Database.SqlQuery<string>(Scripts.Select_Category_Fields).ToList();
                    
                    if (!fieldsCategory.Any(f => f == "Highlighted"))
                        // Create the new columns
                        db.Database.ExecuteSqlCommand(Scripts.AlterTable_Category_Field_Highlighted);

                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_CategoryValue);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductCategory);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductReleated);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_AttributeType);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_Attribute);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_AttributeTypeValues);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_AttributeValues);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductAttribute);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductSub);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_Combo);
                    db.Database.ExecuteSqlCommand(Scripts.CreateTable_ProductUser);

                    // Validate if the field UpdatedAt was already created
                    var fields = db.Database.SqlQuery<string>(Scripts.Select_Product_Fields).ToList();
                    if (fields.Any(f => f == "Updated_At") || fields.Any(f => f == "Created_At"))
                        // Old plugin version should rename UpdatedAt and CreatedAt
                        db.Database.ExecuteSqlCommand(Scripts.AlterTable_RenameUpdatedAndCreatedAt);
                    else if (!fields.Any(f => f == "UpdatedAt"))
                        // Create the new columns
                        db.Database.ExecuteSqlCommand(Scripts.AlterTable_AddUpdatedAndCreatedAt);

                    // Create indexes needed to speedup data, ignoring the exception thrown
                    db.Database.ExecuteSqlCommand(Scripts.CreateGroup_Basic);
                    db.Database.ExecuteSqlCommand(Scripts.CreateField_Description);
                    db.Database.ExecuteSqlCommand(Scripts.CreateField_Url);
                    db.Database.ExecuteSqlCommand(Scripts.CreateField_Text);
                    db.Database.ExecuteSqlCommand(Scripts.CreateField_Gallery);

                    // Plugin configuration
                    #region Plugin configuration
                    if (!Plugin.CMS.Configuration.ContainsKey(Constants.ConfigProductPage, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = Constants.ConfigProductPage,
                            Category = "Configuração",
                            Name = "Página de Navegação dos Produtos e Categorias",
                            Description = "Indica a página de retorno quando uma URL de produto ou categoria é disparada",
                            Order = 0,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "select",
                            Source = "page",
                            Value = "Produto.cshtml"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(Constants.ConfigProductSection, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = Constants.ConfigProductSection,
                            Category = "Configuração",
                            Name = "Seção de navegação da URL",
                            Description = "Se informado, o sistema adicionará esta seção na URL dos links gerados.",
                            Order = 1,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = ""
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(NameSku, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = NameSku,
                            Category = "Nomenclatura",
                            Name = "Nome do SKU",
                            Description = "Nome do SKU exibido na tela de cadastro de produtos",
                            Order = 2,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "SKU"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(DescProduto, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = DescProduto,
                            Category = "Nomenclatura",
                            Name = "Nome da Descrição do Produto",
                            Description = "Nome da Descrição exibido na tela de cadastro de produtos",
                            Order = 3,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Descrição principal do Produto"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(DescImagem, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = DescImagem,
                            Category = "Nomenclatura",
                            Name = "Nome da Imagem do Produto",
                            Description = "Nome da Descrição da Imagem exibido na tela de cadastro de produtos",
                            Order = 4,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Galeria de imagens para o Produto"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(NameProduct, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = NameProduct,
                            Category = "Nomenclatura",
                            Name = "Nome Produto",
                            Description = "Nome exibido em todas as menções do 'Produto'",
                            Order = 6,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Produto"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(NameCombo, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = NameCombo,
                            Category = "Nomenclatura",
                            Name = "Nome Combo",
                            Description = "Nome exibido em todas as menções do 'Produto'",
                            Order = 6,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Combo"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(NameSubProduct, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = NameSubProduct,
                            Category = "Nomenclatura",
                            Name = "Nome Sub-Produto",
                            Description = "Nome exibido em todas as menções do 'Sub-Produto'",
                            Order = 6,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Sub-Produto"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(NameCadastros, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = NameCadastros,
                            Category = "Nomenclatura",
                            Name = "Nome Menu Produtos",
                            Description = "Nome exibido em todas as menções do 'Menu Produtos'",
                            Order = 7,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Produtos"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(SelectOwnerText, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = SelectOwnerText,
                            Category = "Nomenclatura",
                            Name = "Proprietário do produto",
                            Description = "Placeholder para o combo de seleção de proprietário do produto",
                            Order = 8,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Selecione a Empresa"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(OwnerType, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = OwnerType,
                            Category = "Nomenclatura",
                            Name = "Coluna proprietário do produto",
                            Description = "Título da coluna que indica o proprietário do produto",
                            Order = 9,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = "Empresa"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(IsUserOwned, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = IsUserOwned,
                            Category = "Configuração",
                            Name = "Listar por usuário",
                            Description = "Habilita listagem de produtos por usuário",
                            Order = 2,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "checkbox",
                            Value = "false",
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(IsProdDisabByAdmin, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = IsProdDisabByAdmin,
                            Category = "Configuração",
                            Name = "Desabilitado somente por admin",
                            Description = "Quando marcado somente admin pode desabilitar.",
                            Order = 3,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "checkbox",
                            Value = "false",
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(ClearCache, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = ClearCache,
                            Category = "Configuração",
                            Name = "Limpeza manual de cache",
                            Description = "Quando marcado, ao salvar novos produtos ou categorias o cache Global não será limpo",
                            Order = 4,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "checkbox",
                            Value = "false",
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(CategoryParent, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = CategoryParent,
                            Category = "Configuração",
                            Name = "Categorias Exibidas no Cadastro",
                            Description = "Listar categorias Parent separadas pela Vírgula ",
                            Order = 5,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "text",
                            Value = ""
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(UserRolesToList, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = UserRolesToList,
                            Category = "Configuração",
                            Name = "Usuários a serem listados",
                            Description = "Lista de ids de roles de usuários separadas por Vírgula ",
                            Order = 6,
                            Plugin = Plugin.PluginName,
                            System = true,
                            Type = "text",
                            Value = ""
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(UserRolesToEditAll, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = UserRolesToEditAll,
                            Category = "Configuração",
                            Name = "Usuários que podem listar todos os produtos",
                            Description = "Lista de ids de roles de usuários separadas por Vírgula ",
                            Order = 6,
                            Plugin = Plugin.PluginName,
                            System = true,
                            Type = "text",
                            Value = ""
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(AutomaticallyActivateNewProducts, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = AutomaticallyActivateNewProducts,
                            Category = "Configuração",
                            Name = "Ativar novos produtos automáticamente?",
                            Description = "Define a ativação padronizada de novos produtos ",
                            Order = 7,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "checkbox",
                            Value = "true"
                        });
                    if (!Plugin.CMS.Configuration.ContainsKey(GroupedSubProducts, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = GroupedSubProducts,
                            Category = "Configuração",
                            Name = "Agrupar subprodutos ?",
                            Description = "Ativa o agrupamento do sub produto por SKU,Description, Url, Disabled e IdType",
                            Order = 7,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "checkbox",
                            Value = "false"
                        });
                    #endregion

                    // Apply pending changes
                    db.SaveChanges();

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    throw e;
                }
            }
        }

        internal static void Uninstall()
        {
            // Does nothing
        }
    }
}