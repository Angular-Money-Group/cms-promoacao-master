using Bitzar.ECommerce.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_orderdetailfield")]
    public class OrderDetailField : IField
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Property to store the order Id information
        /// </summary>
        [Index("IX_OrderDetailField_IdOrderDetail")]
        [JsonIgnore]
        public int IdOrderDetail { get; set; }

        /// <summary>
        /// Property to store the field name of the order
        /// </summary>
        [MaxLength(50), Index("IX_OrderDetailField_Field")]
        public string Field { get; set; }

        /// <summary>
        /// Property to store the field value
        /// </summary>
        [MaxLength(255)]
        public string Value { get; set; }

        /// <summary>
        /// Property to keep a property invisible in the system
        /// </summary>
        public bool Hidden { get; set; } = false;

        [JsonIgnore]
        [ForeignKey("IdOrderDetail")]
        public virtual OrderDetail OrderDetail { get; set; }
    }
}
