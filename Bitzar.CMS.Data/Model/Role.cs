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
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index("IX_RoleName", IsUnique = true, Order = 1), Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        [Index("IX_RoleName", IsUnique = true, Order = 2)]
        public bool AdminRole { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
