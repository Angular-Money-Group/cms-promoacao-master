using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Bitzar.PagFun.Helpers;
using System.Linq;
using Bitzar.PagFun.Models;
using Newtonsoft.Json.Linq;
using Bitzar.CMS.Data.Model;
using System.Globalization;

namespace Bitzar.PagFun
{
    public class Plugin : IPlugin
    {
        internal static ICMS CMS { get; set; }

        /// <summary>
        /// Internal method to get plugin's name
        /// </summary>
        internal static string PluginName => "Bitzar.PagFun.dll";

        /// <summary>
        /// Menu interface to hold menu information
        /// </summary>
        public IList<IMenu> Menus { get; set; } = new List<IMenu>();

        /// <summary>
        /// Routine to process the requests and return data to the system
        /// </summary>
        /// <param name="function">Function identification to call specific method or function inside library</param>
        /// <param name="parameters">Parameter list to process the requests</param>
        /// <param name="values">Value list to match parameter index position </param>
        /// <returns>Returns a dynamic value that should be handled in the caller</returns>
        public dynamic Execute(string function, string token = null, Dictionary<string, string> parameters = null, HttpFileCollectionBase files = null)
        {
            try
            {
                // Validate the request token to process the operations.
                if (token != CMS.Configuration.Token && !CMS.Security.ValidateToken(token))
                    throw new Exception("Token Expirado!");
                dynamic result = null;

                switch (function.ToUpper())
                {
                    case Configurations.PagFun:
                        {
                            break;
                        }
                    case Configurations.ListCitys:
                        {
                            if (!parameters.ContainsKey("Lang") || !int.TryParse(parameters["Lang"], out int Lang))
                                throw new ArgumentException(Messages.LangNotFound);

                            result = Functions.ListCitys(Lang);

                            break;
                        }
                    case Configurations.DebitCreditValue:
                        {
                            EnsureUserIsAuthenticated();

                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int IdUser))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.TryGetValue("OperationDetail", out string OperationDetail) || string.IsNullOrWhiteSpace(OperationDetail))
                                throw new ArgumentException(Messages.OperationDetailNotFound);

                            if (!parameters.ContainsKey("DebitValue") || !decimal.TryParse(parameters["DebitValue"], out decimal DebitValue))
                                throw new ArgumentException(Messages.DebitValueNotFound);


                            result = Functions.DebitCreditValue(IdUser, OperationDetail, DebitValue);

                            break;
                        }
                    case Configurations.GetExtractByUser:
                        {
                            EnsureUserIsAuthenticated();
                            result = Functions.GetExtractByUser();

                            break;
                        }
                    case Configurations.GetCreditsByUser:
                        {
                            EnsureUserIsAuthenticated();

                            var user = Plugin.CMS.Membership.User.Id;
                            result = Functions.GetCreditsByUser(user);

                            break;
                        }
                    case Configurations.GetPromoterTicketValue:
                        {
                            if (!parameters.TryGetValue("PromoterEmail", out string promoterEmail) || string.IsNullOrWhiteSpace(promoterEmail))
                                throw new ArgumentException(Messages.OperationDetailNotFound);

                            if (!parameters.TryGetValue("EventId", out string EventId) || string.IsNullOrWhiteSpace(EventId))
                                throw new ArgumentException(Messages.OperationDetailNotFound);

                            if (!parameters.TryGetValue("TicketId", out string TicketId) || string.IsNullOrWhiteSpace(TicketId))
                                throw new ArgumentException(Messages.OperationDetailNotFound);

                            result = Functions.GetPromoterTicketValue(promoterEmail, Convert.ToInt32(EventId), Convert.ToInt32(TicketId));

                            break;
                        }
                    case Configurations.TransferCreditValue:
                        {
                            EnsureUserIsAuthenticated();

                            if (!parameters.TryGetValue("emailNewOwner", out string emailNewOwner) || string.IsNullOrWhiteSpace(emailNewOwner))
                                throw new ArgumentException(Messages.OperationDetailNotFound);

                            if (!parameters.ContainsKey("DebitValue") || !decimal.TryParse(parameters["DebitValue"], out decimal DebitValue))
                                throw new ArgumentException(Messages.DebitValueNotFound);


                            result = Functions.TransferCreditValue(emailNewOwner, DebitValue);

                            break;
                        }
                    case Configurations.TransferTicket:
                        {
                            EnsureUserIsAuthenticated();

                            if (!parameters.ContainsKey("idTicket") || !int.TryParse(parameters["idTicket"], out int idTicket))
                                throw new ArgumentException(Messages.RequestTransferTicketFail + "idTicket");

                            if (!parameters.ContainsKey("idNewOwner") || !int.TryParse(parameters["idNewOwner"], out int idNewOwner))
                                throw new ArgumentException(Messages.RequestTransferTicketFail + "idNewOwner");

                            if (!parameters.ContainsKey("nameNewOwner"))
                                throw new ArgumentException(Messages.RequestTransferTicketFail + "nameNewOwner");

                            var idCurrentOwner = Plugin.CMS.Membership.User.Id;

                            result = Functions.TransferTicket(idTicket, idNewOwner, parameters["nameNewOwner"], idCurrentOwner);

                            break;
                        }
                    case Configurations.GetUsersByEmailAndName:
                        {
                            EnsureUserIsAuthenticated();

                            if (!parameters.ContainsKey("Search"))
                                throw new ArgumentException(Messages.RequestUsersFail);

                            result = Functions.GetUsersByEmailAndName(parameters["Search"]);

                            break;
                        }
                    case Configurations.GetBestSellers:
                        {
                            result = Functions.GetBestSellers();
                            break;
                        }
                    case Configurations.ListCategoryBestSellers:
                        {
                            if (!parameters.ContainsKey("IdParent") || !int.TryParse(parameters["IdParent"], out int IdParent))
                                throw new ArgumentException(Messages.IdParentNotFound);

                            result = Functions.ListCategoryBestSellers(IdParent);

                            break;
                        }
                    case Configurations.ListEventsByCategory:
                        {
                            if (!parameters.ContainsKey("IdParent") || !int.TryParse(parameters["IdParent"], out int IdParent))
                                throw new ArgumentException(Messages.IdParentNotFound);

                            result = Functions.ListEventsByCategory(IdParent);

                            break;
                        }
                    case Configurations.XDaysBeforeEvent:
                        {
                            var daysBeforeEvent = CMS.Configuration.Get(Configurations.DaysBeforeEvent, PluginName);
                            Functions.NotificateXDaysBefore(Convert.ToInt32(daysBeforeEvent));
                            break;
                        }
                    case Configurations.RetrieveDashboardData:
                        {
                            var inicio = (parameters.ContainsKey("DataInicio") ? parameters["DataInicio"] : "1900-01-01");
                            var fim = (parameters.ContainsKey("DataFim") ? parameters["DataFim"] : DateTime.Now.ToString("yyyy-MM-dd"));

                            if (!parameters.ContainsKey("IdEvent") && !parameters.ContainsKey("Inativos"))
                                result = Functions.GetEventsFromDashboard(null, true, inicio, fim);
                            else
                                result = Functions.GetEventsFromDashboard(Convert.ToInt32(parameters["IdEvent"]), parameters.ContainsKey("Inativos"), inicio, fim);

                            break;
                        }
                    case Configurations.UpdateUserRolePromoter:
                        {
                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int IdUser))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            Functions.UpdateUserRolePromoter(IdUser);

