using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using Bitzar.Payments.Helpers;
using Bitzar.Payments.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using static Bitzar.Payments.Models.Transaction;

namespace Bitzar.Payments
{
    public class Plugin : IPlugin
    {
        /// <summary>
        /// Internal static reference to the ICMS object
        /// </summary>
        internal static ICMS CMS { get; set; }

        /// <summary>
        /// Internal method to get plugin's name
        /// </summary>
        internal static string PluginName => "Bitzar.Payments.dll";

        /// <summary>
        /// Cria os menus do plugin na área admin
        /// </summary>
        public IList<IMenu> Menus { get; set; } = new List<IMenu>();

        /// <summary>
        /// Método que executa todas as chamadas e ações para o plugin
        /// </summary>
        /// <param name="function"></param>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public dynamic Execute(string function, string token = null, Dictionary<string, string> parameters = null, HttpFileCollectionBase files = null)
        {

            dynamic result = null;
            User user = null;
            CreditCard mask = null;
            PayerIdentifier payer = null;

            try
            {
                //Checa se uma requisição está sendo feita de dentro do CMS
                //Valicao para request vindo da API (API ja envia o request token)
                if (token != "bb835fcc-8caa-4f3c-bfda-828501d0ac24")
                    if (token != CMS.Configuration.Token && !CMS.Security.ValidateToken(token))
                        throw new Exception("Token Expirado!");

                switch (function.ToUpper())
                {
                    case Constants.Payment:
                        {
                            if (!parameters.ContainsKey("amount") || !int.TryParse(parameters["amount"], out int amount))
                                throw new Exception("Valor informado para cobrança não é reconhecido ou não informado. O formato deve ser numérico sem decimais. Ex: R$ 521,36 => 52136.");

                            if (!parameters.ContainsKey("installments") || !int.TryParse(parameters["installments"], out int installments) || (installments < 1 || installments > 12))
                                installments = 1;

                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(CMS.Configuration.Get(Configurations.DefaultOriginTransaction, PluginName), true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            if (!parameters.ContainsKey("type") || !Enum.TryParse(parameters["type"], true, out OperationType type))
                                type = OperationType.Credit;

                            parameters.TryGetValue("splits", out string splits);

                            parameters.TryGetValue("customMainVendor", out string customMainVendor);

                            user = CMS.Membership.User;

                            if (user != null || parameters.ContainsKey("userId"))
                            {
                                var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;
                                payer = Functions.GetPayerIdentifier(userId, gateway);
                            }

                            if (CMS.Configuration.Get(Configurations.GenerateDebugTransaction, PluginName).Contains("true"))
                            {
                                var value = $"05{(new Random()).Next(01, 45)}";
                                amount = int.Parse(parameters["amount"]);
                                parameters.Add("old_amount", amount.ToString());
                                amount = int.Parse(value);
                                parameters.Add("debug_activated", "true");
                            }

                            var transaction = new Transaction
                            {
                                Amount = amount,
                                Reference = parameters["reference"],
                                SoftDescriptor = parameters["softdescriptor"],
                                Installments = installments,
                                Origin = gateway,
                                Kind = type
                            };

                            #region Person

                            if (!parameters.ContainsKey("cpf") || string.IsNullOrEmpty(parameters["cpf"]) || !Extensions.IsValidDocument(parameters["cpf"]))
                                throw new Exception("Valor informado para CPF não é reconhecido ou não informado. O formato deve ser numérico sem caracteres especiais. Ex: 455.956.037-47 => 45595603747.");

                            Person person = new Person
                            {
                                firstName = parameters["firstName"],
                                lastName = parameters["lastName"],
                                cpf = Extensions.RemoveSpecialCharacteres(parameters["cpf"]),
                                email = parameters["email"],
                                payerId = payer != null ? payer.PayerId : null,
                            };

                            #endregion Person

                            #region billingAddress

                            BillingAdress billingAdress = null;

                            if (type != OperationType.Pix)
                            {
                                if (!parameters.ContainsKey("state") || string.IsNullOrEmpty(parameters["state"]) || parameters["state"].Length > 2)
                                    throw new Exception("Valor informado para estado não é reconhecido como válido. Deve-se constar somente as siglas da unidade. Ex: São Paulo => SP");

                                if (!parameters.ContainsKey("postalCode") || !int.TryParse(parameters["postalCode"], out int postalCode))
                                    throw new Exception("Valor informado para postal code não é reconhecido ou não informado. O formato deve ser numérico sem caracteres especiais. Ex: 17523-775 => 17523775.");

                                billingAdress = new BillingAdress
                                {
                                    place = parameters["place"],
                                    complement = parameters["complement"],
                                    neighborhood = parameters["neighborhood"],
                                    city = parameters["city"],
                                    state = parameters["state"],
                                    postalCode = postalCode,
                                };
                            }
                            else
                            {
                                billingAdress = new BillingAdress
                                {
                                    place = user.UserFields.FirstOrDefault(x => x.Name.Equals("Logradouro"))?.Value + ", " + user.UserFields.FirstOrDefault(x => x.Name.Equals("Número"))?.Value,
                                    complement = user.UserFields.FirstOrDefault(x => x.Name.Equals("Complemento"))?.Value,
                                    neighborhood = user.UserFields.FirstOrDefault(x => x.Name.Equals("Bairro"))?.Value,
                                    city = user.UserFields.FirstOrDefault(x => x.Name.Equals("Cidade"))?.Value,
                                    state = user.UserFields.FirstOrDefault(x => x.Name.Equals("Estado"))?.Value,
                                    postalCode = int.Parse(user.UserFields.FirstOrDefault(x => x.Name.Equals("CEP"))?.Value)
                                };
                            }

                            #endregion billingAddress

                            if (!string.IsNullOrWhiteSpace(splits))
                            {
                                var splitObject = JsonConvert.DeserializeObject<List<Split>>(splits).Where(s => !string.IsNullOrWhiteSpace(s.recipient));
                                if (splitObject.Any())
                                    transaction.Splits = splitObject.ToList();
                            }

                            // Trigger event to execute any logic before setting fields.
                            Plugin.CMS.Events.Trigger("OnPaymentRequest", parameters);

                            if (type == OperationType.Boleto)
                            {
                                result = Functions.Pay(transaction, null, person, billingAdress, customMainVendor);

                                //ZOOP
                                //if (user != null || (parameters.ContainsKey("userId") && gateway != PaymentGateway.Braspag))
                                //{
                                //    var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;
                                //    var json = result.Result;
                                //    Functions.AddOrUpdatePayerIdentifier(userId, gateway, (string)json.data.customer);
                                //}
                            }

                            else if (type == OperationType.Pix)
                            {
                                result = Functions.Pay(transaction, null, person, billingAdress, customMainVendor);


                                //ZOOP
                                //if ((user != null || (parameters.ContainsKey("userId")) && gateway != PaymentGateway.Braspag))
                                //{
                                //    var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;
                                //    var json = result.Result;
                                //    Functions.AddOrUpdatePayerIdentifier(userId, gateway, (string)json.data.customer);
                                //}
                            }
                            else if (type == OperationType.CreditWithToken)
                            {
                                if (!parameters.ContainsKey("cardToken"))
                                    throw new Exception("Nenhum token de cartão foi informado.");

                                var payerCard = Functions.GetCardByToken(parameters["cardToken"]);

                                CreditCard creditCard = new CreditCard
                                {
                                    CardToken = payerCard.CardId
                                };

                                result = Functions.Pay(transaction, creditCard, person, billingAdress, customMainVendor);
                            }

                            else if (type == OperationType.Credit)
                            {
                                var creditCard = new CreditCard
                                {
                                    CardHolderName = parameters["cardholdername"],
                                    CardNumber = parameters["cardnumber"],
                                    ExpirationMonth = parameters["expirationmonth"],
                                    ExpirationYear = parameters["expirationyear"],
                                    SecurityCode = parameters["securitycode"]
                                };

                                mask = creditCard.Mask();

                                parameters["cardnumber"] = mask.CardNumber;
                                parameters["securitycode"] = mask.SecurityCode;

                                result = Functions.Pay(transaction, creditCard, person, billingAdress, customMainVendor);

                            }

                            else if (type == OperationType.PlatformCredit)
                            {
                                Plugin.CMS.Events.Trigger($"OnPaymentSucceededPlatformCredit", transaction.Reference);

                                return new { approved = true };
                            }

                            var address = billingAdress.place.Split(',');

                            var paymentResult = new PaymentResult
                            {
                                referenceId = transaction.Reference,
                                Gateway = gateway,
                                Customer = new Customer
                                {
                                    Email = user.Email,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Document = user.UserFields.FirstOrDefault(x => x.Name.Equals("CPF"))?.Value,
                                    Phone = user.UserFields.FirstOrDefault(x => x.Name.Equals("Telefone"))?.Value,
                                    Address = new PAddress
                                    {
                                        Zip = billingAdress.postalCode.ToString(),
                                        PublicPlace = string.Join(",", address.Take(address.Length - 1)),
                                        Number = address.LastOrDefault(),
                                        Neighborhood = billingAdress.complement,
                                        City = billingAdress.city,
                                        State = billingAdress.state,
                                        Country = "BR"
                                    }
                                },
                                Order = new Order
                                {
                                    Amount = (transaction.Amount / 100),
                                    OperationType = type,
                                    Payment = new Payment
                                    {
                                        Installments = transaction.Installments,
                                        CardHolder = parameters.Any(x => x.Key == "cardholdername") ? parameters["cardholdername"] : "",
                                        Last4Digits = parameters.Any(x => x.Key == "cardnumber") ? (parameters["cardnumber"]).Substring(parameters["cardnumber"].Length - 4, 4) : "",
                                    }
                                }
                            };

                            result = Functions.ConvertResults(paymentResult, result);

                            Functions.AddOrderPayment(paymentResult);

                            if (result.Order.Payment.PaymentId == null)
                            {
                                throw new Exception("Problemas ao realizar o envio, tente novamente. " + result.Errors?.Join("|"));
                            }

                            if (result.Errors?.Count == 0)
                            {
                                // Trigger event to execute any logic after payment.
                                Plugin.CMS.Events.Trigger("OnPaymentCreate", result);
                            }


                            break;
                        }
                    case Constants.Cancellation:
                        {
                            if (!parameters.ContainsKey("idTransaction") || string.IsNullOrEmpty(parameters["idTransaction"]))
                                throw new Exception("Valor informado para idTransaction não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("amount") || !int.TryParse(parameters["amount"], out int amount))
                                throw new Exception("Valor informado para cobrança não é reconhecido ou não informado. O formato deve ser numérico sem decimais. Ex: R$ 521,36 => 52136.");

                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(parameters["origin"], true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            var customMainVendor = parameters.ContainsKey("customMainVendor") ? parameters["customMainVendor"] : null;

                            result = Functions.Cancel(parameters["idTransaction"], amount, gateway, customMainVendor);
                            break;
                        }
                    case Constants.Callback:
                        {

                            Plugin.CMS.Log.LogRequest(parameters, "Callback", Plugin.PluginName);

                            var platform = parameters.ContainsKey("platform") ? parameters["platform"] : null;

                            if (platform != null && platform.Equals("Braspag"))
                            {
                                var sale = Functions.GetByPaymentIdBraspag(parameters["paymentId"]).Result;

                                if (sale == null)
                                    throw new Exception(Messages.CommandNotFound);

                                Functions.SetOrderPaymentStatus(parameters["paymentId"], sale.Payment.Status);

                                var referenceId = Functions.GetReferenceIdByPaymentId(parameters["paymentId"]);
                                parameters.Add("referenceId", referenceId);

                                switch (sale.Payment.Status)
                                {
                                    case 0: //Falha ao processar o pagamento.
                                    case 3: //Pagamento negado por autorizador.
                                    case 13: //Pagamento cancelado por falha no processamento.
                                        parameters.Add("event", "Failed");
                                        break;
                                    case 1: //Meio de pagamento apto a ser capturado ou pago (boleto).
                                        if (sale.Payment.Type.Equals("Boleto"))
                                            parameters.Add("event", "Create");
                                        break;
                                    case 2: //Pagamento confirmado e finalizado.
                                        parameters.Add("event", "Succeeded");

                                        var clientData = Functions.GetBuyerDetailsByPaymentId(parameters["paymentId"]).Result;

                                        var sendDataApproved = new
                                        {
                                            NomeCliente = clientData.CustomerName
                                        };

                                        var templateApproved = "registrar-passageiros.cshtml";
                                        var subjectApproved = "Compra realizada com sucesso!";
                                        var contentApproved = Plugin.CMS.Notification.LoadTemplate(templateApproved, sendDataApproved);
                                        Plugin.CMS.Notification.SendNotification(contentApproved, subjectApproved, mailTo: new string[] { clientData.CustomerEmail });

                                        break;
                                    case 10: //Pagamento cancelado.
                                    case 11: //Pagamento cancelado/estornado.
                                        parameters.Add("event", "Canceled");
                                        break;
                                    case 12: //Esperando retorno da instituição financeira.
                                        break;
                                }
                            }

                            var callback = new Callback
                            {
                                platform = platform,
                                evento = parameters.ContainsKey("event") ? parameters["event"] : null, // (Succeeded, Create, Failed, Canceled)
                                eventId = parameters.ContainsKey("eventId") ? parameters["eventId"] : null,
                                referenceId = parameters.ContainsKey("referenceId") ? parameters["referenceId"] : null, // IdOrder
                            };

                            if (!String.IsNullOrEmpty(callback.evento))
                            {
                                Plugin.CMS.Events.Trigger($"OnPayment{callback.evento.ToString()}", callback);
                            }

                            result = new { success = true };

                            break;
                        }
                    case Constants.StoreCard:
                        {
                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(CMS.Configuration.Get(Configurations.DefaultOriginTransaction, PluginName), true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            user = CMS.Membership.User;

                            if (user == null && !parameters.ContainsKey("userId"))
                            {
                                throw new Exception("Usuário não identificado");
                            }

                            var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;
                            payer = Functions.GetPayerIdentifier(userId, gateway);

                            Person person = new Person();
                            if (payer != null)
                            {
                                person.payerId = payer.PayerId;
                            }
                            else
                            {
                                if (!parameters.ContainsKey("cpf") || string.IsNullOrEmpty(parameters["cpf"]) || !Extensions.IsValidDocument(parameters["cpf"]))
                                    throw new Exception("Valor informado para CPF não é reconhecido ou não informado. O formato deve ser numérico sem caracteres especiais. Ex: 455.956.037-47 => 45595603747.");

                                person.firstName = parameters["firstName"];
                                person.lastName = parameters["lastName"];
                                person.cpf = Extensions.RemoveSpecialCharacteres(parameters["cpf"]);
                                person.email = parameters["email"];
                            }

                            var creditCard = new CreditCard
                            {
                                CardHolderName = parameters["cardholdername"],
                                CardNumber = parameters["cardnumber"],
                                ExpirationMonth = parameters["expirationmonth"],
                                ExpirationYear = parameters["expirationyear"],
                                SecurityCode = parameters["securitycode"]
                            };

                            mask = creditCard.Mask();

                            parameters["cardnumber"] = mask.CardNumber;
                            parameters["securitycode"] = mask.SecurityCode;

                            result = Functions.AddCardToPersonOnGateway(creditCard, person, gateway);

                            var json = result.Result;

                            if (payer == null)
                            {
                                payer = Functions.AddOrUpdatePayerIdentifier(userId, gateway, (string)json.data.customer);
                            }

                            PayerCard payerCard = new PayerCard
                            {
                                payerId = payer.Id,
                                CardId = (string)json.data.id,
                                Gateway = gateway,
                                LastFourDigits = (string)json.data.last4_digits,
                                HolderName = (string)json.data.holder_name,
                                CardBrand = (string)json.data.card_brand,
                            };

                            var card = Functions.AddCard(payerCard);
                            result.Result.data = null;

                            break;
                        }
                    case Constants.ListCards:
                        {
                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(CMS.Configuration.Get(Configurations.DefaultOriginTransaction, PluginName), true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            user = CMS.Membership.User;

                            if (user == null && !parameters.ContainsKey("userId"))
                            {
                                throw new Exception("Usuário não identificado");
                            }

                            var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;

                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            result = Functions.LoadCardsByUserPaginated(userId, page, size, gateway);

                            break;
                        }
                    case Constants.ListAllCards:
                        {
                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(CMS.Configuration.Get(Configurations.DefaultOriginTransaction, PluginName), true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            user = CMS.Membership.User;

                            if (user == null && !parameters.ContainsKey("userId"))
                            {
                                throw new Exception("Usuário não identificado");
                            }

                            var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;

                            result = Functions.LoadCardsByUser(userId);

                            break;
                        }
                    case Constants.RemoveCard:
                        {
                            if (!parameters.ContainsKey("origin") || !Enum.TryParse(CMS.Configuration.Get(Configurations.DefaultOriginTransaction, PluginName), true, out PaymentGateway gateway))
                                gateway = PaymentGateway.Zoop;

                            if (!parameters.ContainsKey("cardToken"))
                                throw new Exception("Nenhum token de cartão foi informado.");

                            var userId = parameters.ContainsKey("userId") ? int.Parse(parameters["userId"]) : CMS.Membership.User.Id;

                            result = Functions.RemoveCardFromPayer(userId, parameters["cardToken"], gateway);

                            var json = result.Result;

                            if (result.Result.status == 200)
                            {
                                Functions.DeleteCardByToken(parameters["cardToken"], userId, gateway);
                                result.Result.data = null;
                            }

                            break;
                        }
                    case Constants.GetMerchantCategoryCodes:
                        {
                            result = Functions.GetMerchantCategoryCodes();
                            break;
                        }
                    case Constants.GetSellerDetails:
                        {
                            var seller_cpf = "";
                            if (parameters.ContainsKey("seller_cpf"))
                                seller_cpf = parameters["seller_cpf"];

                            var seller_id = "";
                            if (parameters.ContainsKey("seller_id"))
                                seller_id = parameters["seller_id"];

                            if (string.IsNullOrEmpty(seller_id) && string.IsNullOrEmpty(seller_cpf))
                                throw new Exception("Valor informado para seller_id seller_cpf não é reconhecido ou não informado.");

                            if (string.IsNullOrEmpty(seller_id))
                                result = Functions.GetSellerDetailsByCpf(seller_cpf);
                            else
                                result = Functions.GetSellerDetailsById(seller_id);

                            break;
                        }
                    case Constants.ZoopCreateSeller:
                        {
                            if (!parameters.ContainsKey("firstName") || string.IsNullOrEmpty(parameters["firstName"]))
                                throw new Exception("Valor informado para firstName não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("lastName") || string.IsNullOrEmpty(parameters["lastName"]))
                                throw new Exception("Valor informado para lastName não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("email") || string.IsNullOrEmpty(parameters["email"]))
                                throw new Exception("Valor informado para email não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("phone_number") || string.IsNullOrEmpty(parameters["phone_number"]))
                                throw new Exception("Valor informado para phone_number não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("cpf") || string.IsNullOrEmpty(parameters["cpf"]))
                                throw new Exception("Valor informado para cpf não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("birthdate") || string.IsNullOrEmpty(parameters["birthdate"]))
                                throw new Exception("Valor informado para birthdate não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("statement_descriptor") || string.IsNullOrEmpty(parameters["statement_descriptor"]))
                                throw new Exception("Valor informado para statement_descriptor não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("mcc") || string.IsNullOrEmpty(parameters["mcc"]))
                                throw new Exception("Valor informado para mcc não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("place") || string.IsNullOrEmpty(parameters["place"]))
                                throw new Exception("Valor informado para place não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("neighborhood") || string.IsNullOrEmpty(parameters["neighborhood"]))
                                throw new Exception("Valor informado para neighborhood não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("city") || string.IsNullOrEmpty(parameters["city"]))
                                throw new Exception("Valor informado para city não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("state") || string.IsNullOrEmpty(parameters["state"]))
                                throw new Exception("Valor informado para state não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("postalCode") || !int.TryParse(parameters["postalCode"], out int postalCode))
                                throw new Exception("Valor informado para postal code não é reconhecido ou não informado. O formato deve ser numérico sem caracteres especiais. Ex: 17523-775 => 17523775.");

                            PersonSeller person = new PersonSeller
                            {
                                firstName = parameters["firstName"],
                                lastName = parameters["lastName"],
                                email = parameters["email"],
                                phone_number = parameters["phone_number"],
                                cpf = Extensions.RemoveSpecialCharacteres(parameters["cpf"]),
                                birthdate = parameters["birthdate"],
                                statement_descriptor = parameters["statement_descriptor"],
                                mcc = parameters["mcc"]
                            };

                            var complement = "";
                            if (parameters.ContainsKey("complement"))
                            {
                                complement = parameters["complement"];
                            }

                            BillingAdress billingAdress = new BillingAdress
                            {
                                place = parameters["place"],
                                complement = complement,
                                neighborhood = parameters["neighborhood"],
                                city = parameters["city"],
                                state = parameters["state"],
                                postalCode = postalCode,
                            };

                            result = Functions.CreateSeller(person, billingAdress);

                            break;
                        }
                    case Constants.SendSellerDocuments:
                        {
                            if (!parameters.ContainsKey("file") || string.IsNullOrEmpty(parameters["file"]))
                                throw new Exception("Valor informado para file não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("category") || string.IsNullOrEmpty(parameters["category"]))
                                throw new Exception("Valor informado para category não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("description") || string.IsNullOrEmpty(parameters["description"]))
                                throw new Exception("Valor informado para description não é reconhecido ou não informado.");

                            if (!parameters.ContainsKey("seller_id") || string.IsNullOrEmpty(parameters["seller_id"]))
                                throw new Exception("Valor informado para seller_id não é reconhecido ou não informado.");

                            byte[] bytes = Convert.FromBase64String(parameters["file"]);

                            result = Functions.SendDocuments(bytes, parameters["category"], parameters["description"], parameters["seller_id"]);

                            break;
                        }
                    case Constants.SendBoletoToEmail:
                        {
                            if (!parameters.ContainsKey("zoop_boleto_id") || string.IsNullOrEmpty(parameters["zoop_boleto_id"]))
                                throw new Exception("Valor informado para zoop_boleto_id não é reconhecido ou não informado.");

                            result = Functions.SendBoletoToEmail(parameters["zoop_boleto_id"]);

                            break;
                        }
                    default:
                        throw new Exception(Messages.CommandNotFound);
                }

                CMS.Log.LogRequest(result, "Trace", PluginName, parameters, (CMS.Membership.User ?? CMS.Membership.AdminUser));

                return result;
            }
            catch (Exception ex)
            {
                Functions.LogException(ex, objects: new object[] { parameters, (CMS.Membership.User ?? CMS.Membership.AdminUser) });
                throw ex;
            }
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
        /// Method to return any metric available by the plugin
        /// </summary>
        /// <returns>Returns a list of metrics to the system</returns>
        public IList<IMetric> Metrics()
        {
            return new List<IMetric>();
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
        /// Method to replicate all the idiom keys
        /// </summary>
        public void ReplicateIdiomKeys()
        {
            return;
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

            // Create database tables
            Configurations.Setup();

            // Return a new instance of this class
            return this;
        }

        /// <summary>
        /// Method to Uninstall Plugin
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            // Execute database Uninstall Command
            Configurations.Uninstall();
            return true;
        }

        /// <summary>
        /// Method to trigger system events 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="data"></param>
        /// <param name="exception"></param>
        public void TriggerEvent<T>(string eventType, T data, Exception exception = null)
        {
            return;
        }
    }
}