using Bitzar.CMS.Data.Model;
using Bitzar.Products.Helper;
using Bitzar.Products.Models;
using ClosedXML.Excel;
using ExcelDataReader;
using MethodCache.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Attribute = Bitzar.Products.Models.Attribute;

namespace Bitzar.Products.Helpers
{
    public class Functions : Cacheable
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

        /// <summary>
        /// Method to strip from urls unwanted chars to avoid urls issues
        /// </summary>
        /// <param name="value">Raw input URL</param>
        /// <returns>Returns string without unwanted chars</returns>
        internal static string StripUnwantedChars(string value)
        {
            // Allowed list of chars
            var allowed = new char[] { '+', '-', '_' };

            // Strip only chars and numbers and allowed
            var array = (from c in value
                         where (char.IsLetterOrDigit(c) ||
                                char.IsWhiteSpace(c) ||
                                allowed.Any(a => a == c))
                         select c).ToArray();

            // Return new String
            return new string(array);
        }
        #endregion

        #region Plugin functions to process the requests

        internal static List<Role> ListRolesEditAllProduct()
        {
            var rolesId = Plugin.CMS.Configuration.Get(Database.UserRolesToEditAll, Plugin.PluginName);
            string[] rolesIdArray = rolesId.Split(',');
            var roles = Plugin.CMS.User.AdminRoles.Where(x => rolesIdArray.Contains(Convert.ToString(x.Id))).ToList();
            return roles;
        }

