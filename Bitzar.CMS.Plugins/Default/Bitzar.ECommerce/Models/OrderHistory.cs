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
using static Bitzar.ECommerce.Helpers.Enumerators;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_orderhistory")]
    public class OrderHistory
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
        [JsonIgnore]
        public int IdOrder { get; set; }

        /// <summary>
        /// Date of the history to be tracker
        /// </summary>
        [JsonProperty(Order = 2)]
        public DateTime Date { get; private set; } = DateTime.Now;

        /// <summary>
        /// Property to defined the order type status
        /// </summary>
        [JsonProperty(Order = 3)]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Property to defined the history desciption to be stored
        /// </summary>
        [JsonProperty(Order = 4), MaxLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// Property to store any kind of data to be used in the service. Must be used carefully
        /// </summary>
        [JsonIgnore]
        public string Data { get; set; }

        /// <summary>
        /// Property to create the order and order detail relationship
        /// </summary>
        [JsonIgnore]
        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }
    }
}
