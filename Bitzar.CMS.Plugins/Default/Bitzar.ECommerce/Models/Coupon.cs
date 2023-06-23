using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Bitzar.ECommerce.Helpers.Enumerators;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_coupon")]
    public class Coupon
    {
        /// <summary>
        /// Order id to identify the user order
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdUser { get; set; }

        [MaxLength(30), Index("IX_Coupon_Code", IsUnique = true)]
        public string Code { get; set; }

        public int UsageLimit { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        public int IdEvent { get; set; }

        public int IdCabin { get; set; }

        public int IdOccupancy { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        //Only Product || Product and Fees
        public CouponType CouponType { get; set; }

        //Fixed |! Percentage
        public DiscountType DiscountType { get; set; }

        public decimal DiscountAmount { get; set; }

        public bool Disabled { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    }
}
