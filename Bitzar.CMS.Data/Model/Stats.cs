using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bitzar.CMS.Data.Model
{
    public class Stats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Index("IX_Stats_Date")]
        public DateTime Date { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string UserName { get; set; }
        [MaxLength(50), Index("IX_Stats_Ip")]
        public string Ip { get; set; }
        [MaxLength(255)]
        public string Session { get; set; }
        public bool IsSecure { get; set; }
        [MaxLength(100)]
        public string Host { get; set; }
        [MaxLength(255)]
        [Index("IX_Stats_Url")]
        public string Url { get; set; }
        [MaxLength(255)]
        public string UrlReferrer { get; set; }
        [Index("IX_Stats_Time")]
        public long Time { get; set; }
        [MaxLength(20)]
        public string Type { get; set; }
        [MaxLength(50)]
        public string HttpResult { get; set; }
        [MaxLength(50)]
        [Index("IX_Stats_Browser")]
        public string Browser { get; set; }
        [MaxLength(20)]
        public string Version { get; set; }
        [Index("IX_Stats_Crawler")]
        public bool IsCrawler { get; set; } = false;
        public bool IsMobileDevice { get; set; } = false;
        public string MobileManufacturer { get; set; }
        public string MobileModel { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}-{Id}";
        }
    }
}
