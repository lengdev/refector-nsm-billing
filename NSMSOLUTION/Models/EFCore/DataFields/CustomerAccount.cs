using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerAccount
    {
        public int? CustomerAccountID { get; set; }
        public int? ChartOfAccountID { get; set; }
        public int? CustomerID { get; set; }
        public string InvoiceNo { get; set; }
        public string CreditNoteNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public string InvoiceBy { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string ReceiptBy { get; set; }
        public decimal? DepositFee { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? AdvancePaymentAmount { get; set; }
        public string Type { get; set; }
        public decimal? PartiallyPaid { get; set; }
        public decimal? InputAmount { get; set; }
        public int? StatusID { get; set; }
        public string Description { get; set; }
        public int? PaymentID { get; set; }
        public int? CompanyID { get; set; }
        public decimal? TotalPaid { get; set; }
        public bool? IsIncludedVAT { get; set; }
        public decimal? InvoiceCancelledAmount { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
