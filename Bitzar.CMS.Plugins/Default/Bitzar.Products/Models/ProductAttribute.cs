namespace Bitzar.Products.Models
{
    public class ProductAttribute
    {
        public int IdProduct { get; set; }
        public int IdAttribute { get; set; }

        public Attribute Attribute { get; set; }
    }
}