using Bitzar.CMS.Data.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Bitzar.Products.Models
{
    public class Product : ICloneable
    {
        // Default table fields
        public int Id { get; set; }
        public string SKU { get; set; }
        public int? Sort { get; set; }
        public bool Disabled { get; set; }
        public bool Hide { get; set; }
        public int IdType { get; set; }
        public List<SimpleUser> Owners { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public int Quantity { get; set; }

        public IList<Field> Fields { get; set; } = new List<Field>();
        public IList<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public IList<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public IList<UserProduct> Users { get; set; } = new List<UserProduct>();
        [JsonIgnore]
        public IList<Product> Related { get; set; } = new List<Product>();
        public IList<int> RelatedIds => Related.Select(r => r.Id).ToList();

        public IList<Product> SubProduct { get; set; } = new List<Product>();
        public IList<int> SubProductsIds => SubProduct.Any() ? SubProduct.Select(r => r.Id).ToList() : new List<int>();

        public IList<Product> ComboProduct { get; set; } = new List<Product>();
        public IList<int> ComboIds => ComboProduct.Select(r => r.Id).ToList();

        public string RouteUrl { get; set; }

        // Extensions
        public string Description => GetFieldValue("Description");
        public string Url => GetFieldValue("Url");
        public string Text => GetFieldValue("Text");
        public string Gallery => GetFieldValue("Gallery");

        /// <summary>
        ///  Return Images to the system
        /// </summary>
        public Library[] Images
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Gallery))
                    return Array.Empty<Library>();

                return Gallery.Split(',').Where(g => !string.IsNullOrWhiteSpace(g))
                              .Select(i => Plugin.CMS.Library.Object(Convert.ToInt32(i)))
                              .ToArray();
            }
        }

        /// <summary>
        ///  Return Images to the system
        /// </summary>
        public Library Cover => (Images.Length > 0 ? Images[0] : null);

        /// <summary>
        /// Public method to get field value or return empty value
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public string GetFieldValue(string field)
        {
            return (Fields.FirstOrDefault(f => f.Name.Equals(field, StringComparison.CurrentCultureIgnoreCase))?.Value ?? string.Empty);
        }

        public bool GetFieldReadOnly(string field)
        {
            return (Fields.FirstOrDefault(f => f.Name.Equals(field, StringComparison.CurrentCultureIgnoreCase))?.ReadOnly ?? false);
        }

        [JsonIgnore]
        public string this[string index] => GetFieldValue(index);

        /// <summary>
        /// Method to return the content as HTML content to be printed in the Pages
        /// </summary>
        /// <param name="field">Field name to lookup for the value and cast in the return</param>
        /// <returns>Returns an instance of HTMLString to be used in the views</returns>
        public HtmlString GetFieldValueAsHtml(string field) => new HtmlString(HttpUtility.HtmlDecode(GetFieldValue(field)));

        /// <summary>
        /// Method to return the content of the field converted as JSON object to the system
        /// </summary>
        /// <param name="field">Field name to lookup for the value and cast in the return</param>
        /// <returns>Returns an instance of a JSON object</returns>
        public dynamic GetFieldValueAsJson(string field) => JsonConvert.DeserializeObject(GetFieldValue(field));

        /// <summary>
        /// Function to clone a new object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Function to clone a new object
        /// </summary>
        /// <returns></returns>
        public Product CloneProduct()
        {
            return (Product)this.Clone();
        }
    }
}