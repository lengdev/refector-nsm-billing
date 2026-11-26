namespace NSMSOLUTION.Models
{
    public class TrialBalanceItem
    {
        public string AccountNameIndented { get; set; }
        public string AccountNo { get; set; }
        public string AccountCategory { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public string NormalSide { get; set; }
        public int AncestorLevel { get; set; }
    }

}