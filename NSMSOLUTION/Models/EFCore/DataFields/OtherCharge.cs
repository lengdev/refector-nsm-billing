namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class OtherCharge
    {
        public int OtherChargeID { get; set; }
        public string OtherChargeItem { get; set; }
        public decimal Fee { get; set; }
        public string UM { get; set; }
        public string Status { get; set; }
        public decimal CostPrice { get; set; }
        public int ServiceID { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public int UserID { get; set; }
        public int ExpenseAccountID { get; set; }
        public int IncomeAccountID { get; set; }
        public int DeferredRevenueAccountID { get; set; }
        public int OutputVATAccountID { get; set; }
        public int InputVATAccountID { get; set; }
        public int ARAccountID { get; set; }
        public int SalesDiscountAccountID { get; set; }
        public int PurchaseDiscountAccountID { get; set; }
        public int InventoryAccountID { get; set; }
        public string OtherChargeDescription { get; set; }
    }
}