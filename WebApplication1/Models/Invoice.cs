using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Models
{
    public class Invoice
    {
        public DateTime CreationDate { get; set; }

        public Customer Customer { get; set; }

        public string CustomerNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime DueDate { get; set; } //Kan ha skrivit det som "Expiration date" i databas och på klient-sidan



        public Address BillingAddress { get; set; }
        public Address DeliveryAddress { get; set; }

        public List<InvoiceArticleItem> Articles { get; set; }





        public decimal NetAmount { get; set; }
        public decimal ValueAddedTax { get; set; }
        public decimal RutDeductionAmount { get; set; }
        public decimal GrossAmount { get; set; }

    }
}