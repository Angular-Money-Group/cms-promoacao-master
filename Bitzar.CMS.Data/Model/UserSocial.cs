using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Data.Model
{
    public class UserSocial
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required, MaxLength(100)]
        public string Type { get; set; }

        [MaxLength(255)]
        public string SourceId { get; set; }
   
        public string AccessToken { get; set; }

        public string Data { get; set; }

        [ForeignKey("IdUser")]
        public virtual User User { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
