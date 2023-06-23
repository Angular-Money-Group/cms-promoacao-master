using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using Bitzar.Tickets.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Linq;
using static Bitzar.Tickets.Helpers.Enumerators;
using Bitzar.Tickets.Models;

namespace Bitzar.Tickets
{
    public class Plugin : IPlugin
    {
        /// <summary>
        /// Internal static reference to the ICMS object
        /// </summary>
        public static ICMS CMS { get; set; }

        /// <summary>
        /// Internal method to get plugin's name
        /// </summary>
        internal static string PluginName => "Bitzar.Tickets.dll";

        /// <summary>
        /// Menu interface to hold menu information
        /// </summary>
        public IList<IMenu> Menus { get; set; } = new List<IMenu>();

        static CultureInfo ConverterCulture = new CultureInfo("en-US");

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
                Models.Ticket ticket;
                int idTicket;
                int idSection;
                int idEvent;
                Models.Passenger passenger;
                int userId;

                switch (function.ToUpper())
                {
                    case Configurations.GetPassenger:

                        if (!parameters.ContainsKey("ticketUuid") || string.IsNullOrEmpty(parameters["ticketUuid"]))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        if (!parameters.ContainsKey("PassengerUuid") || string.IsNullOrEmpty(parameters["PassengerUuid"]))
                            throw new ArgumentException("Id do passageiro não encontrado");

                        ticket = Functions.GetTicketByUuid(parameters["ticketUuid"]);
                        if (ticket == null)
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.GetPassenger(ticket.Id, parameters["PassengerUuid"]);

                        break;
                    case Configurations.GetListPassengers:

                        if (!parameters.ContainsKey("UuidTicket") || string.IsNullOrEmpty(parameters["UuidTicket"]))
                            throw new ArgumentException(Messages.TicketInvalidId);


                        ticket = Functions.GetTicketByUuid(parameters["UuidTicket"]);
                        if (ticket == null)
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.GetListPassenger(ticket.Id);

                        break;
                    case Configurations.SavePassenger:

                        if (!parameters.ContainsKey("UuidTicket") || string.IsNullOrEmpty(parameters["UuidTicket"]))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        ticket = Functions.GetTicketById(int.Parse(parameters["UuidTicket"]));

                        if (ticket == null)
                            throw new ArgumentException(Messages.TicketInvalidId);

                        if (ticket.IdUser != CMS.Membership.User.Id)
                            throw new ArgumentException("Usuário não corresponde ao comprador dos tickets");

                        var p = Functions.GetPassenger(ticket.Id, parameters["PassengerUuid"]);

                        passenger = new Passenger
                        {
                            Name = parameters["firstName"],
                            LastName = parameters["lastName"],
                            Nationality = parameters["nation"],
                            PersonalId = parameters["idCPF"],
                            BirthDate = DateTime.Parse(parameters["date1"]),
                            Gender = parameters["gender"],
                            ZipCode = parameters["zipCode"],
                            CityBorn = parameters["bornCity"],
                            Address = parameters["address"],
                            Country = parameters["country"],
                            State = parameters["state"],
                            City = parameters["city"],
                            Telephone = parameters["telephone"],
                            AdditionalTelephone = parameters["additionalTelephone"],
                            Email = parameters["email"],
                            EmergencyName = parameters["emergencyName"],
                            EmergencyNumber = parameters["emergencyNumber"],
                            TicketId = ticket.Id
                        };

                        if (p != null)
                        {
                            passenger.Id = p.Id;
                            passenger.Uuid = p.Uuid;
                        }

                        result = Functions.AddPassenger(passenger);

                        var sendDataApproved = new
                        {
                            NomeCliente = CMS.Membership.User.FirstName,
                            NomePassageiro = passenger.Name,
                            TicketPassageiro = ticket.Uuid,
                            EmailPassageiro = passenger.Email,
                            TelefonePassageiro = passenger.Telephone

                        };

                        var templateApproved = "passageiro-cadastrado.cshtml";
                        var subjectApproved = "Passageiro cadastrado!";
                        var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateApproved, sendDataApproved);
                        Plugin.CMS.Notification.SendNotification(contentApproved, subjectApproved, mailTo: new string[] { CMS.Membership.User.Email });

                        break;
                    case Configurations.AddTicket:
                        if (!parameters.ContainsKey("Nome") || string.IsNullOrEmpty(parameters["Nome"]))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        if (!parameters.ContainsKey("IdSection") || !int.TryParse(parameters["IdSection"], out idSection))
                            throw new ArgumentException(Messages.NewTicketSectionInvalid);

