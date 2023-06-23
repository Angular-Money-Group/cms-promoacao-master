using Bitzar.ECommerce.Helpers;
using Bitzar.ECommerce.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using static Bitzar.ECommerce.Helpers.Enumerators;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_order")]
    public class Order
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Property to store a auto generated key to identify the order through sessions
        /// </summary>
        [MaxLength(50), Index("IX_Order_Uuid", IsUnique = true)]
        [JsonProperty(Order = 1)]
        public string Uuid { get; private set; } = Guid.NewGuid().ToString();

        [NotMapped]
        [JsonProperty(Order = 2)]
        public string Customer { get; set; }

        /// <summary>
        /// Property to store the date and time of the object creation, 
        /// </summary>
        [JsonProperty(Order = 3)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Property to set the User id to the service
        /// </summary>
        [JsonIgnore]
        public int? IdCustomer { get; set; }

        /// <summary>
        /// Property to defined the order type status
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Cart;

        /// <summary>
        /// Property to define the id of payment type
        /// </summary>
        [JsonProperty(Order = 4, NullValueHandling = NullValueHandling.Ignore)]
        public int? IdPaymentMethod { get; set; }

        /// <summary>
        /// Property to define the id of the Payment condition
        /// </summary>
        [JsonProperty(Order = 5, NullValueHandling = NullValueHandling.Ignore)]
        public int? IdPaymentCondition { get; set; }

        /// <summary>
        /// Property to store the shipping id data
        /// </summary>
        [JsonProperty(Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        public int? IdShippingMethod { get; set; }

        /// <summary>
        /// Property to store the current shipping amount
        /// </summary>
        [JsonProperty(Order = 7, NullValueHandling = NullValueHandling.Ignore)]
        public decimal? ShippingAmount { get; set; }

        /// <summary>
        /// Property to store an Index to be added to the order total
        /// </summary>
        [JsonProperty(Order = 8)]
        public decimal Index { get; set; } = 1;

        /// <summary>
        /// Property to return the order items amount
        /// </summary>
        [NotMapped]
        [JsonProperty(Order = 11)]
        public decimal TotalItems => this.Items?.Sum(i => i.Total) ?? 0;

        /// <summary>
        /// Property to return the order amount including shipping tax
        /// </summary>
        [NotMapped]
        [JsonProperty(Order = 12)]
        public decimal TotalOrder => (this.TotalItems * this.Index) + (this.ShippingAmount ?? 0);

        /// <summary>
        /// Property to navigate throught all the order items
        /// </summary>
        [JsonProperty(Order = 10)]
        public virtual ICollection<OrderDetail> Items { get; set; } = new List<OrderDetail>();

        /// <summary>
        /// Property to navigate throught all the order items
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderField> Fields { get; set; } = new List<OrderField>();

        /// <summary>
        /// Property to navigate throught all the order items
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderHistory> History { get; set; } = new List<OrderHistory>();

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
        [JsonProperty(PropertyName = "Fields", Order = 13)]
        public ExpandoObject FieldsFormatted => Functions.DynamicFields(this.Fields.Select(f => (IField)f).ToList());

        /// <summary>
        /// Property to navigate throught all the order items
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
    }
}
