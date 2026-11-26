using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherInformation")]
    public class OtherInformation
    {
        [Key]
        public int OtherInformationID { get; set; }

        [Column("CategoryID")]
        public int? OtherInformationCategoryID { get; set; }

        public string Title { get; set; }
        public string NewsContent { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        public int? TypeID { get; set; }


        // Navigation properties

        [ForeignKey("OtherInformationCategoryID")]
        public virtual OtherInformationCategory OtherInformationCategory { get; set; }

        [ForeignKey("TypeID")]
        public virtual OtherInformationType OtherInformationType { get; set; }

        public virtual ICollection<OtherIChargeImage> Images { get; set; }

        public OtherInformation()
        {
            Images = new List<OtherIChargeImage>();
        }
    }
}
