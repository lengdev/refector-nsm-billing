using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[Table("TblAccountTypes")]
public class TblAccountType
{
    [Key] public int AccountTypesID { get; set; }
    public string AccountTypes { get; set; }
    public string NormalSide { get; set; }
}