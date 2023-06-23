using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class Field
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public int? IdTemplate { get; set; }
        [MaxLength(100), Required]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        public int IdFieldType { get; set; }
        [MaxLength(50), Required]
        public string Group { get; set; }
        public int Order { get; set; }
        public bool Resource { get; set; } = false;
        public string SelectData { get; set; }

        [ForeignKey("IdFieldType")]
        public virtual FieldType FieldType { get; set; }
        [ForeignKey("IdTemplate")]
        public virtual Template Template { get; set; }
        [ForeignKey("IdParent"),InverseProperty("Children")]
        public virtual Field Parent { get; set; }

        public virtual ICollection<Field> Children { get; set; }
        public virtual ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