                            result = new { status = "Ok", message = "Usuário atualizado" };

                            break;
                        }
                    case Configurations.InvitePromoter:
                        {
                            EnsureAdminAuthenticated();

                            var userAuth = Functions.GetAuthenticatedUser();

                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("PromoterName", out string PromoterName) || string.IsNullOrWhiteSpace(PromoterName))
                                throw new ArgumentException(Messages.PromoterNameNotFound);

                            if (!parameters.TryGetValue("PromoterEmail", out string PromoterEmail) || string.IsNullOrWhiteSpace(PromoterEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int IdUser))
                                IdUser = -1;

                            if (!parameters.TryGetValue("codPromoter", out string codPromoter) || string.IsNullOrWhiteSpace(codPromoter))
                                codPromoter = null;

                            var product = Functions.GetProduct(IdEvent);

                            var tickets = JsonConvert.DeserializeObject(parameters["Tickets"]);

                            if (userAuth.IdRole == 5 && product.Owners[0].IdUser == userAuth.Id)
                            {

                                var promoterId = "";

                                if (codPromoter != null)
                                {
                                    promoterId = codPromoter;
                                }
                                else
                                {
                                    promoterId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12);

                                }

                                var currentDate = DateTime.Now;

                                Functions.CreateInvite(promoterId, IdEvent, PromoterName, PromoterEmail, currentDate);
                                Functions.AddPromoterEvents(JArray.FromObject(tickets), promoterId, IdEvent);

                                if (IdUser != -1)
                                {
                                    Functions.SendPromoterMail(userAuth, product, PromoterName, PromoterEmail, promoterId, IdUser);
                                    Functions.UpdateStatusPromoterInvite(IdUser, promoterId);
                                }
                                else
                                {
                                    Functions.SendPromoterMail(userAuth, product, PromoterName, PromoterEmail, promoterId, IdUser);
                                }
                            }
                            else
                            {
                                throw new Exception(Messages.NotAuthorizedOperation);
                            }

                            result = new { status = "Ok", message = "Convite enviado" };

                            break;
                        }
                    case Configurations.SendMailLink:
                        {
                            var userAuth = Functions.GetAuthenticatedUser();

                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("PromoterName", out string PromoterName) || string.IsNullOrWhiteSpace(PromoterName))
                                throw new ArgumentException(Messages.PromoterNameNotFound);

                            if (!parameters.TryGetValue("PromoterEmail", out string PromoterEmail) || string.IsNullOrWhiteSpace(PromoterEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            if (!parameters.TryGetValue("CustomerEmail", out string CustomerEmail) || string.IsNullOrWhiteSpace(CustomerEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            if (!parameters.TryGetValue("Link", out string Link) || string.IsNullOrWhiteSpace(Link))
                                throw new ArgumentException(Messages.LinkNotFound);

                            if (!parameters.TryGetValue("Quantidade", out string Quantidade) || string.IsNullOrWhiteSpace(Quantidade))
                                throw new ArgumentException(Messages.QuantidadeNotFound);

                            var product = Functions.GetProduct(IdEvent);

                            Functions.SendGeneratedLinkMail(product, PromoterName, PromoterEmail, CustomerEmail, Link, Quantidade);

                            result = new { status = "Ok", message = "Link enviado" };

                            break;
                        }
                    case Configurations.ListPromotersInviteByEvent:
                        {
                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            result = Functions.ListPromotersInviteByEvent(IdEvent).ToList();

                            break;
                        }
                    case Configurations.GetPromoterEventsByIduser:
                        {
                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int IdUser))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            result = Functions.GetPromoterEventsByIduser(IdUser).ToList();

                            break;
                        }
                    case Configurations.GetPromoterTicketByPromoterCode:
                        {
                            if (!parameters.TryGetValue("PromoterCode", out string PromoterCode) || string.IsNullOrWhiteSpace(PromoterCode))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.ContainsKey("IdTicket") || !int.TryParse(parameters["IdTicket"], out int IdTicket))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            result = Functions.GetPromoterTicketByPromoterCode(PromoterCode, IdTicket);

                            break;
                        }
                    case Configurations.GetPromoterTicketBalanceByPromoterCode:
                        {
                            if (!parameters.TryGetValue("PromoterCode", out string PromoterCode) || string.IsNullOrWhiteSpace(PromoterCode))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.ContainsKey("IdTicket") || !int.TryParse(parameters["IdTicket"], out int IdTicket))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            result = Functions.GetPromoterTicketBalanceByPromoterCode(PromoterCode, IdTicket);

                            break;
                        }
                    case Configurations.GetAdminTicketsBalanceByEvent:
                        {
                            EnsureAdminAuthenticated();

                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            result = Functions.GetAdminTicketsBalanceByEvent(IdEvent);

                            break;
                        }
                    case Configurations.UpdatePromoterEvents:
                        {
                            if (!parameters.ContainsKey("IdPromoterEvent") || !int.TryParse(parameters["IdPromoterEvent"], out int IdPromoterEvent))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            if (!parameters.ContainsKey("TicketQuantity") || !int.TryParse(parameters["TicketQuantity"], out int TicketQuantity))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            if (!parameters.ContainsKey("TicketValue") || !decimal.TryParse(parameters["TicketValue"], out decimal TicketValue))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            if (!parameters.ContainsKey("ChargeFee") || !bool.TryParse(parameters["ChargeFee"], out bool ChargeFee))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            if (!parameters.ContainsKey("TicketsTax") || !decimal.TryParse(parameters["TicketsTax"], out decimal TicketsTax))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            if (!parameters.ContainsKey("vip") || !bool.TryParse(parameters["vip"], out bool vip))
                                throw new ArgumentException(Messages.UpdatePromoterEventsParamNotFound);

                            result = Functions.UpdatePromoterEvents(IdPromoterEvent, TicketQuantity, TicketValue, ChargeFee, TicketsTax, vip);

                            break;
                        }
                    case Configurations.GerarIngressoPromoter:
                        {

                            //valida se o usuario é um administrador autenticado
                            EnsureAdminAuthenticated();

                            if (!parameters.ContainsKey("QtdIngressos") || !int.TryParse(parameters["QtdIngressos"], out int QtdIngressos))
                                throw new ArgumentException(Messages.TickesQuantityNotFound);

                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.ContainsKey("IdSection") || !int.TryParse(parameters["IdSection"], out int IdSection))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.ContainsKey("Product") || !int.TryParse(parameters["Product"], out int Product))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.ContainsKey("Price") || !decimal.TryParse(parameters["Price"], out decimal Price))
                                throw new ArgumentException(Messages.InvalidCreditValue);

                            if (!parameters.ContainsKey("PriceSection") || !decimal.TryParse(parameters["PriceSection"], out decimal PriceSection))
                                throw new ArgumentException(Messages.InvalidCreditValue);

                            if (!parameters.TryGetValue("EventName", out string EventName) || string.IsNullOrWhiteSpace(EventName))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("SectionDay", out string SectionDay) || string.IsNullOrWhiteSpace(SectionDay))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("RateSection", out string RateSection) || string.IsNullOrWhiteSpace(RateSection))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("TicketName", out string TicketName) || string.IsNullOrWhiteSpace(TicketName))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("PromoterEmail", out string PromoterEmail) || string.IsNullOrWhiteSpace(PromoterEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            if (!parameters.TryGetValue("NameNewOwner", out string NameNewOwner) || string.IsNullOrWhiteSpace(NameNewOwner))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.ContainsKey("IdNewOwner") || !int.TryParse(parameters["IdNewOwner"], out int IdNewOwner))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.TryGetValue("SiteTax", out string SiteTax) || string.IsNullOrWhiteSpace(SiteTax))
                                throw new ArgumentException(Messages.SiteTaxNotFound);

                            if (!parameters.TryGetValue("ChargeFee", out string ChargeFee) || string.IsNullOrWhiteSpace(ChargeFee))
                                throw new ArgumentException(Messages.ChargeFeeNotFound);


                            var product = Functions.GetProduct(IdEvent);
                            var userAuth = Functions.GetAuthenticatedUser();

                            // valida se o ticket é do proprio dono
                            if (userAuth.IdRole != 5 || product.Owners[0].IdUser != userAuth.Id)
                                throw new ArgumentException(Messages.NotAuthorizedOperation);

                            // cria o carrinho, adiciona os items e gera os tickets
                            var returnCreateCartAddTickets = Functions.CreateCartAddTickets(IdEvent, QtdIngressos, Product, Price, IdSection, SectionDay, RateSection, PriceSection, EventName, TicketName, userAuth.Id, userAuth.Email, SiteTax, ChargeFee);

                            // set order status para aprovado e emitir tickets
                            Functions.ApproveOrder(returnCreateCartAddTickets.idOrder);

                            var ticketPlugin = Plugin.CMS.Plugins.Get("Bitzar.Tickets.dll");

                            foreach (var ticket in returnCreateCartAddTickets.idTickets)
                            {
                                var parametersTicketPlugin = new Dictionary<string, string>();

                                parametersTicketPlugin.Add("UuidTicket", ticket);

                                var resultTicketPlugin = ticketPlugin?.Plugin?.Execute("GetTicketByUuid", Plugin.CMS.Security.RequestToken, parameters: parametersTicketPlugin);

                                // transferencia do ticket para o promoter
                                Functions.TransferTicket(resultTicketPlugin.Id, IdNewOwner, NameNewOwner, userAuth.Id);
                            }

                            Functions.SendTicketTransferMail(userAuth, product, NameNewOwner, PromoterEmail, returnCreateCartAddTickets.idTickets.Count);

                            result = new { status = "Ok", message = "Ingressos gerados!" };

                            break;
                        }
                    case Configurations.GetUsersForPromoter:
                        {

                            //valida se o usuario é um administrador autenticado
                            EnsureUserIsAuthenticated();

                            var userAuth = Functions.GetAuthenticatedUser();

                            //valida se o usuario é um promoter
                            if (userAuth.IdRole != 8)
                                throw new ArgumentException(Messages.NotAuthorizedOperation);

                            if (!parameters.TryGetValue("ClienteEmail", out string ClienteEmail) || string.IsNullOrWhiteSpace(ClienteEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            var userExists = Plugin.CMS.User.Users().FirstOrDefault(x => x.Email == ClienteEmail);

                            if (userExists == null)
                                result = new { status = "Error", message = "Usuário não encontrado!" };
                            else
                                result = userExists;

                            break;
                        }
                    case Configurations.CreateRemainingPromoterBalance:
                        {
                            if (!parameters.ContainsKey("IdEvento") || !int.TryParse(parameters["IdEvento"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            Functions.CreateRemainingPromoterBalance(IdEvent);

                            result = "OK";

                            break;
                        }
                    case Configurations.GerarIngressoCliente:
                        {

                            //valida se o usuario é um administrador autenticado
                            EnsureUserIsAuthenticated();

                            if (!parameters.TryGetValue("ClienteEmail", out string ClienteEmail) || string.IsNullOrWhiteSpace(ClienteEmail))
                                throw new ArgumentException(Messages.PromoterEmailNotFound);

                            if (!parameters.ContainsKey("QtdIngressos") || !int.TryParse(parameters["QtdIngressos"], out int QtdIngressos))
                                throw new ArgumentException(Messages.TickesQuantityNotFound);

                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.ContainsKey("IdSection") || !int.TryParse(parameters["IdSection"], out int IdSection))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.ContainsKey("Product") || !int.TryParse(parameters["Product"], out int Product))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.ContainsKey("Price") || !decimal.TryParse(parameters["Price"], out decimal Price))
                                throw new ArgumentException(Messages.InvalidCreditValue);

                            if (!parameters.ContainsKey("PriceSection") || !decimal.TryParse(parameters["PriceSection"], out decimal PriceSection))
                                throw new ArgumentException(Messages.InvalidCreditValue);

                            if (!parameters.TryGetValue("EventName", out string EventName) || string.IsNullOrWhiteSpace(EventName))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("SectionDay", out string SectionDay) || string.IsNullOrWhiteSpace(SectionDay))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("RateSection", out string RateSection) || string.IsNullOrWhiteSpace(RateSection))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("TicketName", out string TicketName) || string.IsNullOrWhiteSpace(TicketName))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.TryGetValue("NameNewOwner", out string NameNewOwner) || string.IsNullOrWhiteSpace(NameNewOwner))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            if (!parameters.ContainsKey("IdNewOwner") || !int.TryParse(parameters["IdNewOwner"], out int IdNewOwner))
                                throw new ArgumentException(Messages.IdUserNotFound);
                          
                            if (!parameters.TryGetValue("SiteTax", out string SiteTax) || string.IsNullOrWhiteSpace(SiteTax))
                                throw new ArgumentException(Messages.SiteTaxNotFound);

                            if (!parameters.TryGetValue("ChargeFee", out string ChargeFee) || string.IsNullOrWhiteSpace(ChargeFee))
                                throw new ArgumentException(Messages.ChargeFeeNotFound);

                            var product = Functions.GetProduct(IdEvent);
                            var userAuth = Functions.GetAuthenticatedUser();

                            // valida se o ticket é do proprio dono
                            if (userAuth.IdRole != 8)
                                throw new ArgumentException(Messages.NotAuthorizedOperation);

                            // cria o carrinho, adiciona os items e gera os tickets
                            var returnCreateCartAddTickets = Functions.CreateCartAddTickets(IdEvent, QtdIngressos, Product, Price, IdSection, SectionDay, RateSection, PriceSection, EventName, TicketName, userAuth.Id, userAuth.Email, SiteTax, ChargeFee);

                            // set order status para aprovado e emitir tickets
                            Functions.ApproveOrder(returnCreateCartAddTickets.idOrder);

                            var ticketPlugin = Plugin.CMS.Plugins.Get("Bitzar.Tickets.dll");

                            foreach (var ticket in returnCreateCartAddTickets.idTickets)
                            {
                                var parametersTicketPlugin = new Dictionary<string, string>();

                                parametersTicketPlugin.Add("UuidTicket", ticket);

                                var resultTicketPlugin = ticketPlugin?.Plugin?.Execute("GetTicketByUuid", Plugin.CMS.Security.RequestToken, parameters: parametersTicketPlugin);

                                // transferencia do ticket para o promoter
                                Functions.TransferTicket(resultTicketPlugin.Id, IdNewOwner, NameNewOwner, userAuth.Id);
                            }

                            Functions.SendTicketTransferMail(userAuth, product, NameNewOwner, ClienteEmail, returnCreateCartAddTickets.idTickets.Count);

                            result = new { status = "Ok", message = "Ingressos gerados!" };

                            break;
                        }
                    case Configurations.GetCMSUser:
                        {
                            //valida se o usuario é um administrador autenticado
                            EnsureAdminAuthenticated();

                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int idUser))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            var users = CMS.User.Users();

                            var user = users.FirstOrDefault(f => f.Id == idUser);

                            result = user;

                            break;
                        }
                    case Configurations.GetEstablishmentUsers:
                        {
                            var users = CMS.Membership.Members().Where(x => x.Role.Name == "Estabelecimento").ToList();

                            result = Functions.FormatarUsers(users);

                            return result;
                        }
                    case Configurations.UploadProductImagesToAzureStorage:
                        {
                            var arquivos = JArray.Parse(parameters["Arquivos"]);

                            if (!parameters.TryGetValue("Repositorio", out string Repositorio) || string.IsNullOrWhiteSpace(Repositorio))
                                throw new ArgumentException(Messages.RepositoryNotFound);

                            result = Functions.UploadProductImagesToAzureStorage(arquivos, Repositorio);

                            break;
                        }
                    case Configurations.GetEstablishmentById:
                        {

                            if (!parameters.ContainsKey("IdUser") || !int.TryParse(parameters["IdUser"], out int idUser))
                                throw new ArgumentException(Messages.IdUserNotFound);

                            var users = CMS.User.Users();

                            var user = users.FirstOrDefault(f => f.Id == idUser);

                            result = user;

                            break;
                        }

                    case Configurations.PauseProductCardapio:
                        {
                            EnsureAdminAuthenticated();

                            if (!parameters.TryGetValue("IdsProducts", out string IdsProducts) || string.IsNullOrWhiteSpace(IdsProducts))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            result = Functions.PauseProductCardapio(IdsProducts);
                            break;
                        }
                    case Configurations.DisableProductCardapio:
                        {
                            EnsureAdminAuthenticated();

                            if (!parameters.TryGetValue("IdsProducts", out string IdsProducts) || string.IsNullOrWhiteSpace(IdsProducts))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            result = Functions.DisableProductCardapio(IdsProducts);
                            break;
                        }
                    case Configurations.ActivateProductCardapio:
                        {
                            EnsureAdminAuthenticated();

                            if (!parameters.TryGetValue("IdsProducts", out string IdsProducts) || string.IsNullOrWhiteSpace(IdsProducts))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            result = Functions.ActivateProductCardapio(IdsProducts);
                            break;
                        }
                    case Configurations.UpdateEventDates:
                        {
                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);

                            if (!parameters.ContainsKey("IdSession") || !int.TryParse(parameters["IdSession"], out int IdSession))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.ContainsKey("StartDate") || !DateTime.TryParse(parameters["StartDate"], out DateTime StartDate))
                                throw new ArgumentException(Messages.UpdateDatesNotFound);

                            if (!parameters.ContainsKey("StartSellingDate") || !DateTime.TryParse(parameters["StartSellingDate"], out DateTime StartSellingDate))
                                throw new ArgumentException(Messages.UpdateDatesNotFound);

                            if (!parameters.ContainsKey("EndSellingDate") || !DateTime.TryParse(parameters["EndSellingDate"], out DateTime EndSellingDate))
                                throw new ArgumentException(Messages.UpdateDatesNotFound);

                            Functions.UpdateEventDates(IdEvent, IdSession, StartDate, StartSellingDate, EndSellingDate);

                            Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Plugins.Get_Bitzar.Products");
                            Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");
                            result = "OK";
                            break;
                        }
                    case Configurations.CheckVersion:
                        {
                            // Get the current Version of published app to return in the Api
                            var version = Plugin.CMS.Configuration.Get(Configurations.AppVersionCheck, Plugin.PluginName);
                            result = new { version };
                            break;
                        }
                    case Configurations.GetEventProductsSold:
                        {
                            if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out int IdEvent))
                                throw new ArgumentException(Messages.IdEventNotFound);
                            
                            result = Functions.GetEventProductsSold(IdEvent);
                            break;
                        }

                    case Configurations.GetEstablishmentOrders:
                        {
                            EnsureAnyUserIsAuthenticated();

                            if (!parameters.TryGetValue("StartDate", out string StartDate) || string.IsNullOrWhiteSpace(StartDate))
                                StartDate = DateTime.Now.ToString("yyyy-MM-dd");

                            if (!parameters.TryGetValue("EndDate", out string EndDate) || string.IsNullOrWhiteSpace(EndDate))
                                EndDate = DateTime.Now.ToString("yyyy-MM-dd");
                           
                            var IdEstablishment = Plugin.CMS.Membership.IsAdminAuthenticated ? Plugin.CMS.Membership.AdminUser.Id : Plugin.CMS.Membership.User.IdParent
                                ?? throw new Exception(Messages.IdEstablishmentParentNotFound);

                            result = Functions.GetEstablishmentOrders(IdEstablishment, StartDate, EndDate);
                            break;
                        }

                    case Configurations.AlterEstablishmentStatus:
                        {
                            EnsureAnyUserIsAuthenticated();

                            if (!parameters.TryGetValue("EstablishmentStatus", out string EstablishmentStatus) || string.IsNullOrWhiteSpace(EstablishmentStatus))
                                EstablishmentStatus = Plugin.CMS.Membership.User.UserFields?.FirstOrDefault(x => x.Name == "SituacaoEstabelecimento")?.Value;

                            var IdEstablishment = Plugin.CMS.Membership.IsAdminAuthenticated ? Plugin.CMS.Membership.AdminUser.Id : Plugin.CMS.Membership.User.IdParent 
                                ?? throw new Exception(Messages.IdEstablishmentParentNotFound);


                            result = Functions.AlterEstablishmentStatus(EstablishmentStatus, IdEstablishment);
                            break;
                        }

                    case Configurations.CancelEstablishmentOrder:
                        {
                            EnsureAnyUserIsAuthenticated();

                            if (!parameters.ContainsKey("IdOrder") || !int.TryParse(parameters["IdOrder"], out int IdOrder))
                                throw new ArgumentException(Messages.IdOrderNotFound);

                            if (!parameters.TryGetValue("CancellationReason", out string CancellationReason) || string.IsNullOrWhiteSpace(CancellationReason))
                                throw new ArgumentException(Messages.CancellationReasonNotFound);

                            result = Functions.CancelEstablishmentOrder(IdOrder, CancellationReason);
                            break;                           
                        }

                    case Configurations.RegisterAppError:
                        {
                            if (!parameters.ContainsKey("AppName"))
                                throw new ArgumentException(Messages.UpdateDatesNotFound);

                            if (!parameters.ContainsKey("Message"))
                                throw new ArgumentException(Messages.UpdateDatesNotFound);

                            var user = CMS.Membership.IsAuthenticated ? CMS.Membership.User : null;

                            var appName = parameters["AppName"];
                            var message = parameters["Message"];
                            var stack = parameters.ContainsKey("Stack") ? parameters["Stack"] : "No stack sent.";
                            var content = $"{message}{Environment.NewLine}{stack}";

                            var source = $"{appName}/{(user == null ? "NotLoggedIn" : user.Id.ToString())}";

                            result = CMS.Log.LogRequest(content, "Exception", source, null);
                            break;
                        }

                    case Configurations.GetNextEvents:
                        {
                            result = Functions.GetNextEvents();
                            break;
                        }
                    case Configurations.GetAllEventsActive:
                        {
                            result = Functions.GetAllEventsActive();
                            break;
                        }
                    case Configurations.GetBestSellersEvents:
                        {
                            result = Functions.GetBestSellersEvents();
                            break;
                        }
                    case Configurations.ActivateLotTicket:
                        {
                            EnsureAdminAuthenticated();

                            if (!parameters.ContainsKey("IdProduct") || !int.TryParse(parameters["IdProduct"], out int IdProduct))
                                throw new ArgumentException(Messages.IdProductNotFound);

                            if (!parameters.TryGetValue("SKU", out string sku) || string.IsNullOrWhiteSpace(sku))
                                throw new ArgumentException(Messages.SKUNotFound);

                            int.TryParse(parameters.ContainsKey("IdTicket") ? parameters["IdTicket"] : null, out int IdTicket); // caso IdTicket não esteja preenchido, é um novo ticket

                            Functions.ActivateLotTicket(sku, IdProduct, IdTicket);

                            break;
                        }
                    default:
                        throw new Exception(Messages.CommandNotFound);
                }

                return result;
            }
            catch (Exception e)
            {
                Functions.LogException(e, objects: new object[] { parameters, (CMS.Membership.User ?? CMS.Membership.AdminUser) });
                throw e;
            }
        }
        /// <summary>
        /// Method to return any kind of notification to the system if has.
        /// </summary>
        /// <returns>Returns a list of notifications to the system</returns>
        public IList<INotification> Notifications()
        {
            return new List<INotification>();
        }

        /// <summary>
        /// Method to return any metric available by the plugin
        /// </summary>
        /// <returns>Returns a list of metrics to the system</returns>
        public IList<IMetric> Metrics()
        {
            return new List<IMetric>();
        }

        /// <summary>
        /// Setup method to install anything in the service. With DbConnection it's possible to call every command in the dabase
        /// to create tables and any other stuff desired
        /// </summary>
        /// <param name="context">Context of the application if necessary to run any kind of thing in the request</param>
        /// <param name="database">Context of the database. Allow creation of a command to be processed by the database provider.</param>
        /// <returns></returns>
        public IPlugin Setup(ICMS cms)
        {
            // Store object context
            Plugin.CMS = cms;

            // Create the menu to show in the system
            this.Menus.Add(new Menu()
            {
                Name = "PRODUTOS VENDIDOS",
                Items = new List<IMenuItem>
                {
                    new MenuItem() { Title = "Produtos", Function = "Tickets", Icon = "wb-table" },
                    new MenuItem() { Title = "Dashboard Geral", Function = "Dashboard", Icon = "wb-graph-up" },

                    //Dashboard por evento
                    new MenuItem() { Title = "Dashboard por Evento", Function = "DashboardByEvent", Icon = "wb-graph-up" }
                }
            });

            // Create database tables
            Configurations.Setup();

            // Return a new instance of this class
            return this;
        }

        /// <summary>
        /// Internal method to ensure the user is Administrator and is authenticated
        /// </summary>
        private static void EnsureAdminAuthenticated()
        {
            if (!CMS.Membership.IsAdminAuthenticated)
                throw new UnauthorizedAccessException("Usuário não autenticado para prosseguir com o Request");
        }

        /// <summary>
        /// Internal method to ensure any user is authenticated
        /// </summary>
        private static void EnsureAnyUserIsAuthenticated()
        {
            if (!CMS.Membership.IsAuthenticated && !CMS.Membership.IsAdminAuthenticated)
                throw new UnauthorizedAccessException("Usuário não autenticado para prosseguir com o Request");
        }

        /// <summary>
        /// Internal method to ensure the user is Administrator and is authenticated
        /// </summary>
        private static void EnsureUserIsAuthenticated()
        {
            if (!CMS.Membership.IsAuthenticated)
                throw new UnauthorizedAccessException("Usuário não autenticado para prosseguir com o Request");
        }

        /// <summary>
        /// Method to uninstall the plugin. Becarefull with database scripts to be executed
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            return true;
        }


        /// <summary>
        /// Method to return default Routes to the system
        /// </summary>
        /// <returns></returns>
        public IList<IRoute> Routes()
        {
            return new List<IRoute>();
        }

        /// <summary>
        /// Method to replicate all the idiom keys
        /// </summary>
        public void ReplicateIdiomKeys()
        {
            return;
        }

        /// <summary>
        /// Method to trigger system events 
        /// </summary>
        /// <typeparam name="T">Type of the model</typeparam>
        /// <param name="eventType">Event that triggered the command</param>
        /// <param name="data">Object model</param>
        /// <param name="exception">Exception if has any error in the flow</param>
        public void TriggerEvent<T>(string eventType, T data, Exception exception = null)
        {
            switch (eventType)
            {
                case "OnMemberUserCreating": 
                    var OnMemberUserCreating = (dynamic)data; 
                    var cpf = (string)((IEnumerable<dynamic>)OnMemberUserCreating.UserFields)?.FirstOrDefault(x => x.Name == "CPF")?.Value;

                    if (string.IsNullOrEmpty(cpf))
                        return;

                    using (var db = new Database())
                    {                       
                        var exists = db.Database.SqlQuery<string>(Scripts.GetUserCPF, cpf).ToList().FirstOrDefault();

                        if (!string.IsNullOrEmpty(exists))
                            throw new Exception(Messages.CPFExists);
                    }
                    break;
                case "OnOrderApproved":
                    var orderApproved = (dynamic)data;
                    var userApproved = CMS.User.Users().FirstOrDefault(u => u.Id == orderApproved.IdCustomer);

                    if (userApproved == null)
                    {
                        break;
                    }

                    var totalOrderApproved = (decimal)orderApproved.TotalOrder;

                    var itemApproved = (IEnumerable<dynamic>)orderApproved.Items;
                    var itemFieldsApproved = (IEnumerable<dynamic>)itemApproved.FirstOrDefault().Fields;
                    var valueField = itemFieldsApproved.FirstOrDefault().Value;


                    if (valueField == "Credit")
                    {
                        var modelExtract = itemFieldsApproved.FirstOrDefault().OrderDetail;

                        Functions.AddCredit(modelExtract);

                        var sendDataCreditApproved = new
                        {
                            NomePagante = userApproved.FirstName + " " + userApproved.LastName,
                            TotalCompra = totalOrderApproved.ToString("C2")
                        };

                        var templateCreditApproved = "PagamentoCreditoAprovado.cshtml";
                        var subjectCreditApproved = "Pagamento de crédito aprovado";

                        var contentCreditApproved = Plugin.CMS.Notification.LoadTemplate(templateCreditApproved, sendDataCreditApproved);
                        Plugin.CMS.Notification.SendNotification(contentCreditApproved, subjectCreditApproved, mailTo: new string[] { userApproved.Email });

                        break;
                    }

                    var eventoApproved = Functions.FormattedEvent(Convert.ToInt32(itemFieldsApproved.FirstOrDefault(f => f.Field == "IdEvent").Value));

                    var credit = ((IEnumerable<dynamic>)orderApproved.Fields)?.FirstOrDefault(f => f.Field == "Credit")?.Value;

                    if (!string.IsNullOrEmpty(credit))
                    {
                        var idUser = userApproved.Id;
                        var nomeEvento = eventoApproved.EventName;
                        var creditValue = Convert.ToDecimal(credit.Replace(".", ","));

                        Functions.DebitCreditValue(idUser, nomeEvento, creditValue);
                    }

                    var sendDataApproved = new
                    {
                        NomePagante = userApproved.FirstName + " " + userApproved.LastName,
                        TotalCompra = totalOrderApproved.ToString("C2"),
                        UrlMeusIngressos = $"{Plugin.CMS.Functions.BaseUrl()}{Plugin.CMS.Functions.Url("MeusIngressos")}",
                        Evento = eventoApproved.EventName,
                    };

                    var templateApproved = "PagamentoAprovado.cshtml";
                    var subjectApproved = "Pagamento aprovado";

                    var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateApproved, sendDataApproved);
                    Plugin.CMS.Notification.SendNotification(contentApproved, subjectApproved, mailTo: new string[] { userApproved.Email });
                    break;
                case "OnOrderAbandoned":
                    var orderAbandoned = (dynamic)data;
                    var orderAbandonedFields = (IEnumerable<dynamic>)orderAbandoned.Fields;

                    Functions.RestoreOrderCredit(orderAbandoned, orderAbandonedFields);
                    break;
                case "OnOrderCanceled":
                    var orderCanceled = (dynamic)data;
                    var userCanceled = CMS.User.Users().FirstOrDefault(u => u.Id == orderCanceled.IdCustomer);

                    if (userCanceled == null)
                    {
                        break;
                    }
                    var totalOrderCanceled = (decimal)orderCanceled.TotalOrder;

                    var orderFields = (IEnumerable<dynamic>)orderCanceled.Fields;
                    var itemCanceled = (IEnumerable<dynamic>)orderCanceled.Items;
                    var itemFieldsCanceled = (IEnumerable<dynamic>)itemCanceled.FirstOrDefault().Fields;
                    var valueFields = itemFieldsCanceled.FirstOrDefault().Value;

                    if (valueFields == "Credit")
                    {
                        var sendDataCreditApproved = new
                        {
                            NomePagante = userCanceled.FirstName + " " + userCanceled.LastName,
                            TotalCompra = totalOrderCanceled.ToString("C2")
                        };

                        var templateCreditApproved = "PedidoCreditoCancelado.cshtml";
                        var subjectCreditApproved = "Cancelamento de pedido";

                        var contentCreditApproved = Plugin.CMS.Notification.LoadTemplate(templateCreditApproved, sendDataCreditApproved);
                        Plugin.CMS.Notification.SendNotification(contentCreditApproved, subjectCreditApproved, mailTo: new string[] { userCanceled.Email });

                        break;
                    }

                    Functions.RestoreOrderCredit(orderCanceled, orderFields);

                    var eventoCanceled = Functions.FormattedEvent(Convert.ToInt32(itemFieldsCanceled.FirstOrDefault(f => f.Field == "IdEvent").Value));

                    var sendDataCanceled = new
                    {
                        NomePagante = userCanceled.FirstName + " " + userCanceled.LastName,
                        TotalCompra = totalOrderCanceled.ToString("C2"),
                        UrlMeusIngressos = $"{Plugin.CMS.Functions.BaseUrl()}{Plugin.CMS.Functions.Url("MeusIngressos")}",
                        Evento = eventoCanceled.EventName,
                    };

                    var templateCanceled = "PedidoCancelado.cshtml";
                    var subjectCanceled = "Cancelamento de pedido";

                    var contentCanceled = Plugin.CMS.Notification.LoadTemplate(templateCanceled, sendDataCanceled);
                    Plugin.CMS.Notification.SendNotification(contentCanceled, subjectCanceled, mailTo: new string[] { userCanceled.Email });
                    break;
                case "OnOrderCartUpdatingItem":

                    var OnOrderCartUpdatingItem = (dynamic)data;

                    if (OnOrderCartUpdatingItem["App"] == "true")
                        return;

                    var type = OnOrderCartUpdatingItem["Item.Type"];                   
                  
                    if (type == "Event")
                    {
                        var productPlugin = Plugin.CMS.Plugins.Get("Bitzar.Products.dll");
                        var parametersProduct = new Dictionary<string, string>();
                        parametersProduct.Add("Lang", "1");
                        parametersProduct.Add("Id", OnOrderCartUpdatingItem["Item.IdSection"]);

                        var productCart = productPlugin?.Plugin?.Execute("GetProduct", Plugin.CMS.Security.RequestToken, parameters: parametersProduct);
                        var siteTax = ((IEnumerable<dynamic>)productCart?.Fields)?.FirstOrDefault(x => x.Name == "Taxa de serviços")?.Value;

                        var price = OnOrderCartUpdatingItem["Price"];
                        var quantity = OnOrderCartUpdatingItem["Quantity"];
                        var idSection = OnOrderCartUpdatingItem["Item.IdSection"];
                        var idEvent = OnOrderCartUpdatingItem["Item.IdEvent"];
                        var sectionDay = OnOrderCartUpdatingItem["Item.SectionDay"];
                        var rateSection = OnOrderCartUpdatingItem["Item.RateSection"];
                        var priceSection = OnOrderCartUpdatingItem["Item.PriceSection"];
                        var nameEvent = OnOrderCartUpdatingItem["Item.EventName"];
                        var origemIngresso = OnOrderCartUpdatingItem.ContainsKey("Item.OrigemIngresso") ?  OnOrderCartUpdatingItem["Item.OrigemIngresso"] : null;

                        var KeyValidator = OnOrderCartUpdatingItem["Validator"];

                        var StringValidator = string.Join("#", new string[] { price.Replace(".", ","), idSection, idEvent, nameEvent, sectionDay,
                                                                        rateSection, priceSection});
                        var ValidatorDecrypt = CMS.Security.Decrypt(KeyValidator);
                        if (StringValidator != ValidatorDecrypt)
                        {
                            throw new ArgumentNullException(Messages.RequestValidatorNotFoundOrInvalid);
                        }

                        if(origemIngresso == "Gerado")
                        {
                            if (OnOrderCartUpdatingItem.ContainsKey("Item.ChargeFee") && OnOrderCartUpdatingItem["Item.ChargeFee"] == "Não")
                                OnOrderCartUpdatingItem["Item.SiteTax"] = "0,00";
                        }
                        else if(origemIngresso == "Site")
                        {
                            OnOrderCartUpdatingItem["Item.SiteTax"] = siteTax;
                        }

                    }
                    else if (type == "Product")
                    {
                        var price = OnOrderCartUpdatingItem["Price"];
                        var quantity = OnOrderCartUpdatingItem["Quantity"];
                        var idSection = OnOrderCartUpdatingItem["Item.IdSection"];
                        var idEvent = OnOrderCartUpdatingItem["Item.IdEvent"];
                        var nameProduct = OnOrderCartUpdatingItem["Item.ProductName"];
                        var KeyValidator = OnOrderCartUpdatingItem["Validator"];

                        var validadorStringProd = string.Join("#", new string[] { nameProduct, price.Replace(".", ","), idEvent, idSection });
                        var ValidatorDecrypt = CMS.Security.Decrypt(KeyValidator);

                        if (validadorStringProd != ValidatorDecrypt)
                        {
                            throw new ArgumentNullException(Messages.RequestValidatorNotFoundOrInvalid);
                        }
                    }
                    break;
                case "OnOrderCartSettingOrderFields":
                    {
                        var OnOrderCartSettingOrderFields = (dynamic)data;

                        var saldoAtual = Functions.GetCreditsByUser(Convert.ToInt32(OnOrderCartSettingOrderFields["IdUser"]));

                        var valorCreditoAplicado = OnOrderCartSettingOrderFields["Order.Credit"];

                        valorCreditoAplicado = Convert.ToDecimal(valorCreditoAplicado.Replace(".", ","));

                        var calculo = saldoAtual - valorCreditoAplicado;

                        if (calculo < 0)
                        {
                            throw new ArgumentNullException(Messages.InvalidCreditValue);
                        }

                        break;
                    }
                case "OnPaymentRequest":
                    {

                        var OnPaymentRequest = (dynamic)data;
                        var userId = CMS.Membership.User.Id;

                        // Valida o tipo se [e credito platforma
                        if (OnPaymentRequest["type"] == "PlatformCredit")
                        {
                            using (var db = new Database())
                            {
                                var cartUuid = (string)OnPaymentRequest["cartUuid"];

                                var order = db.Database.SqlQuery<Order>(Scripts.GetOrderByUuid, cartUuid).FirstOrDefault();

                                var saldoAtual = Functions.GetCreditsByUser(Convert.ToInt32(order.IdCustomer));

                                if (saldoAtual - order.Credit > 0)
                                {
                                    if (order.Price - order.Credit == 0)
                                    {
                                        Functions.DebitCreditValue(userId, order.EventName, order.Credit);

                                        db.Database.ExecuteSqlCommand(Scripts.UpdateOrderStatusApprovedUuid, cartUuid);

                                        db.SaveChanges();

                                        var userCreditApproved = CMS.User.Users().FirstOrDefault(u => u.Id == order.IdCustomer);

                                        var sendDataCreditApproved = new
                                        {
                                            NomePagante = userCreditApproved.FirstName + " " + userCreditApproved.LastName,
                                            TotalCompra = order.Price.ToString("C2"),
                                            UrlMeusIngressos = $"{Plugin.CMS.Functions.BaseUrl()}{Plugin.CMS.Functions.Url("MeusIngressos")}",
                                            Evento = order.EventName,
                                        };

                                        var templateCreditApproved = "PagamentoAprovado.cshtml";
                                        var subjectCreditApproved = "Pagamento aprovado";

                                        var contentCreditApproved = Plugin.CMS.Notification.LoadTemplate(templateCreditApproved, sendDataCreditApproved);
                                        Plugin.CMS.Notification.SendNotification(contentCreditApproved, subjectCreditApproved, mailTo: new string[] { userCreditApproved.Email });
                                    }
                                }
                            }
                        }
                        break;
                    }
                case "OnMemberUserCreated":
                    {
                        var OnMemberUserCreated = (T)data;

                        var codPromoter = (OnMemberUserCreated as User).UserFields?.FirstOrDefault(x => x.Name == "CodigoPromoter")?.Value;

                        if (!string.IsNullOrEmpty(codPromoter))
                        {
                            using (var db = new Database())
                            {
                                db.Database.ExecuteSqlCommand(Scripts.UpdateUserRolePromoter, (OnMemberUserCreated as User).Id);
                                db.Database.ExecuteSqlCommand(Scripts.UpdateStatusPromoterInvite, (OnMemberUserCreated as User).Id, codPromoter);

                            }
                        }

                        break;
                    }
                case "OnOrderCartSettingStatus":
                    {
                        var OnOrderCartSettingStatus = (dynamic)data;
                        var ticketsPlugin = Plugin.CMS.Plugins.Get("Bitzar.Tickets.dll");
                        var parametersEvent = new Dictionary<string, string>();

                        switch (OnOrderCartSettingStatus["Status"])
                        {
                            case "Approved":
                                {
                                    parametersEvent.Add("IdOrder", OnOrderCartSettingStatus["Cart"]);

                                    ticketsPlugin?.Plugin?.Execute("EmitTicketsByOrder", Plugin.CMS.Security.RequestToken, parameters: parametersEvent);

                                    break;
                                }
                            case "Canceled":
                                {
                                    parametersEvent.Add("IdOrder", OnOrderCartSettingStatus["Cart"]);
                                    ticketsPlugin?.Plugin?.Execute("CancelTicketByOrder", Plugin.CMS.Security.RequestToken, parameters: parametersEvent);
                                    break;
                                }
                            case "OnOrderAbandoned":
                                {
                                    parametersEvent.Add("IdOrder", OnOrderCartSettingStatus["Cart"]);

                                    ticketsPlugin?.Plugin?.Execute("CancelTicketByOrder", Plugin.CMS.Security.RequestToken, parameters: parametersEvent);
                                    break;
                                }
                        }

                        break;
                    }

                case "OnOrderEmmited":
                    {
                        var tickets = ((IEnumerable<dynamic>)data).ToList();
                        tickets = tickets.Where(t => t.Type == "Event").ToList();

                        if(tickets.Count() > 0)
                        {
                            int idEvent = tickets[0].IdEvent;
                            List<AdminPromoterEventsBalance> balance = Functions.GetAdminTicketsBalanceByEvent(idEvent);
                            var product = Functions.GetProduct(idEvent);
                            List<dynamic> subProducts = ((IEnumerable<dynamic>)product.SubProduct).ToList();

                            foreach (var ticket in tickets.GroupBy(g => g.IdSection))
                            {
                                int qtd = tickets.Where(t => t.IdSection == ticket.Key).Count();

                                var ticketBalance = balance.FirstOrDefault(b => b.SectionId == ticket.Key);
                                var subProduct = subProducts.FirstOrDefault(p => p.Id == ticket.Key);

                                if (ticketBalance.StorageBalance - qtd <= 0 && ((IEnumerable<dynamic>)subProduct.Fields).FirstOrDefault(f => f.Name == "Ingresso de lote?")?.Value == "1")
                                {
                                    string loteAtual = ((IEnumerable<dynamic>)subProduct.Fields).FirstOrDefault(f => f.Name == "Sequência do lote")?.Value;

                                    if (!string.IsNullOrEmpty(loteAtual) && int.TryParse(loteAtual, out int intLoteAtual))
                                    {
                                        dynamic next = Functions.GetNextProductBatch(subProduct.SKU, intLoteAtual, subProducts);

                                        Functions.ActivateLotTicket(subProduct.SKU, idEvent, next == null ? null : next.Id);

                                    }
                                }
                            }
                        }  

                        break;
                    }

            }
            return;
        }
    }
}