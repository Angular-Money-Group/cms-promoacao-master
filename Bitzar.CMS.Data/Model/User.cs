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
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index("IX_UserName", IsUnique = true, Order = 1), Required, MaxLength(50)]
        public string UserName { get; set; }
        [MaxLength(100), JsonIgnore]
        public string Password { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }
        [MaxLength(255)]
        public string FirstName { get; set; }
        [MaxLength(255)]
        public string LastName { get; set; }
        [Required]
        public int IdRole { get; set; }
        public bool Deleted { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public bool ChangePassword { get; set; }
        public DateTime? LastLogin { get; set; }
        [Index("IX_UserName", IsUnique = true, Order = 2)]
        public bool AdminAccess { get; set; } = true;
        [MaxLength(255)]
        public string ProfilePicture { get; set; }
        public DateTime? Validated { get; set; }
        public bool Completed { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public string Token { get; set; }

        [Obsolete("Depreciado no CMS, utilizar o módulo de permissões", false)]
        public string RestrictedOptions { get; set; }

        [ForeignKey("IdRole")]
        public virtual Role Role { get; set; }

        public int? IdParent { get; set; }

        public virtual ICollection<UserField> UserFields { get; set; } = new List<UserField>();
        public virtual ICollection<UserSocial> UserSocial { get; set; } = new List<UserSocial>();

        // Define the indexer to allow client code to use [] notation.
        public string this[string key] => UserFields?.FirstOrDefault(f => f.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))?.Value ?? string.Empty;

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