                        var ownerName = parameters["Nome"];

                        ticket = new Ticket
                        {
                            OwnerName = ownerName,
                            IdSection = idSection
                        };

                        result = Functions.AddTicket(ticket);

                        break;
                    case Configurations.DeleteTicket:
                        if (!parameters.ContainsKey("Id") || !int.TryParse(parameters["Id"], out idTicket))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.DeleteTicket(idTicket);
                        break;
                    case Configurations.GetTicket:
                        if (!parameters.ContainsKey("Id") || !int.TryParse(parameters["Id"], out idTicket))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.GetTicket(idTicket);
                        break;
                    case Configurations.GetTicketByUuid:
                        if (!parameters.TryGetValue("UuidTicket", out string UuidTicket) || string.IsNullOrWhiteSpace(UuidTicket))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.GetTicketByUuid(UuidTicket);
                        break;
                    case Configurations.ListTickets:
                        result = Functions.ListTickets();
                        break;
                    case Configurations.LoadTicketsBySection:
                        {
                            EnsureUserIsAdministrator();

                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            if (!parameters.ContainsKey("IdSection") || !int.TryParse(parameters["IdSection"], out int IdSection))
                                throw new ArgumentException(Messages.ParametersNotFound);

                            TicketStatus? status = null;
                            if (parameters.ContainsKey("Status") && Enum.TryParse(parameters["Status"], out TicketStatus statusRef))
                                status = statusRef;

                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            result = Functions.LoadTicketsBySection(IdSection, page, size, status, search);
                            break;
                        }
                    case Configurations.LoadTicketsByUser:
                        {
                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            userId = parameters.ContainsKey("UserId") ? Convert.ToInt32(parameters["UserId"]) : CMS.Membership.User.Id;

                            TicketStatus? status = null;
                            if (parameters.ContainsKey("Status") && Enum.TryParse(parameters["Status"], out TicketStatus statusRef))
                                status = statusRef;

                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            result = Functions.LoadTicketsByUserPaginated(userId, page, size, status, search);
                            break;
                        }
                    case Configurations.LoadTicketsByUserWithoutPag:
                        {
                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            userId = parameters.ContainsKey("UserId") ? Convert.ToInt32(parameters["UserId"]) : CMS.Membership.User.Id;

                            TicketStatus? status = null;
                            if (parameters.ContainsKey("Status") && Enum.TryParse(parameters["Status"], out TicketStatus statusRef))
                                status = statusRef;

                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            result = Functions.LoadTicketsByUser(Convert.ToInt32(userId));
                            break;
                        }
                    case Configurations.SetTicketStatus:
                        {
                            if (!parameters.ContainsKey("Ticket") || string.IsNullOrEmpty(parameters["Ticket"]))
                                throw new ArgumentException(Messages.TicketInvalidId);

                            if (!parameters.ContainsKey("Status") || !Enum.TryParse(parameters["Status"], out Enumerators.TicketStatus status))
                                throw new ArgumentException(Messages.ParametersNotFound);

                            string guidTicket = parameters["Ticket"];
                            result = Functions.SetTicketStatus(guidTicket, status);
                            break;
                        }
                    case Configurations.CancelTicket:
                        if (!parameters.ContainsKey("Id") || !int.TryParse(parameters["Id"], out idTicket))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.CancelTicket(idTicket);
                        break;
                    case Configurations.CancelTicketByOrder:
                        if (!parameters.ContainsKey("IdOrder"))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.CancelTicketByOrder(parameters["IdOrder"]);
                        break;
                    case Configurations.EmitTicketsByOrder:
                        if (!parameters.ContainsKey("IdOrder"))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        result = Functions.EmitTicketByOrder(parameters["IdOrder"]);
                        SendEmailPassengers(parameters["IdOrder"]);
                        break;
                    case Configurations.InventoryTotal:
                        if (!parameters.ContainsKey("IdSection") || !int.TryParse(parameters["IdSection"], out idSection))
                            throw new ArgumentException(Messages.SectionInvalidId);

                        if (!parameters.ContainsKey("Type") || string.IsNullOrEmpty(parameters["Type"]))
                            throw new ArgumentException(Messages.SectionInvalidId);

                        string type = parameters["Type"];

