using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class Library
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(255), Index(IsUnique =true), Required]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        [MaxLength(255)]
        public string Path { get; set; }
        [MaxLength(10)]
        public string Extension { get; set; }
        public int IdLibraryType { get; set; }
        [Required]
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Attributes { get; set; }


        [ForeignKey("IdLibraryType")]
        public virtual LibraryType LibraryType { get; set; }

        // Properties for file sizing
        [NotMapped]
        public decimal SizeInKB { get => this.Size / 1024.00M; }
        [NotMapped]
        public decimal SizeInMB { get => this.Size / 1024.00M / 1024.00M; }
        [NotMapped]
        public string FullPath { get => $"{this.Path}/{this.Name}".Replace("~/", "/"); }

        // Override of the Object
        public override string ToString()
        {
            return this.FullPath;
        }
    }
}
