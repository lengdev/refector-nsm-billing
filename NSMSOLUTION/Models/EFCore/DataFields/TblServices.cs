using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblServices")]
    public class TblServices
    {
        [Key]
        public int ServiceID { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }

        // Navigation property
        public virtual ICollection<TblOtherCharge> OtherCharges { get; set; }

        public TblServices()
        {
            OtherCharges = new List<TblOtherCharge>();
        }
    }
}
