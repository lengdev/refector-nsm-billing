using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerBlock
    {
        public int CustomerTypeID { get; set; }
        public int? CustomerGroupID { get; set; }
        public string CustomerGroup { get; set; }
        public string CustomerType { get; set; }
        public string DistrictCode { get; set; }
        public string Prefix { get; set; }
        public int? StartNumber { get; set; }
        public string PrefixVAT { get; set; }
        public int? StartNumberPrefixVAT { get; set; }
        public string PrefixCM { get; set; }
        public int? StartNumberPrefixCM { get; set; }
        public string Memo { get; set; }
        public int? UserID { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? OrderBy { get; set; }
        public string Status { get; set; }
        public bool? ISVAT { get; set; }
        public string SIDNonVAT { get; set; }
        public string SIDVAT { get; set; }
        public string COIDNonVAT { get; set; }
        public string COIDVAT { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}