using Bitzar.CMS.Extension.Interfaces;

namespace Bitzar.CMS.Extension.Classes
{
    public class MenuItem : IMenuItem
    {
        public string Title { get; set; }
        public string Function { get; set; }
        public string Icon { get; set; }
        public string Parameters { get; set; }
    }
}