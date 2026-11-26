using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ItemVariant
    {
        [Column("UMItemID")]
        public int ItemVariantID { get; set; }
        public int? ItemID { get; set; }
        public string Barcode { get; set; }
        public int? UomID { get; set; }
        public string UM { get; set; }
        public double? Multiplier { get; set; }
        public double? CostPrice { get; set; }
        public double? SellingPrice { get; set; }
        public string ItemImage { get; set; }
        public string DestinationFolder { get; set; }
        public string StockNameID { get; set; }
        public string StockName { get; set; }
        public int? ItemGroupID { get; set; }
        public string ItemGroup { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int? ItemSizeID { get; set; }
        public string ItemSize { get; set; }
        public int? ItemBrandID { get; set; }
        public string ItemBrand { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public int? ReorderPoint { get; set; }
        public int? InventoryChartOfAccountID { get; set; }
        public string InventoryAccountName { get; set; }
        public int? PurchaseChartOfAccountID { get; set; }
        public string PurchaseChartOfAccount { get; set; }
        public int? SaleChartOfAccountID { get; set; }
        public string SaleChartOfAccount { get; set; }
        public string TypeName { get; set; }
        public int? ItemTypeID { get; set; }
        public string ItemDescription { get; set; }
        public string Warehouse { get; set; }
        public int? WID { get; set; }
        public string DomainID { get; set; }
        public decimal? OpeningStock { get; set; }
        public decimal? OpeningStockRatePerUnit { get; set; }
        public System.DateTime? ItemExpirationDate { get; set; }
        public string ItemGroupName { get; set; }
        public int? ExpenseAccountID { get; set; }
        public int? IncomeAccountID { get; set; }
        public int? DeferredRevenueAccountID { get; set; }
        public int? OutputVATAccountID { get; set; }
        public int? InputVATAccountID { get; set; }
        public int? ARAccountID { get; set; }
        public int? SalesDiscountAccountID { get; set; }
        public int? PurchaseDiscountAccountID { get; set; }
    }
}