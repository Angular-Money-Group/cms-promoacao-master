using Bitzar.CMS.Data.Model;
using System.Data.Entity;

namespace Bitzar.ECommerce.Models
{
    public class Database : DbDatabaseContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderField> OrderFields { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderDetailField> OrderDetailFields { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<OrderHistory> History { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }
    }
}
