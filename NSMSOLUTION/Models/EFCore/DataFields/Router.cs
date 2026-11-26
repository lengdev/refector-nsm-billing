using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblRouter")]
    public class Router
    {
        [Key]
        public int RouterID { get; set; }
        public string RouterName { get; set; }
        public string IPAddress { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Owner { get; set; }
        public string RouterDescription { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateDate { get; set; }
        public string MikroTikVersion { get; set; }
        public int? MikroTikVersionID { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public int? StatusID { get; set; }
    }
}