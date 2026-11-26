using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherCharge")]
    public class TblOtherCharge
    {
        [Key]
        public int OtherChargeID { get; set; }
        public string OtherChargeItem { get; set; }
        [Column("OtherChargeDescription")]
        public string Description { get; set; }
        public int ServiceID { get; set; }
        public decimal Fee { get; set; }
        public decimal CostPrice { get; set; }
        public string UM { get; set; }
        public int? IncomeAccountID { get; set; }
        public int? ExpenseAccountID { get; set; }
        public string Status { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }

        // Navigation property to Service
        public virtual TblServices Service { get; set; }

        // Note: Images are linked via TblOtherChargeImage.ReferenceID + ReferenceType = 'OtherCharge'
        // They are loaded manually in queries (no EF navigation since FK is polymorphic)
    }
}
