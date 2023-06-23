using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class FieldType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index(IsUnique = true), Required, MaxLength(50)]
        public string Name { get; set; }

        public virtual ICollection<Field> Fields { get; set; } = new List<Field>();
        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
