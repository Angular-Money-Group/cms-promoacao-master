namespace Bitzar.CMS.Core.Areas.api.Models
{
    public class UserSocialModel
    {
        public int? UserId { get; set; }
        public string Type { get; set; }
        public string SourceId { get; set; }
        public string AccessToken { get; set; }
    }
}