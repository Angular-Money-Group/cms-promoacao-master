namespace Bitzar.Products.Models
{
    public class ProductRelated
    {
        public int IdProduct { get; set; }
        public int IdRelated { get; set; }
        public Product Related { get; set; }
    }
}