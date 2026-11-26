namespace NSMSOLUTION.Models.Expenses
{
    public class PaymentVoucherItemVM
    {
        public string AmountDisplay { get; set; }
        public string AccountCode { get; set; }
        public string Description { get; set; }
        public string ItemName_Description { get; set; }
        public string UOM { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public decimal ItemVAT { get; set; }
        public decimal ItemTAX { get; set; }
        public decimal ItemGrandTotal { get; set; }
    }

}