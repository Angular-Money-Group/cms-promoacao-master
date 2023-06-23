namespace Bitzar.CMS.Extension.Interfaces
{
    public interface INotification
    {
        string Title { get; set; }
        int Badge { get; set; }
        string Description { get; set; }
        string Icon { get; set; }
        string UrlFunction { get; set; }
        string Plugin { get; set; }
    }
}