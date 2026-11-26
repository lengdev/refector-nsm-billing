namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ItemChartofAccount
    {
        public int ItemID { get; set; }
        public int? InventoryChartOfAccountID { get; set; }
        public int? PurchaseChartOfAccountID { get; set; }
        public int? SaleChartOfAccountID { get; set; }
        public int? DeferredRevenueAccountID { get; set; }
        public int? OutputVATAccountID { get; set; }
        public int? InputVATAccountID { get; set; }
        public int? ARAccountID { get; set; }
        public int? PurchaseDiscountAccountID { get; set; }
        public int? SalesDiscountAccountID { get; set; }
    }
}