using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TblAccountCategories")]
public class TblAccountCategories
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // change to Identity if auto-increment
    public int AccountCategoryID { get; set; }

    public string AccountCategory { get; set; }          // nvarchar(max)

    [MaxLength(50)] public string Debit { get; set; }
    [MaxLength(50)] public string Credit { get; set; }
    [MaxLength(50)] public string Icon { get; set; }

    public int? OrderBy { get; set; }
    public DateTime? CreateDate { get; set; }

    [MaxLength(50)] public string CreateBy { get; set; }
    [MaxLength(50)] public string Statement { get; set; }
    [MaxLength(50)] public string CommonNumberSequences { get; set; }

    public int? SystemID { get; set; }
    public int? WarehouseID { get; set; }

    [MaxLength(50)] public string DomainID { get; set; }

    public int? AccountTypesID { get; set; }
    public string UsedFor { get; set; }                  // nvarchar(max)
}