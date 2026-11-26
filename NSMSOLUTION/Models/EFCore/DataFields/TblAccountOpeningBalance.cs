
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TblAccount_OpeningBalances")]
public class TblAccountOpeningBalance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OpeningBalanceID { get; set; }

    public int? AccountID { get; set; }

    [Column(TypeName = "decimal")]
    public decimal? OpeningBalance { get; set; }

    [Column(TypeName = "date")]
    public DateTime? OpeningBalanceDate { get; set; }

    [StringLength(4)]
    public string FiscalYear { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CompanyID { get; set; }

    public int? SystemID { get; set; }

    public int? WarehouseID { get; set; }

    [StringLength(50)]
    public string DomainID { get; set; }

    public int? CreatedByUserID { get; set; }
}
