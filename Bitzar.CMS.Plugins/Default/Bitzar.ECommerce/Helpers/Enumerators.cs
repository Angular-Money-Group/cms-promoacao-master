using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.ECommerce.Helpers
{
    public class Enumerators
    {
        /// <summary>
        /// Define the order status possibilities
        /// </summary>
        public enum OrderStatus
        {
            /// <summary>
            /// Initial cart status that the user is handling data inside it
            /// </summary>
            Cart = 0,
            /// <summary>
            /// The user has effectivated the shopping cart and now it's an order
            /// </summary>
            Order = 1,
            /// <summary>
            /// Awaiting approval from external services, like payment gateways
            /// </summary>
            AwaitingApproval = 2,
            /// <summary>
            /// Approved by external services
            /// </summary>
            Approved = 3,
            /// <summary>
            /// Started transference to the customer
            /// </summary>
            InTransfer = 4,
            /// <summary>
            /// Item sended by ecommerce
            /// </summary>
            Sended = 14,
            /// <summary>
            /// Process complete. Ordering, Payment, Transfering
            /// </summary>
            Completed = 5,
            /// <summary>
            /// Status of the cart that the user has left the order without completion.
            /// </summary>
            Abandoned = 11,
            /// <summary>
            /// Cart has been deleted by administrator or something else
            /// </summary>
            Deleted = 12,
            /// <summary>
            /// Cart has been canceled by the administrator or by approval services
            /// </summary>
            Canceled = 13,
            /// <summary>
            /// Cart has been archivced and should not be viewed in management panel
            /// </summary>
            Archived = -1
        }

        public enum CouponType
        {
            Product = 0,
            ProductAndFee = 1
        }

        public enum DiscountType
        {
            Percentage = 0,
            Fixed = 1,
        }
    }
}
