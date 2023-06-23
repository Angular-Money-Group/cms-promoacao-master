using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.PagFun.Models
{
    [Table("pagfun_promoter_events")]
    public class PromoterEvents
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string IdPromoter { get; set; }

        public int IdEvent { get; set; }

        public int IdTicket { get; set; }

        public int TicketQuantity { get; set; }

        public decimal TicketValue { get; set; }

        public bool ChargeFee { get; set; }

        public decimal TicketsTax { get; set; }

        public bool Vip { get; set; }
    }
}