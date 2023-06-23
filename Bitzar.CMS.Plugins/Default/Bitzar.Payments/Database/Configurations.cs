using Bitzar.CMS.Data.Model;
using Bitzar.Payments.Models;

namespace Bitzar.Payments.Helpers
{
    public class Configurations
    {
        //Constants to name configurations
        public const string Zoop_AccessToken = "Zoop_AccessToken";
        public const string Zoop_ModuleUrl = "Zoop_ModuleUrl";
        public const string Zoop_Key = "Zoop_Key";
        public const string Zoop_Id = "Zoop_Id";
        public const string Zoop_MainVendor = "Zoop_MainVendor";

        public const string Braspag_ModuleUrl = "Braspag_ModuleUrl";
        public const string Braspag_ModuleQueryUrl = "Braspag_ModuleQueryUrl";
        public const string Braspag_MerchantId = "Braspag_MerchantId";
        public const string Braspag_MerchantKey = "Braspag_MerchantKey";
        public const string Braspag_BoletoProvider = "Braspag_BoletoProvider";
        public const string Braspag_PixProvider = "Braspag_PixProvider";
        public const string Braspag_CartaoCreditoProvider = "Braspag_CartaoCreditoProvider";

        public const string GenerateDebugTransaction = "GenerateDebugTransaction";
        public const string DefaultOriginTransaction = "DefaultOriginTransaction";
        public const string InstallmentMode = "InstallmentMode";
        public const int PaginationSize = 25;

        /// <summary>
        /// Method to setup database objects
        /// </summary>
        internal static void Setup()
        {
            CMS.Data.Configuration.Migrate<Database>();

            using (var db = new DatabaseConnection())
            {

                #region Config Zoop

                //Create configurations of the plugin
                if (!Plugin.CMS.Configuration.ContainsKey(Zoop_AccessToken, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Zoop_AccessToken,
                        Category = "Zoop",
                        Name = "Token de acesso ao modulo Zoop",
                        Description = "Token de acesso ao modulo Zoop",
                        Order = 0,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Zoop_ModuleUrl, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Zoop_ModuleUrl,
                        Category = "Zoop",
                        Name = "Url de integração Zoop",
                        Description = "Url de integração para o módulo Zoop",
                        Order = 1,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Zoop_Key, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Zoop_Key,
                        Category = "Zoop",
                        Name = "Chave de integração Zoop",
                        Description = "Chave de integração Zoop",
                        Order = 2,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Zoop_Id, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Zoop_Id,
                        Category = "Zoop",
                        Name = "Marketplace Id Zoop",
                        Description = "Marketplace Id Zoop",
                        Order = 3,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Zoop_MainVendor, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Zoop_MainVendor,
                        Category = "Zoop",
                        Name = "Vendedor principal Zoop",
                        Description = "Vendedor principal Zoop",
                        Order = 4,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                #endregion Config Zoop

                #region Config Braspag

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_ModuleUrl, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_ModuleUrl,
                        Category = "Braspag",
                        Name = "Url de integração Braspag",
                        Description = "Url de integração para o módulo Braspag",
                        Order = 0,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_ModuleQueryUrl, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_ModuleQueryUrl,
                        Category = "Braspag",
                        Name = "Url de integração Braspag - ApiQuery",
                        Description = "Url de integração para o módulo Braspag - ApiQuery",
                        Order = 0,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_MerchantId, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_MerchantId,
                        Category = "Braspag",
                        Name = "MerchantId Braspag",
                        Description = "MerchantId de acesso ao módulo Braspag",
                        Order = 1,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_MerchantKey, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_MerchantKey,
                        Category = "Braspag",
                        Name = "MerchantKey Braspag",
                        Description = "MerchantKey de acesso ao módulo Braspag",
                        Order = 2,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "text",
                        Value = ""
                    });


                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_BoletoProvider, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_BoletoProvider,
                        Category = "Braspag",
                        Name = "Provider - Boleto",
                        Description = "Provedores do meio de pagamento Boleto",
                        Order = 3,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "select",
                        Source = "Simulado|Braspag|Bradesco2",
                        Value = "Simulado"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_PixProvider, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_PixProvider,
                        Category = "Braspag",
                        Name = "Provider - Pix",
                        Description = "Provedores do meio de pagamento Pix",
                        Order = 4,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "select",
                        Source = "Cielo30|Bradesco2",
                        Value = "Cielo30"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(Braspag_CartaoCreditoProvider, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = Braspag_CartaoCreditoProvider,
                        Category = "Braspag",
                        Name = "Provider - Cartão de Crédito",
                        Description = "Provedores do meio de pagamento Cartão de Crédito",
                        Order = 5,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "select",
                        Source = "Simulado|Cielo30",
                        Value = "Simulado"
                    });

                #endregion

                #region Config Payments

                if (!Plugin.CMS.Configuration.ContainsKey(DefaultOriginTransaction, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = DefaultOriginTransaction,
                        Category = "Configuração",
                        Name = "Valor padrão para o gateway de pagamento",
                        Description = "Informa o gateway de pagamento padrão caso, o mesmo será utilizado caso especificado no instante de pagamento.",
                        Order = 5,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "select",
                        Source = "Zoop|",
                        Value = "Zoop"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(InstallmentMode, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = InstallmentMode,
                        Category = "Taxas de parcelamento",
                        Name = "Modo de cobrança das taxas de parcelamento",
                        Description = "Regime de taxas vigente sobre os parcelamentos",
                        Order = 6,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "select",
                        Source = "interest_free|with_interest",
                        Value = "interest_free"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(GenerateDebugTransaction, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = GenerateDebugTransaction,
                        Category = "Configuração",
                        Name = "Gera um valor de cobrança entre R$ 0,10 e R$ 0,25 para propositos de debug",
                        Description = "Se habilitado, todas as cobranças ignorarão o valor da transação e será enviado um valor de teste enter 10 e 25 centavos.",
                        Order = 7,
                        Plugin = Plugin.PluginName,
                        System = true,
                        Type = "checkbox",
                        Value = "true"
                    });

                #endregion Config Payments

                //Aplly pending changes
                db.SaveChanges();

                // Clear configuration cache
                Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Configuration");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void Uninstall()
        {
            // Does Nothing
        }
    }
}