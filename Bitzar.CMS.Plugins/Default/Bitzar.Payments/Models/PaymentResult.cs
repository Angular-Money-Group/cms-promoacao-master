using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static Bitzar.Payments.Models.Transaction;

namespace Bitzar.Payments.Models
{
    public class PaymentResult
    {
        public string referenceId { get; set; }
        public PaymentGateway Gateway { get; set; }
        public Customer Customer { get; set; }
        public Order Order { get; set; }
        public HttpStatusCode HttpStatus { get; set; }
        public List<Error> Errors { get; set; } = new List<Error>();
    }

    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Document { get; set; }
        public string Phone { get; set; }
        public PAddress Address { get; set; }
    }

    public class PAddress
    {
        public string Zip { get; set; }
        public string PublicPlace { get; set; }
        public string Number { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class Order
    {
        public decimal Amount { get; set; }
        public OperationType OperationType { get; set; }
        public Payment Payment { get; set; }
    }

    public class Payment
    {
        public string CardHolder { get; set; }
        public string PaymentId { get; set; }
        public int Installments { get; set; }
        public string AuthCode { get; set; }
        public string Nsu { get; set; }
        public string Description { get; set; }
        public string CardBrand { get; set; }
        public string Last4Digits { get; set; }
        public string Url { get; set; }
        public string QrCode { get; set; }
        public string BarCode { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}