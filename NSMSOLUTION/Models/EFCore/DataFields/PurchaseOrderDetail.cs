using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class PurchaseOrderDetail
    {
        public int PurchaseOrderDetailID { get; set; }
        public int? PurchaseOrderID { get; set; }
        public string PONumber { get; set; }
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int? ChartOfAccountID { get; set; }
        public string AccountName { get; set; }
        public string UM { get; set; }
        public double? QTY { get; set; }
        public double? UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? DisP { get; set; }
        public decimal? DisUSD { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? PODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? StatusID { get; set; }
        public string DeliverTo { get; set; }
        public string RequestBy { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public int? UomID { get; set; }
        public int? StockInStatusID { get; set; }
        public string Dimension { get; set; }
    }
}
