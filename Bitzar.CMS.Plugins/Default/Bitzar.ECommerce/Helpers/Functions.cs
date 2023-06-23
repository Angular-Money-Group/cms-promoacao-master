using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.ECommerce.Interfaces;
using Bitzar.ECommerce.Models;
using MPayment = Bitzar.ECommerce.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using static Bitzar.ECommerce.Helpers.Enumerators;
using Database = Bitzar.ECommerce.Models.Database;
using Metric = Bitzar.ECommerce.Models.Metric;

namespace Bitzar.ECommerce.Helpers
{
    internal class Functions
    {
        #region Internal methods to help plugin structure
        /// <summary>
        /// Get Authenticated User
        /// </summary>
        /// <returns></returns>
        private static User GetAuthenticatedUser()
        {
            if (Plugin.CMS.Membership.IsAuthenticated)
                return Plugin.CMS.Membership.User;

            return null;
        }

        /// <summary>
        /// Get Admin Authenticated User
        /// </summary>
        /// <returns></returns>
        private static User GetAdminAuthenticatedUser()
        {
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
                LogException(e, objects: action);
            }
        }

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
        /// Method to create all the fields in the system as Custom objects
        /// </summary>
        /// <param name="userFields"></param>
        /// <returns></returns>
        internal static ExpandoObject DynamicFields(ICollection<IField> userFields)
        {
            var expando = new ExpandoObject() as IDictionary<string, Object>;
            foreach (var field in userFields)
                expando.Add(field.Field.Replace(" ", ""), field.Value);

            return expando as ExpandoObject;
        }
        #endregion

        /// <summary>
        /// Method to load the order data from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Order LoadOrder(string uuid)
        {
            try
            {
                using (var db = new Database())
                {
                    var order = db.Orders
                        .Include(f => f.Fields)
                        .Include(f => f.Items)
                        .Include(f => f.Items.Select(c => c.Fields))
                        .AsNoTracking()
                        .FirstOrDefault(o => o.Uuid == uuid);

                    // Bind customer data
                    if (order.IdCustomer != null)
                    {
                        // Load all users
                        var users = Plugin.CMS.Membership.Members();

                        // Find the right user and set the customer
                        var customer = users.FirstOrDefault(u => u.Id == order.IdCustomer.Value);
                        if (customer != null)
                            order.Customer = $"{customer.FirstName} {customer.LastName}".Trim();
                    }

                    return order;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: uuid);
                throw;
            }
        }


        string[] GetFields(IEnumerable<dynamic> model)
        {
            if (model == null || model.Count() == 0)
                return new string[] { };

            var fields = (IEnumerable<dynamic>)model.SelectMany(m => ((IEnumerable<dynamic>)m.Fields).Where(f => !f.Hidden).Select(f => f));
            if (fields == null || fields.Count() == 0)
                return new string[] { };

            return fields.OrderBy(f => f.Id).ThenBy(f => f.Field).Select(f => (string)f.Field).Distinct().ToArray();
        }

        /// <summary>
        /// Translate order status
        /// </summary>
        /// <param name="status">order status</param>
        /// <returns></returns>
        static string TranslateStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "sem status";

