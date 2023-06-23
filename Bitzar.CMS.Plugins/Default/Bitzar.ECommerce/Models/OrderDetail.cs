using Bitzar.ECommerce.Helpers;
using Bitzar.ECommerce.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_orderdetail")]
    public class OrderDetail
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore()]
        public int Id { get; set; }

        /// <summary>
        /// Property to store the order Id information
        /// </summary>
        [JsonIgnore,]
        public int IdOrder { get; set; }

        /// <summary>
        /// Property to store the product item id
        /// </summary>
        [JsonProperty(Order = 3)]
        public int IdProduct { get; set; }

        /// <summary>
        /// Property to store the quantity of s
        /// </summary>
        [JsonProperty(Order = 4)]
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Property to store the quantity of s
        /// </summary>
        [JsonProperty(Order = 5)]
        public decimal Price { get; set; } = 0;

        /// <summary>
        /// Property to store an additional index to the order item
        /// </summary>
        [JsonProperty(Order = 6)]
        public decimal Index { get; set; } = 1;

        /// <summary>
        /// Property to store the total amount of the current product id stored
        /// </summary>
        [JsonProperty(Order = 7)]
        public decimal Total => (this.Quantity * this.Price) * this.Index;

        /// <summary>
        /// Property to create the order and order detail relationship
        /// </summary>
        [JsonIgnore]
        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }

        /// <summary>
        /// Property to navigate throught all the order item fields
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderDetailField> Fields { get; set; } = new List<OrderDetailField>();

        /// <summary>
        /// Indexer to access the order fields
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [NotMapped, JsonIgnore]
        public string this[string key] => this.Fields?.FirstOrDefault(f => f.Field == key)?.Value ?? string.Empty;

        /// <summary>
        /// Property to return the fields formatted in the service
        /// </summary>
        [JsonProperty(PropertyName = "Fields", Order = 8)]
        public ExpandoObject FieldsFormatted => Functions.DynamicFields(this.Fields.Select(f => (IField)f).ToList());
    }
}
