namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class PackageDetail
    {
        public int PackageDetailID { get; set; }
        public string PackageCode { get; set; }
        public int PackageID { get; set; }
        public string PackageType { get; set; }
        public int IncomeAccountID { get; set; }
        public int DeferredRevenueAccountID { get; set; }
        public int OutputVATAccountID { get; set; }
        public int ExpenseAccountID { get; set; }
        public int InputVATAccountID { get; set; }
        public int Bandwidth { get; set; }
        public int Duration { get; set; }
        public string UM { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal VATRate { get; set; }
        public int DowSpeed { get; set; }
        public int UpSpeed { get; set; }
        public decimal DepositFee { get; set; }
        public int FreeDuration { get; set; }
        public string Feature { get; set; }
        public string ServiceName { get; set; }
        public string PackageName { get; set; }
        public int ServiceID { get; set; }
        public bool IsApplyRediusProfile { get; set; }
        public int RediusProfileID { get; set; }
        public bool IsDoubleSpeedAtNight { get; set; }
        public int ConnectionTypeID { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
        public string Status { get; set; }
        public int ARAccountID { get; set; }
        public int SalesDiscountAccountID { get; set; }
        public int PurchaseDiscountAccountID { get; set; }
    }
}