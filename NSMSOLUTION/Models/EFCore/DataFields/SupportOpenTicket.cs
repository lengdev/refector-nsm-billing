using System;

namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class SupportOpenTicket
    {
        public int OpenTicketID { get; set; }
        public string TicketNumber { get; set; }
        public int? SupportProjectID { get; set; }
        public string ProjectName { get; set; }
        public int? TrackerID { get; set; }
        public string TrackerName { get; set; }
        public int? PostAudienceID { get; set; }
        public string PostAudience { get; set; }
        public string PostAudienceUserLoginID { get; set; }
        public DateTime? Deadline { get; set; }
        public string Subject { get; set; }
        public int? StatusID { get; set; }
        public string Status { get; set; }
        public int? PriorityID { get; set; }
        public string Priority { get; set; }
        public int? SupportDoneID { get; set; }
        public string SupportDone { get; set; }
        public string AssigneeID { get; set; }
        public string AssigneeName { get; set; }
        public string CoWorkerID { get; set; }
        public string CoWorkerName { get; set; }
        public string SID { get; set; }
        public string ConnectionID { get; set; }
        public int? CustomerID { get; set; }
        public string CID { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int? ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int? PackageDetailID { get; set; }
        public string PackageName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Address { get; set; }
        public string Commune { get; set; }
        public string District { get; set; }
        public string ProvinceCity { get; set; }
        public string Description { get; set; }
        public string CreateByFullName { get; set; }
        public string LOTSwitch { get; set; }
        public string PONPort { get; set; }
        public string ONTID { get; set; }
        public string ONTDeviceModel { get; set; }
        public string SerialNumber { get; set; }
        public string ProblemType { get; set; }
        public int? UserLoginID { get; set; }
        public string DescriptionChangeStatus { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string InvoiceNo { get; set; }
        public string TelegramMessageID { get; set; }
        public string TelegramMessage { get; set; }
        public string TelegramUser { get; set; }
        public string TelegramGroupNameAndMessageID { get; set; }
        public int? SystemID { get; set; }
        public int? WarehouseID { get; set; }
        public string DomainID { get; set; }
    }
}
