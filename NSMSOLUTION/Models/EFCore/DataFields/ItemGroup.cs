using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class ItemGroup
    {
        public int ItemGroupID { get; set; }
        [Column("ItemGroup")]
        public string GroupName { get; set; }
        public int? StockNameID { get; set; }
        public string Memo { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}