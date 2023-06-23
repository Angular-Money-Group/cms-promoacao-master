using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class LogLink
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index("UX_LogLink", IsUnique = true, Order = 0)]
        public int ReferenceId { get; set; }
        [Index("UX_LogLink", IsUnique = true, Order = 1)]
        public string ReferenceType { get; set; }
        [Index("UX_LogLink", IsUnique = true, Order = 2)]
        public string Source { get; set; }
        [Index("UX_LogLink", IsUnique = true, Order = 3)]
        public string Type { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        
    }
}
