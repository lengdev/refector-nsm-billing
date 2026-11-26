using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherChargeImage")]
    public class TblOtherChargeImage
    {
        [Key]
        public int OtherChargeImageID { get; set; }

        /// <summary>
        /// Generic reference ID — for OtherCharge images this stores OtherChargeID.
        /// </summary>
        public int ReferenceID { get; set; }

        /// <summary>
        /// Discriminator for the reference type, e.g. "OtherCharge".
        /// </summary>
        public string ReferenceType { get; set; }

        public string FileName { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
