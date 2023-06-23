using Newtonsoft.Json;

namespace Bitzar.CMS.Core.Areas.api.Models
{
    public class NotificationModel
    {
        public string RecurrentPaymentId { get; set; }
        public string PaymentId { get; set; }
        public string ChangeType { get; set; }
    }
}