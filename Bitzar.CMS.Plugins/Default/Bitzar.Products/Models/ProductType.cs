namespace Bitzar.Products.Models
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool EnableSubProduct { get; set; }
        public bool EnableCombo { get; set; }
        public bool DisableSubProduct { get; set; }
    }
}