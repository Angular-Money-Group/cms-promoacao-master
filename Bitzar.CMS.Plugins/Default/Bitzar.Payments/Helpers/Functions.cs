using Bitzar.CMS.Core.Models;
using Bitzar.ECommerce.Models;
using Bitzar.Payments.Gateways.Braspag;
using Bitzar.Payments.Gateways.Braspag.Models;
using Bitzar.Payments.Gateways.Zoop;
using Bitzar.Payments.Gateways.Zoop.Models;
using Bitzar.Payments.Models;
using BraspagApiDotNetSdk.Contracts;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Bitzar.Payments.Models.Transaction;
using CreditCard = Bitzar.Payments.Models.CreditCard;
using Database = Bitzar.Payments.Models.Database;
using Error = Bitzar.Payments.Models.Error;
using Payment = Bitzar.Payments.Models.Payment;

namespace Bitzar.Payments.Helpers
{
    public class Functions
    {
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

        /// <summary>
        /// Convert PaymentGateway Result to PaymentResult
        /// </summary>
        /// <param name="paymentResult"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static PaymentResult ConvertResults(PaymentResult paymentResult, dynamic result)
        {
            dynamic content;

            var response = result?.Result?.Result;
            var jsonDeserializer = new JsonDeserializer { };

            switch (paymentResult.Gateway)
            {
                case PaymentGateway.Zoop:
                    content = response?.data;
                    paymentResult.Order.Payment = new Payment
                    {
                        Nsu = content?.payment_authorization?.authorization_nsu,
                        AuthCode = content?.payment_authorization?.authorization_code,
                        Description = content?.description,
                        CardBrand = content?.payment_method?.card_brand,
                        Last4Digits = content?.payment_method?.last4_digits,
                        BarCode = content?.payment_method?.barcode,
                        QrCode = content?.payment_method?.qr_code?.emv,
                        Url = (paymentResult.Order.OperationType == OperationType.Pix ? content?.payment_method?.pix_link : content?.payment_method?.url),
                        CardHolder = paymentResult.Order.Payment.CardHolder,
                        Installments = paymentResult.Order.Payment.Installments,
                    };

                    List<Gateways.Zoop.Models.Error> errors = jsonDeserializer.Deserialize<List<Gateways.Zoop.Models.Error>>(content?.error);

                    paymentResult.Errors = errors.Select(x => new Error() { Code = x.status_code, Message = x.message }).ToList();

                    break;
                case PaymentGateway.Braspag:
                    content = response?.Payment;
                    paymentResult.Order.Payment = new Payment
                    {
                        PaymentId = content?.PaymentId,
                        Nsu = content?.ProofOfSale,
                        AuthCode = content?.AuthorizationCode,
                        //Description = content.Description,
                        CardBrand = content?.CreditCard?.Brand,
                        BarCode = content?.DigitableLine,
                        QrCode = content?.QrCodeBase64Image,
                        Url = (paymentResult.Order.OperationType == OperationType.Pix ? content?.QrCodeString : content?.Url),
                        CardHolder = paymentResult.Order.Payment.CardHolder,
                        Last4Digits = paymentResult.Order.Payment.Last4Digits,
                        Installments = paymentResult.Order.Payment.Installments,
                    };

                    paymentResult.Errors = new List<Error>();

                    Plugin.CMS.Log.LogRequest(response, "Info", Plugin.PluginName);

                    List<BraspagApiDotNetSdk.Contracts.Error> braspagErrors = response?.ErrorDataCollection;

                    if (braspagErrors != null && braspagErrors.Any())
                        paymentResult.Errors = braspagErrors.Select(x => new Error() { Code = x.Code.ToString(), Message = x.Message }).ToList();

                    if (paymentResult.Order.OperationType == OperationType.Credit)
                    {
                        var statusSucesso = new List<string>() { "1", "2", "12" };

                        if (content == null)
                        {
                            paymentResult.Errors.Add(new Error() { Code = "000", Message = "Não foi possível realizar o envio do pagamento" });
                        }
                        else if (!statusSucesso.Contains((string)content.Status))
                        {
                            paymentResult.Errors.Add(new Error() { Code = "000", Message = content.ProviderReturnMessage });
                        }
                    }

                    Plugin.CMS.Log.LogRequest(paymentResult, "Info", Plugin.PluginName);

                    break;
            }

            return paymentResult;
        }

