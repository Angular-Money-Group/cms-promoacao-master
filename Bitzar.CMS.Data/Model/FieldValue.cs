using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class FieldValue
    {
        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key, Column(Order = 2)]
        public int IdField { get; set; }
        [Key, Column(Order = 3)]
        public int IdLanguage { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }

        [ForeignKey("IdField")]
        public virtual Field Field { get; set; }
        [ForeignKey("IdLanguage")]
        public virtual Language Language { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}-{IdField}-{IdLanguage}";
        }
    }
}
