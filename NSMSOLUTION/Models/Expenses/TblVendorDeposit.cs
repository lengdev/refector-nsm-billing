using System;

namespace NSMSOLUTION.Models.Expenses
{
    public class TblVendorDeposit
    {
        public long VendorDepositID { get; set; }

        public int VendorID { get; set; }

        public long? TransactionID { get; set; }

        public long? ExpenseID { get; set; }

        public string DepositNo { get; set; } = string.Empty;

        public DateTime DepositDate { get; set; }

        public decimal Amount { get; set; }

        public decimal AppliedAmount { get; set; }

        public int AccountID { get; set; }

        public int? BankAccountID { get; set; }

        public string Status { get; set; } = "Open";

        public string PONo { get; set; }= string.Empty;

        public string DomainID { get; set; }

        public int SystemID { get; set; }

        public int WarehouseID { get; set; }

        public int CreateByUserID { get; set; }

        public DateTime CreateDate { get; set; }

        public int? UpdateByUserID { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}