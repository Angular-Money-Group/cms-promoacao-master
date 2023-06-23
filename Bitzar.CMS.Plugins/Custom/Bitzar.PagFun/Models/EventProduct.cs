using System;

namespace Bitzar.PagFun.Models
{
    public class EventProduct
    {
        public string EventName { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }        
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public DateTime? ConsumeDate { get; set; }
        public int Status { get; set; }
        public string UserReadQR { get; set; }
        public DateTime BuyDate { get; set; }
    }
}