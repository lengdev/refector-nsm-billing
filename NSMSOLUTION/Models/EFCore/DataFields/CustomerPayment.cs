using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerPayment
    {
        public int CustomerPaymentID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public int? CustomerID { get; set; }

        public string InvoiceNo { get; set; }

        public decimal? Deposit { get; set; }
        public decimal? Total { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Unpaid { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string PaidBy { get; set; }

        public int? StatusID { get; set; }
        public string Type { get; set; }

        public string OfficialReceiptNo { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        public int? ChartOfAccountID { get; set; }
        public int? PaymentID { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }

        public string Description { get; set; }

        public int? CompanyID { get; set; }
        public int? PaidByUserID { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
