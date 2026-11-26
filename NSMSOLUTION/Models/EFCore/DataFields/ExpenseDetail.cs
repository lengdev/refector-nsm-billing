using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.Expenses
{
    [Table("ExpenseDetails")]
    public class TblExpenseDetail
    {
        [Key] public int ExpenseDetailID { get; set; }
        public int ExpenseID { get; set; }
        public string RefNo { get; set; }
        public string SourceType { get; set; }
        public int? ItemTypeID { get; set; }
        public string ExpenseCategory { get; set; }        // holds ChartOfAccountID as text
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemName_Description { get; set; }
        public string UM { get; set; }
        public decimal? QTY { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DisUSD { get; set; }
        public decimal? Total { get; set; }
        public decimal? ItemTAX { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? ItemVAT { get; set; }              // computed column, read-only

        public decimal? ItemGrandTotal { get; set; }
        public string ExpenseCategoryType { get; set; }
    }
}