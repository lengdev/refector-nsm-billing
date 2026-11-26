namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class StockOut
    {
        public int StockOutID { get; set; }
        public int? ActivateCustomerID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public int? CustomerID { get; set; }
        public int? StockInID { get; set; }
        public string ConnectionID { get; set; }
        public int? ItemID { get; set; }
        public System.DateTime? DateOut { get; set; }
        public string UM { get; set; }
        public double? QTY { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountP { get; set; }
        public decimal? DiscountUSD { get; set; }
        public decimal? Amount { get; set; }
        public int? StatusID { get; set; }
        public string InvoiceNumber { get; set; }
        public string Type { get; set; }
        public bool? OutOfStock { get; set; }
        public int? ClassNameID { get; set; }
        public int? LocationID { get; set; }
        public bool? IsIncludedVAT { get; set; }
        public bool? IsReturn { get; set; }
        public bool? IsUsed { get; set; }
        public string StockOutType { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public double? QtyCalOut { get; set; }
        public int? UomID { get; set; }
        public int? RequestItemID { get; set; }
        public int? SerialNumberID { get; set; }
        public string SerialNumber { get; set; }
        public int? ToWarehouseID { get; set; }
        public int? SalesID { get; set; }
        public int? RequestItemDetailID { get; set; }
    }
}