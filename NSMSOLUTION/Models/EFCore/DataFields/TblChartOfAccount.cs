
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TblChartOfAccount")]
public class TblChartOfAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ChartOfAccountID { get; set; }

    [StringLength(50)]
    public string AccountNo { get; set; }

    public string AccountName { get; set; }

    [StringLength(50)]
    public string AccountType { get; set; }

    [StringLength(50)]
    public string SubOfAccountType { get; set; }

    public int? ParentAccountID { get; set; }

    public int? OrderBy { get; set; }

    public int? StatusID { get; set; }

    public string Memo { get; set; }

    public DateTime? ActivateAccountAsOf { get; set; }

    // SQL decimal(18,2)
    public decimal? OpeningBalance { get; set; }

    public DateTime? OpeningBalanceDate { get; set; }

    public int? SystemID { get; set; }

    public int? WarehouseID { get; set; }

    [StringLength(50)]
    public string DomainID { get; set; } = "001";

    public bool? DefaultForInvoice { get; set; }

    public bool? DefaultForBill { get; set; }

    public int? ClassID { get; set; }

    public bool? IsDefault { get; set; }

    public int? CompanyID { get; set; }

    // SQL decimal(18,2)
    public decimal? DebitBalance { get; set; }

    // SQL decimal(18,2)
    public decimal? CreditBalance { get; set; }

    [StringLength(50)]
    public string NormalSide { get; set; }

    public int? AccountCategoryID { get; set; }

    public int? AccountTypesID { get; set; }
}
