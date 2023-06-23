using System;
using System.Collections.Generic;

namespace Bitzar.PagFun.Models
{
    public class EventDetail
    {
        public int EventId { get; set; }

        public string SKU { get; set; }

        public string EventName { get; set; }

        public string EventLocal { get; set; }

        public int EventImageId { get; set; }

        public string EventImageUrl { get; set; }

        public DateTime EventInicialDate { get; set; }

        public DateTime EventFinalDate { get; set; }

        public int CityId { get; set; }

        public string City { get; set; }

        public bool Disabled { get; set; }

        public bool Hide { get; set; }
        public List<EventTickets> Tickets { get; set; }
    }
}