using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Models.Expenses
{
    public class PostExpenseEntryViewModel
    {
        // COMPANY
        public string CompanyName { get; set; }

        // DOCUMENT
        public string ExpenseNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string ReferenceNo { get; set; }

        // VENDOR
        public string VendorName { get; set; }

        // PAYMENT
        public string PaymentMethod { get; set; }

        // DEPARTMENT
        public string Department { get; set; }

        // USER
        public string PreparedBy { get; set; }
        public string CheckedBy { get; set; }

        // SUMMARY
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        // WORDS
        public string AmountInWords { get; set; }

        // PAYMENT STATUS
        public string PaymentStatus
        {
            get
            {
                return PaidAmount >= TotalAmount ? "FULL PAID" : PaidAmount > 0 ? "PARTIAL PAID" : "POST-PAID";
            }
        }

        // DETAILS
        public List<PostExpenseEntryItemViewModel> Items { get; set; }
            = new List<PostExpenseEntryItemViewModel>();
    }

    public class PostExpenseEntryItemViewModel
    {
        public string AccountCode { get; set; }

        public string Description { get; set; }

        public decimal Qty { get; set; }

        public decimal Price { get; set; }

        public decimal Amount
        {
            get
            {
                return Qty * Price;
            }
        }
    }
}