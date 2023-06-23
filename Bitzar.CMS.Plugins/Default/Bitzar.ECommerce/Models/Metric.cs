using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Bitzar.ECommerce.Helpers.Enumerators;

namespace Bitzar.ECommerce.Models
{
    public class Metric
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Color { get; set; }
        public OrderStatus Status { get; set; }
    }
}