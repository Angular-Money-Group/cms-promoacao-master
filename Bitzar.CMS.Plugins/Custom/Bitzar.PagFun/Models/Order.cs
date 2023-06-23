namespace Bitzar.PagFun.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int IdCustomer { get; set; }

        public string EventName { get; set; }

        public decimal Credit { get; set; }

        public decimal Price { get; set; }
    }
}