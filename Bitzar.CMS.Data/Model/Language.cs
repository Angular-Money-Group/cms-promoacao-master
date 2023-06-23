using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class Language
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(5), Required]
        public string Culture { get; set; }
        [MaxLength(50), Required]
        public string Description { get; set; }
        [MaxLength(255)]
        public string UrlRoute { get; set; }
        [MaxLength(50), Required]
        public string DateTimeFormat { get; set; } = "g";
        [MaxLength(10), Required]
        public string NumberFormat { get; set; } = "N2";
        [MaxLength(10), Required]
        public string CurrencyFormat { get; set; } = "C2";
        [MaxLength(20), Required]
        public string DateFormat { get; set; } = "d";
        [MaxLength(20), Required]
        public string TimeFormat { get; set; } = "T";

        public virtual ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
