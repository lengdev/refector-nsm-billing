namespace NSMSOLUTION.Models.Expenses
{
    public class ExpenseDetail
    {
        public int ExpenseDetailID { get; set; }
        public int ItemTypeID { get; set; }
        public string ExpenseCategory { get; set; }
        public string ExpenseCategoryType { get; set; } = string.Empty;
        public string AccountName { get; set; }
        public int AccountID { get; set; }

        public int? ItemID { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string ItemName_Description { get; set; } = "";
        public string ExpenseTransactionType { get; set; }
        public string Description { get; set; }

        public string UM { get; set; }
        public string RefNo { get; set; }
        public string SourceType { get; set; }

        public decimal QTY { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DisUSD { get; set; }
        public decimal ItemVAT { get; set; }

        public decimal Total { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ItemTAX { get; set; }
        public string RielAction { get; set; } = "Auto";
        public decimal ItemGrandTotal { get; set; }
        public decimal ItemGrandTotalRiel { get; set; }
    }

}