using System;

namespace Bitzar.PagFun.Models
{
    public class FormattedEvent
    {
        public int? EventId { get; set; }

        public string EventName { get; set; }

        public string EventLocal { get; set; }

        public string EventImage { get; set; }

        public int? EventImageId { get; set; }

        public string EventInicialDate { get; set; }

        public string EventFinalDate { get; set; }

        public decimal? PriceProduct { get; set; }
    }
}
