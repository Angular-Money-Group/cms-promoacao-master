using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Data.Model
{
    public class RolePermission
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Index("UX_RolePermission", IsUnique = true, Order = 0)]
        public int IdRole { get; set; }
        [MaxLength(50)]
        [Index("UX_RolePermission", IsUnique = true, Order = 1)]
        public string Source { get; set; }
        [MaxLength(50)]
        [Index("UX_RolePermission", IsUnique = true, Order = 2)]
        public string Module { get; set; }
        [MaxLength(50)]
        [Index("UX_RolePermission", IsUnique = true, Order = 3)]
        public string Function { get; set; }
        [Required]
        public PermissionType Status { get; set; }

        [ForeignKey("IdRole")]
        public virtual Role Role { get; set; }

        public enum PermissionType{
            Deny = 0,
            Allow = 1
        }
    }
}
