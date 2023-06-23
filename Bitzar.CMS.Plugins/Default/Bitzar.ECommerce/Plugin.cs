using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using Bitzar.ECommerce.Helpers;
using Bitzar.ECommerce.Models;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebSockets;
using static Bitzar.ECommerce.Helpers.Enumerators;
using static Bitzar.ECommerce.Models.Event;

namespace Bitzar.ECommerce
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
        internal static string PluginName => "Bitzar.ECommerce.dll";

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
                if (function.ToUpper() != Configurations.GetCoupon)
                {
                    // Validate the request token to process the operations.
                    if (token != CMS.Configuration.Token && !CMS.Security.ValidateToken(token))
                        throw new Exception("Token Expirado!");

                }

                dynamic result = null;

                // Start process the system functions
                switch (function.ToUpper())
                {
                    /*
                    * Front-End allowed methods 
                    **/
                    case Configurations.AddOrUpdateCartItem:
                        {
                            // Trigger event to execute any logic before update item.
                            Plugin.CMS.Events.Trigger("OnOrderCartUpdatingItem", parameters);

                            // Get cart parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;

                            // Load ordem and items fields lists
                            var orderFields = parameters.Where(p => p.Key.StartsWith("Order.")).ToList();
                            var itemFields = parameters.Where(p => p.Key.StartsWith("Item.")).ToList();

                            // If cartUuid is null create a new order in the service
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                cartUuid = Functions.CreateOrder().Uuid;

                            // Validate the parameters provided in the service
                            if (!parameters.ContainsKey("Product") || !int.TryParse(parameters["Product"], out int product))
                                throw new ArgumentException(Messages.ProductIdNotFound);
                            if (!parameters.ContainsKey("Quantity") || !decimal.TryParse(parameters["Quantity"], out decimal quantity))
                                throw new ArgumentException(Messages.ProductQuantityNotProvided);
                            if (!parameters.ContainsKey("Price") || !decimal.TryParse(parameters["Price"], NumberStyles.Number, ConverterCulture, out decimal price))
                                throw new ArgumentException(Messages.ProductQuantityNotProvided);

                            // Check if Validate is presented
                            if (!parameters.TryGetValue("Validator", out string validator) || string.IsNullOrWhiteSpace(validator))
                                throw new ArgumentNullException(Messages.RequestValidatorNotFoundOrInvalid);

                            // Load order to process the item
                            var order = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            // Update order fields
                            Functions.UpdateOrderFields(order, orderFields);

                            result = Functions.AddOrUpdateOrder(order, product, quantity, price, itemFields);

                            var orderEmit = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            Plugin.CMS.Events.Trigger("OnOrderCartUpdateItem", orderEmit);

                            break;
                        }
                    case Configurations.DeleteCartItem:
                        {
                            // Trigger event to execute any logic before delete item.
                            Plugin.CMS.Events.Trigger("OnOrderCartDeletingItem", parameters);

                            // Get cart parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            // Validate the parameters provided in the service
                            if (!parameters.ContainsKey("Product") || !int.TryParse(parameters["Product"], out int product))
                                throw new ArgumentException(Messages.ProductIdNotFound);

                            // Load order to process the item
                            var order = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            // Delete cartItem order fields
                            result = Functions.DeleteCartItem(order, product);

                            break;
                        }
                    case Configurations.ShowCart:
                        {
                            // Get cart parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            // Show cart data
                            result = Functions.ShowCart(cartUuid);
                            break;
                        }
                    case Configurations.ShowConfirmation:
                        {
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            result = Functions.ShowConfirmation(cartUuid);

                            break;
                        }
                    case Configurations.SetCustomer:
                        {
                            // Get cart parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            var userId = parameters.ContainsKey("UserId") ? parameters["UserId"] : (Plugin.CMS.Membership.User?.Id.ToString() ?? null);

                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            if (string.IsNullOrWhiteSpace(userId))
                                throw new Exception(Messages.UserNotFound);

                            var user = Plugin.CMS.Membership.Members().FirstOrDefault(f => f.Id == Convert.ToInt32(userId));

                            // Validate if the user is logged in
                            if (user == null)
                                throw new Exception(Messages.NotAuthorizedOperation);

                            var order = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            // Set order customer information
                            result = Functions.UpdateOrderCustomer(order, user);

                            Plugin.CMS.Events.Trigger("OnOrderSetCustomer", new { Order = order, User = user });
                            break;
                        }
                    case Configurations.SetOrderStatus:
                        {
                            // Handle parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            var order = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            var isCredit = order.Items?.FirstOrDefault()?.Fields.FirstOrDefault(x => x.Field == "Type")?.Value;

                            // Trigger event to execute any logic before setting status.
                            if (isCredit != "Credit")
                                Plugin.CMS.Events.Trigger("OnOrderCartSettingStatus", parameters);

                            // Get other variables
                            var description = parameters.ContainsKey("Description") ? parameters["Description"] : null;
                            var data = parameters.ContainsKey("Data") ? parameters["Data"] : null;

                            // Handle status to be set
                            if (!parameters.ContainsKey("Status") || !Enum.TryParse(parameters["Status"], out OrderStatus status))
                                throw new Exception(Messages.ShoppingCartStatusMustBeProvided);

                            // Set customer status order
                            result = Functions.SetOrderStatus(order, status, description, data);
                            break;
                        }
                    case Configurations.SetOrderFields:
                        {
                            // Trigger event to execute any logic before setting fields.
                            Plugin.CMS.Events.Trigger("OnOrderCartSettingOrderFields", parameters);

                            // Handle parameters
                            var cartUuid = parameters.ContainsKey("Cart") ? parameters["Cart"] : null;
                            if (string.IsNullOrWhiteSpace(cartUuid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            var order = Functions.LoadOrder(cartUuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            // Load ordem and items fields lists
                            var orderFields = parameters.Where(p => p.Key.StartsWith("Order.")).ToList();

                            // Update order fields
                            Functions.UpdateOrderFields(order, orderFields);

                            result = order;
                            break;
                        }

                    /*
                        * Admin Methods. Must be validated if the user has admin access
                        **/
                    case Configurations.LoadUserOrders:
                        {
                            if (!CMS.Membership.IsAuthenticated && !CMS.Membership.IsAdminAuthenticated)
                                throw new Exception(Messages.UserNotAuthenticated);

                            // Get parameters to filter data
                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            // Convert status parameters filter
                            var status = new List<OrderStatus>();
                            if (parameters.ContainsKey("Status"))
                                foreach (var item in parameters["Status"].Split(','))
                                    if (Enum.TryParse(item, out OrderStatus statusRef))
                                        status.Add(statusRef);

                            // Convert search value
                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            // Get authenticated user
                            var user = CMS.Membership.User ?? CMS.Membership.AdminUser;

                            // Load orders from database
                            result = Functions.LoadAllOrders(page, size, status, search, null, null, user.Id);
                            break;
                        }

                    /*
                        * Admin Methods. Must be validated if the user has admin access
                        **/
                    case Configurations.LoadOrders:
                        {
                            EnsureUserIsAdministrator();

                            // Get parameters to filter data
                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            // Convert status parameters filter
                            var status = new List<OrderStatus>();
                            if (parameters.ContainsKey("Status"))
                                foreach (var item in parameters["Status"].Split(','))
                                    if (Enum.TryParse(item, out OrderStatus statusRef))
                                        status.Add(statusRef);

                            // Convert search value
                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            DateTime? startDate = null;
                            if (parameters.ContainsKey("StartDate") && DateTime.TryParse(parameters["StartDate"], out var startParsedDate))
                                startDate = startParsedDate;

                            DateTime? finalDate = null;
                            if (parameters.ContainsKey("FinalDate") && DateTime.TryParse(parameters["FinalDate"], out var finalParsedDate))
                                finalDate = finalParsedDate;

                            // Load orders from database
                            result = Functions.LoadAllOrders(page, size, status, search, startDate, finalDate);
                            break;
                        }
                    case Configurations.LoadStatistics:
                        {
                            EnsureUserIsAdministrator();

                            result = Functions.GetMetrics();
                            break;
                        }
                    case Configurations.DownloadExcel:
                        {
                            // Convert status parameters filter
                            var status = new List<OrderStatus>();
                            if (parameters.ContainsKey("Status"))
                                foreach (var item in parameters["Status"].Split(','))
                                    if (Enum.TryParse(item, out OrderStatus statusRef))
                                        status.Add(statusRef);

                            // Convert search value
                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            DateTime? startDate = null;
                            if (parameters.ContainsKey("StartDate") && DateTime.TryParse(parameters["StartDate"], out var startParsedDate))
                                startDate = startParsedDate;

                            DateTime? finalDate = null;
                            if (parameters.ContainsKey("FinalDate") && DateTime.TryParse(parameters["FinalDate"], out var finalParsedDate))
                                finalDate = finalParsedDate;

                            var file = Functions.ExportExcel(status, search, startDate, finalDate);

                            // Export to the system
                            var cd = new System.Net.Mime.ContentDisposition { FileName = "Pedidos.xlsx" };
                            HttpContext.Current.Response.AppendHeader("Content-Disposition", cd.ToString());

                            var stream = System.IO.File.Open(file, System.IO.FileMode.Open);
                            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        }
                    case Configurations.GenerateAbandonedCarts:
                        {
                            var abandonedTime = CMS.Configuration.Get(Configurations.TimeAbandonedCard, PluginName);
                            Functions.GenerateAbandonedCarts(abandonedTime);
                            break;
                        }
                    case Configurations.CalculateShipping:
                        {
                            var query = (parameters.ContainsKey("Query") ? parameters["Query"] : null);
                            result = Functions.CalculateShipping(query);
                            break;
                        }
                    #region Coupon

                    case Configurations.LoadCoupons:
                        {
                            // Get parameters to filter data
                            if (!parameters.ContainsKey("Page") || !int.TryParse(parameters["Page"], out int page))
                                page = 1;
                            if (!parameters.ContainsKey("Size") || !int.TryParse(parameters["Size"], out int size))
                                size = Configurations.PaginationSize;

                            // Convert status parameters filter
                            var status = new List<OrderStatus>();
                            if (parameters.ContainsKey("Status"))
                                foreach (var item in parameters["Status"].Split(','))
                                    if (Enum.TryParse(item, out OrderStatus statusRef))
                                        status.Add(statusRef);

                            // Convert search value
                            var search = (parameters.ContainsKey("Search") ? parameters["Search"] : null);

                            var coupons = Functions.LoadAllCoupons(page, size, search);
                            return coupons;
                        }

                    case Configurations.GetCoupon:
                        {
                            int id = (parameters.ContainsKey("Id") ? Convert.ToInt32(parameters["Id"]) : 0);

                            //if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            var Event = new Event();
                            var productTypeEvent = Bitzar.Products.Helpers.Functions.ListProductByType(1);
                            var productTypeCabin = Bitzar.Products.Helpers.Functions.ListProductByType(2);
                            var productTypeOccupancy = Bitzar.Products.Helpers.Functions.ListProductByType(3);

                            if (id != 0)
                            {
                                var b = Functions.GetCoupon(id);

                                Event.Id = b.Id;
                                Event.Code = b.Code;
                                Event.CouponUsages = b.CouponUsages;
                                Event.CouponType = b.CouponType;
                                Event.Description = b.Description;

                                Event.IdEvent = new Dictionary<int, string>();
                                foreach (var item in productTypeEvent)
                                {
                                    Event.IdEvent.Add(item.Id, item.SKU);
                                }

                                var cabinRelatedEvent = Bitzar.Products.Helpers.Functions.ListProductSub().Where(p => p.IdProduct == b.IdEvent).ToList();
                                Event.IdCabin = new Dictionary<int, string>();
                                foreach (var item in cabinRelatedEvent)
                                {
                                    Event.IdCabin.Add(item.IdSubProduct, productTypeCabin.Where(p => p.Id == item.SubProduct.Id).FirstOrDefault().SKU);
                                }

                                var occupancyRelatedCabin = Bitzar.Products.Helpers.Functions.ListProductSub().Where(p => p.IdProduct == b.IdCabin).ToList();
                                Event.IdOccupancy = new Dictionary<int, string>();
                                foreach (var item in occupancyRelatedCabin)
                                {
                                    Event.IdOccupancy.Add(item.IdSubProduct, productTypeOccupancy.Where(p => p.Id == item.SubProduct.Id).FirstOrDefault().SKU);
                                }

                                Event.StartDate = b.StartDate;
                                Event.EndDate = b.EndDate;
                                Event.UsageLimit = b.UsageLimit;
                                Event.DiscountAmount = b.DiscountAmount;
                                Event.IdEventSelected = b.IdEvent;
                                Event.IdCabinSelected = b.IdCabin;
                                Event.IdOccupancySelected = b.IdOccupancy;
                            }
                            else
                            {
                                Event.IdEvent = new Dictionary<int, string>();
                                foreach (var item in productTypeEvent)
                                {
                                    Event.IdEvent.Add(item.Id, item.SKU);
                                }

                            }
                            
                            return Event;
                        }
                    case Configurations.SaveCoupon:
                        {
                            int id = (parameters.ContainsKey("Id") ? Convert.ToInt32(parameters["Id"]) : 0);

                            //, code, usageLimit, description, startDate, endDate, couponType, discountType, discountAmount

                            if (!parameters.TryGetValue("Code", out string code) || string.IsNullOrWhiteSpace(code))
                                throw new ArgumentException("O 'Código' do Cupom deve ser informado!");

                            if (!parameters.ContainsKey("UsageLimit") || !int.TryParse(parameters["UsageLimit"], out int usageLimit))
                                throw new ArgumentException("O 'Limite de utilização' do Cupom deve ser informado!");

                            if (!parameters.TryGetValue("Description", out string description) || string.IsNullOrWhiteSpace(description))
                                throw new ArgumentException("A 'Descrição' do Cupom deve ser informado!");

                            if (!parameters.ContainsKey("Event") || !int.TryParse(parameters["Event"], out int idEvent))
                                throw new ArgumentException("O evento deve ser informado!");

                            if (!parameters.ContainsKey("Cabin") || !int.TryParse(parameters["Cabin"], out int idCabin))
                                throw new ArgumentException("A cabine deve ser informada!");

                            if (!parameters.ContainsKey("Occupacity") || !int.TryParse(parameters["Occupacity"], out int idOccupancy))
                                throw new ArgumentException("A ocupação deve ser informada!");

                            if (!parameters.ContainsKey("StartDate") || !DateTime.TryParse(parameters["StartDate"], out DateTime startDate))
                                throw new ArgumentException("A 'Data de Inicio' do Cupom deve ser informada!");

                            if (!parameters.ContainsKey("EndDate") || !DateTime.TryParse(parameters["EndDate"], out DateTime endDate))
                                throw new ArgumentException("A 'Data de Término' do Cupom deve ser informada!");

                            if (!parameters.ContainsKey("CouponType") || !Enum.TryParse(parameters["CouponType"], out CouponType couponType))
                                throw new ArgumentException("O 'Tipo' do Cupom deve ser informado!");

                            if (!parameters.ContainsKey("DiscountType") || !Enum.TryParse(parameters["DiscountType"], out DiscountType discountType))
                                throw new ArgumentException("O 'Tipo do Desconto' deve ser informado!");

                            if (!parameters.ContainsKey("DiscountAmount") || !decimal.TryParse(parameters["DiscountAmount"], out decimal discountAmount))
                                throw new ArgumentException("O Valor do Desconto deve ser informado!");

                            bool.TryParse(parameters["Disabled"], out bool disabled);

                            // Call method to save category
                            return Functions.SaveCoupon(id, code, usageLimit, description, idEvent, idCabin, idOccupancy, startDate, endDate, couponType, discountType, discountAmount, disabled);
                        }
                    case Configurations.DeleteCoupon:
                        {
                            int id = (parameters.ContainsKey("Id") ? Convert.ToInt32(parameters["Id"]) : 0);

                            if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            return Functions.DeleteCoupon(id);
                        }

                    case Configurations.UseCoupon:
                        {
                            bool validate = false;

                            if (!parameters.TryGetValue("Cart", out string cartUUid) || string.IsNullOrWhiteSpace(cartUUid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            if (!parameters.TryGetValue("Code", out string couponCode) || string.IsNullOrWhiteSpace(couponCode))
                                throw new ArgumentException("O 'Código' do Cupom deve ser informado!");

                            var coupon = Functions.GetCouponByCode(couponCode);

                            if (coupon == null)
                                throw new ArgumentException("O Cupom informado é inválido!");

                            if (!Functions.CanUseCouponByCode(couponCode))
                                throw new ArgumentException("O Cupom informado não esta mais disponível para uso.");


                            //Get Event parameters and validate
                            if (!parameters.ContainsKey("Event") || !int.TryParse(parameters["Event"], out int idEvent))
                                throw new ArgumentException("O evento deve ser informado!");

                            if (idEvent == coupon.IdEvent || idEvent == 0)
                                validate = true;
                            else
                                throw new ArgumentException("Cupom não permitido neste evento!");

                            if (!parameters.ContainsKey("Cabin") || !int.TryParse(parameters["Cabin"], out int idCabin))
                                throw new ArgumentException("A cabine deve ser informada!");
                            if (idCabin == coupon.IdCabin || idCabin == 0)
                                validate = true;
                            else
                                throw new ArgumentException("Cupom não permitido nesta cabine!");

                            if (!parameters.ContainsKey("Ocupation") || !int.TryParse(parameters["Ocupation"], out int idOccupancy))
                                throw new ArgumentException("A ocupação deve ser informada!");
                            if (idOccupancy == coupon.IdOccupancy || idOccupancy == 0)
                                validate = true;
                            else
                                throw new ArgumentException("Cupom não permitido nesta ocupação!");


                            //Se cupom está autorizado para Evento, cabine e ocupação
                            if (validate)
                            {
                                // Get cart parameters
                                var cart = Functions.ShowCart(cartUUid);

                                decimal price = 0;

                                var couponDescription = string.Empty;

                                if (coupon.CouponType == CouponType.Product)
                                {
                                    var events = cart.Items.Where(x => x.Fields.Any(y => y.Field == "Type" && y.Value == "Event"));
                                    var transfers = cart.Items.Where(x => x.Fields.Any(y => y.Field == "Type" && y.Value == "Product"));

                                    var totalProducts = events.Sum(x => decimal.Parse(x.Fields.FirstOrDefault(y => y.Field == "PriceSection").Value)) + transfers.Sum(x => x.Price);

                                    if (coupon.DiscountType == DiscountType.Fixed)
                                    {
                                        price = coupon.DiscountAmount;
                                        couponDescription = "R$ " + coupon.DiscountAmount.ToString("0.00");
                                    }
                                    else
                                    {
                                        price = (totalProducts * (coupon.DiscountAmount / 100));
                                        couponDescription = coupon.DiscountAmount + "%";
                                    }

                                    couponDescription += " de desconto nos Produtos";
                                }
                                else if (coupon.CouponType == CouponType.ProductAndFee)
                                {
                                    if (coupon.DiscountType == DiscountType.Percentage)
                                    {
                                        price = (cart.TotalItems * (coupon.DiscountAmount / 100));
                                        couponDescription = coupon.DiscountAmount + "%";
                                    }
                                    else
                                    {
                                        price = coupon.DiscountAmount;
                                        couponDescription = "R$ " + coupon.DiscountAmount.ToString("0.00");
                                    }

                                    couponDescription += " de desconto nos Produtos e Taxas";
                                }

                                price *= -1;

                                var couponParameters = new Dictionary<string, string>
                            {
                                { "Cart", cartUUid },
                                { "Product", "9999" },
                                { "Quantity", "1" },
                                { "Price", price.ToString("0.00").Replace(".", "").Replace(",", ".") },
                                { "Validator", "-" },
                                { "App", "true" },
                                { "Item.Type", "Coupon" },
                                { "Item.CouponId", coupon.Id.ToString() },
                                { "Item.PriceProduct", price.ToString("0.00") },
                                { "Item.ProductName", "CUPOM - " + couponCode },
                                { "Item.ProductDescription", couponDescription }
                            };

                                result = this.Execute("AddOrUpdateCartItem", token, couponParameters);

                                //Add coupon usage
                                Functions.AddCouponUsageByCode(cart.Id, CMS.Membership.User.Id, coupon.Code, coupon.DiscountAmount);
                            }

                            break;
                        }
                         
                    case Configurations.RemoveCoupon:
                        {
                            if (!parameters.TryGetValue("Cart", out string cartUUid) || string.IsNullOrWhiteSpace(cartUUid))
                                throw new Exception(Messages.ShoppingCartUuidMustBeProvided);

                            var couponParameters = new Dictionary<string, string>
                            {
                                { "Cart", cartUUid },
                                { "Product", "9999" }
                            };

                            if (!couponParameters.ContainsKey("Product") || !int.TryParse(couponParameters["Product"], out int product))
                                throw new ArgumentException(Messages.ProductIdNotFound);

                            // Trigger event to execute any logic before delete item.
                            Plugin.CMS.Events.Trigger("OnOrderCartDeletingItem", couponParameters);

                            // Load order to process the item
                            var order = Functions.LoadOrder(cartUUid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                            // Delete cartItem order fields
                            result = Functions.DeleteCartItem(order, product);

                            break;
                        }
                    case Configurations.GetCabin:
                        {
                            int id = (parameters.ContainsKey("Id") ? Convert.ToInt32(parameters["Id"]) : 0);
                            int cabinSelected = (parameters.ContainsKey("cabinSelected") ? Convert.ToInt32(parameters["cabinSelected"]) : 0);

                            var productTypeCabin = Bitzar.Products.Helpers.Functions.ListProductByType(2);

                            var cabonRelatedEvent = Bitzar.Products.Helpers.Functions.ListProductSub().Where(p => p.IdProduct == id).ToList();

                            var cabinList = new List<Cabin>();
                            foreach (var item in cabonRelatedEvent)
                            {
                                var cabin = new Cabin()
                                {
                                    IdCabin = item.IdSubProduct,
                                    NameCabin = productTypeCabin.Where(p => p.Id == item.SubProduct.Id).FirstOrDefault().SKU,
                                    Selected = (item.IdSubProduct == cabinSelected)
                                };

                                cabinList.Add(cabin);
                            }

                            result = cabinList;

                            break;
                        }
                    case Configurations.GetOccupancy:
                        {
                            int id = (parameters.ContainsKey("Id") ? Convert.ToInt32(parameters["Id"]) : 0);
                            int OccupancySelected = (parameters.ContainsKey("OccupancySelected") ? Convert.ToInt32(parameters["OccupancySelected"]) : 0);

                            var productTypeCabin = Bitzar.Products.Helpers.Functions.ListProductByType(3);

                            var occupancyRelatedCabin = Bitzar.Products.Helpers.Functions.ListProductSub().Where(p => p.IdProduct == id).ToList();

                            var occupancyList = new List<Occupancy>();
                            foreach (var item in occupancyRelatedCabin)
                            {
                                var occupancy = new Occupancy()
                                {
                                    IdOccupancy = item.IdSubProduct,
                                    NameOccupancy = productTypeCabin.Where(p => p.Id == item.SubProduct.Id).FirstOrDefault().SKU,
                                    Selected = (item.IdSubProduct == OccupancySelected)
                                };

                                occupancyList.Add(occupancy);
                            }

                            result = occupancyList;

                            break;
                        }

                    #endregion
                    default:
                        throw new Exception(Messages.CommandNotFound);
                }

                // Trace request to the service
                if (CMS.Configuration.Get(Configurations.EnableTraceFlag, PluginName).Contains("true"))
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
                Name = "E-Commerce",
                Items = new List<IMenuItem>
                {
                    new MenuItem() { Title = "Pedidos", Function = "Orders", Icon = "wb-order" },
                    new MenuItem() { Title = "Cupons", Function = "Coupons", Icon = "wb-order" }
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
                case "OnPaymentCreate":
                    var callbackCreate = (dynamic)data;
                    string referenceIdCreate = callbackCreate.referenceId;
                    var orderCreate = Functions.LoadOrder(referenceIdCreate);
                    Functions.SetOrderStatus(orderCreate, OrderStatus.AwaitingApproval);
                    var coupon = orderCreate.Items.FirstOrDefault(x => x.IdProduct.Equals("9999"));
                    if (coupon != null)
                        Functions.AddCouponUsageByCode(orderCreate.Id, (int)orderCreate.IdCustomer, coupon.Fields.FirstOrDefault(x => x.Field == "CouponId").Value, coupon.Price * -1);
                    break;
                case "OnPaymentSucceeded":
                    var callbackSucceeded = (dynamic)data;
                    string referenceIdSucceeded = callbackSucceeded.referenceId;
                    var orderSucceeded = Functions.LoadOrder(referenceIdSucceeded);
                    Functions.SetOrderStatus(orderSucceeded, OrderStatus.Approved);
                    break;
                case "OnPaymentFailed":
                    var callbackFailed = (dynamic)data;
                    string referenceIdFailed = callbackFailed.referenceId;
                    var orderFailed = Functions.LoadOrder(referenceIdFailed);
                    Functions.SetOrderStatus(orderFailed, OrderStatus.Archived);
                    break;
                case "OnPaymentCanceled":
                    var callbackCanceled = (dynamic)data;
                    string referenceIdCanceled = callbackCanceled.referenceId;
                    var orderCanceled = Functions.LoadOrder(referenceIdCanceled);
                    Functions.SetOrderStatus(orderCanceled, OrderStatus.Canceled);
                    break;
            }
            return;
        }
    }
}
