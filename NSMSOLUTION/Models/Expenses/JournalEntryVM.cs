namespace NSMSOLUTION.Models.Expenses
{
    public class JournalEntryVM
    {
        public string AccountNo { get; set; }
        public string Transaction { get; set; }
        public decimal ItemTAX { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

}