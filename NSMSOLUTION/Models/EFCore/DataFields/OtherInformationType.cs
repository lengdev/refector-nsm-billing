using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblOtherInformationType")]
    public class OtherInformationType
    {
        [Key]
        [Column("TypeID")]
        public int OtherInformationTypeID { get; set; }
        public string TypeName { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
        [Column("CategoryID")]
        public int? CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public virtual OtherInformationCategory OtherInformationCategory { get; set; }

    }
}
