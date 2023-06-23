using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.PagFun.Models
{
    [Table("pagfun_promoter_invite")]
    public class PromoterInvite
    {
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string IdPromoter { get; set; }

        public int? IdUser { get; set; }

        [Index("uk_promoter_invite_event_email", 0, IsUnique = true)]
        public int IdEvent { get; set; }

        public string PromoterName { get; set; }

        [Index("uk_promoter_invite_event_email", 1, IsUnique = true)]
        [MaxLength(100)]
        public string PromoterEmail { get; set; }

        public string Status { get; set; }
       
        [DefaultValue("now()")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DefaultValue("now()")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}