using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Bitzar.Payments.Models.Transaction;

namespace Bitzar.Payments.Models
{
    [Table("btz_payercard")]
    public class PayerCard
    {
        /// <summary>
        /// Register identifier
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int payerId { get; set; }

        /// <summary>
        /// Payer
        /// </summary>
        [ForeignKey("payerId")]
        [JsonIgnore]
        public PayerIdentifier payer { get; set; }

        /// <summary>
        /// Card identifier on the especific platform
        /// </summary>
        [MaxLength(50), Index("IX_Card_id", IsUnique = true)]
        [JsonIgnore]
        public string CardId { get; set; }

        /// <summary>
        /// External card identifier
        /// </summary>
        [MaxLength(50), Index("IX_Card_uiid", IsUnique = true)]
        public string Uiid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Payer identifier on the especific platform
        /// </summary>
        [MaxLength(4)]
        public string LastFourDigits { get; set; }

        /// <summary>
        /// Card holder name
        /// </summary>
        public string HolderName { get; set; }

        /// <summary>
        /// Card's brand
        /// </summary>
        public string CardBrand { get; set; }

        /// <summary>
        /// Gateway information
        /// </summary>
        [JsonIgnore]
        public PaymentGateway Gateway { get; set; }

        /// <summary>
        /// Date of creation
        /// </summary>
        [JsonIgnore]
        public DateTime CreationDate { get; set; }

    }
}