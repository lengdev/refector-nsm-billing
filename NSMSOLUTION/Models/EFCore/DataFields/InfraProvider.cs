using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class InfraProvider
    {
        public int InfraProviderID { get; set; }
        public string InfraProviderName { get; set; } = string.Empty;
        public int? UserID { get; set; }
        public string CreateBy { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; } = string.Empty;
        public string InfraProviderDescription { get; set; }
    }
}
