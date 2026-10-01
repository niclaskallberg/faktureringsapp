using System;

namespace WebApplication1.Models
{
    [Serializable]
    public class InvoiceArticleItem
    {
        public string Article { get; set; }
        public string Description { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }

        

        public decimal Amount { get; set; }



        public bool IsNotRut { get; set; }

    }
}