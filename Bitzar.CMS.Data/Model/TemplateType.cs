using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class TemplateType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(256)]
        public string DefaultPath { get; set; }
        [Required, MaxLength(100)]
        public string DefaultExtension { get; set; }
        [Required, MaxLength(25)]
        public string Editor { get; set; }

        [JsonIgnore]
        public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
