using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Bitzar.CMS.Data.Model
{
    public class Template
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(256)]
        public string Path { get; set; }
        [Required, MaxLength(20)]
        public string Extension { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        [MaxLength(100)]
        public string Url { get; set; }
        [Required]
        public int IdTemplateType { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool Released { get; set; }
        [Required, MaxLength(50)]
        public string User { get; set; }
        public int? IdSection { get; set; }
        public bool Restricted { get; set; } = false;
        public string RoleRestriction { get; set; }
        public int Version { get; set; }
        public bool Mapped { get; set; } = true;

        [NotMapped]
        public dynamic ObjectInstance { get; set; }

        [ForeignKey("IdTemplateType")]
        public virtual TemplateType TemplateType { get; set; }
        [ForeignKey("IdSection")]
        public virtual Section Section { get; set; }
        public virtual ICollection<Field> Fields { get; set; } = new List<Field>();

        public static bool operator !=(Template page1, Template page2)
        {
            return !(page1 == page2);
        }
        public static bool operator ==(Template page1, Template page2)
        {
            if (ReferenceEquals(page1, null) && ReferenceEquals(page2, null))
                return true;

            if (ReferenceEquals(page1, null))
                return false;

            if (ReferenceEquals(page2, null))
                return false;

            return page1.Id == page2.Id;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }

        public string NameWithoutExtension()
            => this.Name?.Replace(".cshtml", "");
    }
}
