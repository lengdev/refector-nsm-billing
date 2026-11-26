namespace NSMSOLUTION.Controllers.Expenses
{
    public class PaymentHistoryVM
    {
        /// <summary>
        /// Sequence of payment (1,2,3...)
        /// </summary>
        public int PaymentNo { get; set; }

        /// <summary>
        /// Expense Payment ID
        /// </summary>
        public int ExpensePaymentID { get; set; }

        /// <summary>
        /// Payment Number (PAY260728-00041)
        /// </summary>
        public string PaymentCode { get; set; }

        /// <summary>
        /// Payment Voucher No
        /// </summary>
        public string PVNo { get; set; }

        /// <summary>
        /// Expense Number
        /// </summary>
        public string ExpenseNo { get; set; }

        /// <summary>
        /// Payment Date (Formatted)
        /// </summary>
        public string PaymentDate { get; set; }

        /// <summary>
        /// Total Expense Amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount for this payment
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Remaining Balance
        /// </summary>
        public decimal BalanceAmount { get; set; }

        /// <summary>
        /// Payment Status (Unpaid, Partial, Paid)
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Workflow Status (Draft, Posted, Confirm Verified...)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Current payment row
        /// </summary>
        public bool IsCurrentPayment { get; set; }
    }
}