namespace Bitzar.PagFun.Models
{
    public class FormatedAttribute
    {
        public int Id { get; set; }
        public int IdType { get; set; }
        public int? IdParent { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
        public string Codigo { get; set; }
    }
}