
using Bitzar.CMS.Data.Model;
using System;
using Bitzar.PagFun.Models;

namespace Bitzar.PagFun.Helpers
{
    internal class Configurations
    {
        /// <summary>
        /// Constants to define the case situations
        /// </summary>
        internal const string PagFun = "PAGFUN";
        internal const string XDaysBeforeEvent = "XDAYSBEFOREEVENT";
        internal const string GetBestSellers = "GETBESTSELLERS";
        internal const string ListCategoryBestSellers = "LISTCATEGORYBESTSELLERS";
        internal const string ListEventsByCategory = "LISTEVENTSBYCATEGORY";
        internal const string GetUsersByEmailAndName = "GETUSERSBYEMAILANDNAME";
        internal const string TransferTicket = "TRANSFERTICKET";
        internal const string ListCitys = "LISTCITYS";
        internal const string RetrieveDashboardData = "RETRIEVEDASHBOARDDATA";
        internal const string GetExtractByUser = "GETEXTRACTBYUSER";
        internal const string GetCreditsByUser = "GETCREDITSBYUSER";
        internal const string DebitCreditValue = "DEBITCREDITVALUE";
        internal const string TransferCreditValue = "TRANSFERCREDITVALUE";
        internal const string InvitePromoter = "INVITEPROMOTER";
        internal const string SendMailLink = "SENDMAILLINK";
        internal const string UpdateUserRolePromoter = "UPDATEUSERROLEPROMOTER";
        internal const string ListPromotersInviteByEvent = "LISTPROMOTERSINVITEBYEVENT";
        internal const string GetPromoterEventsByIduser = "GETPROMOTEREVENTSBYIDUSER";
        internal const string GetPromoterTicketByPromoterCode = "GETPROMOTERTICKETBYPROMOTERCODE";
        internal const string GetPromoterTicketBalanceByPromoterCode = "GETPROMOTERTICKETBALANCEBYPROMOTERCODE";
        internal const string UpdatePromoterEvents = "UPDATEPROMOTEREVENTS";
        internal const string GerarIngressoPromoter = "GERARINGRESSOPROMOTER";
        internal const string GerarIngressoCliente = "GERARINGRESSOCLIENTE";
        internal const string GetUsersForPromoter = "GETUSERSFORPROMOTER";
        internal const string CreateRemainingPromoterBalance = "CREATEREMAININGPROMOTERBALANCE";
        internal const string GetAdminTicketsBalanceByEvent = "GETADMINTICKETSBALANCEBYEVENT";
        internal const string GetPromoterTicketValue = "GETPROMOTERTICKETVALUE";
        internal const string GetCMSUser = "GETCMSUSER";
        internal const string GetEstablishmentUsers = "GETESTABLISHMENTUSERS";
        internal const string UploadProductImagesToAzureStorage = "UPLOADPRODUCTIMAGESTOAZURESTORAGE";
        internal const string FormatarUsers = "FORMATARUSERS";
        internal const string GetEstablishmentById = "GETESTABLISHMENTBYID";
        internal const string PauseProductCardapio = "PAUSEPRODUCTCARDAPIO";
        internal const string DisableProductCardapio = "DISABLEPRODUCTCARDAPIO";
        internal const string ActivateProductCardapio = "ACTIVATEPRODUCTCARDAPIO";
        internal const string UpdateEventDates = "UPDATEEVENTDATES";
        internal const string CheckVersion = "CHECKVERSION";
        internal const string GetEventProductsSold = "GETEVENTPRODUCTSSOLD";        
        internal const string RegisterAppError = "REGISTERAPPERROR";
        internal const string GetEstablishmentOrders = "GETESTABLISHMENTORDERS";
        internal const string AlterEstablishmentStatus = "ALTERESTABLISHMENTSTATUS";
        internal const string CancelEstablishmentOrder = "CANCELESTABLISHMENTORDER";
        internal const string GetNextEvents = "GETNEXTEVENTS";
        internal const string GetAllEventsActive = "GETALLEVENTSACTIVE";
        internal const string GetBestSellersEvents = "GETBESTSELLERSEVENTS";        
        internal const string ActivateLotTicket = "ACTIVATELOTTICKET";

        public const string DaysBeforeEvent = "DaysBeforeEvent";
        public const string ListTickets = "ListTickets";
        public const string AppVersionCheck = "PagFun_AppVersionCheck";

        /// <summary>
        /// Routine to setup the plugin and database
        /// </summary>
        internal static void Setup()
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                db.Database.CommandTimeout = 60;
                try
                {
                    // Setup the database 
                    CMS.Data.Configuration.Migrate<Database>();

                    // Create views on database
                    db.Database.ExecuteSqlCommand(Scripts.ViewProductEvent);
                    db.Database.ExecuteSqlCommand(Scripts.ViewOrderDetailField);
                    db.Database.ExecuteSqlCommand(Scripts.ViewSoldTicketsByOrder);
                    db.Database.ExecuteSqlCommand(Scripts.ViewProductEventDetail);
                    db.Database.ExecuteSqlCommand(Scripts.ViewPromoterEventBalance);
                    db.Database.ExecuteSqlCommand(Scripts.ViewAdminEventBalance);
                    db.Database.ExecuteSqlCommand(Scripts.ViewRemainingPromoters);

                    // Create configurations of the plugin
                    if (!Plugin.CMS.Configuration.ContainsKey(DaysBeforeEvent, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = DaysBeforeEvent,
                            Category = "Notificações",
                            Name = "Dias antes do evento",
                            Description = "Notificar X dias antes do evento",
                            Order = 0,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "number",
                            Value = ""
                        });

                    // Create configurations of the plugin
                    if (!Plugin.CMS.Configuration.ContainsKey(AppVersionCheck, Plugin.PluginName))
                        db.Configurations.Add(new Configuration()
                        {
                            Id = AppVersionCheck,
                            Category = "Aplicativo",
                            Name = "Versão Atual do Aplicativo",
                            Description = "Informar a versão do Aplicativo. Ex: 1.8, versões de aplicativos que forem menor que este valor, exibirão uma mensagem para forçar a atualização do app.",
                            Order = 0,
                            Plugin = Plugin.PluginName,
                            System = false,
                            Type = "decimal",
                            Value = "1.0"
                        });

                    //Aplly pending changes
                    db.SaveChanges();
                    transaction.Commit();

                    // Clear configuration cache
                    Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Configuration");
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    throw e;
                }                
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