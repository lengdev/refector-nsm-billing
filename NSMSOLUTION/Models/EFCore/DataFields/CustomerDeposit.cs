using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerDeposit
    {
        public int? DepositFeeID { get; set; }
        public int? CustomerID { get; set; }
        public string ConnectionID { get; set; }
        public decimal? DepositFee { get; set; }
        public string InvoiceNo { get; set; }
        public bool? IsIncludedVAT { get; set; }
        public string Memo { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
