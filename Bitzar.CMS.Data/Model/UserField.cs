using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Bitzar.CMS.Data.Model
{
    public class UserField
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Value { get; set; }       

        [JsonIgnore]
        [ForeignKey("IdUser")]
        public virtual User User { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
