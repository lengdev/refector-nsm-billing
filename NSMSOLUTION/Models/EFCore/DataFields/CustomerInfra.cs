using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class CustomerInfra
    {
        public int CustomerInfraID { get; set; }
        public int CustomerID { get; set; }
        public int InfraProviderID { get; set; }
        public string CID { get; set; }
        public string SID { get; set; }
        public string ConnectionID { get; set; }
        public string CustomerInfraName { get; set; }
        public string CustomerInfraSID { get; set; }
        public string CustomerInfraSurveyID { get; set; }
        public string CustomerInfraCID { get; set; }
        public string CustomerInfraAID { get; set; }
        public int? UserID { get; set; }

        // Audit Fields
        public string InvoiceNo { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }

        // System Fields
        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
