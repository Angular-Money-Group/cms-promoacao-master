namespace Bitzar.Products.Models
{
    public class ProductSub
    {
        public int IdProduct { get; set; }
        public int IdSubProduct { get; set; }
        public Product SubProduct { get; set; }
        public Product Product { get; set; }
    }
}