using System.ComponentModel.DataAnnotations;

namespace Bitzar.CMS.Data.Model
{
    public class Configuration
    {
        [Key, MaxLength(50)]
        public string Id { get; set; }
        [MaxLength(100), Required]
        public string Name { get; set; }
        [MaxLength(255), Required]
        public string Description { get; set; }
        [MaxLength(255)]
        public string Value { get; set; }
        [MaxLength(100), Required]
        public string Category { get; set; } = "Sistema";
        public int Order { get; set; }
        [MaxLength(25)]
        public string Type { get; set; } = "text";
        [MaxLength(255)]
        public string Source { get; set; }
        public bool System { get; set; } = false;
        [MaxLength(255)]
        public string Plugin { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
