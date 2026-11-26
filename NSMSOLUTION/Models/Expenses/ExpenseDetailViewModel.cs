using System;

namespace NSMSOLUTION.Models.Expenses
{
    public class ExpenseDetailViewModel
    {
        public bool IsFromBills { get; set; } = false;
        public int ExpenseDetailID { get; set; }
        public int ItemTypeID { get; set; }
        public int AccountID { get; set; }
        public string ExpenseTransactionType { get; set; }
        public string AccountName { get; set; }
        public string ExpenseCategory { get; set; }
        public string ExpenseCategoryType { get; set; } = string.Empty;
        public string SourceType { get; set; }
        public string RefNo { get; set; }

        public int? ItemID { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string ItemName_Description { get; set; }
        public string Description { get; set; }

        public string UM { get; set; }

        public decimal QTY { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DisUSD { get; set; }
        public decimal Discount { get; set; }

        public decimal GrantTotalAsRiel { get; set; } = 0;
        public decimal Total { get; set; }
        public decimal? ItemTAX { get; set; } = 0;
        public decimal? ItemVAT { get; set; } = 0;
        public string RielAction { get; set; } = "Auto";
        public decimal ItemGrandTotal { get; set; }
        public decimal ItemGrandTotalRiel { get; set; } = 0;
        //public int? PaymentTermsID { get; set; }
        public DateTime? PaymentTermsID { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentStatus { get; set; } // Paid, Partial, Unpaid
    }


}