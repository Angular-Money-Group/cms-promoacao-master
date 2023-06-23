using System;

namespace Bitzar.PagFun.Models
{
    public class EventTickets
    {
        public int EventId { get; set; }
        public string NomeIngresso { get; set; }
        public decimal ValorIngresso { get; set; }
        public DateTime DataInicio { get; set; }
    }
}