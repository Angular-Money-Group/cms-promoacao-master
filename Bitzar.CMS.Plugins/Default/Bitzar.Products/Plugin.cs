using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.Classes;
using Bitzar.CMS.Extension.CMS;
using Bitzar.CMS.Extension.Interfaces;
using Bitzar.CMS.Model;
using Bitzar.Products.Helper;
using Bitzar.Products.Helpers;
using Bitzar.Products.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Bitzar.Products
{
    public class Plugin : IPlugin
    {
        internal static ICMS CMS { get; set; }

        /// <summary>
        /// Internal method to get plugin's name
        /// </summary>
        internal static string PluginName => "Bitzar.Products.dll";

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
                // convert parameters to expando object
                var param = parameters?.ToExpando() ?? null;
                var owner = "";

                // Start process the system functions
                switch (function.ToUpper())
                {
                    /*
                     * Category Methods
                     */
                    case Constants.ListCategory:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var categories = Functions.ListCategory(lang);
                            return categories;
                        }
                    case Constants.ListCategoryChildren:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var IdParent = Convert.ToInt32(param?.IdParent ?? 0);

                            return Functions.ListCategoryChildren(lang, IdParent);
                        }
                    case Constants.GetCategory:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var id = Convert.ToInt32(param?.Id ?? 0);

                            if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            return Functions.GetCategory(id, lang);
                        }
                    case Constants.SaveCategory:
                        {
                            EnsureAdminAuthenticated();

                            // Get parameters
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            int id = Convert.ToInt32(param?.Id ?? 0);
                            string name = param?.Name ?? string.Empty;
                            string sku = param?.SKU ?? string.Empty;
                            string desc = parameters.ContainsKey("Description") ? parameters["Description"] : null;
                            string url = param?.Url ?? string.Empty;
                            string image = param?.Image ?? string.Empty;
                            int? parent = (string.IsNullOrWhiteSpace((string)param?.IdParent ?? "")) ? (int?)null : Convert.ToInt32((string)param.IdParent);
                            bool disabled = Convert.ToBoolean(param?.Disabled ?? Boolean.FalseString);
                            int? sort = (string.IsNullOrWhiteSpace((string)param?.Sort ?? null)) ? (int?)null : Convert.ToInt32((string)param.Sort);
                            bool highlighted = parameters["Highlighted"].ToLower() == "true" ? true : false;

                            // Check if the parameters are ok
                            if (string.IsNullOrWhiteSpace(desc)) throw new Exception("A descrição da categoria deve ser informada");
                            if (string.IsNullOrWhiteSpace(url)) throw new Exception("A Url da categoria deve ser informada");

                            // Call method to save category
                            return Functions.SaveCategory(id, sku, parent, lang, name, desc, url, image, disabled, sort, highlighted);
                        }
                    case Constants.DeleteCategory:
                        {
                            EnsureAdminAuthenticated();
                            int id = Convert.ToInt32(param?.Id ?? 0);

                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);

                            // Call method to delete category
                            return Functions.DeleteCategory(id, lang);
                        }
                    /*
                     * Type Atributte Methods
                     */
                    case Constants.ListAttributeType:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            return Functions.ListAttributeType(lang);
                        }
                    case Constants.GetAttributeType:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var id = Convert.ToInt32(param?.Id ?? 0);

                            if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            return Functions.GetAttributeType(id, lang);
                        }
                    case Constants.SaveAttributeType:
                        {
                            EnsureAdminAuthenticated();

                            // Get parameters
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            int id = Convert.ToInt32(param?.Id ?? 0);
                            string value = param?.Value ?? string.Empty;

                            // Check if the parameters are ok
                            if (string.IsNullOrWhiteSpace(value)) throw new Exception("A descrição da categoria deve ser informada");

                            // Call method to save category
                            return Functions.SaveAttributeType(id, lang, value);
                        }
                    case Constants.DeleteAttributeType:
                        {
                            EnsureAdminAuthenticated();
                            int id = Convert.ToInt32(param?.Id ?? 0);

                            // Call method to delete service
                            return Functions.DeleteAttributeType(id);
                        }

                    /*
                     * Atributte Methods
                     */
                    case Constants.ListAttribute:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            return Functions.ListAttribute(lang);
                        }
                    case Constants.GetAttribute:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var id = Convert.ToInt32(param?.Id ?? 0);

                            if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            return Functions.GetAttribute(id, lang);
                        }
                    case Constants.SaveAttribute:
                        {
                            EnsureAdminAuthenticated();

                            // Get parameters
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            int id = Convert.ToInt32(param?.Id ?? 0);
                            int idType = Convert.ToInt32(param?.IdType ?? 0);
                            string desc = param?.Description ?? string.Empty;
                            string image = param?.Image ?? string.Empty;
                            int? parent = (string.IsNullOrWhiteSpace((string)param?.IdParent ?? "")) ? (int?)null : Convert.ToInt32((string)param.IdParent);

                            // Check if the parameters are ok
                            if (string.IsNullOrWhiteSpace(desc)) throw new Exception("A descrição do atributo deve ser informada");

                            // Call method to save category
                            return Functions.SaveAttribute(id, parent, lang, idType, desc, image);
                        }
                    case Constants.DeleteAttribute:
                        {
                            EnsureAdminAuthenticated();
                            int id = Convert.ToInt32(param?.Id ?? 0);

                            // Call method to delete service
                            return Functions.DeleteAttribute(id);
                        }

                    /*
                     * Product Methods
                     */
                    case Constants.ListProduct:
                        {
                            // Parameter casting
                            if (!parameters.ContainsKey("Lang") || !int.TryParse(parameters["Lang"], out int lang))
                                lang = CMS.I18N.DefaultLanguage.Id;

                            if (!parameters.ContainsKey("FilterByUser") || !int.TryParse(parameters["FilterByUser"], out int filterByUser))
                                filterByUser = 0;

                            if (parameters.ContainsKey("Owner") && !String.IsNullOrWhiteSpace(parameters["Owner"]) && !parameters["Owner"].Contains("Selecione") && parameters["Owner"] != "0")
                                owner = parameters["Owner"];

                            if (!parameters.ContainsKey("Hierarchy") || !bool.TryParse(parameters["Hierarchy"], out bool hierarchy))
                                hierarchy = true;

                            var user = Plugin.CMS.Membership.AdminUser;
                            var listByUser = Plugin.CMS.Configuration.Get(Database.IsUserOwned, Plugin.PluginName).Contains("true");

                            // Validation if need to filter by user or list all
                            int? filterUser = null;
                            var editRoles = Functions.ListRolesEditAllProduct();
                            var editProductRole = editRoles.FirstOrDefault(x => x.Id == user.Role.Id);

                            if (user != null && user.Role.Name != "Administrador" && editProductRole == null && listByUser)
                                filterUser = user.Id;

                            if (filterByUser != 0)
                                filterUser = filterByUser;

                            var products = Functions.ListProduct(lang, hierarchy, filterUser, owner);

                            // Get products and subproducts
                            
                            return products;
                        }
                    case Constants.ListProductsPagged:
                        {
                            var page = 1;
                            var priceRangeMin = "";
                            var priceRangeMax = "";
                            var size = 20;
                            var search = "";
                            var fieldNameFilter = "";
                            var fieldValueFilter = "";
                            var idCategory = -5;
                            var idType = Array.Empty<string>();
                            var idAttribute = -5;
                            var listFilterField = new List<FilterField>();
                            var listFilterAttribute = new List<FilterField>();

                            if (parameters.ContainsKey("Search") && !String.IsNullOrWhiteSpace(param?.Search))
                                search = param?.Search;

                            if (parameters.ContainsKey("FieldNameFilter") && !String.IsNullOrWhiteSpace(param?.FieldNameFilter))
                                fieldNameFilter = param?.FieldNameFilter;

                            if (parameters.ContainsKey("IdType") && !String.IsNullOrWhiteSpace(param?.IdType))
                                idType = param?.IdType == null ? Array.Empty<string>() : JsonConvert.DeserializeObject<List<string>>(parameters["IdType"].ToString()).ToArray();

                            if (parameters.ContainsKey("FieldValue") && !String.IsNullOrWhiteSpace(param?.FieldValue))
                                fieldValueFilter = param?.FieldValue;

                            if (parameters.ContainsKey("PriceRangeMin") && !String.IsNullOrWhiteSpace(param?.PriceRangeMin))
                                priceRangeMin = param?.PriceRangeMin;

                            if (parameters.ContainsKey("PriceRangeMax") && !String.IsNullOrWhiteSpace(param?.PriceRangeMax))
                                priceRangeMax = param?.PriceRangeMax;

                            if (!parameters.ContainsKey("FieldName") || !parameters.TryGetValue("FieldName", out var fieldName))
                                fieldName = "";

                            if (parameters.ContainsKey("FieldFilters") && !String.IsNullOrWhiteSpace(param?.FieldFilters))
                                listFilterField = JsonConvert.DeserializeObject<List<FilterField>>(param?.FieldFilters);

                            if (parameters.ContainsKey("AttributeFilters") && !String.IsNullOrWhiteSpace(param?.AttributeFilters))
                                listFilterAttribute = JsonConvert.DeserializeObject<List<FilterField>>(param?.AttributeFilters);

                            if (parameters.ContainsKey("IdCategory") && !String.IsNullOrWhiteSpace(param?.IdCategory))
                                idCategory = Convert.ToInt32(param?.IdCategory);

                            if (parameters.ContainsKey("IdAttribute") && !String.IsNullOrWhiteSpace(param?.IdAttribute))
                                idAttribute = Convert.ToInt32(param?.IdAttribute);

                            if (!parameters.ContainsKey("Sort") || !parameters.TryGetValue("Sort", out var sort))
                                sort = nameof(Product.Description);
                            if (!parameters.ContainsKey("SortOrder") || !parameters.TryGetValue("SortOrder", out var sortOrder) || (sortOrder != "ASC" && sortOrder != "DESC"))
                                sortOrder = "ASC";

                            if (parameters.ContainsKey("Owner") && !String.IsNullOrWhiteSpace(parameters["Owner"]) && !parameters["Owner"].Contains("Selecione") && parameters["Owner"] != "0")
                                owner = parameters["Owner"];

                            if (parameters.ContainsKey("page"))
                                page = Convert.ToInt32(param?.page);

                            if (parameters.ContainsKey("size"))
                                size = Convert.ToInt32(param?.size);


                            // Parameter casting
                            if (!parameters.ContainsKey("Lang") || !int.TryParse(parameters["Lang"], out int lang))
                                lang = CMS.I18N.DefaultLanguage.Id;
                            if (!parameters.ContainsKey("FilterByUser") || !int.TryParse(parameters["FilterByUser"], out int filterByUser))
                                filterByUser = 0;

                            var user = Plugin.CMS.Membership.AdminUser;
                            var listByUser = Plugin.CMS.Configuration.Get(Database.IsUserOwned, Plugin.PluginName).Contains("true");

                            // Validation if need to filter by user or list all
                            int? filterUser = null;
                            var editRoles = Functions.ListRolesEditAllProduct();
                            var editProductRole = editRoles.FirstOrDefault(x => x.Id == user.Role.Id);
                            if (user != null && user.Role.Name != "Administrador" && editProductRole == null && listByUser)
                                filterUser = user.Id;

                            if (filterByUser != 0)
                                filterUser = filterByUser;

                            // Get products and subproducts
                            var products = Functions.ListProduct(lang, true, filterUser);

                            // TODO: Arrumar esta cagada
                            if (idCategory != -5)
                            {
                                var categories = Functions.GetFlatCategories(Functions.ListCategory(lang));
                                var category = categories.FirstOrDefault(c => c.Id == idCategory);
                                var children = Functions.GetFlatCategories(category.Children);
                                products = products.Where(p => p.Categories.Any(c => c.IdCategory == category.Id || children.Any(i => i.Id == c.IdCategory)) && !p.Disabled).ToList();
                            }

                            if (idAttribute != -5)
                            {
                                products = products.Where(p => p.Attributes.Any(a => a.IdAttribute == idAttribute)).ToList();
                            }

                            if (listFilterAttribute.Any())
                            {
                                var newProducts = new List<Product>();

                                foreach (var field in listFilterAttribute)
                                {
                                    newProducts.AddRange(products.Where(p => p.Attributes.Any(a => a.Attribute.Description == field.Value && a.Attribute.Type == field.Field)).Distinct().ToList());
                                }

                                products = newProducts;
                            }

                            if (idType.Length != 0)
                            {
                                products = products.Where(p => idType.Any(t => t == p.IdType.ToString())).ToList();
                            }

                            // TODO: Arrumar -> cagada!!!
                            if (!string.IsNullOrWhiteSpace(priceRangeMin) && !string.IsNullOrWhiteSpace(priceRangeMax))
                            {
                                var bottom = decimal.Parse(priceRangeMin);
                                var top = decimal.Parse(priceRangeMax);

                                if (bottom < top && top > 0 && fieldName != null)
                                {
                                    products = products.Where(p => p.Fields != null
                                    && p.Fields.Any(f => f.Name != null
                                    && f.Value != null
                                    && f.Name == fieldName
                                    && decimal.Parse(f.Value) >= bottom
                                    && decimal.Parse(f.Value) <= top
                                    )).ToList();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(fieldNameFilter) && !string.IsNullOrWhiteSpace(fieldValueFilter))
                            {
                                products = products.Where(p => p.Fields != null
                                && p.Fields.Any(f => f.Name != null
                                && f.Value != null
                                && f.Name == fieldNameFilter
                                && f.Value.ToUpper().Contains(fieldValueFilter.ToUpper())
                                )).ToList();
                            }

                            // Set product order
                            var fieldType = Functions.ListField(lang).FirstOrDefault(f => f.Name.Equals(sort, StringComparison.CurrentCultureIgnoreCase));
                            if (fieldType == null)
                                fieldType = Functions.ListField(lang).FirstOrDefault(f => f.Name == nameof(Product.Description));

                            if (fieldType?.Type == "number")
                            {
                                products = (sortOrder == "ASC" ? products.OrderBy(p => Convert.ToDecimal(p[sort])).ToList() :
                                                                 products.OrderByDescending(p => Convert.ToDecimal(p[sort])).ToList());
                            }
                            else
                            {
                                products = (sortOrder == "ASC" ? products.OrderBy(p => p[sort]).ToList() :
                                                                 products.OrderByDescending(p => p[sort]).ToList());
                            }

                            // TODO: Revisar lógica capeta
                            if (listFilterField.Any())
                            {
                                //products = products.Where(p => listFilterField.Any(l => p.Fields != null && p.Fields.Any(f => f.Name == l.Field && f.Value == l.Value))).ToList();

                                var newProducts = new List<Product>();

                                foreach (var field in listFilterField)
                                {

                                    newProducts.AddRange(products.Where(p => p.Fields != null && p.Fields.Any(f => f.Name != null && f.Value != null && f.Name.ToUpper() == field.Field.ToUpper() && f.Value.ToUpper() == field.Value.ToUpper())));
                                }

                                products = newProducts;
                            }

                            //If search is not empty filter products.
                            if (owner != "" && !String.IsNullOrEmpty(owner))
                                products = products.Where(x => x.Owners.Any(y => y.IdUser == Convert.ToInt32(owner))).ToList();


                            if (!String.IsNullOrWhiteSpace(search))
                                products = products.Where(x => x.SKU.ToUpper().Contains(search.ToUpper())
                                                         || x.Description.ToUpper().Contains(search.ToUpper())
                                                         || (x.Owners != null && x.Owners.Any(y => y.Name.ToUpper().Contains(search.ToUpper())))).ToList();


                            var count = products.Count();

                            //Paginação dos dados
                            products = products.Skip((page - 1) * size).Take(size).Distinct().ToList();

                            var result = new PaggedResult<Product>
                            {
                                Count = count,
                                Page = page,
                                Size = size,
                                CountPage = Convert.ToInt32(Math.Ceiling((decimal)count / size)),
                                Records = products,
                            };

                            var filteredResult = new FilteredResult
                            {
                                CountPaggedProducts = products.Count,
                                Pagged = result,
                                Filters = new List<Filter>
                                {
                                   new Filter
                                   {
                                       Name = "Search",
                                       Value = search,
                                   }
                                   ,new Filter
                                   {
                                       Name = "Owner",
                                       Value = owner
                                   }
                                }
                            };

                            return filteredResult;
                        }
                    case Constants.CountProductUser:
                        {
                            return Functions.CountProductsUser();
                        }
                    case Constants.ProductUser:
                        {
                            var idUser = Convert.ToInt32(param?.IdUser ?? 0);

                            return Functions.ProductsUser(idUser);
                        }
                    case Constants.GetProduct:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var id = Convert.ToInt32(param?.Id ?? 0);

                            if (id == 0) throw new Exception("O valor do Id deve ser informado.");

                            return Functions.GetProduct(id, lang);
                        }
                    case Constants.GetProductByUrl:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            if (!parameters.ContainsKey("Url") || !parameters.TryGetValue("Url", out var url))
                                url = "";

                            if (string.IsNullOrWhiteSpace(url))
                                throw new Exception("O valor da Url deve ser informado.");

                            return Functions.GetProductByUrl(url, lang);
                        }
                    case Constants.GetProductBySku:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            if (!parameters.ContainsKey("Sku") || !parameters.TryGetValue("Sku", out var sku))
                                sku = "";

                            if (string.IsNullOrWhiteSpace(sku))
                                throw new Exception("O valor do Sku deve ser informado.");

                            return Functions.GetProductBySku(sku, lang);
                        }
                    case Constants.ListUsers:
                        {
                            var rolesId = Plugin.CMS.Configuration.Get(Database.UserRolesToList, Plugin.PluginName);
                            string[] rolesIdArray = rolesId.Split(',');
                            var users = CMS.Membership.Members().Where(x => rolesIdArray.Contains(Convert.ToString(x.Role.Id))).ToList();
                            return users;
                        }
                    case Constants.ListUsersEditAll:
                        {
                            var roles = Functions.ListRolesEditAllProduct();
                            return roles;
                        }
                    case Constants.GetProductsByIds:
                        {
                            var lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var ids = param.Ids;

                            return Functions.ListProduct(ids, lang);
                        }
                    case Constants.SearchProducts:
                        {
                            var lang = parameters.ContainsKey("Lang") ? Convert.ToInt32(parameters["Lang"]) : Convert.ToInt32(CMS.I18N.DefaultLanguage.Id);
                            var search = parameters.ContainsKey("Search") ? parameters["Search"] : null;
                            var idtype = parameters.ContainsKey("IdType") ? parameters["IdType"] : "1";

                            return Functions.SearchProducts(lang, search, idtype);
                        }
                    case Constants.SearchByValuesInclusive:
                        {
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            string attributes = param?.Attributes ?? string.Empty;
                            string category = param?.Category ?? string.Empty;



                            return Functions.SearchByValuesInclusive(lang, attributes, category);
                        }
                    case Constants.SaveProduct:
                        {
                            EnsureAdminAuthenticated();

                            bool hide = false;
                            bool disabled = false;
                            int IdUser = 0;
                            string desc = "";

                            // Get parameters
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            string attributes = "";
                            int id = Convert.ToInt32(param?.Id ?? 0);
                            int idtype = Convert.ToInt32(param?.IdType ?? 0);
                            int idProduct = Convert.ToInt32(param?.IdProduct ?? 0);
                            string sku = param?.SKU ?? string.Empty;

                            string url = param?.Url ?? string.Empty;
                            //string search = param?.Search ?? string.Empty;

                            if (parameters.ContainsKey("Description"))
                                desc = param?.Description ?? string.Empty;

                            if (parameters.ContainsKey("Hide"))
                                hide = parameters["Hide"].ContainsIgnoreCase("true");

                            if (parameters.ContainsKey("Disabled"))
                                disabled = parameters["Disabled"].ContainsIgnoreCase("true");

                            if (parameters.ContainsKey("IdUser"))
                                IdUser = Convert.ToInt32(param?.IdUser ?? 0);

                            string categories = param?.Categories ?? string.Empty;
                            string related = param?.Related ?? string.Empty;
                            if (parameters.ContainsKey("Attributes"))
                                attributes = param?.Attributes ?? string.Empty;

                            // Check if the parameters are ok
                            if (string.IsNullOrWhiteSpace(sku)) throw new Exception("O SKU do produto deve ser informado.");
                            //if (string.IsNullOrWhiteSpace(desc)) throw new Exception("A descrição da categoria deve ser informada");
                            //if (string.IsNullOrWhiteSpace(url)) throw new Exception("A Url da categoria deve ser informada");

                            // Call method to save category
                            return Functions.SaveProduct(id, idtype, lang, sku, disabled, hide, categories, related, param, attributes, idProduct, IdUser);
                        }
                    case Constants.SortProducts:
                        {
                            EnsureAdminAuthenticated();

                            // Convert parameter and call the function
                            var sort = param?.Sort;
                            return Functions.SortProducts(sort);
                        }
                    case Constants.DeleteProduct:
                        {
                            EnsureAdminAuthenticated();
                            int id = Convert.ToInt32(param?.Id ?? 0);

                            // Call method to delete category
                            return Functions.DeleteProduct(id);
                        }
                    case Constants.SaveCombo:
                        {
                            int idcombo = Convert.ToInt32(param?.IdCombo ?? 0);
                            int idproduct = Convert.ToInt32(param?.IdProduct ?? 0);

                            // Call method to delete category
                            return Functions.SaveCombo(idcombo, idproduct);
                        }
                    case Constants.DeleteCombo:
                        {
                            int idcombo = Convert.ToInt32(param?.IdCombo ?? 0);
                            int idproduct = Convert.ToInt32(param?.IdProduct ?? 0);

                            return Functions.DeleteProductCombo(idcombo, idproduct);
                        }
                    case Constants.SaveQuantity:
                        {
                            EnsureAdminAuthenticated();
                            int idcombo = Convert.ToInt32(param?.IdCombo ?? 0);
                            int idproduct = Convert.ToInt32(param?.IdProduct ?? 0);
                            int quantidade = Convert.ToInt32(param?.Quantidade ?? 0);

                            return Functions.SaveQuantity(idcombo, idproduct, quantidade);
                        }
                    case Constants.ListProductTypes:
                        {
                            return Functions.ListProductTypes();
                        }
                    case Constants.ListProductAttribute:
                        {
                            return Functions.ListProductAttribute();
                        }
                    case Constants.SearchByValues:
                        {
                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            string attributes = param?.Attributes ?? string.Empty;
                            string category = param?.Category ?? string.Empty;

                            return Functions.SearchByValues(lang, attributes, category);
                        }
                    case Constants.SortProductsCombo:
                        {
                            EnsureAdminAuthenticated();

                            // Convert parameter and call the function
                            var sort = param?.Sort;
                            var combo = Convert.ToInt32(param?.IdCombo ?? 0);

                            return Functions.SortProductsCombo(sort, combo);
                        }
                    case Constants.ExportProducts:
                        {
                            EnsureAdminAuthenticated();

                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var idType = Convert.ToInt32(param?.IdType);
                            return Functions.ExportProducts(lang, idType);
                        }
                    case Constants.ImportProducts:
                        {
                            EnsureAdminAuthenticated();

                            if (files == null || files.Count == 0)
                                return "";

                            int lang = Convert.ToInt32(param?.Lang ?? CMS.I18N.DefaultLanguage.Id);
                            var filename = files[0].FileName;
                            var stream = files[0].InputStream;
                            var idType = Convert.ToInt32(param?.IdType);

                            int? userId = null;
                            if (param?.userName != null)
                            {
                                var userName = (string)param?.userName;
                                var users = CMS.Membership.Members().Where(x => x.Role.Name != "Administrador" && x.Role.Name != "Designer" && x.Role.Name != "Editor").ToList();
                                userId = users.FirstOrDefault(m => m.FirstName == userName)?.Id;
                            }

                            return Functions.ImportProducts(stream, filename, lang, idType, userId);
                        }

                    /*
                     * Custom Fields
                     */
                    case Constants.GetCustomFields:
                        {
                            int idtype = Convert.ToInt32(param?.IdType ?? 0);
                            return Functions.GetCustomFields(idtype);
                        }

                    // TODO: Remover esta zika
                    case Constants.GetUsers:
                        {
                            var users = CMS.Membership.Members().Where(x => x.Role.Name != "Administrador" && x.Role.Name != "Designer" && x.Role.Name != "Editor").Select(x => x.FirstName).ToList();
                            return users;
                        }
                    case Constants.GetProductRelated:
                        {

                            EnsureAdminAuthenticated();

                            int idproduct = Convert.ToInt32(param?.IdProduct ?? 0);

                            var result = Functions.GetProductRelated(idproduct);

                            return result;

                        }


                    default:
                        throw new Exception("Comando não identificado para processamento.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
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

            var NameProdutos = Plugin.CMS.Configuration.Get(Database.NameCadastros, Plugin.PluginName);
            // Create the menu to show in the system
            this.Menus.Add(new Menu()
            {
                Name = "Produtos",
                Items = new List<IMenuItem> {
                    new MenuItem() { Title = "Categorias", Function = "Category", Icon = "wb-tag" },
                    new MenuItem() { Title = "Tipos de Atributos", Function = "AttributeType", Icon = "wb-book" },
                    new MenuItem() { Title = "Atributos", Function = "Attribute", Icon = "fa-hotel" },
                    new MenuItem() { Title = NameProdutos, Function = "ProductPagged", Icon = "wb-inbox" },
                    //new MenuItem() { Title = "Combo de Produtos", Function = "Combo", Icon = "wb-library" }
                    //new MenuItem() { Title = "Relacionados", Function = "Related", Icon = "wb-link" },
                }
            });

            // Create database tables
            Database.Setup();



            // Return a new instance of this class
            return this;
        }

        public bool Uninstall()
        {
            // Execute database Uninstall Command
            Database.Uninstall();
            return true;
        }


        /// <summary>
        /// Method to return default Routes to the system
        /// </summary>
        /// <returns></returns>
        public IList<IRoute> Routes()
        {
            // Get page configuration
            var routes = new List<IRoute>();
            var page = CMS.Configuration.Get(Constants.ConfigProductPage, PluginName);
            if (string.IsNullOrWhiteSpace(page))
                return routes;

            // Check if the template exists
            var template = CMS.Functions.Templates.FirstOrDefault(t => t.Name.Equals(page, StringComparison.CurrentCultureIgnoreCase));
            if (template == null)
                return routes;

            var section = CMS.Configuration.Get(Constants.ConfigProductSection, PluginName);

            // Loop through each available item to generate the route
            foreach (var lang in CMS.I18N.AvailableLanguages)
            {
                // Get the itens for available language
                var products = Functions.ListProduct(lang.Id);
                var categories = Functions.ListCategory(lang.Id);

                // Create routes for the available products
                routes.AddRange(products.Where(p => !string.IsNullOrWhiteSpace(p?.Url)).Select(p => new Route()
                {
                    IdRoute = $"Product-{p?.Id}",
                    Language = lang,
                    Page = template,
                    Section = section,
                    Url = p?.RouteUrl,
                    Type = "Product",
                    Parameters = new[] { p?.Id.ToString() },
                    LastModified = p.UpdatedAt,
                    Search = new SearchResult()
                    {
                        Category = "Product",
                        Title = p?.Description,
                        Image = p?.Cover?.ToString(),
                        Url = p?.RouteUrl
                    }
                }));

                // Create routes for the available categories
                routes.AddRange(GetCategoryUrl(categories, lang, template, section));
            }

            return routes;
        }

        /// <summary>
        /// Method to process the Category URLs
        /// </summary>
        /// <param name="categories"></param>
        /// <returns></returns>
        private IEnumerable<IRoute> GetCategoryUrl(IList<Category> categories, Language lang, Template template, string section)
        {
            var routes = new List<IRoute>();
            foreach (var category in categories)
            {
                routes.Add(new Route()
                {
                    IdRoute = $"Category-{category.Id}",
                    Language = lang,
                    Page = template,
                    Section = section,
                    Url = category.Url,
                    Type = "Category",
                    Parameters = new[] { category.Id.ToString() },
                    Search = new SearchResult()
                    {
                        Category = "Category",
                        Title = category.Description,
                        Image = category.Image?.ToString(),
                        Url = category.Url
                    }
                });

                if (category.Children.Count > 0)
                    routes.AddRange(GetCategoryUrl(category.Children, lang, template, section));
            }

            return routes;
        }

        /// <summary>
        /// Method to replicate all the idiom keys
        /// </summary>
        public void ReplicateIdiomKeys()
        {
            Functions.ReplicateIdiomKeys();
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
            return;
        }
    }
}