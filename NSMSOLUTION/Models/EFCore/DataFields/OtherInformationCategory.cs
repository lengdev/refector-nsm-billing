using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherInformationCategory")]
    public class OtherInformationCategory
    {
        [Key]
        [Column("CategoryID")]
        public int OtherInformationCategoryID { get; set; }

        public string CategoryName { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }

        // Navigation property
        public virtual ICollection<OtherInformation> Informations { get; set; }

        public OtherInformationCategory()
        {
            Informations = new List<OtherInformation>();
        }
    }
}
