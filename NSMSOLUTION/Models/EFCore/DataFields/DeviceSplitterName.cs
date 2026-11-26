using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblDeviceSplitterName")]
    public class Splitter
    {
        [Key]
        public int SplitterNameID { get; set; }
        public string SplitterName { get; set; }
        public string CreateaBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}