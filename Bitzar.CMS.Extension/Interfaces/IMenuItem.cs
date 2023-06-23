namespace Bitzar.CMS.Extension.Interfaces
{
    public interface IMenuItem
    {
        string Title { get; set; }
        string Function { get; set; }
        string Icon { get; set; }
        string Parameters { get; set; }
    }
}