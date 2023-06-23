using Bitzar.Tickets.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static Bitzar.Tickets.Helpers.Enumerators;

namespace Bitzar.Tickets.Models
{
    public class FormattedSection
    {
        public int? SectionId { get; set; }

        public string SectionName { get; set; }

        public string SectionTicketName { get; set; }

        public string SectionDateTime { get; set; }

        public string SectionTicketTax { get; set; }

        public string SectionTicketValue { get; set; }
    }
}