        /// <summary>
        /// Adds a payment
        /// </summary>
        /// <param name="paymentResult"></param>
        /// <returns></returns>
        public static OrderPayment AddOrderPayment(PaymentResult paymentResult)
        {
            var orderPayment = new OrderPayment
            {
                IdOrder = GetOrderIdByReferenceId(paymentResult.referenceId),
                RequestId = Guid.NewGuid().ToString(),
                RequestStatus = paymentResult.Errors?.Count == 0 ? 1 : 0,
                Gateway = (int)paymentResult.Gateway,
                CustomerFirstName = paymentResult.Customer.FirstName,
                CustomerLastName = paymentResult.Customer.LastName,
                CustommerDocument = paymentResult.Customer.Document,
                CustomerEmail = paymentResult.Customer.Email,
                CustomerPhone = paymentResult.Customer.Phone,
                CustomerAddressZip = paymentResult.Customer.Address.Zip,
                CustomerAddressPublicPlace = paymentResult.Customer.Address.PublicPlace,
                CustomerAddressNumber = paymentResult.Customer.Address.Number,
                CustomerAddressNeighborhood = paymentResult.Customer.Address.Neighborhood,
                CustomerAddressCity = paymentResult.Customer.Address.City,
                CustomerAddressState = paymentResult.Customer.Address.State,
                CustomerAddressCountry = paymentResult.Customer.Address.Country,
                OrderAmount = paymentResult.Order.Amount,
                OrderOperationType = paymentResult.Order.OperationType.ToString(),
                PaymentPaymentId = paymentResult.Order.Payment.PaymentId,
                PaymentInstallments = paymentResult.Order.Payment.Installments,
                PaymentCardHolder = paymentResult.Order.Payment.CardHolder,
                PaymentAuthCode = paymentResult.Order.Payment.AuthCode,
                PaymentNsu = paymentResult.Order.Payment.Nsu,
                PaymentCardBrand = paymentResult.Order.Payment.CardBrand,
                PaymentBarCode = paymentResult.Order.Payment.BarCode,
                PaymentDescription = paymentResult.Order.Payment.Description,
                PaymentLastFourDigits = paymentResult.Order.Payment.Last4Digits,
                PaymentQrCode = paymentResult.Order.Payment.QrCode,
                PaymentUrl = paymentResult.Order.Payment.Url
            };

            using (var db = new Database())
            {
                try
                {
                    orderPayment.CreatedAt = DateTime.Now;

                    db.OrderPayments.Add(orderPayment);
                    db.SaveChanges();

                    return orderPayment;
                }
                catch (Exception e)
                {
                    LogException(e, objects: new object[] { orderPayment });
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to update the payment state.
        /// </summary>
        /// <param name="paymentId">Payment to be updated</param>
        /// <param name="status">Status to be defined in the payment</param>
        /// <returns></returns>
        internal static bool SetOrderPaymentStatus(string paymentId = null, int status = 0)
        {
            try
            {
                using (var db = new Database())
                {
                    var payment = db.OrderPayments.FirstOrDefault(c => c.PaymentPaymentId == paymentId);

                    if (payment == null)
                        return false;

                    payment.RequestStatus = status;

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { paymentId, status });
                throw;
            }
        }

        public static int GetOrderIdByReferenceId(string referenceId)
        {
            using (var db = new Database())
            {
                return db.Database.SqlQuery<int>($@"SELECT Id FROM btz_order WHERE Uuid=@p0", referenceId).FirstOrDefault();
            }
        }

        public static string GetReferenceIdByPaymentId(string paymentId)
        {
            using (var db = new Database())
            {
                return db.Database.SqlQuery<string>($@"SELECT Uuid FROM btz_order AS O 
                                                  INNER JOIN btz_orderpayment AS OP ON OP.IdOrder = O.Id
                                                       WHERE OP.PaymentPaymentId = @p0", paymentId).FirstOrDefault();
            }
        }

        /// <summary>
        /// Método que retorna os detalhes de um comprador
        /// </summary>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        public static async Task<CustomerData> GetBuyerDetailsByPaymentId(string referenceId)
        {
            using (var db = new Database())
            {
                return db.Database.SqlQuery<CustomerData>($@"SELECT CONCAT_WS("" "", CustomerFirstName, CustomerLastName) as CustomerName, CustomerEmail 
                                                         FROM btz_order AS O 
                                                  INNER JOIN btz_orderpayment AS OP ON OP.IdOrder = O.Id
                                                       WHERE OP.PaymentPaymentId = @p0", referenceId).FirstOrDefault();
            }
        }

        public static async Task<BraspagSales> GetByPaymentIdBraspag(string paymentId)
        {
            var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Braspag_ModuleQueryUrl, Plugin.PluginName);
            var merchantKey = Plugin.CMS.Configuration.Get(Configurations.Braspag_MerchantKey, Plugin.PluginName);
            var merchantId = Plugin.CMS.Configuration.Get(Configurations.Braspag_MerchantId, Plugin.PluginName);

            return await BraspagFunctions.GetByPaymentId(paymentId, moduleUrl, merchantId, merchantKey);
        }

        /// <summary>
        /// Add or update a payer
        /// </summary>
        /// <param name="IdUser"></param>
        /// <param name="gateway"></param>
        /// <param name="payerId"></param>
        /// <returns></returns>
        public static PayerIdentifier AddOrUpdatePayerIdentifier(int IdUser, PaymentGateway gateway, string payerId)
        {
            using (var db = new Database())
            {
                try
                {
                    var payIdentifier = db.PayerIdentifiers.FirstOrDefault(c => c.IdUser == IdUser && (PaymentGateway)c.Gateway == gateway);

                    if (payIdentifier == null)
                    {
                        payIdentifier = new PayerIdentifier
                        {
                            IdUser = IdUser,
                            Gateway = gateway,
                            PayerId = payerId,
                            CreationDate = DateTime.Now,
                            AlterationDate = DateTime.Now
                        };

                        db.PayerIdentifiers.Add(payIdentifier);
                        db.SaveChanges();

                    }
                    else
                    {
                        if (payIdentifier.IdUser != IdUser)
                        {
                            payIdentifier.AlterationDate = DateTime.Now;

                            db.Entry(payIdentifier).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }

                    return payIdentifier;

                }
                catch (Exception e)
                {
                    LogException(e, objects: new object[] { IdUser, gateway, payerId });
                    throw;
                }
            }
        }

        /// <summary>
        /// Get a payer
        /// </summary>
        /// <param name="IdUser"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        public static PayerIdentifier GetPayerIdentifier(int IdUser, PaymentGateway gateway)
        {
            using (var db = new Database())
            {
                try
                {
                    var payIdentifier = db.PayerIdentifiers.FirstOrDefault(c => c.IdUser == IdUser && (PaymentGateway)c.Gateway == gateway);
                    return payIdentifier;

                }
                catch (Exception e)
                {
                    LogException(e, objects: new object[] { IdUser, gateway });
                    throw;
                }
            }
        }

        /// <summary>
        /// Add a card to person
        /// </summary>
        /// <param name="creditCard"></param>
        /// <param name="person"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        public static async Task<object> AddCardToPersonOnGateway(CreditCard creditCard, Person person, PaymentGateway gateway)
        {
            using (var db = new Database())
            {
                try
                {
                    if (gateway == PaymentGateway.Zoop)
                    {
                        var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                        var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                        var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                        var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);
                        var mainVendor = Plugin.CMS.Configuration.Get(Configurations.Zoop_MainVendor, Plugin.PluginName);

                        return await ZoopFunctions.AddCardToBuyer(creditCard, person, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);
                    }

                    return null;

                }
                catch (Exception e)
                {
                    LogException(e, objects: new object[] { creditCard, person, gateway });
                    throw;
                }
            }
        }

        /// <summary>
        /// Adds a card to a payer
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static PayerCard AddCard(PayerCard card)
        {
            using (var db = new Database())
            {
                try
                {
                    if (String.IsNullOrEmpty(card.LastFourDigits) || String.IsNullOrEmpty(card.CardId) || card.payerId <= 0)
                        throw new Exception(Messages.NewCardInvalid);

                    card.CreationDate = DateTime.Now;

                    db.PayerCards.Add(card);
                    db.SaveChanges();

                    return card;
                }
                catch (Exception e)
                {
                    LogException(e, objects: new object[] { card });
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a card by Id
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        public static PayerCard GetCardById(int idCard)
        {
            using (var db = new Database())
            {
                var card = db.PayerCards.FirstOrDefault(t => t.Id == idCard);

                return card;

            }
        }

        /// <summary>
        /// Get a card
        /// </summary>
        /// <param name="cardToken"></param>
        /// <param name="userId"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        public static PayerCard GetCard(string cardToken, int userId, PaymentGateway gateway)
        {
            using (var db = new Database())
            {
                var card = db.PayerCards.FirstOrDefault(c => c.Uiid == cardToken && c.payer.IdUser == userId && (PaymentGateway)c.Gateway == gateway);

                return card;

            }
        }

        /// <summary>
        /// Gets a card by token
        /// </summary>
        /// <param name="cardToken"></param>
        /// <returns></returns>
        public static PayerCard GetCardByToken(string cardToken)
        {
            using (var db = new Database())
            {
                var card = db.PayerCards.FirstOrDefault(c => c.Uiid == cardToken);

                return card;

            }
        }

        /// <summary>
        /// Deletes a card by Id
        /// </summary>
        /// <param name="idCard"></param>
        /// <returns></returns>
        public static bool DeleteCard(int idCard)
        {
            if (idCard <= 0)
                throw new Exception(Messages.CardInvalidId);

            try
            {
                using (var db = new Database())
                {
                    var card = db.PayerCards.FirstOrDefault(t => t.Id == idCard);

                    if (card == null)
                        throw new Exception(Messages.CardNotFound);

                    db.PayerCards.Remove(card);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception error)
            {
                LogException(error, objects: new object[] { idCard });
                throw;
            }
        }

        /// <summary>
        /// Delete a card by token
        /// </summary>
        /// <param name="cardToken"></param>
        /// <param name="userId"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        public static bool DeleteCardByToken(string cardToken, int userId, PaymentGateway gateway)
        {
            if (String.IsNullOrEmpty(cardToken))
                throw new Exception(Messages.CardInvalidId);

            try
            {
                using (var db = new Database())
                {
                    var card = db.PayerCards.FirstOrDefault(t => t.Uiid == cardToken && t.payer.IdUser == userId && t.Gateway == gateway);

                    if (card == null)
                        throw new Exception(Messages.CardNotFound);

                    db.PayerCards.Remove(card);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception error)
            {
                LogException(error, objects: new object[] { cardToken, userId });
                throw;
            }
        }

        /// <summary>
        /// List all tickets by User
        /// </summary>
        /// <returns></returns>
        internal static PaggedResult<PayerCard> LoadCardsByUserPaginated(int idUser, int page = 1, int size = Configurations.PaginationSize, PaymentGateway gateway = 0)
        {
            try
            {

                using (var db = new Database())
                {
                    var query = db.PayerCards.Where(q => q.payer.IdUser == idUser)
                                  .AsNoTracking()
                                  .AsQueryable();

                    var count = query.Count();

                    query = query.OrderByDescending(o => o.CreationDate)
                                 .ThenByDescending(o => o.Id)
                                 .Skip((page - 1) * size).Take(size);

                    if (gateway != PaymentGateway.All)
                    {
                        query = query.Where(c => c.Gateway == gateway);
                    }

                    var cards = query.ToList();

                    PaggedResult<PayerCard> result = new PaggedResult<PayerCard>
                    {
                        Records = cards,
                        Page = page,
                        Size = size,
                        Count = count,
                        CountPage = Convert.ToInt32(Math.Ceiling(count / (decimal)size)),
                    };

                    return result;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { page, size });
                throw;
            }
        }

        /// <summary>
        /// List all tickets by User
        /// </summary>
        /// <returns></returns>
        internal static List<PayerCard> LoadCardsByUser(int idUser)
        {
            try
            {

                using (var db = new Database())
                {
                    var query = db.PayerCards.Where(q => q.payer.IdUser == idUser)
                                  .AsNoTracking()
                                  .AsQueryable();

                    var count = query.Count();

                    query = query.OrderByDescending(o => o.CreationDate)
                                 .ThenByDescending(o => o.Id);

                    var cards = query.ToList();

                    return cards;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { idUser });
                throw;
            }
        }

        /// <summary>
        /// Remove a card from a payer collections
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardToken"></param>
        /// <param name="gateway"></param>
        /// <returns></returns>
        internal static async Task<object> RemoveCardFromPayer(int userId, string cardToken, PaymentGateway gateway)
        {
            var payerCard = GetCard(cardToken, userId, gateway);


            if (payerCard == null)
            {
                throw new Exception("Cartão não identificado");
            }

            var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
            var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
            var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
            var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);
            var mainVendor = Plugin.CMS.Configuration.Get(Configurations.Zoop_MainVendor, Plugin.PluginName);

            return await ZoopFunctions.RemoveCardFromBuyer(payerCard.CardId, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);
        }


        /// <summary>
        /// Método que identifica a plataforma de pagamento e o executa
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="creditCard"></param>
        /// <param name="person"></param>
        /// <param name="billingAdress"></param>
        /// <returns></returns>
        public static async Task<object> Pay(Transaction transactions, CreditCard creditCard, Person person = null, BillingAdress billingAdress = null, string customMainVendor = null)
        {
            try
            {
                switch (transactions.Origin)
                {
                    case PaymentGateway.Zoop:
                        return await PayZoop(transactions, creditCard, person, billingAdress, customMainVendor);
                    case PaymentGateway.Braspag:
                        return await PayBrapag(transactions, creditCard, person, billingAdress, customMainVendor);
                }

                return null;
            }
            catch (Exception ex)
            {
                LogException(ex, objects: new object[] { transactions, creditCard.Mask(), person, billingAdress });
                throw ex;
            }

        }

        private static async Task<object> PayZoop(Transaction transactions, CreditCard creditCard, Person person = null, BillingAdress billingAdress = null, string customMainVendor = null)
        {
            var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
            var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
            var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
            var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);
            var mainVendor = String.IsNullOrWhiteSpace(customMainVendor) ? Plugin.CMS.Configuration.Get(Configurations.Zoop_MainVendor, Plugin.PluginName) : customMainVendor;


            if (transactions.Kind == OperationType.Credit)
            {
                return await ZoopFunctions.MakePaymentCredit(creditCard, transactions, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);

            }
            if (transactions.Kind == OperationType.PlatformCredit)
            {
                return await ZoopFunctions.MakePaymentCredit(creditCard, transactions, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);

            }
            if (transactions.Kind == OperationType.CreditWithToken)
            {
                return await ZoopFunctions.MakePaymentCreditToken(creditCard, person, transactions, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);

            }
            if (transactions.Kind == OperationType.Boleto)
            {
                return await ZoopFunctions.MakePaymentBoleto(transactions, person, billingAdress, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);
            }
            if (transactions.Kind == OperationType.Pix)
            {
                return await ZoopFunctions.MakePaymentPix(transactions, person, billingAdress, token, moduleUrl, key, mkpId, mainVendor).ConfigureAwait(false);
            }

            return null;
        }


        private static async Task<object> PayBrapag(Transaction transactions, CreditCard creditCard, Person person = null, BillingAdress billingAdress = null, string customMainVendor = null)
        {
            var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Braspag_ModuleUrl, Plugin.PluginName);
            var merchantKey = Plugin.CMS.Configuration.Get(Configurations.Braspag_MerchantKey, Plugin.PluginName);
            var merchantId = Plugin.CMS.Configuration.Get(Configurations.Braspag_MerchantId, Plugin.PluginName);


            if (transactions.Kind == OperationType.Credit)
            {
                return await BraspagFunctions.MakePaymentCredit(creditCard, transactions, person, billingAdress, moduleUrl, merchantId, merchantKey).ConfigureAwait(false);

            }
            if (transactions.Kind == OperationType.Boleto)
            {
                return await BraspagFunctions.MakePaymentBoleto(transactions, person, billingAdress, moduleUrl, merchantId, merchantKey).ConfigureAwait(false);
            }
            if (transactions.Kind == OperationType.Pix)
            {
                return await BraspagFunctions.MakePaymentPix(transactions, person, moduleUrl, merchantId, merchantKey).ConfigureAwait(false);
            }

            return null;
        }


        /// <summary>
        /// Método que identifica a plataforma de pagamento e o executa cancelamento de compra
        /// </summary>
        /// <param name="idTransaction"></param>
        /// <param name="amount"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static async Task<object> Cancel(string idTransaction, int amount, Transaction.PaymentGateway origin, string customMainVendor)
        {
            try
            {

                if (origin == PaymentGateway.Zoop)
                {
                    var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                    var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                    var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                    var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);
                    var mainVendor = String.IsNullOrWhiteSpace(customMainVendor) ? Plugin.CMS.Configuration.Get(Configurations.Zoop_MainVendor, Plugin.PluginName) : customMainVendor;

                    return await ZoopFunctions.CancelPayment(token, moduleUrl, idTransaction, amount, key, mkpId, mainVendor).ConfigureAwait(false);

                }
                return null;
            }
            catch (Exception ex)
            {
                LogException(ex, objects: new object[] { idTransaction, amount });
                throw ex;
            }
        }

        /// <summary>
        /// Método que retorna as categorias de comércio do zoop
        /// </summary>
        /// <returns></returns>
        public static async Task<object> GetMerchantCategoryCodes()
        {
            try
            {
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);

                return await ZoopFunctions.GetMerchantCategoryCodes(token, key, moduleUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método que cria um vendedor na plataforma zoop
        /// </summary>
        /// <param name="person"></param>
        /// <param name="billingAdress"></param>
        /// <returns></returns>
        public static async Task<object> CreateSeller(PersonSeller person, BillingAdress billingAdress)
        {
            try
            {
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);
                var mainVendor = Plugin.CMS.Configuration.Get(Configurations.Zoop_MainVendor, Plugin.PluginName);

                return await ZoopFunctions.ZoopCreateSeller(person, billingAdress, moduleUrl, token, key, mkpId, mainVendor).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método que retorna os detalhes de um vendedor zoop
        /// </summary>
        /// <param name="seller_id"></param>
        /// <returns></returns>
        public static async Task<object> GetSellerDetailsById(string seller_id)
        {
            try
            {
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);

                return await ZoopFunctions.GetSellerDetailsById(token, key, moduleUrl, mkpId, seller_id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<object> GetSellerDetailsByCpf(string seller_cpf)
        {
            try
            {
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);

                return await ZoopFunctions.GetSellerDetailsByCpf(token, key, moduleUrl, mkpId, seller_cpf).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<object> SendDocuments(byte[] arquivo, string category, string description, string seller_id)
        {
            try
            {
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);

                return await ZoopFunctions.SendSellerDocuments(moduleUrl, key, mkpId, arquivo, description, category, seller_id, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<object> SendBoletoToEmail(string zoop_boleto_id)
        {
            try
            {
                var moduleUrl = Plugin.CMS.Configuration.Get(Configurations.Zoop_ModuleUrl, Plugin.PluginName);
                var token = Plugin.CMS.Configuration.Get(Configurations.Zoop_AccessToken, Plugin.PluginName);
                var key = Plugin.CMS.Configuration.Get(Configurations.Zoop_Key, Plugin.PluginName);
                var mkpId = Plugin.CMS.Configuration.Get(Configurations.Zoop_Id, Plugin.PluginName);

                return await ZoopFunctions.SendBoletoToEmail(moduleUrl, key, mkpId, token, zoop_boleto_id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}