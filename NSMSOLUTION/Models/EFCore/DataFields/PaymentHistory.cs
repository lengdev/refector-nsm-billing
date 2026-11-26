using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class PaymentHistory
    {
        public int PaymentHistoryID { get; set; }
        public int? PaymentID { get; set; }
        public string Payer { get; set; }
        public DateTime? PayDate { get; set; }
        public string PayBy { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentDescription { get; set; }
        public string PayTo { get; set; }
        public decimal? PayAmount { get; set; }
        public string Category { get; set; }
        public string PayDescription { get; set; }
        public string ReceivedFromName { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public int? CustomerID { get; set; }
        public string InvoiceNo { get; set; }
        public string DomainID { get; set; }
    }
}