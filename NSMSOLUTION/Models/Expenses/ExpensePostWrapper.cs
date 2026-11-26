using System.Collections.Generic;

namespace NSMSOLUTION.Models.Expenses
{
    public class ExpensePostWrapper
    {
        public List<ExpenseViewModel> ExpenseData { get; set; }
        public List<ExpenseDetailViewModel> Details { get; set; }
    }
}