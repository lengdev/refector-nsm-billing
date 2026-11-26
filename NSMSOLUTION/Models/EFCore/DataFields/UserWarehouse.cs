using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblUserWarehouse")]
    public class UserWarehouse
    {
        [Key]
        public int WID { get; set; }
        public int? BranchID { get; set; }
        public string BranchName { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseDescription { get; set; }
        public string Address { get; set; }
        public int? CommuneID { get; set; }
        public string Commune { get; set; }
        public int? DistrictID { get; set; }
        public string District { get; set; }
        public string ProvinceCity { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}