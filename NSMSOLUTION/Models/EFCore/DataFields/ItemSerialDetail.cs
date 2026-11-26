namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ItemSerialDetail
    {
        public int ItemSerialDetailID { get; set; }
        public int? ItemSerialID { get; set; }
        public int? StockInID { get; set; }
        public string SerialNumber { get; set; }
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public double? QTY { get; set; }
        public int? UomID { get; set; }
        public string UOM { get; set; }
        public bool? IsAvailable { get; set; }
        public System.DateTime? ItemSerialExpireDate { get; set; }
        public string TransactionType { get; set; }
    }
}