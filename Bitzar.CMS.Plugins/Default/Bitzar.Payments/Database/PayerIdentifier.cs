using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Bitzar.Payments.Models.Transaction;

namespace Bitzar.Payments.Models
{
    [Table("btz_payeridentifier")]
    public class PayerIdentifier
    {
        /// <summary>
        /// Register identifier
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public int IdUser { get; set; }

        /// <summary>
        /// Payer identifier on the especific platform
        /// </summary>
        [MaxLength(50), Index("IX_Payer_id", IsUnique = true)]
        public string PayerId { get; set; }

        /// <summary>
        /// Payer cards collection
        /// </summary>
        public List<PayerCard> PayerCards { get; set; }

        /// <summary>
        /// Gateway information
        /// </summary>
        public PaymentGateway Gateway { get; set; }

        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Date of alteration
        /// </summary>
        public DateTime AlterationDate { get; set; }
    }
}