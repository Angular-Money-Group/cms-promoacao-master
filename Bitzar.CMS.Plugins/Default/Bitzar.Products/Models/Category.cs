using System.Collections.Generic;

namespace Bitzar.Products.Models
{
    public class Category
    {
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public string Image { get; set; }
        public bool Disabled { get; set; }
        public bool Hide { get; set; } = false;
        public string Info { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Level { get; set; }
        public string SKU { get; set; }
        public int? Sort { get; set; }
        public bool Highlighted { get; set; }
        public IList<Category> Children { get; set; } = new List<Category>();
    }
}