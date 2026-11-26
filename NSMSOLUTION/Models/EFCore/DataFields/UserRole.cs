using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblUserRole")]
    public class UserRole
    {
        [Key]
        public int UserRoleID { get; set; }

        [StringLength(50)]
        public string RoleName { get; set; }

        public DateTime? CreateDate { get; set; }

        [StringLength(50)]
        public string CreateBy { get; set; }

        public int? SystemID { get; set; }

        public int? WarehouseID { get; set; }

        [StringLength(50)]
        public string DomainID { get; set; } = "001";
    }
}