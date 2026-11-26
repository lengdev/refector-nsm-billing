using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class Commission
    {
        public int CommissionID { get; set; }
        public int CustomerID { get; set; }
        public string CommissionTypeID { get; set; }

        public decimal CommissionAmountUSD { get; set; }
        public string CommissionAmountP { get; set; }

        public string InvoiceNo { get; set; }
        public string Memo { get; set; }

        public string AgencyName { get; set; }
        public string AgencyContactPhone { get; set; }

        public int UserID { get; set; }

        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
