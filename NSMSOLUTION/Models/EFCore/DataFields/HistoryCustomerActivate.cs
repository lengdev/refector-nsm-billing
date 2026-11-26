using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class HistoryCustomerActivate
    {
        public int HistoryCustomerActivateID { get; set; }
        public DateTime? Date { get; set; }
        public int? ActivateCustomerID { get; set; }
        public int? BillingInvoiceID { get; set; }
        public string ConnectionID { get; set; }
        public string ConnectionType { get; set; }
        public int? CustomerID { get; set; }
        public int? ServiceID { get; set; }
        public int? PackageDetailID { get; set; }
        public DateTime? ActivateDate { get; set; }
        public string ActivateBy { get; set; }
        public string Comment { get; set; }
        public DateTime? LockedDate { get; set; }
        public string LockedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewDate { get; set; }
        public int? Duration { get; set; }
        public int? ConnectivityStatus { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}