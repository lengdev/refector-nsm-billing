using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public string Payer { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMenthod { get; set; }
        public string FromBankName { get; set; }
        public string FromAccountName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public string ChequeNumber { get; set; }
        public int Category { get; set; }
        public string PaymentDescription { get; set; }
        public string ReceivedFromName { get; set; }
        public decimal PaymentAmount { get; set; }
        public string InvoiceNo { get; set; }
        public int? PaidByUserID { get; set; }
        public string PaidBy { get; set; }
        public int StatusID { get; set; }
        public int CustomerID { get; set; }
        public string DestinationFolder { get; set; }
        public string AttachmentFile { get; set; }
        public string OfficialReceiptNo { get; set; }
        public int InvoiceTmpID { get; set; }
        public string ReferenceNo { get; set; }
        public int ChartOfAccountID { get; set; }
        public string Type { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}