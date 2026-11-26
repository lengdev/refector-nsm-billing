namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ItemCategory
    {
        public int ItemCategoryID { get; set; }
        public int? ItemID { get; set; }
        public int? CategoryID { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}