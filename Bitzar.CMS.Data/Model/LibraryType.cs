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
    public class LibraryType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index(IsUnique = true), Required, MaxLength(50)]
        public string Description { get; set; }
        [MaxLength(255)]
        public string MimeTypes { get; set; }
        [MaxLength(255)]
        public string AllowedExtensions { get; set; }
        [MaxLength(255)]
        public string DefaultPath { get; set; }
        public bool IsImageType { get; set; } = false;

        [JsonIgnore]
        public virtual ICollection<Library> Libraries { get; set; } = new List<Library>();

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