            switch (status.ToString())
            {

                case "Cart":
                    return "Carrinho";
                case "Order":
                    return "Pedido";
                case "Abandoned":
                    return "Abandonado";
                case "Approved":
                    return "Aprovado";
                case "AwaitingApproval":
                    return "Ag. Aprovação";
                case "Completed":
                    return "Concluído";
                case "Deleted":
                    return "C. Manualmente";
                case "InTransfer":
                    return "Em Transferência";
                case "Sended":
                    return "Enviado";
                case "Canceled":
                    return "Cancelado";
                case "Archived":
                    return "Arquivado";
                default:
                    return "Sem Status";
            }
        }

        /// <summary>
        /// Load Orders to Export
        /// </summary>
        /// <returns></returns>
        internal static DataTable LoadOrdersToExport(List<OrderStatus> status = null, string search = null, DateTime? startDate = null, DateTime? finalDate = null)
        {
            try
            {
                // Get orders to be exported
                var orders = LoadAllOrders(1, int.MaxValue, status, search, startDate, finalDate).Records;

                // Return fields from Orders
                var fields = orders.SelectMany(m => m.Fields.Select(f => f.Field)).Distinct().ToList();
                var fieldItems = orders.SelectMany(m => m.Items.SelectMany(x => x.Fields.Select(f => f.Field))).Distinct().ToList();

                var fieldList = fields.Union(fieldItems);

                // Create DataTable
                var dataTable = new DataTable();

                // Add Columns from Order
                dataTable.Columns.Add(nameof(Order.Id));
                dataTable.Columns.Add("Cliente");
                dataTable.Columns.Add("ID Referência");
                dataTable.Columns.Add("Criado Em");
                dataTable.Columns.Add(nameof(Order.Status));
                dataTable.Columns.Add("Total", typeof(decimal));

                foreach (var newColum in fieldList)
                    dataTable.Columns.Add(newColum);

                // Add row data
                foreach (var order in orders)
                    foreach (var orderDetail in order.Items)
                    {
                        //Defealt rows
                        var row = dataTable.NewRow();

                        row[nameof(Order.Id)] = order.Id;
                        row["Cliente"] = order.Customer;
                        row["ID Referência"] = order.Uuid;
                        row["Criado Em"] = order.CreatedAt;
                        row[nameof(Order.Status)] = TranslateStatus(order.Status.ToString());
                        row["Total"] = order.TotalOrder;

                        foreach (var field in fields)
                        {
                            var fieldValue = (string)order.Fields.FirstOrDefault(f => f.Field == field)?.Value ?? null;
                            row[field] = !string.IsNullOrEmpty(fieldValue) ? fieldValue : "-";
                        }

                        foreach (var field in fieldItems)
                        {
                            var fieldValue = (string)orderDetail.Fields.FirstOrDefault(f => f.Field == field)?.Value ?? null;
                            row[field] = !string.IsNullOrEmpty(fieldValue) ? fieldValue : "-";
                        }

                        dataTable.Rows.Add(row);
                    }

                return dataTable;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Method to export orders
        /// </summary>
        /// <returns></returns>
        public static string ExportExcel(List<OrderStatus> status = null, string search = null, DateTime? startDate = null, DateTime? finalDate = null)
        {
            try
            {
                var Allorders = LoadOrdersToExport(status, search, startDate, finalDate);
                //DataTable orders = ConvertToDataTable(order);

                var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Pedidos");

                sheet.Cell(1, 1).InsertTable(Allorders, true);
                sheet.Columns().AdjustToContents();

                // Generate the stream to save the excel file
                var file = Path.GetTempFileName();
                file = file.Replace(".tmp", ".xlsx");

                workbook.SaveAs(file);

                return file;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Method to load the order detail information from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<OrderDetail> LoadOrderDetail(Order order)
        {
            try
            {
                // Load order detail information
                using (var db = new Database())
                    return db.OrderDetails.Include(f => f.Fields).Where(o => o.IdOrder == order.Id).ToList();
            }
            catch (Exception e)
            {
                LogException(e, objects: order);
                throw;
            }
        }

        /// <summary>
        /// Method to check if the order is opened to allow edition or not
        /// </summary>
        /// <param name="order">Order instance to validate if it's ok</param>
        public static void EnsureCanEditOrder(Order order)
        {
            if (order == null)
                return;

            if (order.Status == OrderStatus.Cart)
                return;

            throw new InvalidOperationException(Messages.OrderIsNotInOpenedState);
        }

        /// <summary>
        /// Method that will remove an item from shopping cart
        /// </summary>
        /// <param name="order">Order reference to delete item.</param>
        /// <param name="product">Product Id to be removed from Order</param>
        /// <returns>Returns the instance of the Order without removed object</returns>
        internal static Order DeleteCartItem(Order order, int product)
        {
            try
            {
                EnsureCanEditOrder(order);

                // Lookup for the item instance to be removed
                var item = order.Items.FirstOrDefault(i => i.IdProduct == product);
                if (item == null)
                    return order;

                using (var db = new Database())
                {
                    // Remove item from database
                    var itemADeletar = db.OrderDetails.Include(c => c.Fields).FirstOrDefault(c => c.Id == item.Id);
                    Plugin.CMS.Events.Trigger("OnOrderCartDeleteItem", itemADeletar);

                    db.OrderDetails.Remove(itemADeletar);
                    db.SaveChanges();
                }

                // Remove item from order
                order.Items.Remove(item);
                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { order, product });
                throw;
            }
        }

        /// <summary>
        /// Method to return the entire cart shopping
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        internal static Order ShowCart(string uuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uuid))
                    return null;

                // Load order data
                var order = LoadOrder(uuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                // Load ordem item data
                var detail = LoadOrderDetail(order);

                // Return order and items
                order.Items = detail;
                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: uuid);
                throw;
            }
        }

        /// <summary>
        /// Method to return the confirmation order data
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        internal static Order ShowConfirmation(string uuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uuid))
                    return null;

                // Load order data
                var order = LoadOrder(uuid) ?? throw new Exception(Messages.ShoppingCartNotFound);

                // Load ordem item data
                var detail = LoadOrderDetail(order);

                // Return order and items
                order.Items = detail;

                // Load order payment data
                var payments = LoadPayments(order.Id);

                // Return order payment
                order.Payments = payments.OrderByDescending(x => x.Id).ToList();

                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: uuid);
                throw;
            }
        }

        /// <summary>
        /// Method to load the order detail information from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<MPayment.OrderPayment> LoadPayments(int idOrder)
        {
            try
            {
                // Load payment information
                using (var db = new Database())
                    return db.OrderPayments.Where(x => x.IdOrder == idOrder).ToList();
            }
            catch (Exception e)
            {
                LogException(e, objects: idOrder);
                throw;
            }
        }

        /// <summary>
        /// Functions to load the cart data to show in the admin panel
        /// </summary>
        /// <returns></returns>
        internal static PaggedResult<Order> LoadAllOrders(int page = 1, int size = Configurations.PaginationSize, List<OrderStatus> status = null, string search = null, DateTime? startDate = null, DateTime? finalDate = null, int? user = null)
        {
            try
            {

                using (var db = new Database())
                {
                    // Create query object defining the default parameters;
                    var query = db.Orders.AsNoTracking()
                                  .Include(o => o.Items)
                                  .Include(o => o.Fields)
                                  .Include("Items.Fields")
                                  .AsQueryable();

                    // Filter user if has been provided
                    if (user.HasValue)
                        query = query.Where(q => q.IdCustomer == user.Value);

                    // Add status filter
                    if (status != null && status.Any())
                        query = query.Where(q => status.Any(s => s == q.Status));

                    // Add date filter
                    if (startDate != null)
                        query = query.Where(q => q.CreatedAt >= startDate);

                    if (finalDate != null)
                        query = query.Where(q => q.CreatedAt <= finalDate);

                    // Filter users
                    var users = Plugin.CMS.Membership.Members();

                    // Add search filter information
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        var customers = users.Where(u => $"{u.FirstName?.ToLower()} {u.LastName?.ToLower()}".Trim().Contains(search.ToLower())).Select(u => u.Id).ToArray();

                        if (int.TryParse(search, out int idSearch))
                        {
                            query = (from o in query
                                     where (o.Id == idSearch) ||
                                           (o.Uuid == search) ||
                                           (customers.Any(c => c == o.IdCustomer)) ||
                                           (o.Fields.Any(x => x.Value.Contains(search)))
                                     select o);
                        }
                        else
                        {
                            query = (from o in query
                                     where (o.Uuid == search) ||
                                           (customers.Any(c => c == o.IdCustomer)) ||
                                           (o.Fields.Any(x => x.Value.Contains(search)))
                                     select o);
                        }
                    }

                    // execute count
                    var count = query.Count();

                    // Add query pagination and sort
                    query = query.OrderByDescending(o => o.CreatedAt)
                                 .ThenByDescending(o => o.Id)
                                 .Skip((page - 1) * size).Take(size);

                    // Return object from database
                    var orders = query.Include(o => o.Items)
                                      .Include(o => o.Fields)
                                      .Include("Items.Fields").ToList();

                    // Bind customer data
                    foreach (var order in orders.Where(o => o.IdCustomer != null))
                    {
                        var customer = users.FirstOrDefault(u => u.Id == order.IdCustomer.Value);
                        if (customer == null)
                            continue;

                        order.Customer = $"{customer.FirstName} {customer.LastName}".Trim();
                    }

                    return new PaggedResult<Order>
                    {
                        Records = orders,
                        Page = page,
                        Size = size,
                        Count = count,
                        CountPage = Convert.ToInt32(Math.Ceiling(count / (decimal)size))
                    };
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { page, size, status, search });
                throw;
            }
        }

        /// <summary>
        /// Method to update the order state.
        /// </summary>
        /// <param name="order">Order reference to be updated</param>
        /// <param name="status">Status to be defined in the order</param>
        /// <param name="description">Description to be recorded in the order state</param>
        /// <param name="data">Additional internal data to be recorded</param>
        /// <returns></returns>
        internal static Order SetOrderStatus(Order order, OrderStatus status, string description = null, string data = null)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Orders.Attach(order);
                    order.Items = db.OrderDetails.Include(f => f.Fields).Where(o => o.IdOrder == order.Id).ToList();

                    // Update the order state
                    if (order.Status != status)
                        order.Status = status;

                    // Trigger Event for Canceling Order
                    Plugin.CMS.Events.Trigger($"OnOrder{status.ToString()}", order);

                    // Create the order history entry
                    var history = new OrderHistory
                    {
                        IdOrder = order.Id,
                        Status = status,
                        Description = description,
                        Data = data
                    };

                    // Add it and apply changes in the database
                    db.History.Add(history);
                    db.SaveChanges();
                }

                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { order, status, description, data });
                throw;
            }
        }

        /// <summary>
        /// Method to update the order customer in the database
        /// </summary>
        /// <param name="order">Order reference to be updated</param>
        /// <param name="user">User reference to be set in the order</param>
        /// <returns>Returns the order refence</returns>
        internal static Order UpdateOrderCustomer(Order order, User user)
        {
            try
            {
                using (var db = new Database())
                {
                    db.Orders.Attach(order);

                    // Find item and update it
                    order.IdCustomer = user.Id;
                    order.Customer = $"{user.FirstName} {user.LastName}".Trim();

                    db.SaveChanges();
                }

                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { order, user });
                throw;
            }
        }

        /// <summary>
        /// Method to create an order to the current user
        /// </summary>
        /// <returns>Returns the order instance</returns>
        public static Order CreateOrder()
        {
            try
            {
                var customer = GetAuthenticatedUser();
                using (var db = new Database())
                {
                    /// Create the order instance binding the customer id if it's logged
                    var order = new Order();
                    if (customer != null)
                        order.IdCustomer = customer.Id;

                    // Add the new order in the database
                    db.Orders.Add(order);

                    // Apply changes
                    db.SaveChanges();

                    // Update the history
                    order = SetOrderStatus(order, order.Status, "Carrinho criado.");

                    return order;
                }
            }
            catch (Exception e)
            {
                LogException(e);
                throw;
            }
        }

        /// <summary>
        /// Method to add or update an order item in the current cart
        /// </summary>
        /// <param name="uuid">Uuid of the cart to be handled</param>
        /// <param name="idProduct">Identification of the product to be added in the cart</param>
        /// <param name="quantity">Quantity information</param>
        /// <param name="price">Current selected price</param>
        /// <param name="fields">Custom fields to process the request</param>
        /// <returns>Returns the instance of the object</returns>
        public static Order AddOrUpdateOrder(Order order, int idProduct, decimal quantity, decimal price, List<KeyValuePair<string, string>> fields = null)
        {
            try
            {
                // Load order to check if it's able to be edited
                EnsureCanEditOrder(order);

                // Load order detail
                var details = LoadOrderDetail(order);

                // Get the current order item to check if it exists
                var item = details.FirstOrDefault(d => d.IdProduct == idProduct);
                var exists = item != null;

                // Create a new item instance in case of null
                item = item ?? new OrderDetail()
                {
                    IdOrder = order.Id,
                    IdProduct = idProduct
                };

                // Update values of the new item instance
                item.Quantity = quantity;
                item.Price = price;

                // Update the item if it already exists
                using (var db = new Database())
                {
                    // Attach item to the context and set state
                    db.OrderDetails.Attach(item);
                    db.Entry(item).State = (exists ? EntityState.Modified : EntityState.Added);

                    // Apply Changes
                    db.SaveChanges();
                }

                // Update item fields
                UpdateItemFields(item, fields);

                // Bind order items in order to return
                if (!exists)
                    details.Add(item);
                order.Items = details;

                return order;
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { order, idProduct, quantity, price, fields });
                throw;
            }
        }

        /// <summary>
        /// Internal method to update all the order fields
        /// </summary>
        /// <param name="uuid">Uuid to identify the order</param>
        /// <param name="fields">Field list to be updated</param>
        public static void UpdateOrderFields(Order order, List<KeyValuePair<string, string>> fields = null)
        {
            if (fields == null || fields.Count == 0)
                return;

            try
            {
                // Load order to check if it's able to be edited
                EnsureCanEditOrder(order);

                // Update field order
                using (var db = new Database())
                {
                    // Clear all the fields for that order
                    var orderFields = db.OrderFields.Where(o => o.IdOrder == order.Id).ToList();

                    // Apply deletion on the database
                    db.SaveChanges();

                    // Add new order fields items
                    foreach (var item in fields)
                    {
                        var hidden = item.Key.StartsWith("Order.Hidden.");
                        var fieldName = item.Key.Replace("Order.Hidden.", "").Replace("Order.", "");
                        var fieldValue = item.Value;

                        // Check if exists and should be updated or inserted
                        var orderField = orderFields.FirstOrDefault(o => o.Field == fieldName);
                        if (orderField != null)
                            orderField.Value = fieldValue;
                        else
                        {
                            db.OrderFields.Add(new OrderField()
                            {
                                IdOrder = order.Id,
                                Field = fieldName,
                                Value = fieldValue,
                                Hidden = hidden
                            });
                        }
                    }

                    // Apply changes on the database
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { order, fields });
                throw;
            }
        }

        /// <summary>
        /// Method to update the fields of an item in the order
        /// </summary>
        /// <param name="OrderDetail">OrderDetail instance to be handled</param>
        /// <param name="fields">Fields to be updated in the product</param>
        public static void UpdateItemFields(OrderDetail product, List<KeyValuePair<string, string>> fields = null)
        {
            if (fields == null || fields.Count == 0)
                return;

            product.Fields.Clear();

            try
            {
                // Update field order
                using (var db = new Database())
                {
                    // Clear all the fields for that order
                    var itemFields = db.OrderDetailFields.Where(o => o.IdOrderDetail == product.Id).ToList();

                    // Apply deletion on the database
                    db.SaveChanges();

                    // Add new order fields items
                    foreach (var item in fields)
                    {
                        var hidden = item.Key.StartsWith("Item.Hidden.");
                        var fieldName = item.Key.Replace("Item.Hidden.", "").Replace("Item.", "");
                        var fieldValue = item.Value;

                        // Check if exists and should be updated or inserted
                        var itemField = itemFields.FirstOrDefault(o => o.Field == fieldName);
                        if (itemField != null)
                        {
                            itemField.Value = fieldValue;
                            product.Fields.Add(itemField);
                        }
                        else
                        {
                            // Create field
                            var field = new OrderDetailField()
                            {
                                IdOrderDetail = product.Id,
                                Field = fieldName,
                                Value = fieldValue,
                                Hidden = hidden
                            };

                            db.OrderDetailFields.Add(field);
                            product.Fields.Add(field);
                        }
                    }

                    // Apply changes on the database
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { product, fields });
                throw;
            }

        }

        internal static List<Metric> GetMetrics()
        {
            try
            {
                using (var db = new Database())
                {
                    var concluido = db.Orders.Where(f => f.Status == OrderStatus.Completed).Count();
                    var cancelado = db.Orders.Where(f => f.Status == OrderStatus.Canceled).Count();
                    var carrinho = db.Orders.Where(f => f.Status == OrderStatus.Cart).Count();

                    var list = new List<Metric>
                    {
                        new Metric()
                        {
                            Title = "Concluído",
                            Value = concluido.ToString(),
                            Color = "#46be8a",
                            Status = OrderStatus.Completed
                        },
                        new Metric()
                        {
                            Title = "Cancelado",
                            Value = cancelado.ToString(),
                            Color = "#f96868",
                            Status = OrderStatus.Canceled
                        },
                        new Metric()
                        {
                            Title = "Carrinho",
                            Value = carrinho.ToString(),
                            Color = "#f2a654",
                            Status = OrderStatus.Cart
                        }
                    };

                    return list;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Generate abandoned carts based on creation time
        /// </summary>
        /// <returns></returns>
        public static void GenerateAbandonedCarts(string abandonedTime)
        {

            using (var db = new Database())
            {
                var listOrder = db.Orders.Where(o => o.Status == OrderStatus.Cart).AsNoTracking().ToList();

                foreach (var order in listOrder.Where(o => o.CreatedAt.AddMinutes(Convert.ToInt32(abandonedTime)) < DateTime.Now))
                {
                    SetOrderStatus(order, OrderStatus.Abandoned);
                }

            }
        }

        public static string CalculateShipping(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new InvalidOperationException("Query inválida para envio.");
            }

            var url = "http://ws.correios.com.br/calculador/CalcPrecoPrazo.aspx";
            var client = new HttpClient();

            //returns the acesstoken and JSON
            var response = client.GetAsync($"{url}?{query}").Result;

            string result = response.Content.ReadAsStringAsync().Result;

            return result;
        }


        /// <summary>
        /// Method to get all the coupons available in the systema
        /// </summary>
        /// <returns>Returns a list with all coupons available</returns>
        /// 
        internal static PaggedResult<Coupon> LoadAllCoupons(int page = 1, int size = Configurations.PaginationSize, string search = null)
        {
            try
            {

                using (var db = new Database())
                {
                    // Create query object defining the default parameters;
                    var query = db.Coupons.AsNoTracking()
                                  .AsQueryable();

                    // Add search filter information
                    if (!string.IsNullOrWhiteSpace(search))
                    {

                        query = (from o in query
                                 where (o.Description.Contains(search))
                                 select o);
                    }

                    // execute count
                    var count = query.Count();

                    // Add query pagination and sort
                    query = query.OrderByDescending(o => o.CreatedAt)
                                 .ThenByDescending(o => o.Id)
                                 .Skip((page - 1) * size).Take(size);

                    // Return object from database
                    var coupons = query.ToList();

                    return new PaggedResult<Coupon>
                    {
                        Records = coupons,
                        Page = page,
                        Size = size,
                        Count = count,
                        CountPage = Convert.ToInt32(Math.Ceiling(count / (decimal)size))
                    };
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { page, size, search });
                throw;
            }
        }

        /// <summary>
        /// Method to get an Specific Coupon in the system
        /// </summary>
        /// <param name="id">Identification of the Coupon</param>
        /// <returns></returns>
        internal static Coupon GetCoupon(int couponId)
        {
            try
            {
                // Load order detail information
                using (var db = new Database())
                    return db.Coupons.FirstOrDefault(o => o.Id == couponId);
            }
            catch (Exception e)
            {
                LogException(e, objects: couponId);
                throw;
            }
        }


        /// <summary>
        /// Method to get an Specific Coupon in the system
        /// </summary>
        /// <param name="code">Code of the Coupon</param>
        /// <returns></returns>
        internal static Coupon GetCouponByCode(string code)
        {
            try
            {
                // Load order detail information
                using (var db = new Database())
                    return db.Coupons.FirstOrDefault(o => o.Code == code);
            }
            catch (Exception e)
            {
                LogException(e, objects: code);
                throw;
            }
        }

        /// <summary>
        /// Method to get an Specific Coupon in the system
        /// </summary>
        /// <param name="orderId">Identification of Order</param>
        /// <param name="customerId">Identification of Customer</param>
        /// <param name="code">Code of the Coupon</param>
        /// <param name="discountAmount">Discount Ammount</param>
        /// <returns></returns>
        internal static void AddCouponUsageByCode(int orderId, int customerId, string code, decimal discountAmount)
        {
            try
            {
                // Load order detail information
                using (var db = new Database())
                {
                    var coupon = db.Coupons.FirstOrDefault(o => o.Code == code);

                    var couponUsage = new CouponUsage
                    {
                        IdCoupon = coupon.Id,
                        IdOrder = orderId,
                        IdCustomer = customerId,
                        CreatedAt = DateTime.Now,
                        DiscountAmount = discountAmount
                    };

                    db.CouponUsages.Add(couponUsage);

                    // Apply changes
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { orderId, customerId, code, discountAmount });
                throw;
            }
        }



        /// <summary>
        /// Method to get an Specific Coupon in the system
        /// </summary>
        /// <param name="code">Code of the Coupon</param>
        /// <returns></returns>
        internal static bool CanUseCouponByCode(string code)
        {
            try
            {
                // Load order detail information
                using (var db = new Database())
                {
                    var coupon = db.Coupons.FirstOrDefault(o => o.Code == code);

                    var couponUsages = db.CouponUsages.Count(x => x.IdCoupon == coupon.Id);

                    return (coupon.UsageLimit >= couponUsages);
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: code);
                throw;
            }
        }

        /// <summary>
        /// Method to save the Coupon in the system
        /// </summary>
        /// <returns></returns>
        internal static int SaveCoupon(int id, string code, int usageLimit, string description, int idEvent, int idCabin, int idOccupacy, DateTime startDate, DateTime endDate, CouponType couponType, DiscountType discountType, decimal discountAmount, bool disabled = false)
        {

            try
            {
                var user = GetAdminAuthenticatedUser();

                using (var db = new Database())
                {
                    var coupon = new Coupon()
                    {
                        Code = code,
                        UsageLimit = usageLimit,
                        Description = description,
                        IdEvent = idEvent,
                        IdCabin = idCabin,
                        IdOccupancy = idOccupacy,
                        StartDate = startDate,
                        EndDate = endDate,
                        CouponType = couponType,
                        DiscountType = discountType,
                        DiscountAmount = discountAmount,
                        Disabled = disabled
                    };

                    if (id == 0)
                    {
                        coupon.IdUser = user.Id;
                    }
                    else
                    {
                        coupon.Id = id;
                        coupon.UpdatedAt = new DateTime();
                    }

                    db.Coupons.AddOrUpdate(coupon);

                    // Apply changes
                    var newId = db.SaveChanges();

                    return newId;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { id, code, usageLimit, description, startDate, endDate, couponType, discountType, discountAmount });
                throw;
            }
        }

        /// <summary>
        /// Method to remove the category from the database
        /// </summary>
        /// <param name="id">Identification of the category</param>
        /// <returns></returns>
        internal static bool DeleteCoupon(int id)
        {
            try
            {

                using (var db = new Database())
                {

                    var coupon = db.Coupons.Find(id);

                    if (coupon != null)
                    {
                        db.Coupons.Remove(coupon);
                    }

                    // Apply changes
                    var newId = db.SaveChanges();

                    return newId != 0;
                }
            }
            catch (Exception e)
            {
                LogException(e, objects: new object[] { id });
                throw;
            }
        }

    }
}
