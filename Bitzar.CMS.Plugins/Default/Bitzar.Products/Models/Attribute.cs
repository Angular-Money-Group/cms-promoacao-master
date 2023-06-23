using System.Collections.Generic;

namespace Bitzar.Products.Models
{
    public class Attribute
    {
        public int Id { get; set; }
        public int IdType { get; set; }
        public int Level { get; set; }
        public int? IdParent { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
        public string Codigo { get; set; }
        public IList<Attribute> Children { get; set; } = new List<Attribute>();
    }
}