using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblServices")]
    public class Service
    {
        [Key]  public int ServiceID { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string SIDNonVAT { get; set; }
        public string SIDVAT { get; set; }
        public string COIDNonVAT { get; set; }
        public string COIDVAT { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}