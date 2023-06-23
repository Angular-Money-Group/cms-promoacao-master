using Bitzar.CMS.Data.Model;
using System.Data.Entity;

namespace Bitzar.Tickets.Models
{
    public class Database : DbDatabaseContext
    {
        public DbSet<Ticket> Ticket { get; set; }

        public DbSet<Passenger> Passenger { get; set; }

        public DbSet<EmailUnresgisteredPassenger> EmailUnresgisteredPassenger { get; set; }
    }
}
