using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.PagFun.Models
{
    [Table("pagfun_extract")]
    public class Extract
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdUser { get; set; }

        public string OperationType { get; set; }

        public string OperationDetail { get; set; }

        public decimal CreditValue { get; set; }

        public bool Disabled { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}