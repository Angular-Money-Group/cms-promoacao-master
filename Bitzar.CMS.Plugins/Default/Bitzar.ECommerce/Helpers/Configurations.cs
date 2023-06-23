using Bitzar.CMS.Data.Model;
using Bitzar.ECommerce.Models;

namespace Bitzar.ECommerce.Helpers
{
    internal class Configurations
    {
        /// <summary>
        /// Constants to define the case situations
        /// </summary>
        internal const string AddOrUpdateCartItem = "ADDORUPDATECARTITEM";
        internal const string ShowCart = "SHOWCART";
        internal const string SetCustomer = "SETCUSTOMER";
        internal const string DeleteCartItem = "DELETECARTITEM";
        internal const string LoadOrders = "LOADORDERS";
        internal const string SetOrderStatus = "SETORDERSTATUS";
        internal const string SetOrderFields = "SETORDERFIELDS";
        internal const string LoadUserOrders = "LOADUSERORDERS";
        internal const string LoadStatistics = "LOADSTATISTICS";
        internal const string DownloadExcel = "DOWNLOADEXCEL";
        internal const string GenerateAbandonedCarts = "GENERATEABANDONEDCARTS";
        internal const string CalculateShipping = "CALCULATESHIPPING";
        internal const string ShowConfirmation = "SHOWCONFIRMATION";
        //Coupon
        internal const string LoadCoupons = "LOADCOUPONS";
        internal const string GetCoupon = "GETCOUPON";
        internal const string SaveCoupon = "SAVECOUPON";
        internal const string DeleteCoupon = "DELETECOUPON";
        internal const string UseCoupon = "USECOUPON";
        internal const string RemoveCoupon = "REMOVECOUPON";
        internal const string GetCabin = "GETCABIN";
        internal const string GetOccupancy = "GETOCCUPANCY";

        // Default pagination size for the plugin
        internal const int PaginationSize = 25;
        internal const string EnableTraceFlag = "EcommerceTraceEnabled";
        internal const string HiddenDashbordOrder = "HiddenDashbordOrder";
        internal const string TimeAbandonedCard = "TimeAbandonedCard";


        /// <summary>
        /// Routine to setup the plugin and database
        /// </summary>
        internal static void Setup()
        {
            // Setup the database 
            CMS.Data.Configuration.Migrate<Database>();

            // Add configuration in the database if not exists
            using (var db = new DatabaseConnection())
            {
                if (!Plugin.CMS.Configuration.ContainsKey(EnableTraceFlag, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = EnableTraceFlag,
                        Category = "Configuração",
                        Name = "Habilita trace de request e response para E-commerce",
                        Description = "Se habilitado, todos os requests do plugin de e-commerce serão rastreados com os dados da chamada e da saída.",
                        Order = 0,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "checkbox",
                        Value = "true"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(TimeAbandonedCard, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = TimeAbandonedCard,
                        Category = "Configuração",
                        Name = "Tempo em minutos para considerar um carrinho abandonado",
                        Description = "Tempo em minutos após a criação de um carrinho para considera-lo abandonado",
                        Order = 1,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "number",
                        Value = "60"
                    });

                if (!Plugin.CMS.Configuration.ContainsKey(HiddenDashbordOrder, Plugin.PluginName))
                    db.Configurations.Add(new Configuration()
                    {
                        Id = HiddenDashbordOrder,
                        Category = "Configuração",
                        Name = "Desabilitar dashboard dos pedidos",
                        Description = "Se habilitado, desabilita o dashboard dos pedidos.",
                        Order = 2,
                        Plugin = Plugin.PluginName,
                        System = false,
                        Type = "checkbox",
                        Value = "false"
                    });

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Performs nothing
        /// </summary>
        internal static void Uninstall()
        {
            return;
        }
    }
}