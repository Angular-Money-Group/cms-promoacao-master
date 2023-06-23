namespace Bitzar.CMS.Extension.CMS
{
    public interface IConfiguration
    {
        bool AllowMembershipManagement { get; }
        string DefaultLanguage { get; }
        bool EnforceCaptcha { get; }
        bool EnforceSSL { get; }
        bool MembershipEnabled { get; }
        string SiteName { get; }
        string Token { get; }
        string DefaultUrl { get; }

        string Get(string key, string plugin = null);
        bool ContainsKey(string key, string plugin = null);
        void Refresh();
        void AutoRefreshSiteMap();
        void GenerateSiteMap();
    }
}