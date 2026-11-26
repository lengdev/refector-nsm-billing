using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Category
    {
        public int StockNameID { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public int? Parent_Category_ID { get; set; }
        public int? DisplayOrder { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public string Memo { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}