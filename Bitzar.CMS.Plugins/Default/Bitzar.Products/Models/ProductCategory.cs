namespace Bitzar.Products.Models
{
    public class ProductCategory
    {
        public int IdProduct { get; set; }
        public int IdCategory { get; set; }

        public Category Category { get; set; }
    }
}