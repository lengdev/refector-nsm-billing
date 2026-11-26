namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class StockInDetail
    {
        public int StockInDetailID { get; set; }
        public int? ActivateCustomerID { get; set; }
        public int? RequestItemID { get; set; }
        public int? StockInID { get; set; }
        public int? StockOutID { get; set; }
        public int? BillID { get; set; }
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public System.DateTime? ItemExpirationDate { get; set; }
        public double? OrderQTY { get; set; }
        public double? QTY { get; set; }
        public double? QtyCal { get; set; }
        public double? QTYOUT { get; set; }
        public double? QtyCalOut { get; set; }
        public double? CostPrice { get; set; }
        public double? CostPriceCal { get; set; }
        public decimal? Amount { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public int? SerialNumberID { get; set; }
        public string SerialNumber { get; set; }
        public System.DateTime? StockInDate { get; set; }
        public System.DateTime? StockOutDate { get; set; }
        public string StockOutType { get; set; }
        public int? CustomerID { get; set; }
        public string InvoiceNumber { get; set; }
        public int? StatusID { get; set; }
        public bool? OutOfStock { get; set; }
        public int? ClassNameID { get; set; }
        public int? LocationID { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int? VendorID { get; set; }
        public int? QuantityAvliable { get; set; }
        public int? QuantityOnHand { get; set; }
        public int? QuantityAdjusted { get; set; }
        public string ItemType { get; set; }
        public int? ItemVariantID { get; set; }
        public int? UomID { get; set; }
        public string UM { get; set; }
        public int? StockTransfers { get; set; }
        public int? AdjustID { get; set; }
        public string AdjustType { get; set; }
        public double? OpeningStock { get; set; }
        public int? ChartOfAccountID { get; set; }
        public string AccountName { get; set; }
        public int? ItemOpeningStockID { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public double? QTYRemain { get; set; }
        public int? SalesID { get; set; }
        public string StockExpireDate { get; set; }
        public int? QTYUnusable { get; set; }
    }
}