namespace Bitzar.PagFun.Models
{
    public class FormattedCategory
    {
        public int IdCategory { get; set; }
        public int IdParent { get; set; }
        public bool Disabled { get; set; }
        public bool Highlighted { get; set; }
        public string ImageDefault { get; set; }
        public string CategoryName { get; set; }
        public string CategoryUrl { get; set; }
    }
}
