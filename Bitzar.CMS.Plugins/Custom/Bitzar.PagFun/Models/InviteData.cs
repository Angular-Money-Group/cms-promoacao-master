using System;

namespace Bitzar.PagFun.Models
{
    public class InviteData
    {
        public string NomePromoter { get; set; }
        public string EmailPromoter { get; set; }
        public string IdPromoter { get; set; }        
        public string EventoNome { get; set; }
        public DateTime EventoData { get; set; }
        public string EventoLocal { get; set; }
        public string EventoCidade { get; set; }
        public string EventoBanner { get; set; }
        public string NomeOrganizador { get; set; }
        public string Url { get; set; }
    }
}