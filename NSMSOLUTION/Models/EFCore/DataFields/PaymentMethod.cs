using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    [Table("TblPaymentMethod")]
    public class PaymentMethod
    {
        [Key] public int PaymentMethodID { get; set; }
        [Column("PaymentMethod")] public string Method { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }

}