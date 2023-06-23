using System;

namespace Bitzar.PagFun.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Uuid { get; set; }

        public string OwnerName { get; set; }

        public int Status { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime AlterationDate { get; set; }

        public string Type { get; set; }

        public int IdSection { get; set; }

        public int IdEvent { get; set; }

        public string uuidOrder { get; set; }

        public int? IdUser { get; set; }

        public string QRguid { get; set; }
    }
}