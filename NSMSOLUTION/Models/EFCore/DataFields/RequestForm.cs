using System;

namespace NSMSOLUTION.Models.EFCore
{
    public class RequestForm
    {
        public int RequestFormID { get; set; }

        public DateTime? RequestDate { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime? RequirementDate { get; set; }
        public string RequestType { get; set; }
        public string Remark { get; set; }

        public string PhoneNumber { get; set; }
        public string BankInformation { get; set; }

        public int? VendorID { get; set; }
        public string VendorName { get; set; }

        public int? DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        public string PaymentMethod { get; set; }
        public decimal? TotalEstimate { get; set; }
        public decimal? SubTotal { get; set; }

        public string DestinationFolder { get; set; }
        public string AttachmentFile { get; set; }

        public string PaymentMean { get; set; }
        public int? PaymentMeanID { get; set; }

        public int? ClassID { get; set; }
        public string ClassName { get; set; }

        public int? StatusID { get; set; }
        public int? RequestStatusID { get; set; }

        public int? ExchangeRate { get; set; }
        public string ImagePath { get; set; }

        public string DeliverTo { get; set; }
        public int? WID { get; set; }

        public int? RequestByID { get; set; }
        public string RequestBy { get; set; }

        public string NeedToCheckBy { get; set; }
        public int? NeedToCheckUserID { get; set; }

        public string CheckedBy { get; set; }
        public int? CheckedByUserID { get; set; }
        public DateTime? CheckedDate { get; set; }
        public string CheckedByUsername { get; set; }

        public int? NeedApprovalByUserID { get; set; }
        public string NeedApprovalBy { get; set; }

        public int? ApprovedByUserID { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalFrom { get; set; }

        public int? RejectByUserID { get; set; }
        public string RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectFrom { get; set; }

        public int? CancelByUserID { get; set; }
        public string CancelBy { get; set; }
        public DateTime? CancelDate { get; set; }

        public int? NeedReceievedByID { get; set; }
        public string NeedReceievedBy { get; set; }

        public int? ReceivedByID { get; set; }
        public string ReceivedBy { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReceivedByUsername { get; set; }

        public int? DisapproveByUserID { get; set; }
        public string DisapproveBy { get; set; }
        public DateTime? DisapproveDate { get; set; }
        public string DisapproveFrom { get; set; }

        public string Comment { get; set; }

        public int? TelegramMessageID { get; set; }
        public string TelegramMessage { get; set; }
        public string TelegramUserName { get; set; }
        public string TelegramGroupID { get; set; }

        public int? UserID { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }

        public int SystemID { get; set; }
        public int WarehouseID { get; set; }
        public string DomainID { get; set; }

        // Next Approval Workflow
        public int? NeedNextApprovalByUserID { get; set; }
        public string NeedNextApprovalBy { get; set; }
        public string NeedNextApprovalByUsername { get; set; }

        public string NextApprovedBy { get; set; }
        public int? NextApprovedByID { get; set; }
        public DateTime? NextApprovedByDate { get; set; }
        public string NextApprovedFrom { get; set; }
        public int? NextApproveStatusID { get; set; }

        public int? NextDisapproveByID { get; set; }
        public string NextDisapproveBy { get; set; }
        public DateTime? NextDisapproveDate { get; set; }
    }
}
