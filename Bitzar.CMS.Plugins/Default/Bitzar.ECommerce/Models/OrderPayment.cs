using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.ECommerce.Models
{
    [Table("btz_orderpayment")]
    public class OrderPayment
    {
        /// <summary>
        /// Register identifier
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Property to store the order Id information
        /// </summary>
        [JsonIgnore]
        public int IdOrder { get; set; }

        /// <summary>
        /// Request Id
        /// </summary>
        [MaxLength(50)]
        public string RequestId { get; set; }

        /// <summary>
        /// Request Status
        /// </summary>
        public int RequestStatus { get; set; }

        /// <summary>
        /// Gateway information
        /// </summary>
        [JsonIgnore]
        public int Gateway { get; set; }

        /// <summary>
        /// Customer First Name
        /// </summary>
        [MaxLength(255)]
        public string CustomerFirstName { get; set; }

        /// <summary>
        /// Customer Last Name
        /// </summary>
        [MaxLength(255)]
        public string CustomerLastName { get; set; }

        /// <summary>
        /// Customer E-mail
        /// </summary>
        [MaxLength(255)]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Customer Document
        /// </summary>
        [MaxLength(40)]
        public string CustommerDocument { get; set; }

        /// <summary>
        /// Customer Phone
        /// </summary>
        [MaxLength(40)]
        public string CustomerPhone { get; set; }

        /// <summary>
        /// Customer Address Zip
        /// </summary>
        [MaxLength(20)]
        public string CustomerAddressZip { get; set; }

        /// <summary>
        /// Customer Address Public Place
        /// </summary>
        [MaxLength(255)]
        public string CustomerAddressPublicPlace { get; set; }

        /// <summary>
        /// Customer Address Number
        /// </summary>
        [MaxLength(40)]
        public string CustomerAddressNumber { get; set; }

        /// <summary>
        /// Customer Address Neighborhood
        /// </summary>
        [MaxLength(255)]
        public string CustomerAddressNeighborhood { get; set; }

        /// <summary>
        /// Customer Address City
        /// </summary>
        [MaxLength(255)]
        public string CustomerAddressCity { get; set; }

        /// <summary>
        /// Customer Address State
        /// </summary>
        [MaxLength(255)]
        public string CustomerAddressState { get; set; }

        /// <summary>
        /// Customer Address Country
        /// </summary>
        [MaxLength(255)]
        public string CustomerAddressCountry { get; set; }

        /// <summary>
        /// Order Amount
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// Order Operation Type (Pix/CreditCard/Boleto)
        /// </summary>
        [MaxLength(30)]
        public string OrderOperationType { get; set; }

        /// <summary>
        /// Card holder name
        /// </summary>
        [MaxLength(50)]
        public string PaymentCardHolder { get; set; }

        /// <summary>
        /// Payment Id on the especific platform
        /// </summary>
        [MaxLength(50)]
        public string PaymentPaymentId { get; set; }

        /// <summary>
        /// Installments
        /// </summary>
        public int PaymentInstallments { get; set; }

        /// <summary>
        /// Auth Code
        /// </summary>
        [MaxLength(80)]
        public string PaymentAuthCode { get; set; }

        /// <summary>
        /// NSU
        /// </summary>
        [MaxLength(80)]
        public string PaymentNsu { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [MaxLength(100)]
        public string PaymentDescription { get; set; }

        /// <summary>
        /// Card brand name
        /// </summary>
        [MaxLength(40)]
        public string PaymentCardBrand { get; set; }

        /// <summary>
        /// Last digits of Credit Card
        /// </summary>
        [MaxLength(4)]
        public string PaymentLastFourDigits { get; set; }

        /// <summary>
        /// Url on the especific operation type (Pix/Boleto)
        /// </summary>
        [MaxLength(255)]
        public string PaymentUrl { get; set; }

        /// <summary>
        /// Url on the especific operation type (Pix/Boleto)
        /// </summary>
        public string PaymentQrCode { get; set; }

        /// <summary>
        /// Url on the especific operation type (Boleto)
        /// </summary>
        [MaxLength(80)]
        public string PaymentBarCode { get; set; }

        /// <summary>
        /// Property to create the order and order payment relationship
        /// </summary>
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Property to create the order and order detail relationship
        /// </summary>
        [JsonIgnore]
        [ForeignKey("IdOrder")]
        public virtual Order Order { get; set; }

    }
}