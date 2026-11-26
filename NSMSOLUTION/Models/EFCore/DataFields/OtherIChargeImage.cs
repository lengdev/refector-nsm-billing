using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherIChargeImage")]
    public class OtherIChargeImage
    {
        [Key]
        public int OtherIChargeImageID { get; set; }
        public int OtherInformationID { get; set; }
        public string FileName { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }

        // Navigation property
        [ForeignKey("OtherInformationID")]
        public virtual OtherInformation OtherInformation { get; set; }
    }
}
