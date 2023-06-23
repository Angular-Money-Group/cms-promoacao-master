using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_coupon_usage")]
    public class CouponUsage
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore()]
        public int Id { get; set; }

        [JsonIgnore]
        public int IdCoupon { get; set; }

        [JsonIgnore]
        public int IdCustomer { get; set; }

        [JsonIgnore]
        public int IdOrder { get; set; }

        [JsonIgnore]
        [ForeignKey("IdCoupon")]
        public virtual Coupon Coupon { get; set; }

        public decimal DiscountAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
