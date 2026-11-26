using System;

namespace NSMSOLUTION.Models.Expenses
{
    public class ExpenseViewModel
    {
        public enum ExpenseStatus
        {
            Draft = 1,
            Checked = 2,
            Verified = 3,
            Approved = 4,
            Posted = 5,
            Paid = 6,
            Cancelled = 7
        }
        public int? BillExpenseID { get; set; } = 0;
        public int? VendorDepositID { get; set; } = 0;
        public decimal? AmountToApply { get; set; } = 0;
        public int ExpenseDetailID { get; set; }
        public bool IsFromBills { get; set; } = false;
        public int ExpenseID { get; set; }
        public int? ExpensePaymentID { get; set; } = 0;
        public int ClassName { get; set; }
        public int? PaymentMethodID { get; set; } = 0;
        public int PayeeID { get; set; }
        public string ExpenseTransactionType { get; set; }
        public string PayeeType { get; set; }
        public string VendorName { get; set; }
        public string Type { get; set; }

        public string PostedPaymentNo { get; set; }
        public string No { get; set; }
        public string NoPrefix { get; set; }

        public string PaymentAccountID { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? PaidOnDate { get; set; }

        public string ExpenseNo { get; set; }
        public string PVNo { get; set; }
        public string PVNoPrefix { get; set; }

        public string CheckedBy { get; set; }
        public string CheckedByTelegramUserName { get; set; }
        public string CheckedByUserID { get; set; }

        public string VerifiedBy { get; set; }
        public string VerifiedByTelegramUserName { get; set; }
        public string VerifiedByUserID { get; set; }

        public string Description { get; set; }
        public string VATRemark { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByTelegramUserName { get; set; }
        public string ApprovedByUserID { get; set; }
        public string PostedBy { get; set; }
        public string PostedByTelegramUserName { get; set; }
        public string PostedByUserID { get; set; }
        public string PostingNumber { get; set; }
        public string PostingPrefix { get; set; }

        public string Reference { get; set; }
        public string BankInfo { get; set; }
        public string BankerName { get; set; }
        public string ChequeBankerName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string ChequeTo { get; set; }
        public string ChequeNo { get; set; }
        public decimal Total { get; set; }
        public decimal? ItemVAT { get; set; } = 0;
        public decimal ExchangeRate { get; set; } = 0;
        public decimal HeaderTax { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public string TransactionType { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int? CustomerID { get; set; } = 0;
        public int? EmployeeID { get; set; } = 0;
        public int? VendorID { get; set; } = 0;
        public int? SystemID { get; set; } = 0;
        public int? WarehouseID { get; set; } = 0;
        public string DomainID { get; set; }
        public string FileName { get; set; }
        //public int? PaymentTermsID { get; set; } = 0;
        public DateTime? PaymentTermsID { get; set; }
        public string TermName { get; set; } = string.Empty;
        public string PaymentNo { get; set; } = string.Empty;
        //public string No { get; set; } = string.Empty;
        public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;

        public decimal GrantTotalAsRiel { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentStatus { get; set; } // Paid, Partial, Unpaid
    }

}