        [Cache]
        internal static IList<ProductField> GetCustomFields(int idtype)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<ProductField>(Scripts.Select_ProductField, idtype).Where(f => f.FieldGroup != "Básico").ToList();
        }

        /// <summary>
        /// Method to get all the categories available in the systema
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        [Cache]
        internal static IList<Category> ListCategory(int lang)
        {
            using (var db = new DatabaseConnection())
            {
                var categories = db.Database.SqlQuery<Category>(Scripts.Select_Category, lang).ToList();

                // Set hierarchy of the categories
                var hierarchy = new List<Category>();
                hierarchy.AddRange(GetCategoryChildren(categories, null, 0, null));

                return hierarchy;
            }
        }

        /// <summary>
        /// Method to get children the categories available in the systema
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        [Cache]
        internal static IList<Category> ListCategoryChildren(int lang, int IdParent)
        {
            using (var db = new DatabaseConnection())
            {
                var categories = db.Database.SqlQuery<Category>(Scripts.Select_Category, lang).ToList();


                var list = new List<Category>();
                var level = 0;
                foreach (var category in categories.Where(c => c.Id == IdParent))
                {
                    category.Level = level;
                    category.Children = GetCategoryChildren(categories, category.Id, level + 1, category);

                    list.Add(category);
                }

                return list;
            }
        }

        /// <summary>
        /// Internal method to create the system hierarchy
        /// </summary>
        /// <returns></returns>
        private static IList<Category> GetCategoryChildren(IList<Category> categories, int? parent, int level, Category last)
        {
            var list = new List<Category>();
            foreach (var category in categories.Where(c => c.IdParent == parent))
            {
                if (last != null)
                    category.Url = $"{last.Url}/{category.Url}";
                category.Level = level;
                category.Children = GetCategoryChildren(categories, category.Id, level + 1, category);

                list.Add(category);
            }
            return list;
        }

        /// <summary>
        /// Method to get an Specific category in the system
        /// </summary>
        /// <param name="id">Identification of the Category</param>
        /// <param name="lang">Lang to identify what value should be shown</param>
        /// <returns></returns>
        [Cache]
        internal static Category GetCategory(int id, int lang)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<Category>(Scripts.Select_Category, lang).FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Method to save the category in the system
        /// </summary>
        /// <param name="id">Identification of the category</param>
        /// <param name="lang">Language to set the right value</param>
        /// <param name="desc">Category description for that language</param>
        /// <param name="url">Url information for that language</param>
        /// <param name="image">Image related to that category</param>
        /// <param name="highlighted">Featured category</param>
        /// <returns></returns>
        internal static int SaveCategory(int id, string SKU, int? parent, int lang, string desc, string info, string url, string image, bool disabled, int? sort, bool highlighted)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                try
                {
                    var newid = id;
                    url = StripUnwantedChars(url);
                    // Create the category if it not exists
                    if (id == 0)
                    {
                        var sqlCategory = Scripts.Insert_Category;
                        var sqlValue = Scripts.Insert_CategoryValue;

                        // Execute commands
                        newid = db.Database.SqlQuery<int>(sqlCategory, parent, image, disabled, SKU, sort, highlighted).FirstOrDefault();
                        db.Database.ExecuteSqlCommand(sqlValue, newid, lang, desc, url, info);
                    }
                    else
                    {
                        // Update the category
                        var sqlCategory = Scripts.Update_Category;
                        var sqlValue = Scripts.Insert_CategoryValue;

                        db.Database.ExecuteSqlCommand(sqlCategory, id, parent, image, disabled, SKU, sort, highlighted);

                        // Check if the value exists to perform insert or delete
                        if (db.Database.SqlQuery<int>(Scripts.Check_CategoryValue, id, lang).First() > 0)
                            sqlValue = Scripts.Update_CategoryValue;

                        // Execute commands
                        db.Database.ExecuteSqlCommand(sqlValue, id, lang, desc, url, info);
                    }

                    transaction?.Commit();

                    // FLush cache if allowed
                    if (Plugin.CMS.Configuration.Get(Database.ClearCache, Plugin.PluginName).Contains("true"))
                    {
                        var sqlSelectCategory = Scripts.Select_CategoryById;
                        var category = db.Database.SqlQuery<Category>(sqlSelectCategory, lang, newid).FirstOrDefault();

                        InsertObjectInCache("ListCategory", category);
                    }
                    else
                    {
                        Plugin.CMS.ClearCache(typeof(Functions).FullName);
                        Plugin.CMS.ClearRoutes();
                    }

                    return newid;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();

                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Method to remove the category from the database
        /// </summary>
        /// <param name="id">Identification of the category</param>
        /// <returns></returns>
        internal static bool DeleteCategory(int id, int lang)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    // Execute command to remove the relationship of this category from products
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_CategoryProduct, id);

                    // Execute command to update all the children category to null
                    db.Database.ExecuteSqlCommandAsync(Scripts.Update_ChildrenCategoryNull, id);

                    // Execute command to remove the category from database
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_Category, id);

                    // Apply changes
                    transaction.Commit();
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);
                    Plugin.CMS.ClearRoutes();

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Method to return Attributes
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Cache]
        internal static IList<Attribute> ListAttribute(int lang)
        {
            using (var db = new DatabaseConnection())
            {
                var attributes = db.Database.SqlQuery<Attribute>(Scripts.Select_Attribute, lang).ToList();

                var hierarchy = new List<Attribute>();
                hierarchy.AddRange(GetAttributeChildren(attributes, null, 0));

                return hierarchy;
            }
        }

        private static IList<Attribute> GetAttributeChildren(IList<Attribute> Attributes, int? parent, int level)
        {
            var list = new List<Attribute>();
            foreach (var attribute in Attributes.Where(c => c.IdParent == parent))
            {
                attribute.Level = level;
                attribute.Children = GetAttributeChildren(Attributes, attribute.Id, level + 1);

                list.Add(attribute);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Cache]
        internal static Attribute GetAttribute(int id, int lang)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<Attribute>(Scripts.Select_Attribute, lang).FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        ///  Method to save the Attribute in the system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <param name="idType"></param>
        /// <param name="desc"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        internal static int SaveAttribute(int id, int? parent, int lang, int? idType, string desc, string image)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                try
                {
                    var newid = id;
                    // Create the category if it not exists
                    if (id == 0)
                    {
                        var sqlServices = Scripts.Insert_Attribute;
                        var sqlValue = Scripts.Insert_AttributeValues;

                        // Execute commands
                        newid = db.Database.SqlQuery<int>(sqlServices, idType, image, parent).FirstOrDefault();
                        db.Database.ExecuteSqlCommand(sqlValue, newid, lang, desc);
                    }
                    else
                    {
                        // Update the category
                        var sqlServices = Scripts.Update_Attribute;
                        var sqlValue = Scripts.Insert_AttributeValues;

                        db.Database.ExecuteSqlCommand(sqlServices, id, idType, image, parent);

                        // Check if the value exists to perform insert or delete
                        if (db.Database.SqlQuery<int>(Scripts.Check_AttributeValues, id, lang).First() > 0)
                            sqlValue = Scripts.Update_AttributeValues;

                        // Execute commands
                        db.Database.ExecuteSqlCommand(sqlValue, id, lang, desc);
                    }

                    transaction?.Commit();

                    // FLush cache
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return newid;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();

                    Trace(e);
                    throw e;
                }
            }
        }

        internal static IList<ProductType> ListProductTypes()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<ProductType>(Scripts.Select_ProductType).ToList();
        }

        /// <summary>
        /// Method to remove the Service from the database
        /// </summary>
        /// <param name="id">Identification of the service</param>
        /// <returns></returns>
        internal static bool DeleteAttribute(int id)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_Attribute, id);

                    // Apply changes
                    transaction.Commit();
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }

        internal static dynamic SaveCombo(int idcombo, int idproduct)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.Database.ExecuteSqlCommandAsync(Scripts.Insert_Combo, idcombo, idproduct);

                    // Apply changes
                    transaction.Commit();
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }

        internal static dynamic DeleteProductCombo(int idcombo, int idproduct)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_ProductCombo, idcombo, idproduct);

                    // Apply changes
                    transaction.Commit();
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Filter product list by attributes and categories
        /// </summary>
        /// <param name="lang">Lang to get product</param>
        /// <param name="attribute">List of attributes</param>
        /// <param name="category">List of categories</param>
        /// <returns>Return product list filtered</returns>
        internal static List<Product> SearchByValues(int lang, string attribute, string category)
        {
            var products = ListProduct(lang);
            var attributes = attribute.Split(',');
            var categories = category.Split(',');

            // Filter by attributes
            if (!string.IsNullOrWhiteSpace(attribute))
                foreach (var item in attributes)
                    products = products.Where(p => p.Attributes.Any(a => string.Equals(item, a.Attribute.Description, StringComparison.CurrentCultureIgnoreCase))).ToList();

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
                foreach (var item in categories)
                    products = products.Where(p => p.Categories.Any(a => string.Equals(item, a.Category.Description, StringComparison.CurrentCultureIgnoreCase))).ToList();

            return products;
        }

        /// <summary>
        /// Filter product list by product descritpion and attributes
        /// </summary>
        /// <param name="lang">Lang to get product</param>
        /// <param name="search">String to search</param>
        /// <param name="idtype">String to product type</param>
        /// <returns>Return product list filtered</returns>
        internal static List<Product> SearchProducts(int lang, string search,string idtype)
        {
            var products = ListProduct(lang);

            products = products.Where(x => idtype.Any(f => x.IdType == Convert.ToInt32(idtype))).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p => p.Attributes.Any(a => a.Attribute.Description.ToUpper().Contains(search.ToUpper()))
                                            || p.Description.Any(n => p.Description.ToUpper().Contains(search.ToUpper()))).ToList();
            }

            return products;
        }

        /// <summary>
        /// Filter product list by attributes and categories
        /// </summary>
        /// <param name="lang">Lang to get product</param>
        /// <param name="attribute">List of attributes</param>
        /// <param name="category">List of categories</param>
        /// <returns>Return product list filtered</returns>
        internal static List<Product> SearchByValuesInclusive(int lang, string attribute, string category)
        {
            var products = ListProduct(lang);
            var attributes = attribute.Split(',');
            var categories = category.Split(',');



            // Filter by attributes
            if (!string.IsNullOrWhiteSpace(attribute))
                foreach (var item in attributes)
                    products = products.Where(p => p.Attributes.Any(a => a.Attribute.Description.Contains(item))).ToList();



            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
                foreach (var item in categories)
                    products = products.Where(p => p.Categories.Any(a => a.Category.Description.Contains(item))).ToList();



            return products;
        }

        internal static dynamic SaveQuantity(int idcombo, int idproduct, int quantidade)
        {
            using (var db = new DatabaseConnection())
            {
                try
                {
                    db.Database.ExecuteSqlCommand(Scripts.Insert_ComboQuantidade, quantidade, idcombo, idproduct);

                    foreach (var key in Plugin.CMS.Cache.AllKeys)
                        if (key.Contains("Bitzar.Products.Helpers.Functions.ListProduct"))
                        {
                            var content = Plugin.CMS.Cache.Retrieve<object>(key);
                            if (content.GetType() != typeof(List<Product>))
                                continue;

                            var product = ((List<Product>)content).FirstOrDefault(p => p.Id == idcombo);
                            var comboItem = product.ComboProduct.FirstOrDefault(c => c.Id == idproduct);
                            comboItem.Quantity = quantidade;
                        }

                    return true;
                }
                catch (Exception e)
                {
                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Method to lookup for a given object in the Cache and update if found
        /// </summary>
        /// <typeparam name="T">Type of object to lookup in the cache</typeparam>
        /// <param name="cacheKey">Cachekey to Lookup</param>
        /// <param name="obj">Object to be added</param>
        public static void InsertObjectInCache<T>(string cacheKey, T obj)
        {
            foreach (var key in Plugin.CMS.Cache.AllKeys)
            {
                if (!key.Contains(cacheKey))
                    continue;

                var content = Plugin.CMS.Cache.Retrieve<object>(key);
                if (content.GetType() != typeof(List<T>))
                    continue;

                var search = ((List<T>)content).FirstOrDefault(c => ((dynamic)c).Id == ((dynamic)obj).Id);
                if (search == null)
                    ((List<T>)content).Add(obj);
                else
                    search = obj;
            }
        }

        [Cache]
        internal static IList<ProductAttribute> ListProductAttribute()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<ProductAttribute>(Scripts.Select_ProductAttribute).ToList();

        }

        /// <summary>
        /// Method to list AttributesType
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Cache]
        internal static IList<AttributeType> ListAttributeType(int lang)
        {
            using (var db = new DatabaseConnection())
            {
                return db.Database.SqlQuery<AttributeType>(Scripts.Select_AttributeType, lang).ToList();
            }
        }

        /// <summary>
        ///  Method to get AttributesType
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Cache]
        internal static AttributeType GetAttributeType(int id, int lang)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<AttributeType>(Scripts.Select_AttributeType, lang).FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Method to save Attribute Type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int SaveAttributeType(int id, int lang, string value)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                try
                {
                    var newid = id;
                    // Create the category if it not exists
                    if (id == 0)
                    {
                        var sqlServices = Scripts.Insert_AttributeType;
                        var sqlValue = Scripts.Insert_AttributeTypeValues;

                        // Execute commands
                        newid = db.Database.SqlQuery<int>(sqlServices).FirstOrDefault();
                        db.Database.ExecuteSqlCommand(sqlValue, newid, lang, value);
                    }
                    else
                    {
                        // Update the category
                        var sqlValue = Scripts.Insert_AttributeTypeValues;

                        // Check if the value exists to perform insert or delete
                        if (db.Database.SqlQuery<int>(Scripts.Check_AttributeTypeValues, id, lang).First() > 0)
                            sqlValue = Scripts.Update_AttributeTypeValues;

                        // Execute commands
                        db.Database.ExecuteSqlCommand(sqlValue, id, lang, value);
                    }

                    transaction?.Commit();

                    // FLush cache
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return newid;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();

                    Trace(e);
                    throw e;
                }
            }
        }

        internal static bool DeleteAttributeType(int id)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_AttributeType, id);

                    // Apply changes
                    transaction.Commit();
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }


        /// <summary>
        /// Method to remove a product from the database
        /// </summary>
        /// <param name="id">Identification of the product</param>
        /// <returns></returns>
        internal static bool DeleteProduct(int id)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    // Execute command to remove product x attribute relationship
                    db.Database.ExecuteSqlCommandAsync("DELETE FROM btz_productattribute WHERE IdProduct = @p0", id);

                    // Execute command to remove product x combo relationship
                    db.Database.ExecuteSqlCommandAsync("DELETE FROM btz_productcombo WHERE IdCombo = @p0", id);

                    // Execute command to remove product x combo relationship
                    db.Database.ExecuteSqlCommandAsync("DELETE FROM btz_productcombo WHERE IdProduct = @p0", id);

                    //  Execute command to remove productsub
                    db.Database.ExecuteSqlCommandAsync("DELETE FROM btz_productsub WHERE IdSubProduct = @p0", id);

                    //  Execute command to remove productrelated
                    db.Database.ExecuteSqlCommandAsync("DELETE FROM btz_productrelated WHERE IdRelated = @p0", id);

                    // Execute command to delete all the product field values
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_ProductValues, id);

                    // Execute command to remove product x category relationship
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_ProductCategory, id);

                    // Execute command to remove the product
                    db.Database.ExecuteSqlCommandAsync(Scripts.Delete_Product, id);

                    // Apply changes
                    transaction.Commit();
                    
                    // Flush cache object
                    Plugin.CMS.ClearCache(typeof(Functions).FullName);
                    Plugin.CMS.ClearRoutes();

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Method to get all the products available in the system
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        [Cache]
        internal static List<Product> ListProduct(int lang, bool hierarchy = false, int? userId = null, string owner = "")
        {

            var products = new List<Product>();

            using (var db = new DatabaseConnection())
            {
                // TODO: Arrumar esta cagada.
                if (userId == null)
                    products = db.Database.SqlQuery<Product>(Scripts.Select_Product).ToList();
                else
                    products = db.Database.SqlQuery<Product>(Scripts.Select_ProductByUser, userId).ToList();

                if(products.Count == 0)
                    return products;

                var fields = ListField(lang);
                var categories = ListProductCategory(lang);
                var related = ListProductRelated().ToList();
                var subproduct = ListProductSub().ToList();
                var attributes = ListProductAttribute(lang);
                var combo = ListProductCombo().ToList();
                var productUser = ListProductUser().ToList();

                // Bind related with Product list
                related.ForEach(r => r.Related = products.FirstOrDefault(p => p.Id == r.IdRelated));

                // Bind subproduct with product list
                subproduct.ForEach(r =>
                {
                    r.Product = products.FirstOrDefault(p => p.Id == r.IdProduct);
                    r.SubProduct = products.FirstOrDefault(p => p.Id == r.IdSubProduct);
                });

                products.ForEach(p =>
                {
                    p.Fields = fields.Where(f => f.IdProduct == p.Id).ToList();
                    p.Categories = categories.Where(c => c.IdProduct == p.Id).ToList();
                    p.Attributes = attributes.Where(c => c.IdProduct == p.Id).ToList();
                    p.Related = related.Where(r => r.IdProduct == p.Id).Select(r => r.Related).ToList();
                    p.SubProduct = subproduct.Where(s => s.IdProduct == p.Id).Select(s => s.SubProduct).ToList();
                    p.ComboProduct = combo.Where(s => s.IdCombo == p.Id).Select(s => s.Product).ToList();
                    p.Owners = productUser.Where(x => x.IdProduto == p.Id).Select(u => new SimpleUser { IdUser = u.IdUser, Name = u.UserName }).ToList();
                });

                // Bind related with Product list
                combo.ForEach(r =>
                {
                    r.ComboProduct = products.FirstOrDefault(p => p.Id == r.IdCombo);

                    var productRef = products.FirstOrDefault(p => p.Id == r.IdProduct);
                    if(productRef != null)
                    {
                        r.Product = productRef.CloneProduct();
                        r.Product.Fields = productRef.Fields;

                        r.Product.Quantity = r.Quantidade;

                    }
                });
                products.ForEach(p => p.ComboProduct = combo.Where(s => s.IdCombo == p.Id).Select(s => s.Product).ToList());

                // Define subproduct hierarchy
                if (hierarchy)
                    products = SubProductHierarchy(products.Where(p => !subproduct.Any(s => s.IdSubProduct == p.Id)).ToList(), products, subproduct);

                products.ForEach(p => p.RouteUrl = ((p.Categories?.Count ?? 0) > 0 ? $"{p.Categories.FirstOrDefault()?.Category.Url.ToLower() ?? ""}/{p.Url.ToLower()}" : p.Url.ToLower()));

                if (owner != "" && !String.IsNullOrEmpty(owner))
                    products = products.Where(x => x.Owners.Any(y => y.IdUser == Convert.ToInt32(owner))).ToList();

                // Return product list to the system
                return products;
            }
        }

        private static List<UserProduct> ListProductUser()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<UserProduct>(Scripts.Select_UserByProduct).ToList();
        }

        public static List<Product> ListProductByType(int type)
        {
            using (var db = new DatabaseConnection())
            {
                var products = db.Database.SqlQuery<Product>(Scripts.Select_Product).ToList();
                var productByType = products.Where(t => t.IdType == type).ToList();
                return productByType;
            }
                
        }

        [Cache]
        private static IList<Combo> ListProductCombo()
        {

            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<Combo>("SELECT * FROM btz_productcombo ORDER BY IdCombo, Sort, IdProduct").ToList();
        }



        /// <summary>
        /// Routine to create the Subproduct hierarchy
        /// </summary>
        /// <param name="parents">Parent objects to process hierarchy</param>
        /// <param name="products">All products to match parents Id and references</param>
        /// <param name="subproducts">Subproduct matrix</param>
        /// <returns></returns>
        private static List<Product> SubProductHierarchy(List<Product> parents, List<Product> products, List<ProductSub> subproducts)
        {
            var list = new List<Product>();
            foreach (var product in parents)
            {
                product.SubProduct = SubProductHierarchy(products.Where(p => subproducts.Any(s => s.IdSubProduct == p.Id && s.IdProduct == product.Id)).ToList(), products, subproducts);
                list.Add(product);
            }
            return list;
        }

        /// <summary>
        /// Method to filter products by Ids
        /// </summary>
        /// <param name="ids">List of products to show</param>
        /// <param name="lang">Language to filter products</param>
        /// <returns>Return a list of products</returns>
        [NoCache]
        internal static List<Product> ListProduct(string ids, int lang)
        {
            var products = ListProduct(lang);

            // Filter
            var filter = ids.Split(',').Select(i => Convert.ToInt32(i));

            return products.Where(p => filter.Any(f => p.Id == f)).ToList();
        }

        /// <summary>
        /// Method to load all the product related information
        /// </summary>
        /// <returns></returns>
        [Cache]
        private static IList<ProductRelated> ListProductRelated()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<ProductRelated>("SELECT * FROM btz_productrelated").ToList();
        }

        /// <summary>
        /// Method to load all the product related information
        /// </summary>
        /// <returns></returns>
        [Cache]
        public static IList<ProductSub> ListProductSub()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<ProductSub>("SELECT * FROM btz_productsub WHERE IdSubProduct IN (SELECT Id FROM btz_product)").ToList();
        }

        /// <summary>
        /// Method to load all the products categories
        /// </summary>
        /// <returns></returns>
        [Cache]
        private static IList<ProductCategory> ListProductCategory(int lang)
        {
            using (var db = new DatabaseConnection())
            {
                var categories = ListCategory(lang);
                var data = db.Database.SqlQuery<ProductCategory>("SELECT * FROM btz_productcategory").ToList();

                // Set object data
                data.ForEach(d => d.Category = SearchCategory(categories, d.IdCategory));

                return data;
            }
        }

        /// <summary>
        /// Lookup for a Category in the system
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="idCategory"></param>
        /// <returns></returns>
        internal static Category SearchCategory(IList<Category> categories, int idCategory)
        {
            var item = categories.FirstOrDefault(f => f.Id == idCategory);
            if (item != null)
                return item;

            foreach (var child in categories)
            {
                var inner = SearchCategory(child.Children, idCategory);
                if (inner != null)
                    return inner;
            }

            return item;
        }

        /// <summary>
        /// Method to load all the products categories
        /// </summary>
        /// <returns></returns>
        [Cache]
        private static IList<ProductAttribute> ListProductAttribute(int lang)
        {
            using (var db = new DatabaseConnection())
            {
                var attributes = ListAttribute(lang);
                var data = db.Database.SqlQuery<ProductAttribute>("SELECT * FROM btz_productattribute").ToList();

                // Set object data
                data.ForEach(d => d.Attribute = SearchAttribute(attributes, d.IdAttribute));

                return data;
            }
        }


        internal static Attribute SearchAttribute(IList<Attribute> attributes, int idAttribute)
        {
            var item = attributes.FirstOrDefault(f => f.Id == idAttribute);
            if (item != null)
                return item;

            foreach (var child in attributes)
            {
                var inner = SearchAttribute(child.Children, idAttribute);
                if (inner != null)
                    return inner;
            }

            return item;
        }



        /// <summary>
        /// Method to sort all the products in the given order
        /// </summary>
        /// <param name="order">Sort order defined</param>
        /// <returns></returns>
        internal static dynamic SortProducts(string order)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    // Execute each update for each question of the survey
                    var sortOrder = order.Split(',');
                    for (var i = 0; i < sortOrder.Length; i++)
                        db.Database.ExecuteSqlCommand("UPDATE btz_product SET Sort = @p1 WHERE Id = @p0", sortOrder[i], (i + 1));

                    // Apply changes
                    transaction.Commit();

                    // Clear cache
                    foreach (var key in Plugin.CMS.Cache.AllKeys)
                        if (key.StartsWith("Bitzar.Products.Helpers.Functions.ListProduct"))
                            Plugin.CMS.Cache.Remove(key);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }


        internal static dynamic SortProductsCombo(string order, int idCombo)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    // Execute each update for each question of the survey
                    var sortOrder = order.Split(',');
                    for (var i = 0; i < sortOrder.Length; i++)
                        db.Database.ExecuteSqlCommand("UPDATE btz_productcombo SET Sort = @p1 WHERE IdProduct = @p0 AND IdCombo = @p2", sortOrder[i], (i + 1), idCombo);

                    // Apply changes
                    transaction.Commit();

                    // Clear cache
                    foreach (var key in Plugin.CMS.Cache.AllKeys)
                        if (key.StartsWith("Bitzar.Products.Helpers.Functions.ListProductCombo"))
                            Plugin.CMS.Cache.Remove(key);

                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    Trace(e);
                    throw e;
                }
            }
        }


        /// <summary>
        /// Method to get all the fields available to all the products.
        /// Due the fact this will cached should be returned once and filter in code
        /// </summary>
        /// <param name="lang">Language ID to identify the current idiom</param>
        /// <returns>Returns a list of Field of all products</returns>
        [Cache]
        internal static IList<Models.Field> ListField(int lang)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<Models.Field>(Scripts.Select_Field, lang).ToList();
        }

        internal static IList<Models.Field> ListProductField(int lang, int idProduct)
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<Models.Field>(Scripts.Select_Field, lang).Where(x => x.IdProduct == idProduct).ToList();
        }

        /// <summary>
        /// Method to get an specific product and all related data to show in the system
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        internal static Product GetProduct(int id, int lang)
        {
            var products = ListProduct(lang);
            var product = products.FirstOrDefault(p => p.Id == id);
            product.Fields = ListProductField(lang, id);
            product.Users = ListProductUser();
            return product;
        }

        /// <summary>
        /// Method to get an specific product by url and all related data to show in the system
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        internal static Product GetProductByUrl(string url, int lang)
        {
            var products = ListProduct(lang);
            var product = products.FirstOrDefault(p => p.Url == url);
            product.Fields = ListProductField(lang, product.Id);
            product.Users = ListProductUser();
            return product;
        }

        /// <summary>
        /// Method to get an specific product by sku and all related data to show in the system
        /// </summary>
        /// <returns>Returns a list with all categories available</returns>
        internal static Product GetProductBySku(string sku, int lang)
        {
            var products = ListProduct(lang);
            var product = products.FirstOrDefault(p => p.SKU == sku);
            product.Fields = ListProductField(lang, product.Id);
            product.Users = ListProductUser();
            return product;
        }

        /// <summary>
        /// Routine to save the product data in the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        internal static int SaveProduct(int id, int idtype, int lang, string sku, bool disabled, bool hide, string categories, string related, dynamic properties, string attributes, int idProduct, int IdUser)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                try
                {
                    // Create product if it not exists
                    if (id == 0)
                    {
                        var sqlProduct = Scripts.Insert_Product;
                        // Execute commands
                        id = db.Database.SqlQuery<int>(sqlProduct, sku, disabled, hide, idtype, DateTime.Now, DateTime.Now).FirstOrDefault();
                    }
                    else
                    {
                        // Update the product
                        var sqlProduct = Scripts.Update_Product;
                        db.Database.ExecuteSqlCommand(sqlProduct, id, sku, disabled, hide, idtype, DateTime.Now);
                    }

                    // Update categories
                    db.Database.ExecuteSqlCommand(Scripts.Delete_ProductCategory, id);
                    if (!string.IsNullOrWhiteSpace(categories))
                        foreach (var category in categories.Split(',').Where(i => i != "none"))
                            db.Database.ExecuteSqlCommand(Scripts.Insert_ProductCategory, id, Convert.ToInt32(category));

                    // Update Related
                    db.Database.ExecuteSqlCommand(Scripts.Delete_ProductRelated, id);
                    if (!string.IsNullOrWhiteSpace(related))
                        foreach (var item in related.Split(',').Where(i => i != "none"))
                            db.Database.ExecuteSqlCommand(Scripts.Insert_ProductRelated, id, Convert.ToInt32(item));

                    //Update SubProduct
                    db.Database.ExecuteSqlCommand(Scripts.Delete_SubProduct, id);
                    if (idProduct != 0)
                        db.Database.ExecuteSqlCommand(Scripts.Insert_SubProduct, idProduct, id);

                    // Update Attribute
                    db.Database.ExecuteSqlCommand(Scripts.Delete_ProductAttribute, id);
                    if (!string.IsNullOrWhiteSpace(attributes))
                        foreach (var attribute in attributes.Split(',').Where(i => i != "none"))
                            db.Database.ExecuteSqlCommand(Scripts.Insert_ProductAttribute, id, Convert.ToInt32(attribute));

                    // Update Owner
                    db.Database.ExecuteSqlCommand(Scripts.Delete_ProductUser, id);
                    if (IdUser != 0)
                        db.Database.ExecuteSqlCommand(Scripts.Insert_ProductUser, id, IdUser);

                    // Set product values
                    SetProductValues(id, lang, properties);
                    transaction?.Commit();

                    //Flush Cache
                    if (Plugin.CMS.Configuration.Get(Database.ClearCache, Plugin.PluginName).Contains("true"))
                    {
                        var sqlSelectProduct = Scripts.Select_ProductById;
                        var product = db.Database.SqlQuery<Product>(sqlSelectProduct, id).FirstOrDefault();

                        product.Fields = ListProductField(lang, id);

                        InsertObjectInCache("ListProduct", product);
                    }
                    else
                    {
                        Plugin.CMS.ClearCache(typeof(Functions).FullName);
                        Plugin.CMS.ClearRoutes();
                    }

                    // Refresh site map if allowed
                    Plugin.CMS.Configuration.AutoRefreshSiteMap();

                    return id;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();

                    Trace(e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Method to save all the properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <param name="properties"></param>
        private static void SetProductValues(int id, int lang, dynamic properties)
        {
            using (var db = new DatabaseConnection())
            {
                var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                try
                {
                    // Get fields and values for the desired item
                    var fields = db.Database.SqlQuery<ProductField>(Scripts.Select_ProductField_2).ToList();
                    var values = db.Database.SqlQuery<Models.Field>(Scripts.Select_ProductFieldValue, id, lang).ToList();
                    var dictionary = (IDictionary<string, object>)properties;

                    // Loop through each item 
                    foreach (var item in dictionary)
                    {
                        //if (item.Key == "Id" || item.Key == "Description" || item.Key == "Text" )
                        //    continue;


                        var value = item.Value.ToString();

                        // Check if the field exists
                        var field = fields.FirstOrDefault(f => f.Name.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
                        if (field == null)
                            continue;

                        // Check if field key is URL
                        if (item.Key == "Url")
                            value = StripUnwantedChars(value);

                        // Check if should insert it or ignore it
                        var sql = Scripts.Insert_ProductFieldValue;
                        if (values.Any(v => v.Name.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase)))
                            sql = Scripts.Update_ProductFieldValue;

                        // Execute command
                        db.Database.ExecuteSqlCommand(sql, field.IdField, id, lang, value);
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction?.Rollback();

                    Trace(e);
                    throw e;
                }
            }
        }

        public static IList<Category> GetFlatCategories(IList<Category> categories)
        {
            var list = new List<Category>();
            foreach (var category in categories)
            {
                list.Add(category);
                if (category.Children.Count > 0)
                {
                    list.AddRange(GetFlatCategories(category.Children));
                }
            }

            return list;
        }


        /// <summary>
        /// Method to replicate all the idiom Keys
        /// </summary>
        internal static void ReplicateIdiomKeys()
        {
            // Get Default culture
            var culture = Plugin.CMS.I18N.DefaultLanguage;

            using (var db = new DatabaseConnection())
            {
                try
                {
                    // Get all the Fields from default language
                    var values = db.Database.SqlQuery<ProductFieldValue>(Scripts.Select_ProductFieldValueRaw, culture.Id).ToList();
                    foreach (var idiom in Plugin.CMS.I18N.AvailableLanguages)
                    {
                        // Skip default
                        if (idiom.Culture == culture.Culture)
                            continue;

                        // Get all fields from the idiom
                        var idiomValues = db.Database.SqlQuery<ProductFieldValue>(Scripts.Select_ProductFieldValueRaw, idiom.Id).ToList();

                        // Replicate all the translation values
                        foreach (var value in values)
                        {
                            // Get current value and insert if it's null
                            var idiomValue = idiomValues.FirstOrDefault(v => v.IdProduct == value.IdProduct && v.IdField == value.IdField);
                            if (idiomValue == null)
                            {
                                // Insert new idiom
                                db.Database.ExecuteSqlCommand(Scripts.Insert_ProductFieldValue, value.IdField, value.IdProduct, idiom.Id, value.Value);
                                continue;
                            }

                            // Skip if idiom is already filled
                            if (value.Value == null || string.IsNullOrWhiteSpace(value.Value))
                                continue;

                            // Update field
                            if (idiomValue.Value == null || string.IsNullOrWhiteSpace(idiomValue.Value) || idiomValue.Value.Trim() == "[]")
                                db.Database.ExecuteSqlCommandAsync(Scripts.Update_ProductFieldValueRaw, value.IdField, value.IdProduct, idiom.Id, value.Value);
                        }
                    }

                    //Update Attributes
                    /*var attributes = db.Database.SqlQuery<AttributeValue>(Scripts.Select_AttributeValue, culture.Id).ToList();
                    foreach(var idiom in Plugin.CMS.I18N.AvailableLanguages)
                    {
                        //Skip default
                        if (idiom.Culture == culture.Culture)
                            continue;
                        
                        //get all fields from the idiom
                        var idiomAttributes = db.Database.SqlQuery<AttributeValue>(Scripts.Select_AttributeValue, idiom.Id).ToList();

                        //Replicate all the translation attributes
                        foreach(var attribute in attributes)
                        {
                            // Get current value and insert if it's null
                            var idiomAttribute = idiomAttributes.FirstOrDefault(a => a.Id == attribute.Id);                            
                            if (idiomAttribute == null)
                            {
                                // Insert new idiom
                                db.Database.ExecuteSqlCommand(Scripts.Insert_AttributeValues, attribute.Id, idiom.Id, attribute.Desc);
                                continue;
                            }

                            // Skip if idiom is already filled
                            if (attribute.Desc == null || string.IsNullOrWhiteSpace(attribute.Desc))
                                continue;

                            // Update field
                            if (idiomAttribute.Desc == null || string.IsNullOrWhiteSpace(idiomAttribute.Desc))
                                db.Database.ExecuteSqlCommand(Scripts.Update_AttributeValues, attribute.Id, idiom.Id, attribute.Desc);
                        }
                    }*/

                    // Update categories
                    var categoryValues = db.Database.SqlQuery<CategoryValue>(Scripts.Select_CategoryValue, culture.Id).ToList();
                    foreach (var idiom in Plugin.CMS.I18N.AvailableLanguages)
                    {
                        // Skip default
                        if (idiom.Culture == culture.Culture)
                            continue;

                        // Get all fields from the idiom
                        var idiomCategoryValues = db.Database.SqlQuery<CategoryValue>(Scripts.Select_CategoryValue, idiom.Id).ToList();

                        // Replicate all the translation values
                        foreach (var value in categoryValues)
                        {
                            // Get current value and insert if it's null
                            var idiomValue = idiomCategoryValues.FirstOrDefault(v => v.IdCategory == value.IdCategory);
                            if (idiomValue == null)
                            {
                                // Insert new idiom
                                db.Database.ExecuteSqlCommand(Scripts.Insert_CategoryValue, value.IdCategory, idiom.Id, value.Value, value.Url);
                                continue;
                            }

                            // Skip if idiom is already filled
                            if (value.Value == null || string.IsNullOrWhiteSpace(value.Value))
                                continue;

                            // Update field
                            if (idiomValue.Value == null || string.IsNullOrWhiteSpace(idiomValue.Value))
                                db.Database.ExecuteSqlCommandAsync(Scripts.Update_CategoryValue, value.IdCategory, idiom.Id, value.Value, value.Url);
                        }
                    }

                }
                catch (Exception e)
                {
                    Trace(e);
                    throw e;
                }
            }
        }

        public static string CountProductsUser()
        {
            var owner = "";
            var lang = 1;

            var user = Plugin.CMS.Membership.AdminUser;

            // Validation if need to filter by user or list all
            int? filterUser = null;
            if (user != null && user.Role.Name != "Administrador")
                filterUser = user.Id;

            // Get products and subproducts
            var products = Functions.ListProduct(lang, true, filterUser);

            var ativos = products.Where(x => x.Disabled == false || x.Hide == false).ToList();
            var inativos = products.Where(x => x.Disabled == true || x.Hide == true).ToList();

            if (filterUser != null)
            {
                ativos = ativos.Where(x => x.Owners.Any(y => y.Name.ToUpper().Contains(owner.ToUpper()))).ToList();
                inativos = inativos.Where(x => x.Owners.Any(y => y.Name.ToUpper().Contains(owner.ToUpper()))).ToList();
            }


            var count = "" + ativos.Count() + "," + inativos.Count();

            return count;
        }

        public static List<Product> ProductsUser(int? filterUser)
        {
            var owner = "";
            var lang = 1;

            // Get products and subproducts
            var products = ListProduct(lang, true, filterUser);

            var ativos = products.Where(x => x.Disabled == false || x.Hide == false).ToList();

            if (filterUser != null)
            {
                ativos = ativos.Where(x => x.Owners.Any(y => y.Name.ToUpper().Contains(owner.ToUpper()))).ToList();
            }

            return ativos;
        }

        public static List<ProductRelated> GetProductRelated(int idproduct)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var productRelated = db.Database.SqlQuery<ProductRelated>(Scripts.Select_ProductRelated, idproduct).ToList();

                    return productRelated;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        internal static FileStreamResult ExportProducts(int lang, int idType)
        {
            try
            {
                var isUserOwned = Convert.ToBoolean(Plugin.CMS.Configuration.Get("IsUserOwned", Plugin.PluginName));
                var user = Plugin.CMS.Membership.AdminUser;
                var products = new List<Product>();

                if (isUserOwned && user.Role.Name != "Administrador")
                    products = Functions.ListProduct(lang: lang, userId: user.Id);
                else
                    products = Functions.ListProduct(lang);
                
                var dataTable = new DataTable("Produtos");
                var prod = products.First();

                dataTable.Columns.Add(nameof(prod.Id));
                dataTable.Columns.Add(nameof(prod.Description));
                dataTable.Columns.Add(nameof(prod.Disabled));
                dataTable.Columns.Add(nameof(prod.Gallery));
                dataTable.Columns.Add(nameof(prod.Hide));
                dataTable.Columns.Add(nameof(prod.Quantity));
                dataTable.Columns.Add(nameof(prod.SKU));
                dataTable.Columns.Add(nameof(prod.Sort));
                dataTable.Columns.Add(nameof(prod.Text));
                dataTable.Columns.Add(nameof(prod.Url));
                
                var productFields = Functions.GetCustomFields(idType);

                foreach (var field in productFields)
                    dataTable.Columns.Add(field.Name);

                using (var db = new DatabaseConnection())
                {
                    var fieldValues = db.Database.SqlQuery<ProductFieldValue>(Scripts.Select_ProductFieldValue_2).ToList();
                    foreach (var product in products)
                    {
                        var dataRow = dataTable.NewRow();
                        dataRow[nameof(prod.Id)] = product.Id;
                        dataRow[nameof(prod.Description)] = product.Description;
                        dataRow[nameof(prod.Disabled)] = product.Disabled;
                        dataRow[nameof(prod.Gallery)] = product.Gallery;
                        dataRow[nameof(prod.Hide)] = product.Hide;
                        dataRow[nameof(prod.Quantity)] = product.Quantity;
                        dataRow[nameof(prod.SKU)] = product.SKU;
                        dataRow[nameof(prod.Sort)] = product.Sort;
                        dataRow[nameof(prod.Text)] = product.Text;
                        dataRow[nameof(prod.Url)] = product.Url;

                        var values = fieldValues.Where(v => v.IdProduct == product.Id).ToList();

                        foreach (var field in productFields)
                            dataRow[field.Name] = values.FirstOrDefault(v => v.IdField == field.IdField)?.Value;
                        
                        dataTable.Rows.Add(dataRow);
                    }
                }
                
                var workbook = new XLWorkbook();
                var workSheet = workbook.Worksheets.Add("Produtos");
                workSheet.Cell(1, 1).InsertTable(dataTable);

                var file = Path.GetTempFileName();
                var fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                workbook.SaveAs(fileStream);
                fileStream.Seek(0, SeekOrigin.Begin);

                var fileStreamResult = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fileStreamResult.FileDownloadName = "Produtos.xlsx";
                return fileStreamResult;
            }
            catch
            {
                throw;
            }
        }

        internal static string ImportProducts(Stream stream, string fileName, int lang, int idType, int? userId = null)
        {
            try
            {
                var isUserOwned = Convert.ToBoolean(Plugin.CMS.Configuration.Get("IsUserOwned", Plugin.PluginName));
                var user = Plugin.CMS.Membership.AdminUser;
                var productFields = Functions.GetCustomFields(idType);

                if (isUserOwned && user.Role.Name != "Administrador")
                    userId = user.Id;

                if (isUserOwned && user.Role.Name == "Administrador" && userId == null)
                    throw new InvalidOperationException("É obrigatória a seleção de um usuário para o vínculo com os produtos.");

                DataSet dataSet = null;
                using (var dataReader = CreateExcelDataReader(stream, fileName))
                {
                    dataSet = dataReader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });
                }

                var dataTable = dataSet.Tables[0];
                using (var db = new DatabaseConnection())
                {
                    var productIds = db.Database.SqlQuery<int>(Scripts.Select_ProductId).ToList();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        var productId = Convert.ToInt32(row["Id"]);
                        var exist = productIds.Any(p => p == productId);
                        var sql = exist ? Scripts.Update_ProductFieldValue : Scripts.Insert_ProductFieldValue;
                        
                        if (exist)
                        {
                            var productUser = db.Database.SqlQuery<UserProduct>(Scripts.Select_ProductUser, productId).FirstOrDefault();
                            if (isUserOwned && productUser != null && productUser.IdUser != userId)
                                throw new InvalidOperationException("O produto já pertence a outro usuário.");

                            db.Database.ExecuteSqlCommand(Scripts.Update_Product, productId, row["SKU"].ToString(), Convert.ToBoolean(row["Disabled"]), Convert.ToBoolean(row["Hide"]), idType, DateTime.Now);
                        }
                        else
                            productId = db.Database.SqlQuery<int>(Scripts.Insert_Product, row["SKU"].ToString(), Convert.ToBoolean(row["Disabled"]), Convert.ToBoolean(row["Hide"]), idType, DateTime.Now, DateTime.Now).FirstOrDefault();
                        
                        if (isUserOwned)
                            db.Database.ExecuteSqlCommand(Scripts.Insert_ProductUser, productId, userId);

                        foreach (var field in productFields)
                            db.Database.ExecuteSqlCommand(sql, field.IdField, productId, lang, row[field.Name].ToString());
                        
                        db.Database.ExecuteSqlCommand(Scripts.Delete_ProductFieldValue_NullOrEmpty, productId, lang);
                    }
                }
                
                return "IMPORTPRODUCTS_OK";
            }
            catch
            {
                throw;
            }
        }

        private static IExcelDataReader CreateExcelDataReader(Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (extension == ".xls")
                return ExcelReaderFactory.CreateBinaryReader(fileStream);

            if (extension == ".xlsx")
                return ExcelReaderFactory.CreateOpenXmlReader(fileStream);

            // Try to create excel 
            return ExcelReaderFactory.CreateCsvReader(fileStream, new ExcelReaderConfiguration()
            {
                AutodetectSeparators = new char[] { ';', ',' },
                FallbackEncoding = System.Text.Encoding.UTF8
            });
        }
        #endregion
    }
}