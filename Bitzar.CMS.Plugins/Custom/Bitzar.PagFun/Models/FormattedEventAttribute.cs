using System;

namespace Bitzar.PagFun.Models
{
    public class FormattedEventAttribute
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLocal { get; set; }
        public string EventImage { get; set; }
        public int EventImageId { get; set; }
        public string EventInicialDate { get; set; }
        public string EventFinalDate { get; set; }
        public string CityId { get; set; }

    }
}
