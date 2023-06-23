using Bitzar.CMS.Data.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;

namespace Bitzar.PagFun.Models
{
    public class Database : DbDatabaseContext
    {
        public DbSet<Bitzar.Tickets.Models.Ticket> Ticket { get; set; }
        public DbSet<Bitzar.PagFun.Models.Extract> Extract { get; set; }
        public DbSet<Bitzar.PagFun.Models.PromoterInvite> PromoterInvite { get; set; }
        public DbSet<Bitzar.PagFun.Models.PromoterEvents> PromoterEvents { get; set; }
    }
}
