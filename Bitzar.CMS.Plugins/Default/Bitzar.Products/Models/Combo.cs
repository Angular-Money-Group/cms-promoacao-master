namespace Bitzar.Products.Models
{
    public class Combo
    {
        public int IdCombo { get; set; }
        public int IdProduct { get; set; }
        public int? Sort { get; set; }
        public int Quantidade { get; set; }
        public string Description { get; set; }

        public Product ComboProduct { get; set; }
        public Product Product { get; set; }
    }
}