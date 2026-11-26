using NSMSOLUTION.Controllers.Expenses;
using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.Expenses
{
    public class PaymentVoucherViewModel
    {
        public decimal RemainingAP { get; set; }
        public decimal VAT { get; set; } = 0;
        public string ExpenseTransactionType { get; set; }
        public string CompanyName { get; set; }
        public string Type { get; set; }
        public string VendorCompanyName { get; set; }
        public string VendorCompanyAddress { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string PVNo { get; set; }
        public string No { get; set; }
        public DateTime PrintDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string VendorName { get; set; }
        public string Reference { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }

        public string BankName { get; set; }
        public string ChequeBankerName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string ChequeTo { get; set; }
        public string ChequeNo { get; set; }
        public string SwiftCode { get; set; }

        public decimal Discount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalanceAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ItemVAT { get; set; }
        public decimal GrantTotalAsRiel { get; set; }
        public decimal GrandTotal { get; set; }
        public string AmountInWords { get; set; }

        public List<PaymentVoucherItemVM> Items { get; set; }
        public List<JournalEntryVM> JournalEntries { get; set; }
        public List<PaymentHistoryVM> PaymentHistory { get; set; } = new List<PaymentHistoryVM>();
        public string Signature { get; set; }
        public string CheckedSignature { get; set; }
        public string CheckedDate { get; set; }
        public string PreparedBy { get; set; }
        public string CheckedBy { get; set; }
        public string VerifiedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string PostedBy { get; set; }
        public string Status { get; set; }
        public string FileName { get; set; }
        public decimal ExchangeRate { get; set; } = 0;
        //public int? PaymentTermsID { get; set; }
        public DateTime? PaymentTermsID { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentStatus { get; set; } // Paid, Partial, Unpaid
        public bool IsPartialPayment { get; set; }
        public bool IsPosted { get; set; }
        public string PostedDate { get; set; }
        public string VerifiedDate { get; set; }
        public string Description { get; set; }
        public string PostedSignature { get; set; }
        public string VerifiedSignature { get; set; }
    }

}