                        result = Functions.InventoryTotal(idSection, type);
                        break;
                    case Configurations.ConsumeTicket:
                        if (!parameters.ContainsKey("QrGuidEncoded") || string.IsNullOrEmpty(parameters["QrGuidEncoded"]))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        if (!parameters.ContainsKey("Type") || string.IsNullOrEmpty(parameters["Type"]))
                            throw new ArgumentException(Messages.TicketInvalidId);

                        if (!parameters.ContainsKey("IdEvent") || !int.TryParse(parameters["IdEvent"], out idEvent))
                            throw new ArgumentException(Messages.SectionInvalidId);

                        string qrGuidEncoded = parameters["QrGuidEncoded"];
                        string qrType = parameters["Type"];

                        result = Functions.ConsumeTicketByQrGuidEncoded(qrGuidEncoded, idEvent, qrType);
                        break;
                    default:
                        throw new Exception(Messages.CommandNotFound);
                }

                CMS.Log.LogRequest(result, "Trace", PluginName, parameters, (CMS.Membership.User ?? CMS.Membership.AdminUser));

                return result;
            }
            catch (Exception e)
            {
                Functions.LogException(e, objects: new object[] { parameters, (CMS.Membership.User ?? CMS.Membership.AdminUser) });
                throw e;
            }
        }

        /// <summary>
        /// Method to validate if the user is Administrator
        /// </summary>
        private void EnsureUserIsAdministrator()
        {
#if !DEBUG
            if (!CMS.Membership.IsAdminAuthenticated)
                throw new UnauthorizedAccessException(Messages.NotAuthorizedOperation);
#endif
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
            // Check plugin dependency
            // TODO: Only Allow plugin upload if there is any kind of 

            // Store object context
            CMS = cms;

            // Create the menu to show in the system
            this.Menus.Add(new Menu()
            {
                Name = "INGRESSOS VENDIDOS",
                Items = new List<IMenuItem>
                {
                    new MenuItem() { Title = "Ingressos", Function = "Tickets", Icon = "wb-table" }
                }
            });

            // Create database tables
            Configurations.Setup();

            // Return a new instance of this class
            return this;
        }

        /// <summary>
        /// Method to uninstall the plugin
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            // Execute database Uninstall Command
            Configurations.Uninstall();
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
                case "OnOrderCartUpdateItem":
                    var orderAddOrUpdate = (dynamic)data;

                    foreach (var item in (IEnumerable<dynamic>)orderAddOrUpdate.Items)
                    {

                        var itemFields = (IEnumerable<dynamic>)item.Fields;

                        var typeTicket = itemFields.FirstOrDefault(f => f.Field == "Type")?.Value;

                        if (typeTicket == "Credit" || typeTicket == "Estabelecimento" || typeTicket == "Coupon")
                            return;

                        var ticketUuidUpdate = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "IdTicket")?.Value) ? itemFields.FirstOrDefault(f => f.Field == "IdTicket")?.Value : null;

                        var ticketUpdate = Functions.GetTicketByUuid(ticketUuidUpdate);


                        if (ticketUpdate != null)
                        {
                            ticketUpdate.OwnerName = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "OwnerName")?.Value) ? itemFields.FirstOrDefault(f => f.Field == "OwnerName")?.Value : "";
                            ticketUpdate.IdSection = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "IdSection")?.Value) ? Convert.ToInt32(itemFields.FirstOrDefault(f => f.Field == "IdSection")?.Value) : 0;
                            ticketUpdate.IdEvent = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "IdEvent")?.Value) ? Convert.ToInt32(itemFields.FirstOrDefault(f => f.Field == "IdEvent")?.Value) : 0;
                            ticketUpdate.uuidOrder = orderAddOrUpdate.Uuid;
                            ticketUpdate.QRguid = ticketUpdate.QRguid;
                            Functions.UpdateTicket(ticketUpdate);
                        }
                        else
                        {
                            Models.Ticket newTicket = new Models.Ticket();
                            newTicket.Uuid = ticketUuidUpdate;
                            newTicket.OwnerName = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "OwnerName")?.Value) ? itemFields.FirstOrDefault(f => f.Field == "OwnerName")?.Value : "";
                            newTicket.IdSection = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "IdSection")?.Value) ? Convert.ToInt32(itemFields.FirstOrDefault(f => f.Field == "IdSection")?.Value) : 0;
                            newTicket.IdEvent = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "IdEvent")?.Value) ? Convert.ToInt32(itemFields.FirstOrDefault(f => f.Field == "IdEvent")?.Value) : 0;
                            newTicket.Type = !string.IsNullOrWhiteSpace(itemFields.FirstOrDefault(f => f.Field == "Type")?.Value) ? itemFields.FirstOrDefault(f => f.Field == "Type").Value : "";
                            newTicket.uuidOrder = orderAddOrUpdate.Uuid;
                            newTicket.Status = TicketStatus.Created;
                            Functions.AddTicket(newTicket);
                        }

                    }
                    break;
                case "OnOrderCartDeleteItem":
                    var itemADeletar = (dynamic)data;

                    var itemFieldsDelete = (IEnumerable<dynamic>)itemADeletar.Fields;
                    var ticketUuid = !string.IsNullOrWhiteSpace(itemFieldsDelete.FirstOrDefault(f => f.Field == "IdTicket")?.Value) ? itemFieldsDelete.FirstOrDefault(f => f.Field == "IdTicket")?.Value : null;

                    var ticket = Functions.GetTicketByUuid(ticketUuid);

                    if (ticket != null)
                    {
                        Functions.DeleteTicket(ticket.Id);
                    }
                    break;
                case "OnOrderSetCustomer":
                    Dictionary<string, object> values = ((object)data)
                                     .GetType()
                                     .GetProperties()
                                     .ToDictionary(p => p.Name, p => p.GetValue(data));

                    var userSetCustomer = values.FirstOrDefault(c => c.Key == "User").Value;
                    var ouderSetCustomer = values.FirstOrDefault(c => c.Key == "Order").Value;
                    Functions.SetOrderTicketsToUser(((dynamic)ouderSetCustomer), ((dynamic)userSetCustomer).Id);
                    break;
                case "OnOrderAbandoned":
                    var orderAbandoned = (dynamic)data;
                    Functions.CancelTicketByOrder(orderAbandoned.Uuid);
                    break;
                case "OnPaymentSucceeded":
                    var callbackSucceeded = (dynamic)data;
                    string referenceIdSucceeded = callbackSucceeded.referenceId;
                    Functions.EmitTicketByOrder(referenceIdSucceeded);
                    SendEmailPassengers(referenceIdSucceeded);
                    break;
                case "OnPaymentSucceededPlatformCredit":
                    var orderId = (dynamic)data;
                    Functions.EmitTicketByOrder(orderId);
                    SendEmailPassengers(orderId);
                    break;
                case "OnPaymentCanceled":
                    var callbackCanceled = (dynamic)data;
                    string referenceIdCanceled = callbackCanceled.referenceId;
                    Functions.CancelTicketByOrder(referenceIdCanceled);
                    break;
                case "emailPassengerUnregistered":
                    var ticketsNotRegister = Functions.CheckPassengerPerWeek();

                    foreach (var item in ticketsNotRegister)
                    {
                        if (Functions.consultLastEmailSend(item.Id, item.DataConsumo, item.CreationDate))
                        {
                            var users = CMS.Membership.Members().Where(u => u.Id == item.IdUser).ToList();
                            var usersName = users.Select(x => x.FirstName).FirstOrDefault();
                            var usersEmail = users.Select(x => x.Email).FirstOrDefault();
                            var sDApproved = new
                            {
                                NomeCliente = usersName,
                                Ticket = item.Uuid
                            };

                            var tApproved = "registrar-passageiros.cshtml";
                            var sApproved = "É necessario o cadastro dos passageiros!";
                            var cApproved = Plugin.CMS.Notification.LoadTemplate(tApproved, sDApproved);
                            Plugin.CMS.Notification.SendNotification(cApproved, sApproved, mailTo: new string[] { usersEmail });

                            Functions.resgisterEmailSend(item.Id, usersEmail);
                        }
                    }

                    break;
            }
            return;
        }

        private static void SendEmailPassengers(string uuidOrder)
        {
            var ticket = Functions.GetTicketByOrder(uuidOrder);
            var customerId = Functions.GetUserByOrderUuid(uuidOrder);

            var users = CMS.Membership.Members().Where(u => u.Id == customerId).ToList();
            var usersName = users.Select(x => x.FirstName).FirstOrDefault();
            var usersEmail = users.Select(x => x.Email).FirstOrDefault();

            var sendDataApproved = new
            {
                NomeCliente = usersName,
                Ticket = ticket.Uuid
            };

            var templateApproved = "registrar-passageiros.cshtml";
            var subjectApproved = "Compra realizada com sucesso!";
            var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateApproved, sendDataApproved);
            Plugin.CMS.Notification.SendNotification(contentApproved, subjectApproved, mailTo: new string[] { usersEmail });
        }
    }
}
