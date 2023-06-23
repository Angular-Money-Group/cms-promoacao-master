using System.Collections.Generic;

namespace Bitzar.Products.Models
{
    public class AttributeType
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Codigo { get; set; }
        public virtual ICollection<AttributeType> ChildChildren { get; set; } = new List<AttributeType>();
    }
}