namespace NSMSOLUTION.Controllers.Expenses.Models
{
    public class VendorDepositOptionViewModel
    {
        public decimal CurrentExpenseAppliedAmount { get; set; }

        public decimal AvailableAmount { get; set; }
        public int VendorDepositID { get; set; }

        public int VendorID { get; set; }

        public int TransactionID { get; set; }

        public string DepositNo { get; set; }

        public string DepositDate { get; set; }

        public decimal Amount { get; set; }

        public decimal AppliedAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }
    }
}