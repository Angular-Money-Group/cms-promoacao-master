using System;

namespace Bitzar.PagFun.Models
{
    public class Callback
    {
        public string platform { get; set; }
        public string evento { get; set; }
        public string eventId { get; set; }
        public string referenceId { get; set; }
    }
}