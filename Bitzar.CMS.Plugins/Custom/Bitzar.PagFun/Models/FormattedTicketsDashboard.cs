using System;

namespace Bitzar.PagFun.Models
{
    public class FormattedTicketsDashboard
    {
        public int IdProduct { get; set; }

        public int? Solds { get; set; }

        public int? IdSection { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public decimal? Price { get; set; }

        public decimal? RatePrice { get; set; }
    }
}
