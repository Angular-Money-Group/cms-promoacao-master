using Bitzar.CMS.Data.Model;
using Bitzar.ECommerce.Models;
using System.Data.Entity;

namespace Bitzar.Payments.Models
{
    public class Database : DbDatabaseContext
    {
        public DbSet<PayerIdentifier> PayerIdentifiers { get; set; }

        public DbSet<PayerCard> PayerCards { get; set; }

        public DbSet<OrderPayment> OrderPayments { get; set; }
    }
}
