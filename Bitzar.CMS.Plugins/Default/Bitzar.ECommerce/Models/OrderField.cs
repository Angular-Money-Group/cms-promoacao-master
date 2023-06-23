using Bitzar.ECommerce.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_orderfield")]
    public class OrderField : IField
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Property to store the order Id information
        /// </summary>
        [Index("IX_OrderField_IdOrder")]
        [JsonIgnore]
        public int IdOrder { get; set; }

        /// <summary>
        /// Property to store the field name of the order
        /// </summary>
        [MaxLength(50), Index("IX_OrderField_Field")]
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
        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }
    }
}
