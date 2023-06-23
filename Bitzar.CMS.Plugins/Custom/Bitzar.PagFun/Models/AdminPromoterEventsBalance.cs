using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.PagFun.Models
{    public class AdminPromoterEventsBalance
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public decimal FullValue { get; set; }
        public decimal? PromoterValue { get; set; }
        public int Orders { get; set; }
        public int Storage { get; set; }
        public int SoldTickets { get; set; }
        public int StorageBalance { get; set; }
        public int DirectSoldTickets { get; set; }
        public int PromoterSoldTickets { get; set; }
        public int PromoterStorage { get; set; }
        public int PromoterStorageBalance { get; set; }
        public decimal DirectRecipy { get; set; }
        public decimal PromoterRecipy { get; set; }
    }
}