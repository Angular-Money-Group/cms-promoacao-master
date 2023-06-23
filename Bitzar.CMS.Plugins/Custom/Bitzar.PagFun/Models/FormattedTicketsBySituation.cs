using System;

namespace Bitzar.PagFun.Models
{
    public class FormattedTicketsBySituation
    {
        public int IdEvent{ get; set; }

        public int Created { get; set; }

        public int Emitted { get; set; }

        public int Cancelled { get; set; }
    }
}
