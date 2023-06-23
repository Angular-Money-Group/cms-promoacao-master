
using Bitzar.CMS.Data.Model;
using Bitzar.PagFun.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Database = Bitzar.PagFun.Models.Database;
using Bitzar.Tickets.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;

namespace Bitzar.PagFun.Helpers
{
    public class Functions
    {
        #region Internal methods to help plugin structure
        /// <summary>
        /// Get Authenticated User
        /// </summary>
        /// <returns></returns>
        internal static User GetAuthenticatedUser()
        {
            if (Plugin.CMS.Membership.IsAuthenticated)
                return Plugin.CMS.Membership.User;

            if (Plugin.CMS.Membership.IsAdminAuthenticated)
                return Plugin.CMS.Membership.AdminUser;

            return null;
        }

        /// <summary>
        /// Method to execute any kind of code ignoring if a error is thrown by the system
        /// </summary>
        /// <param name="action">Action to be executed</param>
        internal static void RunIgnoreError(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Trace(e);
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Internal trace message for debugging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="caller"></param>
        internal static void Trace(Exception ex, [CallerMemberName] string caller = "")
        {
            Debug.WriteLine($"** {caller} **");
            Debug.WriteLine(ex.Message);
        }
        #endregion

        private static char[] SEPARATOR = new[] { ',' };

        /// <summary>
        /// Internal trace message for debugging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="caller"></param>
        internal static void LogException(Exception ex, [CallerMemberName] string caller = "", params object[] objects)
        {
            // Get Logged User
            var loggedUser = Plugin.CMS.Membership.User ?? Plugin.CMS.Membership.AdminUser;

            // Call the method to perform the log
            Plugin.CMS.Log.LogRequest(ex, "Exception", Plugin.PluginName, caller, objects, loggedUser);

            Debug.WriteLine($"** {caller} **");
            Debug.WriteLine(ex.Message);
        }
        public static dynamic ListCitys(int lang)
        {
            using (var db = new Database())
            {
                var eventsActive = db.Database.SqlQuery<FormattedEventAttribute>(Scripts.GetAllEventsActive).ToList();
                var listAttributesActive = new List<FormatedAttribute>();

                foreach (var ev in eventsActive)
                {
                    DateTime dateFinal = DateTime.Parse(ev.EventFinalDate);

                    if (dateFinal > DateTime.Now)
                    {
                        var attributeActive = db.Database.SqlQuery<FormatedAttribute>(Scripts.SelectAttributes, lang).FirstOrDefault(f => f.Id.ToString() == ev.CityId);

                        if (listAttributesActive.Find(attr => attr.Id == attributeActive.Id) == null)
                            listAttributesActive.Add(attributeActive);
                    }
                }

                return listAttributesActive.ToList();
            }
        }
        public static dynamic TransferTicket(int idTicket, int idNewOwner, string nameNewOwner, int idCurrentOwner)
        {
            try
            {
                using (var db = new Database())
                {

                    var ownerNew = db.Database.SqlQuery<FormatedUsers>(Scripts.GetUserById, idNewOwner).FirstOrDefault();

                    if (ownerNew == null)
                        throw new Exception(Messages.UnknowUserTransfer);

                    var ticket = db.Ticket.FirstOrDefault(f => f.Id == idTicket && f.Status == Enumerators.TicketStatus.Emitted);

                    if (ticket == null)
                        throw new Exception(Messages.TransferNotAutorized);

                    if (ticket.IdUser != idCurrentOwner)
                        throw new Exception(Messages.TransferNotAutorized);

                    ticket.IdUser = idNewOwner;
                    ticket.OwnerName = nameNewOwner;
                    ticket.AlterationDate = DateTime.Now;

                    db.SaveChanges();

                    return ticket;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception TransferTicket" });
                throw;
            }
        }

        public static dynamic GetUsersByEmailAndName(string search)
        {
            try
            {
                using (var db = new Database())
                {
                    var dataUsers = db.Database.SqlQuery<FormatedUsers>(Scripts.GerUsersByEmailAndName, search).ToList();

                    return dataUsers;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetUsersByEmailOrName" });
                throw;
            }
        }

        public static dynamic ListEventsByCategory(int idParent)
        {
            try
            {
                using (var db = new Database())
                {
                    var dataCategories = db.Database.SqlQuery<FormattedCategory>(Scripts.RetriveCategoriesByIdParent, idParent).ToList();

                    if (dataCategories.Count() == 0)
                        throw new Exception(Messages.RequestCategoriesFail);

                    // Create a list of dynamic object as output
                    dynamic output = new List<dynamic>();

                    foreach (var category in dataCategories)
                    {
                        if (!category.Disabled)
                        {

                            var eventsByCategory = db.Database.SqlQuery<EventsByCategory>(Scripts.RetriveAllEventsByCategory, category.IdCategory).ToList();

                            dynamic row = new ExpandoObject();

                            row.CategoryId = category.IdCategory;
                            row.CategoryName = category.CategoryName;
                            row.CategoryUrl = category.CategoryUrl;
                            row.CategoryImage = category.ImageDefault;
                            row.CategoryDisabled = category.Disabled;
                            row.CategoryHighlighted = category.Highlighted;
                            row.Events = new List<dynamic>();

                            if (eventsByCategory.Count > 0)
                            {
                                foreach (var itemEvent in eventsByCategory.Where(f => !f.Disabled))
                                {
                                    var returnEvent = db.Database.SqlQuery<FormattedEvent>(Scripts.RetriveFormatedEvent, itemEvent.Id).FirstOrDefault();

                                    DateTime date = DateTime.Parse(returnEvent.EventFinalDate);
                                    if (date > DateTime.Now)
                                    {
                                        dynamic eventData = new ExpandoObject();

                                        eventData.Id = returnEvent.EventId;
                                        eventData.Name = returnEvent.EventName;
                                        eventData.ImageId = returnEvent.EventImageId;
                                        eventData.Disabled = itemEvent.Disabled;
                                        eventData.DataInicio = returnEvent.EventInicialDate;
                                        eventData.DataFinal = returnEvent.EventFinalDate;

                                        row.Events.Add(eventData);
                                    }
                                }
                            }

                            output.Add(row);
                        }
                    }

                    return output;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception ListEventsBestSellerByCategory" });
                throw;
            }
        }

        public static dynamic ListCategoryBestSellers(int idParent)
        {
            try
            {
                using (var db = new Database())
                {
                    var dataCategories = db.Database.SqlQuery<FormattedCategory>(Scripts.RetriveCategoriesByIdParent, idParent).ToList();

                    if (dataCategories.Count() == 0)
                        throw new Exception(Messages.RequestCategoriesFail);

                    // Create a list of dynamic object as output
                    dynamic output = new List<dynamic>();

                    foreach (var category in dataCategories)
                    {
                        if (!category.Disabled)
                        {
                            var bestSellerCategory = db.Database.SqlQuery<BestSellersCategory>(Scripts.RetriveBestSellersCategory, category.IdCategory).ToList();

                            if (bestSellerCategory != null)
                            {
                                var eventBestSeller = new List<FormattedEvent>();

                                foreach (var bestSellerItem in bestSellerCategory)
                                {
                                    var dataItem = db.Database.SqlQuery<FormattedEvent>(Scripts.RetriveFormatedEvent, bestSellerItem.Event).FirstOrDefault();

                                    DateTime dateItem = DateTime.Parse(dataItem.EventFinalDate);
                                    if (dateItem > DateTime.Now)
                                    {
                                        eventBestSeller.Add(dataItem);
                                    }
                                }

                                var eventSelected = eventBestSeller.FirstOrDefault();

                                dynamic row = new ExpandoObject();

                                row.HasSale = false;
                                row.CategoryId = category.IdCategory;
                                row.CategoryName = category.CategoryName;
                                row.CategoryUrl = category.CategoryUrl;
                                row.CategoryDisabled = category.Disabled;
                                row.CategoryHighlighted = category.Highlighted;
                                row.ImageDefault = category.ImageDefault;

                                if (eventSelected != null)
                                {
                                    DateTime date = DateTime.Parse(eventSelected.EventFinalDate);
                                    if (date > DateTime.Now)
                                    {
                                        row.HasSale = true;
                                        row.EventId = eventSelected.EventId;
                                        row.EventImageId = eventSelected.EventImageId;
                                        row.EventName = eventSelected.EventName;
                                        row.DataInicio = eventSelected.EventInicialDate;
                                        row.DataFinal = eventSelected.EventFinalDate;

                                        output.Add(row);
                                    }
                                }
                            }
                            else
                            {
                                dynamic row = new ExpandoObject();

                                row.HasSale = false;
                                row.CategoryId = category.IdCategory;
                                row.CategoryName = category.CategoryName;
                                row.CategoryUrl = category.CategoryUrl;
                                row.CategoryDisabled = category.Disabled;
                                row.CategoryHighlighted = category.Highlighted;
                                row.ImageDefault = category.ImageDefault;

                                output.Add(row);
                            }
                        }
                    }

                    return output;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception ListCategoryBestSellers" });
                throw;
            }
        }

        public static dynamic GetBestSellers()
        {
            try
            {
                using (var db = new Database())
                {
                    var listBestSellers = db.Database.SqlQuery<BestSellers>(Scripts.RetriveBestSellers).ToList();

                    ArrayList dataBestSellers = new ArrayList();
                    DateTime now = DateTime.Now;

                    if (listBestSellers.Count > 0)
                    {
                        for (var b = 0; b < listBestSellers.Count; b++)
                        {
                            var idEvent = listBestSellers[b].Event;

                            var item = db.Database.SqlQuery<FormatedEventBestSellers>(Scripts.RetriveFormatedEventBestSellers, idEvent).FirstOrDefault();

                            if (item.EventId != null)
                            {
                                item.EventQuantitySolded = listBestSellers[b].Solds;

                                DateTime dateItemFinal = DateTime.Parse(item.EventFinalDate);

                                if (dateItemFinal > now)
                                {
                                    dataBestSellers.Add(item);
                                }
                            }
                        }
                    }

                    return dataBestSellers;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetBestSellers" });
                throw;
            }
        }

        /// <summary>
        /// Retrives a section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public static FormattedSection FormattedSection(int sectionId)
        {
            try
            {
                using (var db = new Database())
                {
                    var section = db.Database.SqlQuery<FormattedSection>(Scripts.RetriveFormatedSection, sectionId).FirstOrDefault();
                    return section;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { sectionId });
                throw;
            }
        }

        public static void NotificateXDaysBefore(int daysBefore)
        {
            try
            {
                using (var db = new Database())
                {
                    var sectionsId = db.Database.SqlQuery<int>(Scripts.RetriveSectionsBeforeXDays, daysBefore).ToList();

                    foreach (var sectionId in sectionsId)
                    {
                        var section = FormattedSection(sectionId);
                        var eventId = db.Database.SqlQuery<int>(Scripts.RetriveEventIdFromSectionId, sectionId).FirstOrDefault();
                        var evento = FormattedEvent(eventId);
                        var userList = db.Database.SqlQuery<int>(Scripts.RetriveDistinctUserFromSection, sectionId).ToList();

                        foreach (var userId in userList)
                        {
                            var user = Plugin.CMS.User.Users().FirstOrDefault(u => u.Id == userId);
                            var sectionDate = DateTime.Parse(section.SectionDateTime.Replace("T", " "));

                            var sendDataApproved = new
                            {
                                NomeCliente = user.FirstName + " " + user.LastName,
                                Evento = evento.EventName,
                                EventoLocal = evento.EventLocal,
                                Sessao = section.SectionName,
                                SessaoData = sectionDate.ToString("dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture),
                                UrlMeusIngressos = $"{Plugin.CMS.Functions.BaseUrl()}{Plugin.CMS.Functions.Url("MeusIngressos")}",
                            };

                            var templateApproved = "XDiasAntes.cshtml";
                            var subjectApproved = "Seu evento se aproxima";

                            var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateApproved, sendDataApproved);
                            Plugin.CMS.Notification.SendNotification(contentApproved, subjectApproved, mailTo: new string[] { user.Email });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { daysBefore });
                throw;
            }
        }

        /// <summary>
        /// Retrives a event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public static FormattedEvent FormattedEvent(int eventId)
        {
            try
            {
                using (var db = new Database())
                {
                    var evento = db.Database.SqlQuery<FormattedEvent>(Scripts.RetriveFormatedEvent, eventId).FirstOrDefault();
                    return evento;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { eventId });
                throw;
            }
        }

        public static Dictionary<string, dynamic> GetEventsFromDashboard(int? IdEvent = null, bool Inativos = false, string inicio = null, string fim = null)
        {
            try
            {
                using (var db = new Database())
                {
                    dynamic events = null;

                    if (Inativos)
                    {
                        events = db.Database.SqlQuery<FormattedEventAttribute>(Scripts.GetAllEvents).OrderBy(e => e.EventName).ToList();
                    }
                    else
                    {
                        events = db.Database.SqlQuery<FormattedEventAttribute>(Scripts.GetAllEventsActive).OrderBy(e => e.EventName).ToList();
                    }

                    var ticketsEventSold = db.Database.SqlQuery<TicketsSold>(Scripts.RetrieveTicketsCount, inicio, fim, "Event", Enumerators.TicketStatus.Emitted).ToList();
                    var ticketsProductSold = db.Database.SqlQuery<TicketsSold>(Scripts.RetrieveTicketsCount, inicio, fim, "Product", Enumerators.TicketStatus.Emitted).ToList();
                    var ticketsSessionSold = db.Database.SqlQuery<TicketsSold>(Scripts.RetrieveTicketsCount, inicio, fim, "Event", Enumerators.TicketStatus.Emitted).ToList();
                    var ticketsBySituation = db.Database.SqlQuery<FormattedTicketsBySituation>(Scripts.RetrieveTicketsBySituation_New, inicio, fim).ToList();

                    if (IdEvent != null)
                    {
                        ticketsEventSold = ticketsEventSold.Where(e => e.IdEvent == IdEvent).ToList();
                        ticketsProductSold = ticketsProductSold.Where(e => e.IdEvent == IdEvent).ToList();
                        ticketsSessionSold = ticketsSessionSold.Where(e => e.IdEvent == IdEvent).ToList();
                        ticketsBySituation = ticketsBySituation.Where(e => e.IdEvent == IdEvent).ToList();
                    }

                    var ticketsEventFormatted = GenerateListFormattedTickets(ticketsEventSold, "Event");
                    var ticketsProductFormatted = GenerateListFormattedTickets(ticketsProductSold, "Product");
                    var ticketsSessionSoldFormatted = GenerateListFormattedSectionSolds(ticketsSessionSold, "Event");
                    var ticketsBySituationTotal = GenerateListFormattedTicektBySituation(ticketsBySituation);

                    var data = new Dictionary<string, dynamic>
                    {
                        { "dataInicio", inicio },
                        { "dataFim", fim },
                        { "events", events },
                        { "eventoSelecionado", IdEvent },
                        { "eventosInativos", Inativos },
                        { "ticketsEventFormatted", ticketsEventFormatted },
                        { "ticketsProductFormatted", ticketsProductFormatted },
                        { "ticketsSessionSoldFormatted",ticketsSessionSoldFormatted },
                        { "ticketsBySituationFormatted", ticketsBySituation },
                        { "ticketsBySituationTotal", ticketsBySituationTotal }
                    };

                    return data;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Err" });
                throw;
            }
        }

        private static dynamic GenerateListFormattedTicektBySituation(List<FormattedTicketsBySituation> tickets)
        {
            try
            {
                using (var db = new Database())
                {
                    ArrayList result = new ArrayList();

                    int totalCreated = 0;
                    int totalEmitted = 0;
                    int totalCancelled = 0;

                    foreach (var ticket in tickets)
                    {
                        totalCreated += ticket.Created;
                        totalEmitted += ticket.Emitted;
                        totalCancelled += ticket.Cancelled;
                    }

                    result.Add(new { totalCreated, totalEmitted, totalCancelled });

                    return result;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Err" });
                throw;
            }
        }

        private static dynamic GenerateListFormattedTickets(List<TicketsSold> tickets, string type)
        {
            try
            {
                using (var db = new Database())
                {
                    ArrayList result = new ArrayList();
                    ArrayList Items = new ArrayList();

                    var TotalSolds = 0;
                    decimal TotalPrice = 0;

                    foreach (var ticket in tickets)
                    {
                        var newTicket = new FormattedTicketsDashboard();

                        var id = type == "Event" ? ticket.IdEvent : ticket.IdSection;

                        var evento = db.Database.SqlQuery<FormattedEvent>(Scripts.RetriveFormatedEvent, id)?.FirstOrDefault();
                        if(evento.EventId != null)
                        {
                            var section = new FormattedSection();
                            if (type == "Event")
                            {
                                section = db.Database.SqlQuery<FormattedSection>(Scripts.RetriveFormatedSection, ticket.IdSection)?.FirstOrDefault();
                            }

                            newTicket.IdProduct = (int)evento.EventId;
                            newTicket.Solds = ticket.Solds;
                            newTicket.Name = evento.EventName;
                            newTicket.Type = type;
                            newTicket.Price = type == "Event" ? Convert.ToDecimal(section.SectionTicketValue) : evento.PriceProduct;
                            newTicket.RatePrice = type == "Event" ? Convert.ToDecimal(section.SectionTicketTax) : 0;

                            TotalSolds += ticket.Solds;
                            TotalPrice += ticket.Solds * (Convert.ToDecimal(newTicket.RatePrice) + Convert.ToDecimal(newTicket.Price));

                            Items.Add(newTicket);
                        }
                    }
                    result.Add(new { Items, TotalSolds, TotalPrice });

                    return result;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Err" });
                throw;
            }
        }

        private static dynamic GenerateListFormattedSectionSolds(List<TicketsSold> tickets, string type)
        {
            try
            {
                using (var db = new Database())
                {
                    ArrayList result = new ArrayList();
                    ArrayList Items = new ArrayList();

                    foreach (var ticket in tickets)
                    {
                        var newTicket = new FormattedTicketsDashboard();

                        var id = ticket.IdEvent;

                        var section = new FormattedSection();

                        section = db.Database.SqlQuery<FormattedSection>(Scripts.RetriveFormatedSection, ticket.IdSection)?.FirstOrDefault();

                        newTicket.IdSection = section.SectionId;
                        newTicket.Solds = ticket.Solds;
                        newTicket.Name = section.SectionName;
                        newTicket.Price = Convert.ToDecimal(section.SectionTicketValue);
                        newTicket.RatePrice = Convert.ToDecimal(section.SectionTicketTax);

                        Items.Add(newTicket);

                    }
                    result.Add(new { Items });

                    return result;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Err" });
                throw;
            }
        }
        /// <summary>
        /// Retrives an user extract, debits and credits
        /// </summary>
        /// <param name="IdUser"></param>
        /// <returns></returns>
        public static dynamic GetExtractByUser()
        {
            try
            {
                using (var db = new Database())
                {
                    var userAuth = Plugin.CMS.Membership.User;

                    var extract = db.Database.SqlQuery<Extract>(Scripts.GetExtractByUser, userAuth.Id).ToList();

                    return extract;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetExtractByUser" });
                throw;
            }
        }

        /// <summary>
        /// Retrives a user credits
        /// </summary>
        /// <param name="IdUser"></param>
        /// <returns></returns>
        public static dynamic GetCreditsByUser(int IdUser)
        {
            try
            {
                using (var db = new Database())
                {


                    var extract = db.Database.SqlQuery<Extract>(Scripts.GetExtractByUser, IdUser).ToList();

                    decimal totalValue = 0;

                    foreach (var item in extract)
                    {
                        if (item.OperationType == "C")
                            totalValue += item.CreditValue;
                        else
                            totalValue -= item.CreditValue;
                    }

                    return totalValue;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetExtractByUser" });
                throw;
            }
        }

        public static dynamic GetPromoterTicketValue(string email, int idEvent, int idTicket)
        {
            try
            {
                using (var db = new Database())
                {
                    var result = db.Database.SqlQuery<PromoterEvents>(Scripts.GetPromoterTicketValue, new object[] { email, idEvent, idTicket }).FirstOrDefault();

                    if(result == null)
                    {
                        return null;
                    }

                    dynamic ret = new
                    {
                        TicketQuantity = result.TicketQuantity.ToString("0.00"),
                        TicketValue = result.TicketValue.ToString("0.00"),
                        TicketsTax = result.TicketsTax.ToString("0.00")
                    };

                    return ret;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetExtractByUser" });
                throw;
            }
        }

        public static void RestoreOrderCredit(dynamic order, IEnumerable<dynamic> fields)
        {
            bool parsedCredit = decimal.TryParse(fields.FirstOrDefault(f => f.Field == "Credit")?.Value ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out decimal orderCredit);

            if (parsedCredit && orderCredit > 0)
            {
                Functions.AddCredit(new
                {
                    Order = new
                    {
                        order.IdCustomer
                    },
                    Total = orderCredit
                }, "Estorno de crédito");
            }
        }

        /// <summary>
        /// Method to add an credit extract to the current user
        /// </summary>
        /// <returns>Returns the order instance</returns>
        public static Extract AddCredit(dynamic dataextract, string detail = "Compra de crédito")
        {
            try
            {

                using (var db = new Database())
                {
                    /// Create the order instance binding the customer id if it's logged
                    var extract = new Extract();
                    extract.IdUser = dataextract.Order.IdCustomer;
                    extract.OperationType = "C";
                    extract.OperationDetail = detail;
                    extract.CreditValue = dataextract.Total;
                    extract.Disabled = false;
                    extract.CreatedAt = DateTime.Now;
                    extract.UpdatedAt = DateTime.Now;

                    // Add the new order in the database
                    db.Extract.Add(extract);

                    // Apply changes
                    db.SaveChanges();

                    return extract;
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to debit credit value from extract to the current user
        /// </summary>
        /// <returns>Returns the order instance</returns>
        public static Extract DebitCreditValue(int IdUser, string OperationDetail, decimal DebitValue)
        {
            try
            {

                using (var db = new Database())
                {
                    /// Create the order instance binding the customer id if it's logged
                    var extract = new Extract();
                    extract.IdUser = IdUser;
                    extract.OperationType = "D";
                    extract.OperationDetail = OperationDetail;
                    extract.CreditValue = DebitValue;
                    extract.Disabled = false;
                    extract.CreatedAt = DateTime.Now;
                    extract.UpdatedAt = DateTime.Now;

                    // Add the new order in the database
                    db.Extract.Add(extract);

                    // Apply changes
                    db.SaveChanges();

                    return extract;
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get product by id
        /// </summary>
        public static dynamic GetProduct(int IdEvent)
        {
            try
            {
                var product = Plugin.CMS.Plugins.Get("Bitzar.Products.dll");
                var parameters = new Dictionary<string, string>();
                parameters.Add("Lang", "1");
                parameters.Add("Id", IdEvent.ToString());

                var result = product.Plugin.Execute("getProduct", parameters: parameters);

                return result;                
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }       


        /// <summary>
        /// Method to transfer credit value from another user
        /// </summary>
        public static Extract TransferCreditValue(string emailNewOwner, decimal DebitValue)
        {
            try
            {
                using (var db = new Database())
                {
                    var userAuth = Plugin.CMS.Membership.User;
                    var userCredit = GetCreditsByUser(userAuth.Id);

                    if (DebitValue > userCredit)
                        throw new Exception(Messages.TransferNotAutorized);

                    var newOwner = db.Database.SqlQuery<FormatedUsers>(Scripts.GetUserByEmail, emailNewOwner).First();

                    var extract = new Extract();
                    extract.IdUser = userAuth.Id;
                    extract.OperationType = "D";
                    extract.OperationDetail = "Transferência para " + newOwner.CompleteName;
                    extract.CreditValue = DebitValue;
                    extract.Disabled = false;
                    extract.CreatedAt = DateTime.Now;
                    extract.UpdatedAt = DateTime.Now;

                    // Add the new extrac in the database
                    db.Extract.Add(extract);

                    // Apply changes
                    db.SaveChanges();

                    var extractNewOwner = new Extract();
                    extractNewOwner.IdUser = newOwner.Id;
                    extractNewOwner.OperationType = "C";
                    extractNewOwner.OperationDetail = "Transferência de " + userAuth.UserFields?.FirstOrDefault(x => x.Name == "Nome Completo")?.Value;
                    extractNewOwner.CreditValue = DebitValue;
                    extractNewOwner.Disabled = false;
                    extractNewOwner.CreatedAt = DateTime.Now;
                    extractNewOwner.UpdatedAt = DateTime.Now;

                    // Add the new extrac in the database
                    db.Extract.Add(extractNewOwner);

                    // Apply changes
                    db.SaveChanges();

                    return extract;
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        
        /// <summary>
        /// Send new ticket transfer email
        /// </summary>
        public static void SendTicketTransferMail(dynamic userAuth, dynamic product, string nomeDestinatario, string emailDestinatario, int quantidade)
        {
            try
            {
                var productFields = ((IEnumerable<dynamic>)product.Fields);                

                var sendDataInvite = new
                {
                    NomeOrigem = userAuth.FirstName+" "+userAuth.LastName,
                    NomeDestinatario = nomeDestinatario,
                    EmailDestinatario = emailDestinatario,
                    EventoNome = product.Description,
                    EventoData = Convert.ToDateTime(productFields?.FirstOrDefault(f => f.Name == "Data de inicio").Value),
                    EventoLocal = productFields?.FirstOrDefault(f => f.Name == "Nome local").Value,
                    EventoCidade = product.Attributes?[0].Attribute.Description,
                    EventoBanner = productFields?.FirstOrDefault(f => f.Name == "Mini banner").Formatted,
                    Quantidade = quantidade
                };

                var templateInvite = "TransferTicketBackoffice.cshtml";
                var subjectInvite = "Ingresso Emitido";

                var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateInvite, sendDataInvite);
                Plugin.CMS.Notification.SendNotification(contentApproved, subjectInvite, mailTo: new string[] { emailDestinatario });
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Send new promoter email
        /// </summary>
        public static void SendPromoterMail(dynamic userAuth, dynamic product, string promoterName, string promoterEmail, string promoterId, int? IdUser)
        {
            try
            {
                var productFields = ((IEnumerable<dynamic>)product.Fields);

                var sendDataInvite = new InviteData
                {
                    NomePromoter = promoterName,
                    EmailPromoter = promoterEmail,
                    IdPromoter = promoterId,
                    EventoNome = product.Description,
                    EventoData = Convert.ToDateTime(productFields?.FirstOrDefault(f => f.Name == "Data de inicio").Value),
                    EventoLocal = productFields?.FirstOrDefault(f => f.Name == "Nome local").Value,
                    EventoCidade = product.Attributes?[0].Attribute.Description,
                    EventoBanner = productFields?.FirstOrDefault(f => f.Name == "Mini banner").Formatted,
                    NomeOrganizador = userAuth.FirstName + " " + userAuth.LastName,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri
                };

                var templateInvite = "";

                if (IdUser == -1 )
                    templateInvite = "convite_promoter.cshtml";
                else
                    templateInvite = "convite_promoter_exists.cshtml";


                var subjectInvite = "Convite para promover evento";

                var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateInvite, sendDataInvite);
                Plugin.CMS.Notification.SendNotification(contentApproved, subjectInvite, mailTo: new string[] { promoterEmail });
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Send new promoter email
        /// </summary>
        public static void SendGeneratedLinkMail(dynamic product, string promoterName, string promoterEmail, string customerEmail, string link, string quantidade)
        {
            try
            {
                var productFields = ((IEnumerable<dynamic>)product.Fields);

                var sendDataInvite = new
                {
                    NomePromoter = promoterName,
                    EmailPromoter = promoterEmail,
                    EmailCustomer = customerEmail,
                    EventoNome = product.Description,
                    EventoData = Convert.ToDateTime(productFields?.FirstOrDefault(f => f.Name == "Data de inicio").Value),
                    EventoLocal = productFields?.FirstOrDefault(f => f.Name == "Nome local").Value,
                    EventoCidade = product.Attributes?[0].Attribute.Description,
                    EventoBanner = productFields?.FirstOrDefault(f => f.Name == "Mini banner").Formatted,
                    Link = link,
                    Quantidade = quantidade
                };

                var templateInvite = "link_promotor.cshtml";
                var subjectInvite = "Link de promoção do evento";

                var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateInvite, sendDataInvite);
                Plugin.CMS.Notification.SendNotification(contentApproved, subjectInvite, mailTo: new string[] { customerEmail });
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// create promoter invite
        /// </summary>
        public static string CreateInvite(string promoterId, int IdEvent, string promoterName, string promoterEmail, DateTime currentDate)
        {
            try
            {
                using (var db = new Database())
                {
                    var user = Plugin.CMS.User.Users().FirstOrDefault(x => x.Email == promoterEmail);

                    if(user == null)
                    {
                        db.Database.ExecuteSqlCommand(Scripts.InsertPromoterInvite, promoterId, IdEvent, promoterName, promoterEmail, currentDate, currentDate);
                        return "Ok";
                    }
                    else
                    {
                        var promoterInviteExists = db.Database.SqlQuery<PromoterInvite>(Scripts.ValidateIfPromoterInviteExists, user.Id, IdEvent).ToList();

                        if (promoterInviteExists.Count == 0)
                        {
                            db.Database.ExecuteSqlCommand(Scripts.InsertPromoterInvite, promoterId, IdEvent, promoterName, promoterEmail, currentDate, currentDate);
                            return "Ok";
                        }
                        else
                            throw new ArgumentException("Usuário já possui um convite.");

                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }        

        /// <summary>
        /// Update Status promoter invite
        /// </summary>
        public static void UpdateStatusPromoterInvite(int IdUser, string codPromoter)
        {
            try
            {               
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.UpdateStatusPromoterInvite, IdUser, codPromoter);
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Update user role
        /// </summary>

        public static void UpdateUserRolePromoter(int IdUser)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.UpdateUserRolePromoter, IdUser);
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// add promoter event
        /// </summary>

        public static void AddPromoterEvents(JArray tickets, string promoterId, int IdEvent)
        {
            try
            {
                foreach (var ticket in tickets)
                {                    
                    var idSessao = ticket["idSessao"].Value<int>();
                    var promoterTickets = ticket["promoterTickets"].Value<int>();
                    var ticketsValue = ticket["ticketValue"].Value<decimal>();
                    var chargeFee = ticket["chargeFee"].Value<bool>();
                    var ticketsTax = ticket["taxValue"].Value<decimal>();
                    var ticketVip = ticket["ticketVip"].Value<bool>();

                    using (var db = new Database())
                    {
                        db.Database.ExecuteSqlCommand(Scripts.InsertPromoterEvent, promoterId, IdEvent, idSessao, promoterTickets, ticketsValue, chargeFee, ticketsTax, ticketVip);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// list promoters by event
        /// </summary>

        public static List<PromoterInvite> ListPromotersInviteByEvent(int IdEvent)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<PromoterInvite>(Scripts.ListPromotersInviteByEvent, IdEvent).ToList();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get promoter events by iduser
        /// </summary>
        public static List<dynamic> GetPromoterEventsByIduser(int IdUser)
        {
            try
            {
                using (var db = new Database())
                {
                    var promoterInvites = db.Database.SqlQuery<PromoterInvite>(Scripts.GetPromoterInvitesByIdUser, IdUser).ToList();
                    List<dynamic> eventos = new List<dynamic>();

                    foreach (var evento in promoterInvites)
                    {                        
                        eventos.Add(GetProduct(evento.IdEvent));
                    }

                    return eventos;
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get promoter tickt by promoter code
        /// </summary>
        public static PromoterEvents GetPromoterTicketByPromoterCode(string PromoterCode, int IdTicket)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<PromoterEvents>(Scripts.GetPromoterTicketByPromoterCode, PromoterCode, IdTicket).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get promoter tickt by promoter code
        /// </summary>
        public static List<AdminPromoterEventsBalance> GetAdminTicketsBalanceByEvent(int IdEvent)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<AdminPromoterEventsBalance>(Scripts.GetAdminTicketsBalanceByEvent, IdEvent).ToList();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }        

        /// <summary>
        /// Method to get promoter tickt by promoter code
        /// </summary>
        public static PromoterEventsBalance GetPromoterTicketBalanceByPromoterCode(string PromoterCode, int IdTicket)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<PromoterEventsBalance>(Scripts.GetPromoterTicketBalanceByPromoterCode, PromoterCode, IdTicket).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to update promoter events
        /// </summary>
        public static string UpdatePromoterEvents(int IdPromoterEvent, int TicketQuantity, decimal TicketValue, bool ChargeFee, decimal TicketsTax, bool vip)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.UpdatePromoterEvents, IdPromoterEvent, TicketQuantity, TicketValue, ChargeFee, TicketsTax, vip);

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public static string CreateRemainingPromoterBalance(int IdEvent)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.CreateRemainingPromoterBalance, IdEvent);

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        // <summary>
        // method to create cart and add tickets
        // </summary>
        public static dynamic CreateCartAddTickets(int IdEvent, int QtdIngressos, int Product, decimal Price, int IdSection, string SectionDay, string RateSection, decimal PriceSection, string EventName, string TicketName, int IdUserAuth, string GeneratorEmail, string SiteTax, string ChargeFee)
        {
            try
            {
                // funcao para gerar o carrinho
                dynamic idCarrinho = null;

                NumberStyles style;
                CultureInfo provider;
                style = NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
                provider = new CultureInfo("pt-BR");

                var taxaDeServicoValidador = RateSection;
                var valorDoIngressoValidador = PriceSection.ToString();
                var idSecaoValidador = IdSection;
                var idEventoValidador = IdEvent;
                var nomeEvento = EventName;
                decimal valorDoIngressoMostrar = Decimal.Parse(valorDoIngressoValidador, style, provider);
                decimal taxaDeServicoMostrar = Decimal.Parse(taxaDeServicoValidador, style, provider);
                var numQuantity = 1.00;
                var dataHoraSecaoFormat2 = DateTime.Parse(SectionDay).ToString("dd/MM");
                var nomeDaSemanaFormat2 = DateTime.Parse(SectionDay).ToString("HH:mm");
                var nomeDaSecao = TicketName;

                var carrinhoDataSecao = dataHoraSecaoFormat2 + " - " + nomeDaSemanaFormat2 + " | " + nomeDaSecao;

                var valorFinal = (valorDoIngressoMostrar + taxaDeServicoMostrar).ToString();

                var validadorString = string.Join("#", new string[] { Price.ToString(), numQuantity.ToString(), idSecaoValidador.ToString(), idEventoValidador.ToString(), nomeEvento, carrinhoDataSecao, taxaDeServicoValidador, valorDoIngressoValidador });

                var Validator = Plugin.CMS.Security.Encrypt(validadorString);
                var ticks = DateTime.Now.TimeOfDay.TotalMilliseconds;
                ticks = ticks < 1000000 ? 1000000 + ticks : ticks;

                List<dynamic> idTickets = new List<dynamic>();

                var ecommercePlugin = Plugin.CMS.Plugins.Get("Bitzar.Ecommerce.dll");                

                for (int i = 0; i < QtdIngressos; i++)
                {
                    var parameters = new Dictionary<string, string>();
                    var IdTicket = Guid.NewGuid().ToString();
                    var product = Convert.ToInt32(ticks + i).ToString();
                    
                    parameters.Add("Cart", idCarrinho);
                    parameters.Add("Product", product);
                    parameters.Add("Quantity", "1");
                    parameters.Add("Price", Price.ToString().Replace(".", "").Replace(",", "."));
                    parameters.Add("Validator", Validator);
                    parameters.Add("App", "false");
                    parameters.Add("Item.Type", "Event");
                    parameters.Add("Item.IdSection", IdSection.ToString());
                    parameters.Add("Item.IdEvent", IdEvent.ToString());
                    parameters.Add("Item.IdTicket", IdTicket);
                    parameters.Add("Item.SectionDay", carrinhoDataSecao);
                    parameters.Add("Item.RateSection", RateSection);
                    parameters.Add("Item.PriceSection", PriceSection.ToString());
                    parameters.Add("Item.EventName", EventName);
                    parameters.Add("Item.GeneratorEmail", GeneratorEmail);
                    parameters.Add("Item.OrigemIngresso", "Gerado");
                    parameters.Add("Item.SiteTax", SiteTax);
                    parameters.Add("Item.ChargeFee", ChargeFee);

                    var result = ecommercePlugin?.Plugin?.Execute("AddOrUpdateCartItem", Plugin.CMS.Security.RequestToken, parameters: parameters);

                    idCarrinho = result.Uuid;

                    idTickets.Add(IdTicket);
                }

                var parametersSetCustomer = new Dictionary<string, string>();

                parametersSetCustomer.Add("Cart", idCarrinho);
                parametersSetCustomer.Add("UserId", IdUserAuth.ToString());

                var resultSetCustomer = ecommercePlugin?.Plugin?.Execute("SetCustomer", Plugin.CMS.Security.RequestToken, parameters: parametersSetCustomer);

                dynamic tickets = new System.Dynamic.ExpandoObject();

                tickets.idOrder = idCarrinho;
                tickets.idTickets = idTickets;

                return tickets;

            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to approve order
        /// </summary>
        public static void ApproveOrder(string UuidOrder)
        {
            try
            {
                var callback = new Callback
                {
                    evento = "Succeeded", // action do retorno do postman (Succeeded, Create, Failed, Canceled)
                    referenceId = UuidOrder, // IdOrder
                };

                if (!String.IsNullOrEmpty(callback.evento))
                {
                    Plugin.CMS.Events.Trigger($"OnPayment{callback.evento.ToString()}", callback);
                }               
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Endpoint responsible for upload product images to azure storage
        /// </summary>
        /// <returns></returns>
        public static List<string> UploadProductImagesToAzureStorage(JArray arquivos, string repositorio)
        {
            try
            {
                var urls = new List<string>();

                foreach (var item in arquivos)
                {
                    var content = Convert.FromBase64String(item.Value<string>("Base64"));

                    var name = $"{repositorio}/{item.Value<string>("Nome")}";
                    var url = Plugin.CMS.Library.UploadToAzureStorage(content, name);

                    urls.Add(url);
                }

                return urls;
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        
        /// <summary>
        /// Update de product status to paused
        /// </summary>
        /// <returns></returns>
        public static string PauseProductCardapio(string IdsProducts)
        {
            try
            {
                using (var db = new Database())
                {
                    var arrayIds = IdsProducts.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var IdProduct in arrayIds)
                    {
                        db.Database.ExecuteSqlCommand(Scripts.UpdateProductStatusToPaused, IdProduct);
                    }

                    Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Plugins.Get_Bitzar.Products");
                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Update de product status to disable
        /// </summary>
        /// <returns></returns>
        public static string DisableProductCardapio(string IdsProducts)
        {
            try
            {
                using (var db = new Database())
                {
                    var arrayIds = IdsProducts.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var IdProduct in arrayIds)
                    {
                        db.Database.ExecuteSqlCommand(Scripts.UpdateProductStatusToDisabled, IdProduct);
                    }

                    Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Plugins.Get_Bitzar.Products");
                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Update de product status to active
        /// </summary>
        /// <returns></returns>
        public static string ActivateProductCardapio(string IdsProducts)
        {
            try
            {
                using (var db = new Database())
                {
                    var arrayIds = IdsProducts.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var IdProduct in arrayIds)
                    {
                        db.Database.ExecuteSqlCommand(Scripts.UpdateProductStatusToActive, IdProduct);
                    }

                    Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.Plugins.Get_Bitzar.Products");
                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public static string UpdateEventDates(int idEvent, int idSession, DateTime startDate, DateTime startSellingDate, DateTime endSellingDate)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.UpdateSessionDates, idEvent, idSession, startDate.ToString("yyyy-MM-ddTHH:mm"), startSellingDate.ToString("yyyy-MM-ddTHH:mm"), endSellingDate.ToString("yyyy-MM-ddTHH:mm"));

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get establishment users formatted
        /// </summary>
        public static List<dynamic> FormatarUsers(List<User> users)
        {
            try
            { 
            
                var arrayUsers = new List<dynamic>();


                foreach (var user in users)
                {

                    var userFormatted = new Dictionary<string, dynamic>();

                    userFormatted.Add("Id", user.Id);
                    userFormatted.Add("FirstName", user.FirstName);
                    userFormatted.Add("LastName", user.LastName);
                    userFormatted.Add("ProfilePicture", user.ProfilePicture);
                    userFormatted.Add("Email", user.Email);
                    userFormatted.Add("UserFields", user.UserFields);

                    arrayUsers.Add(userFormatted);
                }

                return arrayUsers;

            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public static dynamic GetNextProductBatch(string sku, int currentBatch, List<dynamic> subProducts)
        {
            dynamic result = null;
            int? minBatch = null;

            subProducts.Where(s => s.SKU == sku).ToList().ForEach(s =>
            {
                string batchStr = ((IEnumerable<dynamic>)s.Fields).FirstOrDefault(f => f.Name == "Sequência do lote")?.Value;
                int.TryParse(batchStr, out int batch);

                if (batch > currentBatch && (batch < minBatch || minBatch == null))
                {
                    result = s;
                    minBatch = batch;
                }
            });

            return result;
        }

        /// <summary>
        /// Method to get sold products in events
        /// </summary>
        public static List<EventProduct> GetEventProductsSold(int idEvent)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<EventProduct>(Scripts.GetEventProductsSold, idEvent).ToList();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to get Establishment Orders
        /// </summary>
        public static List<EstablishmentOrder> GetEstablishmentOrders(int IdEstablishment, string StartDate, string EndDate)
        {
            try
            {
                using (var db = new Database())
                {
                    return db.Database.SqlQuery<EstablishmentOrder>(Scripts.GetEstablishmentOrders, IdEstablishment, StartDate, EndDate).ToList();
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }        

        /// <summary>
        /// Method to alter Establishment situation
        /// </summary>
        public static string AlterEstablishmentStatus(string EstablishmentStatus, int IdEstablishment)
        {
            try
            {
                var status = EstablishmentStatus.ToLower();

                using (var db = new Database())
                {
                    if (status == "aberto")                    
                        db.Database.ExecuteSqlCommand(Scripts.AlterEstablishmentStatus, "Fechado", IdEstablishment);                                            
                    else                    
                        db.Database.ExecuteSqlCommand(Scripts.AlterEstablishmentStatus, "Aberto", IdEstablishment);                    

                    Plugin.CMS.ClearCache("Bitzar.CMS.Core.Functions.Internal.User");
                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Cancel Establishment order
        /// </summary>
        /// <returns></returns>
        public static string CancelEstablishmentOrder(int IdOrder, string CancelEstablishmentOrder)
        {
            try
            {
                using (var db = new Database())
                {       
                    db.Database.ExecuteSqlCommand(Scripts.CancelEstablishmentOrder, IdOrder, CancelEstablishmentOrder);
                 
                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");

                    return "Ok";
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Get Next Events
        /// </summary>
        /// <returns></returns>
        public static List<EventDetail> GetNextEvents()
        {
            try
            {
                using (var db = new Database())
                {
                    var listNextEvents = db.Database.SqlQuery<EventDetail>(Scripts.GetNextEvents).ToList();

                    foreach (var @event in listNextEvents)
                    {
                        @event.EventImageUrl = Plugin.CMS.Library.Object(@event.EventImageId)?.FullPath;
                        @event.Tickets = db.Database.SqlQuery<EventTickets>(Scripts.GetEventTickets, @event.EventId).ToList();
                    }

                    return listNextEvents;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetNextEvents" });
                throw;
            }
        }

        /// <summary>
        /// Get All Events Active
        /// </summary>
        /// <returns></returns>
        public static List<EventDetail> GetAllEventsActive()
        {
            try
            {
                using (var db = new Database())
                {
                    var listAllEventsActive = db.Database.SqlQuery<EventDetail>(Scripts.GetAllActiveEvents).ToList();

                    foreach (var @event in listAllEventsActive) {
                        @event.EventImageUrl = Plugin.CMS.Library.Object(@event.EventImageId)?.FullPath;
                        @event.Tickets = db.Database.SqlQuery<EventTickets>(Scripts.GetEventTickets, @event.EventId).ToList();
                    }

                    return listAllEventsActive;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetAllEventsActive" });
                throw;
            }
        }

        /// <summary>
        /// Get Best Sellers Events Active
        /// </summary>
        /// <returns></returns>
        public static List<BestSellerEvent> GetBestSellersEvents()
        {
            try
            {
                using (var db = new Database())
                {
                    var listBestSellersEvents = db.Database.SqlQuery<BestSellerEvent>(Scripts.GetBestSellersEvents).ToList();

                    foreach (var @event in listBestSellersEvents)
                    {
                        @event.EventImageUrl = Plugin.CMS.Library.Object(@event.EventImageId)?.FullPath;
                        @event.Tickets = db.Database.SqlQuery<EventTickets>(Scripts.GetEventTickets, @event.EventId).ToList();
                    }

                    return listBestSellersEvents;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception GetBestSellersEvents" });
                throw;
            }
        }

        /// <summary>
        /// Método para atualizar todos os ingressos da sessão para inativo e ativar somente o igresso selecionado
        /// </summary>
        /// <param name="IdProduct"></param>
        /// <param name="IdTicket"></param>
        /// <returns></returns>
        public static void ActivateLotTicket(string sku, int IdProduct, int? IdTicket)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Database.ExecuteSqlCommand(Scripts.ActivateLotTicket, sku, IdProduct, IdTicket);

                    Plugin.CMS.ClearCache("Bitzar.Products.Helpers.Functions");
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { "Exception ActivateLotTicket" });
                throw;
            }
        }
    }
}